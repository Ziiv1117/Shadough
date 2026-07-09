using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TopdownAudioSceneSetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string RequestPath = "Temp/TopdownAudioSceneSetup.request";
    private const string ReportPath = "Temp/TopdownAudioSceneSetup.report.txt";
    private const string PlayProbeReportPath = "Temp/TopdownAudioPlayProbe.report.txt";

    private const string BgmMainPath = "Assets/_Shadough/Audio/BGM/bgm_Main Gameplay.mp3";
    private const string BgmVictoryPath = "Assets/_Shadough/Audio/BGM/bgm_Victory.mp3";
    private const string FootstepPath = "Assets/_Shadough/Audio/SFX/Player/SFX_Footstep.wav";
    private const string CutShadowPath = "Assets/_Shadough/Audio/SFX/Shadow/SFX_CutShadow.mp3";
    private const string PasteShadowPath = "Assets/_Shadough/Audio/SFX/Shadow/SFX_PasteShadow.mp3";
    private const string RevealShadowPath = "Assets/_Shadough/Audio/SFX/Shadow/SFX_Shadow Reveal.wav";
    private const string LanternPath = "Assets/_Shadough/Audio/SFX/Interaction/SFX_Lantern.wav";
    private const string KeyPath = "Assets/_Shadough/Audio/SFX/Interaction/SFX_key.mp3";
    private const string DoorOpenPath = "Assets/_Shadough/Audio/SFX/Interaction/SFX_Door Open.mp3";
    private const string PressurePlatePath = "Assets/_Shadough/Audio/SFX/Interaction/SFX_Pressure Plate.mp3";
    private const string ButtonPath = "Assets/_Shadough/Audio/SFX/Interaction/SFX_ButtonClick.mp3";
    private const string ShadowSeekerPath = "Assets/_Shadough/Audio/SFX/Enemy/SFX_Shadow Seeker.mp3";

    private static StringBuilder playProbeReport;
    private static int playProbeStage;
    private static double nextProbeTime;

    [InitializeOnLoadMethod]
    private static void RegisterAutoSetup()
    {
        EditorApplication.update -= TryAutoSetup;
        EditorApplication.update += TryAutoSetup;
    }

    [MenuItem("Shadough/Audio/Setup Topdown Scene Audio")]
    public static void SetupFromMenu()
    {
        Setup();
    }

    [MenuItem("Shadough/Audio/Run Topdown Audio Play Probe")]
    public static void RequestPlayProbeFromMenu()
    {
        File.WriteAllText(FullPath(RequestPath), "audio-play-probe");
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

        string request = File.ReadAllText(requestPath).Trim().ToLowerInvariant();
        if (request == "setup")
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            File.Delete(requestPath);
            Setup();
            return;
        }

        if (request == "audio-play-probe")
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                EditorApplication.isPlaying = true;
                return;
            }

            if (EditorApplication.isPlaying && playProbeReport == null)
            {
                StartPlayProbe();
            }
        }
    }

    private static void Setup()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        StringBuilder report = new StringBuilder();
        report.AppendLine("Topdown Audio Scene Setup");
        report.AppendLine("Scene: " + ScenePath);

        ConfigureAudioListener(report);
        ConfigureAudioManager(report);
        CountMissingSceneData(out int missingScripts, out int missingReferences);
        report.AppendLine("missingScripts=" + missingScripts);
        report.AppendLine("missingReferences=" + missingReferences);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        File.WriteAllText(FullPath(ReportPath), report.ToString());
        Debug.Log("Topdown audio scene setup complete. Report: " + FullPath(ReportPath));
    }

    private static void ConfigureAudioListener(StringBuilder report)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = FindSceneObject("Main Camera");
            mainCamera = cameraObject != null ? cameraObject.GetComponent<Camera>() : null;
        }

        bool mainCameraExists = mainCamera != null;
        report.AppendLine("mainCamera.exists=" + PassFail(mainCameraExists));
        if (!mainCameraExists)
        {
            return;
        }

        AudioListener[] mainListeners = mainCamera.GetComponents<AudioListener>();
        if (mainListeners.Length == 0)
        {
            mainCamera.gameObject.AddComponent<AudioListener>();
            report.AppendLine("mainCamera.audioListener=added");
        }
        else
        {
            report.AppendLine("mainCamera.audioListener=already_present");
        }

        mainListeners = mainCamera.GetComponents<AudioListener>();
        for (int i = 1; i < mainListeners.Length; i++)
        {
            Object.DestroyImmediate(mainListeners[i]);
        }

        int removedExtraListeners = 0;
        AudioListener[] allListeners = Resources.FindObjectsOfTypeAll<AudioListener>();
        for (int i = 0; i < allListeners.Length; i++)
        {
            AudioListener listener = allListeners[i];
            if (listener == null
                || !listener.gameObject.scene.IsValid()
                || listener.gameObject.scene.path != ScenePath
                || listener.gameObject == mainCamera.gameObject)
            {
                continue;
            }

            Object.DestroyImmediate(listener);
            removedExtraListeners++;
        }

        report.AppendLine("audioListener.extraRemoved=" + removedExtraListeners);
        report.AppendLine("audioListener.sceneCount=" + CountSceneAudioListeners());
    }

    private static void ConfigureAudioManager(StringBuilder report)
    {
        AudioManager audioManager = FindComponent<AudioManager>("AudioManager");
        report.AppendLine("audioManager.exists=" + PassFail(audioManager != null));
        if (audioManager == null)
        {
            return;
        }

        AudioSource[] sources = audioManager.GetComponents<AudioSource>();
        AudioSource bgmSource = audioManager.bgmSource != null ? audioManager.bgmSource : sources.Length > 0 ? sources[0] : audioManager.gameObject.AddComponent<AudioSource>();
        sources = audioManager.GetComponents<AudioSource>();
        AudioSource sfxSource = audioManager.sfxSource != null ? audioManager.sfxSource : sources.Length > 1 ? sources[1] : audioManager.gameObject.AddComponent<AudioSource>();

        ConfigureBgmSource(bgmSource);
        ConfigureSfxSource(sfxSource);

        SerializedObject serializedManager = new SerializedObject(audioManager);
        SetObject(serializedManager, "bgmSource", bgmSource);
        SetObject(serializedManager, "sfxSource", sfxSource);
        SetObject(serializedManager, "bgmMainGameplay", LoadClip(BgmMainPath));
        SetObject(serializedManager, "bgmVictory", LoadClip(BgmVictoryPath));
        SetObject(serializedManager, "footstep", LoadClip(FootstepPath));
        SetObject(serializedManager, "cutShadow", LoadClip(CutShadowPath));
        SetObject(serializedManager, "pasteShadow", LoadClip(PasteShadowPath));
        SetObject(serializedManager, "revealShadow", LoadClip(RevealShadowPath));
        SetObject(serializedManager, "lantern", LoadClip(LanternPath));
        SetObject(serializedManager, "key", LoadClip(KeyPath));
        SetObject(serializedManager, "doorOpen", LoadClip(DoorOpenPath));
        SetObject(serializedManager, "pressurePlate", LoadClip(PressurePlatePath));
        SetObject(serializedManager, "button", LoadClip(ButtonPath));
        SetObject(serializedManager, "shadowSeeker", LoadClip(ShadowSeekerPath));
        serializedManager.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(audioManager);

        report.AppendLine("audioManager.bgmSource=" + PassFail(audioManager.bgmSource != null));
        report.AppendLine("audioManager.sfxSource=" + PassFail(audioManager.sfxSource != null));
        report.AppendLine("audioClip.bgmMainGameplay=" + ClipStatus(audioManager.bgmMainGameplay, "bgm_Main Gameplay"));
        report.AppendLine("audioClip.footstep=" + ClipStatus(audioManager.footstep, "SFX_Footstep"));
        report.AppendLine("audioClip.lantern=" + ClipStatus(audioManager.lantern, "SFX_Lantern"));
        report.AppendLine("audioClip.revealShadow=" + ClipStatus(audioManager.revealShadow, "SFX_Shadow Reveal"));
        report.AppendLine("audioClip.cutShadow=" + ClipStatus(audioManager.cutShadow, "SFX_CutShadow"));
        report.AppendLine("audioClip.pasteShadow=" + ClipStatus(audioManager.pasteShadow, "SFX_PasteShadow"));
        report.AppendLine("audioClip.bgmVictory=" + ClipStatus(audioManager.bgmVictory, "bgm_Victory"));
        report.AppendLine("audioClip.key=" + ClipStatus(audioManager.key, "SFX_key"));
        report.AppendLine("audioClip.doorOpen=" + ClipStatus(audioManager.doorOpen, "SFX_Door Open"));
        report.AppendLine("audioClip.pressurePlate=" + ClipStatus(audioManager.pressurePlate, "SFX_Pressure Plate"));
        report.AppendLine("audioClip.button=" + ClipStatus(audioManager.button, "SFX_ButtonClick"));
        report.AppendLine("audioClip.shadowSeeker=" + ClipStatus(audioManager.shadowSeeker, "SFX_Shadow Seeker"));
        report.AppendLine("editorMuteAudio=" + GetEditorMuteStatus());
    }

    private static void ConfigureBgmSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.volume = Mathf.Max(source.volume, 0.75f);
    }

    private static void ConfigureSfxSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.volume = Mathf.Max(source.volume, 0.9f);
    }

    private static AudioClip LoadClip(string assetPath)
    {
        return AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
    }

    private static string ClipStatus(AudioClip clip, string expectedName)
    {
        return clip != null && clip.name == expectedName
            ? "PASS " + clip.name
            : "FAIL " + (clip == null ? "null" : clip.name);
    }

    private static void StartPlayProbe()
    {
        playProbeReport = new StringBuilder();
        playProbeReport.AppendLine("Topdown Audio Play Probe");
        playProbeReport.AppendLine("Scene: " + ScenePath);
        playProbeStage = 0;
        nextProbeTime = EditorApplication.timeSinceStartup + 1.0d;

        EditorApplication.update -= RunPlayProbeStep;
        EditorApplication.update += RunPlayProbeStep;
    }

    private static void RunPlayProbeStep()
    {
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.update -= RunPlayProbeStep;
            playProbeReport = null;
            return;
        }

        if (EditorApplication.timeSinceStartup < nextProbeTime)
        {
            return;
        }

        AudioManager audioManager = AudioManager.Instance != null ? AudioManager.Instance : Object.FindObjectOfType<AudioManager>();
        switch (playProbeStage)
        {
            case 0:
                playProbeReport.AppendLine("audioListener.sceneCount=" + CountSceneAudioListeners());
                playProbeReport.AppendLine("audioManager.instance=" + PassFail(audioManager != null));
                playProbeReport.AppendLine("bgm_Main Gameplay.isPlaying=" + PassFail(audioManager != null
                    && audioManager.bgmSource != null
                    && audioManager.bgmSource.isPlaying
                    && audioManager.bgmSource.clip == audioManager.bgmMainGameplay));
                ProbeSfx(audioManager, "footstep", audioManager != null ? audioManager.footstep : null);
                AdvancePlayProbe(1, 0.2d);
                break;
            case 1:
                ProbeSfx(audioManager, "lantern", audioManager != null ? audioManager.lantern : null);
                AdvancePlayProbe(2, 0.2d);
                break;
            case 2:
                ProbeSfx(audioManager, "revealShadow", audioManager != null ? audioManager.revealShadow : null);
                AdvancePlayProbe(3, 0.2d);
                break;
            case 3:
                ProbeSfx(audioManager, "cutShadow", audioManager != null ? audioManager.cutShadow : null);
                AdvancePlayProbe(4, 0.2d);
                break;
            case 4:
                ProbeSfx(audioManager, "pasteShadow", audioManager != null ? audioManager.pasteShadow : null);
                playProbeReport.AppendLine("editorMuteAudio=" + GetEditorMuteStatus());
                FinishPlayProbe();
                break;
        }
    }

    private static void ProbeSfx(AudioManager audioManager, string label, AudioClip clip)
    {
        bool clipReady = audioManager != null && audioManager.sfxSource != null && clip != null;
        if (clipReady)
        {
            audioManager.sfxSource.Stop();
            audioManager.PlaySFX(clip);
        }

        bool sourcePlaying = clipReady && audioManager.sfxSource.isPlaying;
        playProbeReport.AppendLine("sfx." + label + "=" + PassFail(clipReady && sourcePlaying) + " clip=" + (clip != null ? clip.name : "null"));
    }

    private static void AdvancePlayProbe(int nextStage, double delay)
    {
        playProbeStage = nextStage;
        nextProbeTime = EditorApplication.timeSinceStartup + delay;
    }

    private static void FinishPlayProbe()
    {
        EditorApplication.update -= RunPlayProbeStep;
        string requestPath = FullPath(RequestPath);
        if (File.Exists(requestPath))
        {
            File.Delete(requestPath);
        }

        File.WriteAllText(FullPath(PlayProbeReportPath), playProbeReport.ToString());
        Debug.Log("Topdown audio play probe complete. Report: " + FullPath(PlayProbeReportPath));
        playProbeReport = null;
        EditorApplication.isPlaying = false;
    }

    private static string GetEditorMuteStatus()
    {
        System.Type audioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
        if (audioUtilType == null)
        {
            return "unknown";
        }

        System.Reflection.MethodInfo getMute = audioUtilType.GetMethod("GetMasterMute", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        if (getMute == null)
        {
            return "unknown";
        }

        object result = getMute.Invoke(null, null);
        bool muted = result is bool && (bool)result;
        if (muted)
        {
            System.Reflection.MethodInfo setMute = audioUtilType.GetMethod("SetMasterMute", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (setMute != null)
            {
                setMute.Invoke(null, new object[] { false });
                return "was_on_set_off";
            }
        }

        return muted ? "ON" : "OFF";
    }

    private static int CountSceneAudioListeners()
    {
        int count = 0;
        AudioListener[] listeners = Resources.FindObjectsOfTypeAll<AudioListener>();
        for (int i = 0; i < listeners.Length; i++)
        {
            AudioListener listener = listeners[i];
            if (listener != null && listener.gameObject.scene.IsValid() && listener.gameObject.scene.path == ScenePath)
            {
                count++;
            }
        }

        return count;
    }

    private static GameObject FindSceneObject(string objectName)
    {
        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject sceneObject = objects[i];
            if (sceneObject == null || sceneObject.name != objectName)
            {
                continue;
            }

            if (sceneObject.scene.IsValid() && sceneObject.scene.path == ScenePath)
            {
                return sceneObject;
            }
        }

        return null;
    }

    private static T FindComponent<T>(string objectName) where T : Component
    {
        GameObject gameObject = FindSceneObject(objectName);
        return gameObject != null ? gameObject.GetComponent<T>() : null;
    }

    private static void CountMissingSceneData(out int missingScripts, out int missingReferences)
    {
        missingScripts = 0;
        missingReferences = 0;

        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject sceneObject = objects[i];
            if (!sceneObject.scene.IsValid() || sceneObject.scene.path != ScenePath)
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

    private static string PassFail(bool passed)
    {
        return passed ? "PASS" : "FAIL";
    }

    private static void SetObject(SerializedObject serializedObject, string propertyName, Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }
}
