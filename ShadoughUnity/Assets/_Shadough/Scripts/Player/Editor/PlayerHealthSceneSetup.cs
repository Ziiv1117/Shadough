using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class PlayerHealthSceneSetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const string AutoSetupRequestPath = "Temp/PlayerHealthSetup.request";
    private static readonly Vector3 SafePointPosition = new Vector3(14.25f, 6.35f, 0f);

    [InitializeOnLoadMethod]
    private static void RegisterAutoSetup()
    {
        EditorApplication.update -= TryAutoSetup;
        EditorApplication.update += TryAutoSetup;
    }

    [MenuItem("Shadough/Setup Player Health")]
    public static void SetupFromMenu()
    {
        Setup();
    }

    private static void TryAutoSetup()
    {
        string requestPath = GetAutoSetupRequestFullPath();
        if (!File.Exists(requestPath))
        {
            EditorApplication.update -= TryAutoSetup;
            return;
        }

        if (EditorApplication.isCompiling)
        {
            return;
        }

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.isPlaying = false;
            return;
        }

        EditorApplication.update -= TryAutoSetup;

        try
        {
            Setup();
            File.Delete(requestPath);
            Debug.Log("Player health auto setup complete.");
        }
        catch (System.Exception exception)
        {
            Debug.LogException(exception);
        }
    }

    private static string GetAutoSetupRequestFullPath()
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "..", AutoSetupRequestPath));
    }

    public static void Setup()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        PlayerHealth playerHealth = ConfigurePlayerHealth();
        ConfigureHealthUI(playerHealth);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        ValidateScene();
    }

    private static PlayerHealth ConfigurePlayerHealth()
    {
        GameObject player = GameObject.Find("Player_Topdown");
        if (player == null)
        {
            Debug.LogWarning("Player_Topdown not found. Player health was not attached.");
            return null;
        }

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = player.AddComponent<PlayerHealth>();
        }

        Transform safePoint = EnsureSafePoint();
        EnemyShadowSeeker[] seekers = Object.FindObjectsOfType<EnemyShadowSeeker>();

        SerializedObject serializedHealth = new SerializedObject(playerHealth);
        serializedHealth.FindProperty("maxHealth").intValue = 3;
        serializedHealth.FindProperty("currentHealth").intValue = 3;
        serializedHealth.FindProperty("invulnerabilityDuration").floatValue = 1.2f;
        serializedHealth.FindProperty("damageMessage").stringValue = "The seeker caught you.";
        serializedHealth.FindProperty("defeatedMessage").stringValue = "You were consumed by shadow.";
        serializedHealth.FindProperty("defeatedPromptDuration").floatValue = 2f;
        serializedHealth.FindProperty("playHurtAnimation").boolValue = true;
        serializedHealth.FindProperty("safePoint").objectReferenceValue = safePoint;
        serializedHealth.FindProperty("restoreHealthOnDefeat").boolValue = true;
        serializedHealth.FindProperty("clearAttractingPastedShadowsOnDefeat").boolValue = true;
        serializedHealth.FindProperty("clearHeldAttractingShadowOnDefeat").boolValue = true;
        serializedHealth.FindProperty("resetSeekersOnDefeat").boolValue = true;
        serializedHealth.FindProperty("animatorDriver").objectReferenceValue = player.GetComponent<HeroPlayerAnimatorDriver>();
        serializedHealth.FindProperty("playerBody").objectReferenceValue = player.GetComponent<Rigidbody2D>();
        serializedHealth.FindProperty("shadowInventory").objectReferenceValue = player.GetComponent<ShadowInventory>();

        SerializedProperty seekersProperty = serializedHealth.FindProperty("seekersToReset");
        seekersProperty.arraySize = seekers.Length;
        for (int i = 0; i < seekers.Length; i++)
        {
            seekersProperty.GetArrayElementAtIndex(i).objectReferenceValue = seekers[i];
        }

        serializedHealth.ApplyModifiedPropertiesWithoutUndo();
        return playerHealth;
    }

    private static void ConfigureHealthUI(PlayerHealth playerHealth)
    {
        GameObject uiRoot = GameObject.Find("UI");
        if (uiRoot == null)
        {
            uiRoot = new GameObject("UI");
        }

        PlayerHealthUI healthUI = uiRoot.GetComponent<PlayerHealthUI>();
        if (healthUI == null)
        {
            healthUI = uiRoot.AddComponent<PlayerHealthUI>();
        }

        SerializedObject serializedUI = new SerializedObject(healthUI);
        serializedUI.FindProperty("playerHealth").objectReferenceValue = playerHealth;
        serializedUI.FindProperty("panelOffset").vector2Value = new Vector2(14f, 14f);
        serializedUI.FindProperty("panelSize").vector2Value = new Vector2(150f, 32f);
        serializedUI.FindProperty("anchorTopRight").boolValue = true;
        serializedUI.FindProperty("hideWhileTutorialPromptOpen").boolValue = true;
        serializedUI.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Transform EnsureSafePoint()
    {
        GameObject safePoint = GameObject.Find("ShadowSeekerSafePoint_Topdown");
        if (safePoint == null)
        {
            safePoint = new GameObject("ShadowSeekerSafePoint_Topdown");
        }

        GameObject world = GameObject.Find("World");
        if (world != null)
        {
            safePoint.transform.SetParent(world.transform, true);
        }

        safePoint.transform.position = SafePointPosition;
        safePoint.transform.rotation = Quaternion.identity;
        safePoint.transform.localScale = Vector3.one;
        return safePoint.transform;
    }

    private static void ValidateScene()
    {
        int missingScripts = 0;
        int missingReferences = 0;
        int healthCount = 0;
        int healthUICount = 0;
        int safePointCount = 0;
        int seekerCount = 0;

        GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject sceneObject = objects[i];
            if (!sceneObject.scene.IsValid() || sceneObject.scene.path != ScenePath)
            {
                continue;
            }

            if (sceneObject.GetComponent<PlayerHealth>() != null)
            {
                healthCount++;
            }

            if (sceneObject.GetComponent<PlayerHealthUI>() != null)
            {
                healthUICount++;
            }

            if (sceneObject.name == "ShadowSeekerSafePoint_Topdown")
            {
                safePointCount++;
            }

            if (sceneObject.GetComponent<EnemyShadowSeeker>() != null)
            {
                seekerCount++;
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

        Debug.Log("Player health validation: missingScripts=" + missingScripts
            + ", missingReferences=" + missingReferences
            + ", healthCount=" + healthCount
            + ", healthUICount=" + healthUICount
            + ", safePointCount=" + safePointCount
            + ", seekerCount=" + seekerCount);
    }
}
