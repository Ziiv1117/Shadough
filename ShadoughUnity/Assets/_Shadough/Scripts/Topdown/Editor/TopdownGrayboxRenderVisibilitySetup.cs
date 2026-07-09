using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TopdownGrayboxRenderVisibilitySetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string ReportPath = "Logs/TopdownGrayboxRenderVisibilitySetup.report.txt";
    private const string GrayboxRootName = "Graybox_Collision_Topdown";
    private const string ManualWallsRootName = "Level01_ManualWalls";

    [MenuItem("Shadough/Setup Topdown Graybox Render Visibility")]
    public static void SetupFromMenu()
    {
        Setup();
    }

    public static void Setup()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        StringBuilder report = new StringBuilder();
        report.AppendLine("Topdown Graybox Render Visibility Setup");
        report.AppendLine("Scene: " + ScenePath);

        List<GameObject> candidates = FindGrayboxCandidates();
        candidates.Sort((left, right) => string.CompareOrdinal(GetHierarchyPath(left), GetHierarchyPath(right)));

        int spriteRenderersDisabled = 0;
        int meshRenderersDisabled = 0;
        int collidersChanged = 0;
        int activeStateChanged = 0;

        report.AppendLine("candidate.count=" + candidates.Count);

        for (int i = 0; i < candidates.Count; i++)
        {
            GameObject candidate = candidates[i];
            Collider2D[] collidersBefore = candidate.GetComponents<Collider2D>();
            bool wasActive = candidate.activeSelf;

            SpriteRenderer[] spriteRenderers = candidate.GetComponents<SpriteRenderer>();
            MeshRenderer[] meshRenderers = candidate.GetComponents<MeshRenderer>();

            int disabledSpritesOnObject = 0;
            for (int rendererIndex = 0; rendererIndex < spriteRenderers.Length; rendererIndex++)
            {
                SpriteRenderer renderer = spriteRenderers[rendererIndex];
                if (renderer != null && renderer.enabled)
                {
                    renderer.enabled = false;
                    disabledSpritesOnObject++;
                    spriteRenderersDisabled++;
                    EditorUtility.SetDirty(renderer);
                }
            }

            int disabledMeshesOnObject = 0;
            for (int rendererIndex = 0; rendererIndex < meshRenderers.Length; rendererIndex++)
            {
                MeshRenderer renderer = meshRenderers[rendererIndex];
                if (renderer != null && renderer.enabled)
                {
                    renderer.enabled = false;
                    disabledMeshesOnObject++;
                    meshRenderersDisabled++;
                    EditorUtility.SetDirty(renderer);
                }
            }

            Collider2D[] collidersAfter = candidate.GetComponents<Collider2D>();
            if (ColliderStateChanged(collidersBefore, collidersAfter))
            {
                collidersChanged++;
            }

            if (candidate.activeSelf != wasActive)
            {
                activeStateChanged++;
            }

            if (disabledSpritesOnObject > 0 || disabledMeshesOnObject > 0 || spriteRenderers.Length > 0 || meshRenderers.Length > 0)
            {
                report.AppendLine(GetHierarchyPath(candidate)
                    + ", spriteRenderers=" + spriteRenderers.Length
                    + ", meshRenderers=" + meshRenderers.Length
                    + ", disabledSprites=" + disabledSpritesOnObject
                    + ", disabledMeshes=" + disabledMeshesOnObject
                    + ", collider2DCount=" + collidersAfter.Length);
            }

            EditorUtility.SetDirty(candidate);
        }

        ValidateScene(report, candidates, spriteRenderersDisabled, meshRenderersDisabled, collidersChanged, activeStateChanged);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        File.WriteAllText(FullPath(ReportPath), report.ToString());
        Debug.Log("Topdown graybox render visibility setup complete. Report: " + FullPath(ReportPath));
    }

    private static List<GameObject> FindGrayboxCandidates()
    {
        List<GameObject> candidates = new List<GameObject>();
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();

        for (int i = 0; i < objects.Length; i++)
        {
            GameObject sceneObject = objects[i];
            if (!IsObjectInTargetScene(sceneObject) || !IsGrayboxCandidate(sceneObject))
            {
                continue;
            }

            candidates.Add(sceneObject);
        }

        return candidates;
    }

    private static bool IsGrayboxCandidate(GameObject sceneObject)
    {
        if (sceneObject == null || sceneObject.GetComponent<Collider2D>() == null || HasGameplayComponent(sceneObject))
        {
            return false;
        }

        string objectName = sceneObject.name.ToLowerInvariant();
        if (objectName.Contains("graybox_template") || objectName.StartsWith("graybox_"))
        {
            return true;
        }

        if (sceneObject.name == "Wall_River_BrokenBridge_Blocker")
        {
            return true;
        }

        if (sceneObject.name.StartsWith("Wall_") && IsUnderNamedParent(sceneObject.transform, ManualWallsRootName))
        {
            return true;
        }

        if (IsUnderNamedParent(sceneObject.transform, GrayboxRootName))
        {
            return true;
        }

        return false;
    }

    private static bool HasGameplayComponent(GameObject sceneObject)
    {
        Component[] components = sceneObject.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            Component component = components[i];
            if (component == null)
            {
                continue;
            }

            string typeName = component.GetType().Name;
            if (typeName == "PastedShadowObject"
                || typeName == "ShadowInteractable"
                || typeName == "ShadowPressureTrigger"
                || typeName == "ShadowLockTrigger"
                || typeName == "PressurePlateController"
                || typeName == "LockController"
                || typeName == "DoorController"
                || typeName == "EnemyShadowSeeker"
                || typeName == "TopdownFinalClockCore"
                || typeName == "FinalClockCore"
                || typeName == "PlayerController"
                || typeName == "LightDrivenShadow"
                || typeName == "ShadowPasteArea")
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsUnderNamedParent(Transform transform, string parentName)
    {
        Transform current = transform.parent;
        while (current != null)
        {
            if (current.name == parentName)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static bool ColliderStateChanged(Collider2D[] before, Collider2D[] after)
    {
        if (before.Length != after.Length)
        {
            return true;
        }

        for (int i = 0; i < before.Length; i++)
        {
            Collider2D beforeCollider = before[i];
            Collider2D afterCollider = after[i];
            if (beforeCollider == null || afterCollider == null || beforeCollider != afterCollider)
            {
                return true;
            }
        }

        return false;
    }

    private static void ValidateScene(
        StringBuilder report,
        List<GameObject> candidates,
        int spriteRenderersDisabled,
        int meshRenderersDisabled,
        int collidersChanged,
        int activeStateChanged)
    {
        CountMissingSceneData(out int missingScripts, out int missingReferences);

        int enabledRenderers = 0;
        int colliderCount = 0;
        int triggerColliderCount = 0;
        int inactiveCandidateCount = 0;

        for (int i = 0; i < candidates.Count; i++)
        {
            GameObject candidate = candidates[i];
            if (!candidate.activeSelf)
            {
                inactiveCandidateCount++;
            }

            Renderer[] renderers = candidate.GetComponents<Renderer>();
            for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
            {
                if (renderers[rendererIndex] != null && renderers[rendererIndex].enabled)
                {
                    enabledRenderers++;
                }
            }

            Collider2D[] colliders = candidate.GetComponents<Collider2D>();
            colliderCount += colliders.Length;
            for (int colliderIndex = 0; colliderIndex < colliders.Length; colliderIndex++)
            {
                if (colliders[colliderIndex] != null && colliders[colliderIndex].isTrigger)
                {
                    triggerColliderCount++;
                }
            }
        }

        report.AppendLine("spriteRenderers.disabled=" + spriteRenderersDisabled);
        report.AppendLine("meshRenderers.disabled=" + meshRenderersDisabled);
        report.AppendLine("candidate.enabledRenderersAfter=" + enabledRenderers);
        report.AppendLine("candidate.collider2DCount=" + colliderCount);
        report.AppendLine("candidate.triggerCollider2DCount=" + triggerColliderCount);
        report.AppendLine("candidate.inactiveCount=" + inactiveCandidateCount);
        report.AppendLine("collidersChanged=" + collidersChanged);
        report.AppendLine("activeStateChanged=" + activeStateChanged);
        report.AppendLine("grayboxesHiddenInGameView=" + PassFail(enabledRenderers == 0));
        report.AppendLine("grayboxesStillHaveColliders=" + PassFail(colliderCount >= candidates.Count && collidersChanged == 0));
        report.AppendLine("grayboxActiveStatePreserved=" + PassFail(activeStateChanged == 0));
        report.AppendLine("missingScripts=" + missingScripts);
        report.AppendLine("missingReferences=" + missingReferences);
        report.AppendLine("consoleCompileErrors=checked_after_unity_refresh");
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

    private static bool IsObjectInTargetScene(GameObject sceneObject)
    {
        return sceneObject != null
            && sceneObject.scene.IsValid()
            && sceneObject.scene.path == ScenePath;
    }

    private static string GetHierarchyPath(GameObject sceneObject)
    {
        if (sceneObject == null)
        {
            return "<null>";
        }

        Stack<string> names = new Stack<string>();
        Transform current = sceneObject.transform;
        while (current != null)
        {
            names.Push(current.name);
            current = current.parent;
        }

        return string.Join("/", names.ToArray());
    }

    private static string FullPath(string projectRelativePath)
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "..", projectRelativePath));
    }

    private static string PassFail(bool passed)
    {
        return passed ? "PASS" : "FAIL";
    }
}
