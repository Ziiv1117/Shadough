using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TopdownAuthoredLayoutPlayProbe
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string RequestPath = "Temp/TopdownAuthoredLayoutPlayProbe.request";
    private const string SnapshotPath = "Temp/TopdownAuthoredLayoutPlayProbe.snapshot.txt";
    private const string ReportPath = "Logs/TopdownAuthoredLayoutPlayProbe.report.txt";
    private const float PositionTolerance = 0.001f;

    private static readonly string[] FixedObjectNames =
    {
        "Level01_MapReference",
        "Level01_FinalMap_Reference",
        "Level01_ManualWalls",
        "Tree_01",
        "TreeShadowRootAnchor_Topdown",
        "PressurePlate_Topdown",
        "Door_Pressure_Topdown",
        "KeySource_Topdown",
        "KeyShadow_Topdown",
        "Door_Lock_Topdown",
        "Lock_Topdown",
        "BeamSource_Topdown",
        "BeamShadow_Topdown",
        "FinalClockCore_Topdown"
    };

    [InitializeOnLoadMethod]
    private static void Register()
    {
        EditorApplication.update -= Poll;
        EditorApplication.update += Poll;
    }

    [MenuItem("Shadough/Tests/Verify Authored Layout In Play Mode")]
    public static void RequestProbe()
    {
        File.WriteAllText(FullPath(RequestPath), "capture");
    }

    private static void Poll()
    {
        string request = FullPath(RequestPath);
        if (!File.Exists(request) || EditorApplication.isCompiling)
        {
            return;
        }

        string state;
        try
        {
            state = File.ReadAllText(request).Trim();
        }
        catch (IOException)
        {
            return;
        }
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            if (SceneManager.GetActiveScene().path != ScenePath)
            {
                WriteFailure("Open ClockTower_TopdownPrototype before running the layout probe.");
                File.Delete(request);
                return;
            }

            CaptureSnapshot();
            File.WriteAllText(request, DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture));
            EditorApplication.isPlaying = true;
            return;
        }

        if (!EditorApplication.isPlaying)
        {
            return;
        }

        long capturedTicks;
        if (!long.TryParse(state, NumberStyles.Integer, CultureInfo.InvariantCulture, out capturedTicks))
        {
            return;
        }

        if (new TimeSpan(DateTime.UtcNow.Ticks - capturedTicks).TotalSeconds < 1d)
        {
            return;
        }

        CompareRuntimePositions();
        File.Delete(request);
        EditorApplication.isPlaying = false;
    }

    private static void CaptureSnapshot()
    {
        StringBuilder snapshot = new StringBuilder();
        for (int i = 0; i < FixedObjectNames.Length; i++)
        {
            GameObject obj = FindSceneObject(FixedObjectNames[i]);
            if (obj == null)
            {
                snapshot.AppendLine(FixedObjectNames[i] + "\tMISSING");
                continue;
            }

            Vector3 position = obj.transform.position;
            snapshot.AppendLine(FixedObjectNames[i]
                + "\t" + position.x.ToString("R", CultureInfo.InvariantCulture)
                + "\t" + position.y.ToString("R", CultureInfo.InvariantCulture)
                + "\t" + position.z.ToString("R", CultureInfo.InvariantCulture));
        }

        File.WriteAllText(FullPath(SnapshotPath), snapshot.ToString());
    }

    private static void CompareRuntimePositions()
    {
        Dictionary<string, Vector3?> authoredPositions = ReadSnapshot();
        StringBuilder report = new StringBuilder();
        report.AppendLine("Topdown Authored Layout Play Probe");
        report.AppendLine("Scene: " + ScenePath);

        bool allPresentPositionsPreserved = true;
        foreach (KeyValuePair<string, Vector3?> pair in authoredPositions)
        {
            GameObject obj = FindSceneObject(pair.Key);
            if (!pair.Value.HasValue)
            {
                report.AppendLine(pair.Key + ": intentionally missing before Play");
                continue;
            }

            if (obj == null)
            {
                allPresentPositionsPreserved = false;
                report.AppendLine(pair.Key + ": FAIL (missing at runtime)");
                continue;
            }

            Vector3 runtimePosition = obj.transform.position;
            float delta = Vector3.Distance(pair.Value.Value, runtimePosition);
            bool preserved = delta <= PositionTolerance;
            allPresentPositionsPreserved &= preserved;
            report.AppendLine(pair.Key + ": " + (preserved ? "PASS" : "FAIL")
                + ", delta=" + delta.ToString("R", CultureInfo.InvariantCulture)
                + ", authored=" + Format(pair.Value.Value)
                + ", runtime=" + Format(runtimePosition));
        }

        report.AppendLine("All existing fixed positions preserved: " + (allPresentPositionsPreserved ? "PASS" : "FAIL"));

        LightDrivenShadow treeShadow = UnityEngine.Object.FindObjectOfType<LightDrivenShadow>();
        if (treeShadow == null)
        {
            report.AppendLine("TreeShadow root locked to anchor: FAIL (LightDrivenShadow missing)");
        }
        else
        {
            float rootDelta = Vector3.Distance(treeShadow.ShadowRootWorldPosition, treeShadow.RootAnchorWorldPosition);
            report.AppendLine("TreeShadow root locked to anchor: " + (rootDelta <= PositionTolerance ? "PASS" : "FAIL")
                + ", delta=" + rootDelta.ToString("R", CultureInfo.InvariantCulture)
                + ", root=" + Format(treeShadow.ShadowRootWorldPosition)
                + ", anchor=" + Format(treeShadow.RootAnchorWorldPosition));

            PlayerLanternController lantern = UnityEngine.Object.FindObjectOfType<PlayerLanternController>();
            Transform lightPoint = lantern != null ? lantern.LightPoint : null;
            if (lightPoint == null)
            {
                report.AppendLine("TreeShadow root remains locked after light movement: FAIL (light point missing)");
            }
            else
            {
                Vector3 originalLightPosition = lightPoint.position;
                lightPoint.position = originalLightPosition + new Vector3(1.25f, -0.75f, 0f);
                treeShadow.SendMessage("Update", SendMessageOptions.DontRequireReceiver);

                float movedLightRootDelta = Vector3.Distance(treeShadow.ShadowRootWorldPosition, treeShadow.RootAnchorWorldPosition);
                report.AppendLine("TreeShadow root remains locked after light movement: "
                    + (movedLightRootDelta <= PositionTolerance ? "PASS" : "FAIL")
                    + ", delta=" + movedLightRootDelta.ToString("R", CultureInfo.InvariantCulture));

                lightPoint.position = originalLightPosition;
                treeShadow.SendMessage("Update", SendMessageOptions.DontRequireReceiver);
            }
        }

        File.WriteAllText(FullPath(ReportPath), report.ToString());
        Debug.Log("Authored layout play probe complete: " + FullPath(ReportPath));
    }

    private static Dictionary<string, Vector3?> ReadSnapshot()
    {
        Dictionary<string, Vector3?> result = new Dictionary<string, Vector3?>();
        string[] lines = File.ReadAllLines(FullPath(SnapshotPath));
        for (int i = 0; i < lines.Length; i++)
        {
            string[] fields = lines[i].Split('\t');
            if (fields.Length == 2 && fields[1] == "MISSING")
            {
                result[fields[0]] = null;
                continue;
            }

            if (fields.Length != 4)
            {
                continue;
            }

            result[fields[0]] = new Vector3(
                float.Parse(fields[1], CultureInfo.InvariantCulture),
                float.Parse(fields[2], CultureInfo.InvariantCulture),
                float.Parse(fields[3], CultureInfo.InvariantCulture));
        }

        return result;
    }

    private static GameObject FindSceneObject(string objectName)
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i].name == objectName && objects[i].scene.IsValid())
            {
                return objects[i];
            }
        }

        return null;
    }

    private static void WriteFailure(string message)
    {
        File.WriteAllText(FullPath(ReportPath), "Topdown Authored Layout Play Probe\nFAIL: " + message + "\n");
    }

    private static string FullPath(string projectRelativePath)
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "..", projectRelativePath));
    }

    private static string Format(Vector3 value)
    {
        return string.Format(CultureInfo.InvariantCulture, "({0:R},{1:R},{2:R})", value.x, value.y, value.z);
    }
}
