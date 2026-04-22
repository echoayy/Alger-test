using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using Game.Player;
using Game.System;

namespace Game.Test
{
    public static class TestSceneSetup
    {
        const string ScenePath      = "Assets/Game/Test/Scenes/TestScene.unity";
        const string PanelPrefab    = "Assets/Game/Test/Resources/UI/TestStartPanel.prefab";
        const string PausePrefab    = "Assets/Game/Test/Resources/UI/TestPausePanel.prefab";
        const string BulletPrefab   = "Assets/Game/Test/Resources/Game/Prefabs/Bullet.prefab";
        const string NpcPrefab      = "Assets/Game/Test/Resources/Game/Prefabs/NPC.prefab";
        const string ConfigPath     = "Assets/Game/Test/Config/TestGameConfig.asset";
        const string MatGround      = "Assets/Game/Test/Materials/Ground.mat";
        const string MatNpc         = "Assets/Game/Test/Materials/NPC_Red.mat";

        [MenuItem("壳木AI/创建 TestScene")]
        static void CreateTestScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EnsureDirectory("Assets/Game/Test/Scenes");
            EnsureDirectory("Assets/Game/Test/Resources/UI");
            EnsureDirectory("Assets/Game/Test/Resources/Game/Prefabs");
            EnsureDirectory("Assets/Game/Test/Config");
            EnsureDirectory("Assets/Game/Test/Materials");

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var config = GetOrCreateConfig();

            BuildLighting();

            var envGroup      = new GameObject("--Environment--");
            var gameplayGroup = new GameObject("--Gameplay--");
            var managersGroup = new GameObject("--Managers--");

            BuildGround(envGroup.transform);

            var player     = BuildPlayer(gameplayGroup.transform);
            var muzzle     = BuildGun(player);
            var spawnPoint = BuildSpawnPoint(gameplayGroup.transform);

            BuildGameManager(managersGroup.transform);
            BuildBootstrap(managersGroup.transform, player, spawnPoint);
            BuildNpcSpawner(managersGroup.transform, config);
            BuildPauseController(managersGroup.transform);

            var cameraGo = BuildCamera(player.transform);

            var wc = player.GetComponent<WeaponController>();
            SetField(wc, "_camera", cameraGo.GetComponent<Camera>());
            SetField(wc, "_muzzle", muzzle);
            SetField(wc, "_config", config);

            BuildEventSystem();

            CreateStartPanelPrefab();
            CreatePausePanelPrefab();
            CreateBulletPrefab();
            CreateNpcPrefab();

            AssetDatabase.SaveAssets();
            var saved = EditorSceneManager.SaveScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene(), ScenePath);
            AssetDatabase.Refresh();

            if (saved) Debug.Log("[壳木AI] TestScene 创建完成 → " + ScenePath);
            else       Debug.LogError("[壳木AI] 场景保存失败，请检查路径权限");
        }

        // ── Config ────────────────────────────────────────────────────────────

        static TestGameConfig GetOrCreateConfig()
        {
            var existing = AssetDatabase.LoadAssetAtPath<TestGameConfig>(ConfigPath);
            if (existing != null) return existing;
            var cfg = ScriptableObject.CreateInstance<TestGameConfig>();
            AssetDatabase.CreateAsset(cfg, ConfigPath);
            return cfg;
        }

        // ── Scene objects ─────────────────────────────────────────────────────

        static void BuildLighting()
        {
            var go    = new GameObject("Directional Light");
            var light = go.AddComponent<Light>();
            light.type      = LightType.Directional;
            light.intensity = 1f;
            go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        static void BuildGround(Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
            go.name = "Ground";
            go.transform.SetParent(parent, false);
            go.transform.localScale = new Vector3(10f, 1f, 10f);

            var mat = GetOrCreateColorMat(MatGround, new Color(0.35f, 0.35f, 0.35f));
            if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }

        static GameObject BuildPlayer(Transform parent)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "Player";
            go.transform.SetParent(parent, false);
            go.SetActive(false);
            Object.DestroyImmediate(go.GetComponent<CapsuleCollider>());
            go.AddComponent<CharacterController>();
            go.AddComponent<PlayerController>();
            return go;
        }

        static Transform BuildGun(GameObject player)
        {
            var mount = new GameObject("GunMount");
            mount.transform.SetParent(player.transform, false);
            mount.transform.localPosition = new Vector3(0.3f, 0.9f, 0.1f);

            BuildBox("Stock",    mount.transform, new Vector3(0.04f, 0.04f, 0.20f), new Vector3(0f,    0f,    -0.10f));
            BuildBox("Body",     mount.transform, new Vector3(0.05f, 0.07f, 0.30f), new Vector3(0f,    0f,     0.15f));
            BuildBox("Barrel",   mount.transform, new Vector3(0.03f, 0.03f, 0.40f), new Vector3(0f,    0.01f,  0.45f));
            BuildBox("Magazine", mount.transform, new Vector3(0.04f, 0.08f, 0.04f), new Vector3(0f,   -0.05f,  0.10f));

            var muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(mount.transform, false);
            muzzle.transform.localPosition = new Vector3(0f, 0.01f, 0.65f);

            var wc = player.AddComponent<WeaponController>();
            SetField(wc, "_gunMount", mount.transform);

            return muzzle.transform;
        }

        static Transform BuildSpawnPoint(Transform parent)
        {
            var go = new GameObject("SpawnPoint");
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(0f, 5f, 0f);
            return go.transform;
        }

        static void BuildGameManager(Transform parent)
        {
            var go = new GameObject("GameManager");
            go.transform.SetParent(parent, false);
            go.AddComponent<GameManager>();
        }

        static void BuildBootstrap(Transform parent, GameObject player, Transform spawnPoint)
        {
            var go        = new GameObject("TestSceneBootstrap");
            go.transform.SetParent(parent, false);
            var bootstrap = go.AddComponent<TestSceneBootstrap>();
            SetField(bootstrap, "_playerGo",   player);
            SetField(bootstrap, "_spawnPoint", spawnPoint);
        }

        static void BuildNpcSpawner(Transform parent, TestGameConfig config)
        {
            var go      = new GameObject("NpcSpawner");
            go.transform.SetParent(parent, false);
            var spawner = go.AddComponent<NpcSpawner>();
            SetField(spawner, "_config", config);
        }

        static void BuildPauseController(Transform parent)
        {
            var go = new GameObject("PauseController");
            go.transform.SetParent(parent, false);
            go.AddComponent<PauseController>();
        }

        static GameObject BuildCamera(Transform playerTransform)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.AddComponent<Camera>();
            go.transform.position = new Vector3(0f, 14f, -3f);

            var tpc = go.AddComponent<ThirdPersonCamera>();
            SetField(tpc, "_target", playerTransform);

            go.AddComponent<CrosshairUI>();
            return go;
        }

        static void BuildEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        // ── Prefabs ───────────────────────────────────────────────────────────

        static void CreateStartPanelPrefab()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                    ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            var root     = new GameObject("TestStartPanel");
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            root.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.1f, 0.9f);

            var panel   = root.AddComponent<TestStartPanel>();
            var panelSO = new SerializedObject(panel);
            SetBool(panelSO, "panelAttr.showBlur",  false);
            SetBool(panelSO, "panelAttr.autoScale", false);
            panelSO.ApplyModifiedProperties();

            AddText(root.transform, "Title",
                anchor: new Vector2(0.5f, 0.62f), size: new Vector2(500f, 80f),
                text: "第三人称 · 俯视角射击", fontSize: 40, font: font);

            var btn = AddButton(root.transform, "StartButton",
                anchor: new Vector2(0.5f, 0.42f), size: new Vector2(220f, 64f),
                label: "开始游戏", fontSize: 24, font: font);
            SetField(panel, "_startButton", btn);

            PrefabUtility.SaveAsPrefabAsset(root, PanelPrefab);
            Object.DestroyImmediate(root);
        }

        static void CreatePausePanelPrefab()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                    ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

            var root     = new GameObject("TestPausePanel");
            var rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            var panel   = root.AddComponent<TestPausePanel>();
            var panelSO = new SerializedObject(panel);
            SetBool(panelSO, "panelAttr.showBlur",  true);
            SetBool(panelSO, "panelAttr.autoScale", false);
            panelSO.ApplyModifiedProperties();

            AddText(root.transform, "Title",
                anchor: new Vector2(0.5f, 0.62f), size: new Vector2(300f, 70f),
                text: "暂停", fontSize: 48, font: font);

            var btn = AddButton(root.transform, "ResumeButton",
                anchor: new Vector2(0.5f, 0.44f), size: new Vector2(200f, 60f),
                label: "继续游戏", fontSize: 24, font: font);
            SetField(panel, "_resumeButton", btn);

            PrefabUtility.SaveAsPrefabAsset(root, PausePrefab);
            Object.DestroyImmediate(root);
        }

        static void CreateBulletPrefab()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Bullet";
            go.transform.localScale = Vector3.one * 0.08f;
            Object.DestroyImmediate(go.GetComponent<SphereCollider>());
            var col      = go.AddComponent<SphereCollider>();
            col.radius   = 0.5f;
            col.isTrigger = true;
            var rb         = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity  = false;
            go.AddComponent<BulletBehaviour>();
            PrefabUtility.SaveAsPrefabAsset(go, BulletPrefab);
            Object.DestroyImmediate(go);
        }

        static void CreateNpcPrefab()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = "NPC";
            // 红色材质
            var mat = GetOrCreateColorMat(MatNpc, Color.red);
            if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            // Kinematic Rigidbody → 子弹可以触发 OnTriggerEnter
            var rb         = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity  = false;
            // 保留默认 CapsuleCollider（非 Trigger），供子弹碰撞检测
            go.AddComponent<NpcController>();
            PrefabUtility.SaveAsPrefabAsset(go, NpcPrefab);
            Object.DestroyImmediate(go);
        }

        // ── UI helpers ────────────────────────────────────────────────────────

        static void AddText(Transform parent, string name,
            Vector2 anchor, Vector2 size, string text, int fontSize, Font font)
        {
            var go   = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin        = anchor;
            rect.anchorMax        = anchor;
            rect.sizeDelta        = size;
            rect.anchoredPosition = Vector2.zero;
            var t       = go.AddComponent<Text>();
            t.text      = text;
            t.fontSize  = fontSize;
            t.color     = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            if (font != null) t.font = font;
        }

        static Button AddButton(Transform parent, string name,
            Vector2 anchor, Vector2 size, string label, int fontSize, Font font)
        {
            var go   = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin        = anchor;
            rect.anchorMax        = anchor;
            rect.sizeDelta        = size;
            rect.anchoredPosition = Vector2.zero;
            var img          = go.AddComponent<Image>();
            img.color        = new Color(0.18f, 0.55f, 0.95f, 1f);
            var btn          = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var tRect = textGo.AddComponent<RectTransform>();
            tRect.anchorMin = Vector2.zero;
            tRect.anchorMax = Vector2.one;
            tRect.offsetMin = Vector2.zero;
            tRect.offsetMax = Vector2.zero;
            var t       = textGo.AddComponent<Text>();
            t.text      = label;
            t.fontSize  = fontSize;
            t.color     = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            if (font != null) t.font = font;

            return btn;
        }

        // ── Mesh / Material helpers ───────────────────────────────────────────

        static GameObject BuildBox(string name, Transform parent, Vector3 scale, Vector3 localPos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale    = scale;
            Object.DestroyImmediate(go.GetComponent<BoxCollider>());
            return go;
        }

        static Material GetOrCreateColorMat(string assetPath, Color color)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (existing != null) return existing;

            var shader = Shader.Find("Universal Render Pipeline/Lit")
                      ?? Shader.Find("Standard");
            if (shader == null) return null;

            var mat = new Material(shader);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color"))     mat.SetColor("_Color",     color);

            EnsureDirectory(Path.GetDirectoryName(assetPath)?.Replace('\\', '/') ?? "");
            AssetDatabase.CreateAsset(mat, assetPath);
            return mat;
        }

        // ── Generic helpers ───────────────────────────────────────────────────

        static void EnsureDirectory(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) return;
            var parent     = Path.GetDirectoryName(assetPath)?.Replace('\\', '/') ?? "";
            var folderName = Path.GetFileName(assetPath);
            EnsureDirectory(parent);
            AssetDatabase.CreateFolder(parent, folderName);
        }

        static void SetField(Object target, string field, Object value)
        {
            var so   = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null) { Debug.LogWarning($"[TestSceneSetup] 找不到字段 '{field}'"); return; }
            prop.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }

        static void SetBool(SerializedObject so, string field, bool value)
        {
            var prop = so.FindProperty(field);
            if (prop != null) prop.boolValue = value;
        }
    }
}
