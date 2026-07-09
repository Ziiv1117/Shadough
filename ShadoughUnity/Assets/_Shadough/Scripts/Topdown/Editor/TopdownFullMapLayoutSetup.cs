using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class TopdownFullMapLayoutSetup
{
    private const string ScenePath = "Assets/_Shadough/Scenes/ClockTower_TopdownPrototype.unity";
    private const float CanvasWidth = 1448f;
    private const float CanvasHeight = 1086f;
    private const float PixelsPerUnit = 40f;
    private const string LayoutRootName = "Level01_Blockout_FromMap";
    private const string AutoSetupRequestPath = "Temp/TopdownFullMapLayoutSetup.request";

    private static readonly Color GrassColor = new Color(0.28f, 0.46f, 0.24f, 1f);
    private static readonly Color FloorColor = new Color(0.45f, 0.48f, 0.47f, 1f);
    private static readonly Color ConnectorColor = new Color(0.56f, 0.56f, 0.50f, 1f);
    private static readonly Color WaterColor = new Color(0.08f, 0.29f, 0.45f, 1f);
    private static readonly Color TriggerColor = new Color(0.68f, 0.30f, 0.86f, 0.42f);
    private static readonly Color WallColor = new Color(0.18f, 0.17f, 0.15f, 1f);
    private static readonly Color DoorColor = new Color(0.56f, 0.42f, 0.24f, 1f);

    [InitializeOnLoadMethod]
    private static void RegisterAutoSetup()
    {
        EditorApplication.update -= TryAutoSetup;
        EditorApplication.update += TryAutoSetup;
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
            Debug.Log("Topdown full map layout auto setup complete.");
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

    [MenuItem("Shadough/Setup Topdown Full Map Layout")]
    public static void SetupFromMenu()
    {
        Setup();
    }

    public static void Setup()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        EnsureCoreRoots();
        Transform layoutRoot = RecreateLayoutRoot();
        CreateMapBlockout(layoutRoot);
        RepositionGameplayObjects(layoutRoot);
        ConfigureCamera();
        ValidateScene();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        Debug.Log("Topdown full map layout setup complete.");
    }

    private static void EnsureCoreRoots()
    {
        EnsureRoot("World");
        EnsureRoot("Lights");
        EnsureRoot("ShadowVisuals");
        EnsureRoot("ShadowLogic");
        EnsureRoot("Interactables");
        EnsureRoot("Enemies");
        EnsureRoot("UI");
    }

    private static GameObject EnsureRoot(string name)
    {
        GameObject root = GameObject.Find(name);
        if (root == null)
        {
            root = new GameObject(name);
        }

        root.transform.position = Vector3.zero;
        root.transform.rotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;
        return root;
    }

    private static Transform RecreateLayoutRoot()
    {
        GameObject oldRoot = GameObject.Find(LayoutRootName);
        if (oldRoot != null)
        {
            Object.DestroyImmediate(oldRoot);
        }

        GameObject world = EnsureRoot("World");
        GameObject root = new GameObject(LayoutRootName);
        root.transform.SetParent(world.transform, false);
        return root.transform;
    }

    private static void CreateMapBlockout(Transform root)
    {
        Transform floors = CreateChild(root, "Walkable_Areas");
        Transform triggers = CreateChild(root, "Trigger_And_Anchor_Visuals");
        Transform blockers = CreateChild(root, "Blockers_And_Walls");

        CreatePolygon(floors, "Start_Island_Walkable", GrassColor, new Vector2[]
        {
            P(58, 848), P(155, 782), P(291, 824), P(305, 945), P(220, 992), P(93, 988), P(45, 916)
        }, 0);
        CreatePolygon(floors, "TreeShadow_Bridge_Guide", new Color(0.50f, 0.32f, 0.82f, 0.55f), new Vector2[]
        {
            P(153, 800), P(201, 772), P(343, 843), P(301, 891), P(188, 851)
        }, 1);
        CreatePolygon(floors, "East_Bank_Before_First_Gate", GrassColor, new Vector2[]
        {
            P(232, 702), P(388, 662), P(503, 710), P(493, 782), P(358, 820), P(229, 760)
        }, 0);
        CreatePolygon(floors, "Room_01_CanPress_Floor", FloorColor, new Vector2[]
        {
            P(322, 650), P(528, 555), P(700, 542), P(792, 636), P(704, 748), P(478, 778)
        }, 0);
        CreatePolygon(floors, "Connector_Room01_To_Room02", ConnectorColor, new Vector2[]
        {
            P(592, 522), P(662, 492), P(711, 552), P(637, 590), P(577, 558)
        }, 0);
        CreatePolygon(floors, "Room_02_CanUnlock_Floor", FloorColor, new Vector2[]
        {
            P(533, 417), P(651, 357), P(784, 364), P(824, 466), P(743, 548), P(584, 533), P(500, 466)
        }, 0);
        CreatePolygon(floors, "Connector_Room02_To_Seeker", ConnectorColor, new Vector2[]
        {
            P(759, 389), P(835, 341), P(878, 388), P(805, 440)
        }, 0);
        CreatePolygon(floors, "Seeker_Lure_Corridor_Floor", FloorColor, new Vector2[]
        {
            P(779, 382), P(838, 344), P(895, 285), P(1006, 286), P(1052, 255), P(1182, 260),
            P(1200, 352), P(1108, 388), P(1037, 379), P(991, 423), P(881, 410), P(810, 457), P(735, 421)
        }, 0);
        CreatePolygon(floors, "Connector_Seeker_To_Final", ConnectorColor, new Vector2[]
        {
            P(1082, 262), P(1160, 218), P(1210, 267), P(1127, 326)
        }, 0);
        CreatePolygon(floors, "FinalClockCore_Chamber_Floor", FloorColor, new Vector2[]
        {
            P(1139, 126), P(1204, 86), P(1327, 88), P(1392, 139), P(1378, 229), P(1290, 282), P(1170, 260), P(1109, 204)
        }, 0);

        CreatePolygon(floors, "River_Water_Blockout", WaterColor, new Vector2[]
        {
            P(100, 758), P(205, 724), P(372, 759), P(420, 830), P(344, 907), P(181, 894), P(78, 824)
        }, -1);

        CreatePolygon(triggers, "Trigger_TreeShadowBridge", TriggerColor, new Vector2[]
        {
            P(148, 795), P(195, 770), P(346, 842), P(305, 894), P(186, 854)
        }, 2);
        CreateBoxVisual(triggers, "Trigger_OuterGate", RectCenter(360, 650, 470, 728), RectSize(360, 650, 470, 728), TriggerColor, false, true, 2);
        CreateBoxVisual(triggers, "Trigger_CanPress", RectCenter(610, 520, 702, 585), RectSize(610, 520, 702, 585), TriggerColor, false, true, 2);
        CreateBoxVisual(triggers, "Trigger_CanUnlock", RectCenter(660, 405, 733, 470), RectSize(660, 405, 733, 470), TriggerColor, false, true, 2);
        CreatePolygon(triggers, "Trigger_PlayerShadowLure", TriggerColor, new Vector2[]
        {
            P(895, 286), P(1050, 260), P(1175, 285), P(1130, 374), P(1005, 382), P(872, 407)
        }, 2);
        CreateBoxVisual(triggers, "Trigger_FinalClockCore", RectCenter(1234, 128, 1302, 188), RectSize(1234, 128, 1302, 188), TriggerColor, false, true, 2);

        CreateRouteWalls(blockers);
        CreateBridgeBlocker(blockers);
    }

    private static void CreateRouteWalls(Transform root)
    {
        CreateBoxVisual(root, "Wall_Room01_West", W(325, 675), new Vector2(0.35f, 3.2f), WallColor, true, false, 5);
        CreateBoxVisual(root, "Wall_Room01_South", W(530, 770), new Vector2(5.8f, 0.35f), WallColor, true, false, 5);
        CreateBoxVisual(root, "Wall_Room01_East", W(780, 650), new Vector2(0.35f, 3.2f), WallColor, true, false, 5);

        CreateBoxVisual(root, "Wall_Room02_West", W(520, 455), new Vector2(0.35f, 3.0f), WallColor, true, false, 5);
        CreateBoxVisual(root, "Wall_Room02_North", W(680, 360), new Vector2(4.6f, 0.35f), WallColor, true, false, 5);
        CreateBoxVisual(root, "Wall_Room02_East", W(820, 455), new Vector2(0.35f, 2.7f), WallColor, true, false, 5);

        CreateBoxVisual(root, "Wall_SeekerCorridor_North", W(1015, 270), new Vector2(8.1f, 0.35f), WallColor, true, false, 5);
        CreateBoxVisual(root, "Wall_SeekerCorridor_South", W(995, 405), new Vector2(7.5f, 0.35f), WallColor, true, false, 5);

        CreateBoxVisual(root, "Wall_Final_North", W(1265, 92), new Vector2(5.0f, 0.35f), WallColor, true, false, 5);
        CreateBoxVisual(root, "Wall_Final_East", W(1390, 170), new Vector2(0.35f, 3.3f), WallColor, true, false, 5);
        CreateBoxVisual(root, "Wall_Final_South", W(1265, 275), new Vector2(5.5f, 0.35f), WallColor, true, false, 5);
    }

    private static void CreateBridgeBlocker(Transform root)
    {
        GameObject blocker = CreateBoxVisual(root, "River_CrossingBlocker_FromMap", W(246, 835), new Vector2(4.7f, 2.0f), new Color(0.05f, 0.16f, 0.24f, 0.72f), true, false, 6);
        GameObject crossingZone = GameObject.Find("CrossingHint_01");
        TopdownBridgeCrossingZone crossing = crossingZone != null ? crossingZone.GetComponent<TopdownBridgeCrossingZone>() : null;
        if (crossing != null)
        {
            SerializedObject serializedCrossing = new SerializedObject(crossing);
            SetObject(serializedCrossing, "crossingBlocker", blocker);
            SetFloat(serializedCrossing, "detectionRadius", 1.45f);
            serializedCrossing.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void RepositionGameplayObjects(Transform layoutRoot)
    {
        Transform world = EnsureRoot("World").transform;
        Transform interactables = EnsureRoot("Interactables").transform;
        Transform enemies = EnsureRoot("Enemies").transform;
        Transform shadows = EnsureRoot("ShadowLogic").transform;

        MoveObject("Player_Topdown", W(162, 884), world, Vector3.one);
        MoveObject("Tree_01", W(127, 789), world, new Vector3(1.25f, 1.25f, 1f));
        MoveObject("Tree_Trunk", W(127, 789), world, new Vector3(0.55f, 1.1f, 1f));
        MoveObject("Tree_Canopy", W(127, 773), world, new Vector3(1.5f, 1.15f, 1f));
        MoveObject("TreeShadow_Topdown", W(210, 820), shadows, Vector3.one);
        MoveObject("CrossingHint_01", W(246, 835), interactables, Vector3.one);

        MoveObject("Door_Pressure_Topdown", W(665, 548), interactables, new Vector3(1.1f, 0.25f, 1f));
        MoveObject("PressurePlate_Topdown", W(548, 653), interactables, new Vector3(1.05f, 0.72f, 1f));
        MoveObject("BeamSource_Topdown", W(455, 620), interactables, new Vector3(0.75f, 0.75f, 1f));
        MoveObject("BeamShadow_Topdown", W(508, 640), shadows, new Vector3(1.45f, 0.65f, 1f));

        MoveObject("Door_Lock_Topdown", W(825, 390), interactables, new Vector3(0.28f, 1.25f, 1f));
        MoveObject("Lock_Topdown", W(696, 436), interactables, new Vector3(0.85f, 0.85f, 1f));
        MoveObject("Lock_Topdown_Trigger", W(696, 436), interactables, new Vector3(1f, 1f, 1f));
        MoveObject("KeySource_Topdown", W(605, 455), interactables, new Vector3(0.75f, 0.75f, 1f));
        MoveObject("KeyShadow_Topdown", W(640, 472), shadows, new Vector3(0.9f, 0.55f, 1f));

        MoveObject("ShadowSeeker_Topdown", W(988, 338), enemies, new Vector3(1.6f, 1.6f, 1f));
        MoveObject("ShadowSeeker_Home_Topdown", W(988, 338), enemies, Vector3.one);
        MoveObject("LureArea_Topdown", W(1125, 318), interactables, new Vector3(1.65f, 0.9f, 1f));
        MoveObject("ShadowSeekerSafePoint_Topdown", W(830, 430), world, Vector3.one);

        MoveObject("FinalClockCore_Topdown", W(1268, 158), interactables, new Vector3(1.35f, 1.35f, 1f));

        ConfigureShadowInteractable("TreeShadow_Topdown", "Tree Shadow", true, false, false, false, false);
        ConfigureShadowInteractable("BeamShadow_Topdown", "CanPress Shadow", false, true, false, false, false);
        ConfigureShadowInteractable("KeyShadow_Topdown", "CanUnlock Shadow", false, false, true, false, false);
        ConfigureShadowInteractable("Player_Shadow", "PlayerShadow", false, false, false, true, false);

        ConfigurePressureReferences();
        ConfigureLockReferences();
        ConfigureSeekerReferences();
        ConfigureFinalCore();
        DisableOldVisualBlockers();
    }

    private static void ConfigurePressureReferences()
    {
        PressurePlateController plate = FindComponent<PressurePlateController>("PressurePlate_Topdown");
        DoorController door = FindComponent<DoorController>("Door_Pressure_Topdown");
        ShadowPressureTrigger trigger = FindComponent<ShadowPressureTrigger>("PressurePlate_Topdown");
        if (plate != null)
        {
            SerializedObject serializedPlate = new SerializedObject(plate);
            SetObject(serializedPlate, "targetDoor", door);
            serializedPlate.ApplyModifiedPropertiesWithoutUndo();
        }

        if (trigger != null)
        {
            SerializedObject serializedTrigger = new SerializedObject(trigger);
            SetObject(serializedTrigger, "pressurePlate", plate);
            serializedTrigger.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void ConfigureLockReferences()
    {
        LockController lockController = FindComponent<LockController>("Lock_Topdown");
        DoorController door = FindComponent<DoorController>("Door_Lock_Topdown");
        ShadowLockTrigger trigger = FindComponent<ShadowLockTrigger>("Lock_Topdown_Trigger");
        if (lockController != null)
        {
            SerializedObject serializedLock = new SerializedObject(lockController);
            SetObject(serializedLock, "targetDoor", door);
            serializedLock.ApplyModifiedPropertiesWithoutUndo();
        }

        if (trigger != null)
        {
            SerializedObject serializedTrigger = new SerializedObject(trigger);
            SetObject(serializedTrigger, "lockController", lockController);
            SetBool(serializedTrigger, "requireAngleCheck", false);
            serializedTrigger.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void ConfigureSeekerReferences()
    {
        EnemyShadowSeeker seeker = FindComponent<EnemyShadowSeeker>("ShadowSeeker_Topdown");
        GameObject home = GameObject.Find("ShadowSeeker_Home_Topdown");
        GameObject player = GameObject.Find("Player_Topdown");
        if (seeker != null)
        {
            SerializedObject serializedSeeker = new SerializedObject(seeker);
            SetObject(serializedSeeker, "homePoint", home != null ? home.transform : null);
            SetObject(serializedSeeker, "playerTarget", player != null ? player.transform : null);
            SetFloat(serializedSeeker, "detectionRadius", 4.35f);
            SetFloat(serializedSeeker, "attackDistance", 0.65f);
            SetBool(serializedSeeker, "showDebugGizmos", true);
            SetBool(serializedSeeker, "tintRendererByState", false);
            serializedSeeker.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void ConfigureFinalCore()
    {
        TopdownFinalClockCore finalCore = FindComponent<TopdownFinalClockCore>("FinalClockCore_Topdown");
        if (finalCore != null)
        {
            SerializedObject serializedCore = new SerializedObject(finalCore);
            SetString(serializedCore, "completeText", "Topdown Demo Complete");
            SetString(serializedCore, "logMessage", "Topdown demo complete");
            serializedCore.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void ConfigureShadowInteractable(string objectName, string displayName, bool canStandOn, bool canPress, bool canUnlock, bool canAttractEnemy, bool canBlock)
    {
        ShadowInteractable shadow = FindComponent<ShadowInteractable>(objectName);
        if (shadow == null)
        {
            return;
        }

        SerializedObject serializedShadow = new SerializedObject(shadow);
        SetString(serializedShadow, "displayName", displayName);
        SetBool(serializedShadow, "canStandOn", canStandOn);
        SetBool(serializedShadow, "canPress", canPress);
        SetBool(serializedShadow, "canUnlock", canUnlock);
        SetBool(serializedShadow, "canAttractEnemy", canAttractEnemy);
        SetBool(serializedShadow, "canBlock", canBlock);
        serializedShadow.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void DisableOldVisualBlockers()
    {
        string[] oldNames =
        {
            "Ground", "Ground_Base", "River_Gap_01", "RiverBlocker_Right", "EnemyPassage_Topdown",
            "CrossingBlocker_01",
            "Area_01_TreeCrossing", "Area_02_PressureDoor", "Area_03_LockDoor", "Area_04_EnemyLure",
            "Goal_Platform", "River_Bank_Left", "River_Bank_Right"
        };

        for (int i = 0; i < oldNames.Length; i++)
        {
            GameObject oldObject = GameObject.Find(oldNames[i]);
            if (oldObject != null)
            {
                oldObject.SetActive(false);
            }
        }
    }

    private static void ConfigureCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = GameObject.Find("Main Camera");
            camera = cameraObject != null ? cameraObject.GetComponent<Camera>() : null;
        }

        if (camera == null)
        {
            return;
        }

        camera.orthographic = true;
        camera.orthographicSize = 5.4f;
        GameObject player = GameObject.Find("Player_Topdown");
        camera.transform.position = player != null
            ? new Vector3(player.transform.position.x, player.transform.position.y, -10f)
            : new Vector3(0f, 0f, -10f);

        TopdownCameraFollow follow = camera.GetComponent<TopdownCameraFollow>();
        if (follow != null && player != null)
        {
            SerializedObject serializedFollow = new SerializedObject(follow);
            SetObject(serializedFollow, "target", player.transform);
            serializedFollow.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void ValidateScene()
    {
        int missingScripts = 0;
        int missingReferences = 0;
        int blockoutChildren = 0;
        GameObject layoutRoot = GameObject.Find(LayoutRootName);
        if (layoutRoot != null)
        {
            blockoutChildren = layoutRoot.GetComponentsInChildren<Transform>(true).Length - 1;
        }

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

        Debug.Log("Topdown full map layout validation: blockoutChildren=" + blockoutChildren
            + ", missingScripts=" + missingScripts
            + ", missingReferences=" + missingReferences
            + ", player=" + Exists("Player_Topdown")
            + ", treeShadow=" + Exists("TreeShadow_Topdown")
            + ", canPress=" + Exists("PressurePlate_Topdown")
            + ", canUnlock=" + Exists("Lock_Topdown")
            + ", seeker=" + Exists("ShadowSeeker_Topdown")
            + ", finalCore=" + Exists("FinalClockCore_Topdown"));
    }

    private static bool Exists(string objectName)
    {
        return GameObject.Find(objectName) != null;
    }

    private static Transform CreateChild(Transform parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent, false);
        return child.transform;
    }

    private static GameObject CreatePolygon(Transform parent, string name, Color color, Vector2[] points, int sortingOrder)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(parent, false);

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = CreateMaterial(color);
        meshRenderer.sortingOrder = sortingOrder;

        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            vertices[i] = new Vector3(points[i].x, points[i].y, 0f);
        }

        mesh.vertices = vertices;
        mesh.triangles = Triangulate(points).ToArray();
        mesh.RecalculateBounds();
        meshFilter.sharedMesh = mesh;
        return gameObject;
    }

    private static GameObject CreateBoxVisual(Transform parent, string name, Vector2 center, Vector2 size, Color color, bool collider, bool trigger, int sortingOrder)
    {
        Vector2 halfSize = size * 0.5f;
        GameObject gameObject = CreatePolygon(parent, name, color, new Vector2[]
        {
            new Vector2(center.x - halfSize.x, center.y - halfSize.y),
            new Vector2(center.x + halfSize.x, center.y - halfSize.y),
            new Vector2(center.x + halfSize.x, center.y + halfSize.y),
            new Vector2(center.x - halfSize.x, center.y + halfSize.y)
        }, sortingOrder);

        if (collider)
        {
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.offset = center;
            boxCollider.size = size;
            boxCollider.isTrigger = trigger;
        }

        return gameObject;
    }

    private static Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }

        Material material = new Material(shader);
        material.color = color;
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        return material;
    }

    private static List<int> Triangulate(Vector2[] sourcePoints)
    {
        List<int> triangles = new List<int>();
        List<int> indices = new List<int>();
        for (int i = 0; i < sourcePoints.Length; i++)
        {
            indices.Add(i);
        }

        bool ccw = SignedArea(sourcePoints) > 0f;
        int guard = 0;
        while (indices.Count > 3 && guard < 500)
        {
            guard++;
            bool clipped = false;
            for (int i = 0; i < indices.Count; i++)
            {
                int previousIndex = indices[(i - 1 + indices.Count) % indices.Count];
                int currentIndex = indices[i];
                int nextIndex = indices[(i + 1) % indices.Count];

                Vector2 previous = sourcePoints[previousIndex];
                Vector2 current = sourcePoints[currentIndex];
                Vector2 next = sourcePoints[nextIndex];

                if (!IsConvex(previous, current, next, ccw))
                {
                    continue;
                }

                bool containsPoint = false;
                for (int other = 0; other < indices.Count; other++)
                {
                    int otherIndex = indices[other];
                    if (otherIndex == previousIndex || otherIndex == currentIndex || otherIndex == nextIndex)
                    {
                        continue;
                    }

                    if (PointInTriangle(sourcePoints[otherIndex], previous, current, next))
                    {
                        containsPoint = true;
                        break;
                    }
                }

                if (containsPoint)
                {
                    continue;
                }

                if (ccw)
                {
                    triangles.Add(previousIndex);
                    triangles.Add(currentIndex);
                    triangles.Add(nextIndex);
                }
                else
                {
                    triangles.Add(nextIndex);
                    triangles.Add(currentIndex);
                    triangles.Add(previousIndex);
                }

                indices.RemoveAt(i);
                clipped = true;
                break;
            }

            if (!clipped)
            {
                break;
            }
        }

        if (indices.Count == 3)
        {
            if (ccw)
            {
                triangles.Add(indices[0]);
                triangles.Add(indices[1]);
                triangles.Add(indices[2]);
            }
            else
            {
                triangles.Add(indices[2]);
                triangles.Add(indices[1]);
                triangles.Add(indices[0]);
            }
        }

        return triangles;
    }

    private static float SignedArea(Vector2[] points)
    {
        float area = 0f;
        for (int i = 0; i < points.Length; i++)
        {
            Vector2 current = points[i];
            Vector2 next = points[(i + 1) % points.Length];
            area += current.x * next.y - next.x * current.y;
        }

        return area * 0.5f;
    }

    private static bool IsConvex(Vector2 previous, Vector2 current, Vector2 next, bool ccw)
    {
        float cross = Cross(current - previous, next - current);
        return ccw ? cross > 0f : cross < 0f;
    }

    private static bool PointInTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
    {
        float area = Mathf.Abs(Cross(b - a, c - a));
        float area1 = Mathf.Abs(Cross(a - point, b - point));
        float area2 = Mathf.Abs(Cross(b - point, c - point));
        float area3 = Mathf.Abs(Cross(c - point, a - point));
        return Mathf.Abs(area - (area1 + area2 + area3)) <= 0.0001f;
    }

    private static float Cross(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    private static Vector2 P(float x, float y)
    {
        return new Vector2((x - CanvasWidth * 0.5f) / PixelsPerUnit, (CanvasHeight * 0.5f - y) / PixelsPerUnit);
    }

    private static Vector2 W(float x, float y)
    {
        return P(x, y);
    }

    private static Vector2 RectCenter(float xMin, float yMin, float xMax, float yMax)
    {
        return P((xMin + xMax) * 0.5f, (yMin + yMax) * 0.5f);
    }

    private static Vector2 RectSize(float xMin, float yMin, float xMax, float yMax)
    {
        return new Vector2(Mathf.Abs(xMax - xMin) / PixelsPerUnit, Mathf.Abs(yMax - yMin) / PixelsPerUnit);
    }

    private static GameObject MoveObject(string objectName, Vector2 position, Transform parent, Vector3 scale)
    {
        GameObject gameObject = GameObject.Find(objectName);
        if (gameObject == null)
        {
            return null;
        }

        gameObject.transform.SetParent(parent, true);
        gameObject.transform.position = new Vector3(position.x, position.y, gameObject.transform.position.z);
        gameObject.transform.rotation = Quaternion.identity;
        gameObject.transform.localScale = scale;
        return gameObject;
    }

    private static T FindComponent<T>(string objectName) where T : Component
    {
        GameObject gameObject = GameObject.Find(objectName);
        return gameObject != null ? gameObject.GetComponent<T>() : null;
    }

    private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.floatValue = value;
        }
    }

    private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.boolValue = value;
        }
    }

    private static void SetString(SerializedObject serializedObject, string propertyName, string value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.stringValue = value;
        }
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
