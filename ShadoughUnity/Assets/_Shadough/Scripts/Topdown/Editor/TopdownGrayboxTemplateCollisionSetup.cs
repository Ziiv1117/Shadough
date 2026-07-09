using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TopdownGrayboxTemplateCollisionSetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string RequestPath = "Temp/TopdownGrayboxTemplateCollisionSetup.request";
    private const string ReportPath = "Logs/TopdownGrayboxTemplateCollisionSetup.report.txt";
    private const string ReferenceWallName = "Wall_StartIsland_Bottom";

    private static readonly Regex TemplateNamePattern = new Regex(
        "^graybox_template( \\([0-9]+\\))?$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    [InitializeOnLoadMethod]
    private static void RegisterAutoSetup()
    {
        EditorApplication.update -= TryAutoSetup;
        EditorApplication.update += TryAutoSetup;
    }

    [MenuItem("Shadough/Setup Topdown Graybox Template Collision")]
    public static void SetupFromMenu()
    {
        Setup();
    }

    private static void TryAutoSetup()
    {
        if (EditorApplication.isCompiling)
        {
            return;
        }

        string requestPath = FullPath(RequestPath);
        if (!File.Exists(requestPath))
        {
            return;
        }

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.isPlaying = false;
            return;
        }

        File.Delete(requestPath);
        Setup();
    }

    public static void Setup()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        StringBuilder report = new StringBuilder();
        report.AppendLine("Topdown Graybox Template Collision Setup");
        report.AppendLine("Scene: " + ScenePath);
        report.AppendLine("referenceWall=" + ReferenceWallName);

        GameObject referenceWall = FindSceneObject(ReferenceWallName);
        if (referenceWall == null)
        {
            throw new MissingReferenceException(ReferenceWallName + " was not found in " + ScenePath);
        }

        BoxCollider2D referenceCollider = referenceWall.GetComponent<BoxCollider2D>();
        if (referenceCollider == null)
        {
            throw new MissingReferenceException(ReferenceWallName + " has no BoxCollider2D.");
        }

        List<GameObject> templates = FindTemplateObjects();
        templates.Sort(CompareTemplateNames);

        report.AppendLine("reference.layer=" + LayerMask.LayerToName(referenceWall.layer) + "(" + referenceWall.layer + ")");
        report.AppendLine("reference.tag=" + referenceWall.tag);
        report.AppendLine("reference.colliderType=BoxCollider2D");
        report.AppendLine("reference.isTrigger=" + referenceCollider.isTrigger);
        report.AppendLine("template.count=" + templates.Count);

        int addedColliders = 0;
        int updatedColliders = 0;
        int missingRenderers = 0;

        for (int i = 0; i < templates.Count; i++)
        {
            GameObject template = templates[i];
            SpriteRenderer renderer = template.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                missingRenderers++;
                report.AppendLine(template.name + ".skipped=no SpriteRenderer");
                continue;
            }

            BoxCollider2D collider = template.GetComponent<BoxCollider2D>();
            bool added = false;
            if (collider == null)
            {
                collider = template.AddComponent<BoxCollider2D>();
                added = true;
                addedColliders++;
            }
            else
            {
                updatedColliders++;
            }

            Vector2 visualSize = GetRendererLocalSize(renderer);
            template.layer = referenceWall.layer;
            template.tag = referenceWall.tag;
            collider.sharedMaterial = referenceCollider.sharedMaterial;
            collider.isTrigger = false;
            collider.usedByEffector = referenceCollider.usedByEffector;
            collider.usedByComposite = referenceCollider.usedByComposite;
            collider.offset = Vector2.zero;
            collider.size = visualSize;

            EditorUtility.SetDirty(template);
            EditorUtility.SetDirty(collider);

            report.AppendLine(template.name
                + ".collider=" + (added ? "added" : "updated")
                + ", layer=" + LayerMask.LayerToName(template.layer) + "(" + template.layer + ")"
                + ", tag=" + template.tag
                + ", isTrigger=" + collider.isTrigger
                + ", size=" + FormatVector2(collider.size)
                + ", rendererSize=" + FormatVector2(visualSize));
        }

        ValidateScene(report, templates, addedColliders, updatedColliders, missingRenderers);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        File.WriteAllText(FullPath(ReportPath), report.ToString());
        Debug.Log("Topdown graybox template collision setup complete. Report: " + FullPath(ReportPath));
    }

    private static Vector2 GetRendererLocalSize(SpriteRenderer renderer)
    {
        if (renderer.drawMode == SpriteDrawMode.Sliced || renderer.drawMode == SpriteDrawMode.Tiled)
        {
            return renderer.size;
        }

        if (renderer.sprite != null)
        {
            return renderer.sprite.bounds.size;
        }

        return Vector2.one;
    }

    private static void ValidateScene(StringBuilder report, List<GameObject> templates, int addedColliders, int updatedColliders, int missingRenderers)
    {
        CountMissingSceneData(out int missingScripts, out int missingReferences);

        bool allHaveBlockingColliders = templates.Count > 0;
        for (int i = 0; i < templates.Count; i++)
        {
            BoxCollider2D collider = templates[i].GetComponent<BoxCollider2D>();
            SpriteRenderer renderer = templates[i].GetComponent<SpriteRenderer>();
            if (collider == null
                || collider.isTrigger
                || collider.size.x <= 0f
                || collider.size.y <= 0f
                || renderer == null)
            {
                allHaveBlockingColliders = false;
                break;
            }
        }

        report.AppendLine("colliders.added=" + addedColliders);
        report.AppendLine("colliders.updated=" + updatedColliders);
        report.AppendLine("templates.missingSpriteRenderer=" + missingRenderers);
        report.AppendLine("templates.allHaveBlockingBoxCollider=" + PassFail(allHaveBlockingColliders));
        report.AppendLine("missingScripts=" + missingScripts);
        report.AppendLine("missingReferences=" + missingReferences);
        report.AppendLine("consoleCompileErrors=checked_after_unity_refresh");
    }

    private static List<GameObject> FindTemplateObjects()
    {
        List<GameObject> templates = new List<GameObject>();
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject sceneObject = objects[i];
            if (IsObjectInTargetScene(sceneObject) && TemplateNamePattern.IsMatch(sceneObject.name))
            {
                templates.Add(sceneObject);
            }
        }

        return templates;
    }

    private static int CompareTemplateNames(GameObject left, GameObject right)
    {
        return GetTemplateIndex(left.name).CompareTo(GetTemplateIndex(right.name));
    }

    private static int GetTemplateIndex(string objectName)
    {
        Match match = Regex.Match(objectName, "\\(([0-9]+)\\)");
        if (!match.Success)
        {
            return 0;
        }

        return int.Parse(match.Groups[1].Value);
    }

    private static GameObject FindSceneObject(string objectName)
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject sceneObject = objects[i];
            if (IsObjectInTargetScene(sceneObject) && sceneObject.name == objectName)
            {
                return sceneObject;
            }
        }

        return null;
    }

    private static bool IsObjectInTargetScene(GameObject sceneObject)
    {
        return sceneObject != null
            && sceneObject.scene.IsValid()
            && sceneObject.scene.path == ScenePath;
    }

    private static void CountMissingSceneData(out int missingScripts, out int missingReferences)
    {
        missingScripts = 0;
        missingReferences = 0;

        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject sceneObject = objects[i];
            if (!IsObjectInTargetScene(sceneObject))
            {
                continue;
            }

            Component[] components = sceneObject.GetComponents<Component>();
            for (int componentIndex = 0; componentIndex < components.Length; componentIndex++)
            {
                Component component = components[componentIndex];
                if (component == null)
                {
                    missingScripts++;
                    continue;
                }

                SerializedObject serializedObject = new SerializedObject(component);
                SerializedProperty property = serializedObject.GetIterator();
                bool enterChildren = true;
                while (property.NextVisible(enterChildren))
                {
                    enterChildren = false;
                    if (property.propertyType == SerializedPropertyType.ObjectReference
                        && property.objectReferenceValue == null
                        && property.objectReferenceInstanceIDValue != 0)
                    {
                        missingReferences++;
                    }
                }
            }
        }
    }

    private static string FullPath(string projectRelativePath)
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "..", projectRelativePath));
    }

    private static string FormatVector2(Vector2 value)
    {
        return "(" + value.x.ToString("0.###") + ", " + value.y.ToString("0.###") + ")";
    }

    private static string PassFail(bool passed)
    {
        return passed ? "PASS" : "FAIL";
    }
}
