using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AlignTreeShadowAnchorToMapRoot
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string AnchorName = "TreeShadowRootAnchor_Topdown";
    private const string RequestPath = "Temp/AlignTreeShadowAnchorToMapRoot.request";
    private const string ReportPath = "Logs/AlignTreeShadowAnchorToMapRoot.report.txt";

    // Pixel (118, 879) on level01_full_map.png, converted using its centered pivot and 40 PPU.
    private static readonly Vector2 MapTreeRootPosition = new Vector2(-15.15f, -8.4f);

    [InitializeOnLoadMethod]
    private static void Register()
    {
        EditorApplication.update -= Poll;
        EditorApplication.update += Poll;
    }

    [MenuItem("Shadough/Setup/Align Tree Shadow Anchor To Map Root")]
    public static void RunFromMenu()
    {
        Align();
    }

    private static void Poll()
    {
        string request = FullPath(RequestPath);
        if (!File.Exists(request) || EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        File.Delete(request);
        Align();
    }

    private static void Align()
    {
        if (SceneManager.GetActiveScene().path != ScenePath)
        {
            WriteReport("FAIL: Open ClockTower_TopdownPrototype before aligning the tree shadow anchor.");
            return;
        }

        GameObject anchor = GameObject.Find(AnchorName);
        if (anchor == null)
        {
            WriteReport("FAIL: " + AnchorName + " was not found.");
            return;
        }

        Vector3 previousPosition = anchor.transform.position;
        Undo.RecordObject(anchor.transform, "Align tree shadow anchor to map root");
        anchor.transform.position = new Vector3(MapTreeRootPosition.x, MapTreeRootPosition.y, previousPosition.z);

        EditorUtility.SetDirty(anchor.transform);
        EditorSceneManager.MarkSceneDirty(anchor.scene);
        EditorSceneManager.SaveScene(anchor.scene);

        WriteReport(string.Format(
            CultureInfo.InvariantCulture,
            "PASS\nScene: {0}\nObject: {1}\nPrevious: ({2:R}, {3:R}, {4:R})\nCurrent: ({5:R}, {6:R}, {7:R})\nMap root pixel: (118, 879)",
            ScenePath,
            AnchorName,
            previousPosition.x,
            previousPosition.y,
            previousPosition.z,
            anchor.transform.position.x,
            anchor.transform.position.y,
            anchor.transform.position.z));
    }

    private static void WriteReport(string message)
    {
        File.WriteAllText(FullPath(ReportPath), "Align Tree Shadow Anchor To Map Root\n" + message + "\n");
        Debug.Log("Tree shadow anchor alignment complete: " + FullPath(ReportPath));
    }

    private static string FullPath(string projectRelativePath)
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "..", projectRelativePath));
    }
}
