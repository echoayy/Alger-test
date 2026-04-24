using UnityEngine;
using UnityEditor;

namespace Game.Portia.Editor
{
    public class GatherNodePlacerWindow : EditorWindow
    {
        [MenuItem("工具/放置可采集物")]
        public static void Open()
        {
            GetWindow<GatherNodePlacerWindow>("放置可采集物");
        }

        enum GatherType
        {
            Tree = 0,
            TieKuangShi = 1,
            Stone = 2
        }

        GatherType _selectedType = GatherType.Tree;
        int _count = 5;
        float _radius = 10f;
        int _requiredPresses = 3;
        bool _fallOnGather = false;

        string[] _typeNames = { "Tree (树)", "TieKuangShi (铁矿)", "Stone (石头)" };
        string[] _prefabPaths = {
            "Assets/Model/GameObject/ltree001.prefab",
            "Assets/Model/interactive/stone/stone_inter_volcanicRock.prefab",
            "Assets/Model/interactive/stone/ItemOneHand_Stone.prefab"
        };

        void OnGUI()
        {
            GUILayout.Label("可采集物批量放置工具", EditorStyles.boldLabel);

            _selectedType = (GatherType)EditorGUILayout.Popup("类型", (int)_selectedType, _typeNames);
            _count = EditorGUILayout.IntSlider("数量", _count, 1, 50);
            _radius = EditorGUILayout.Slider("分布半径", _radius, 1f, 50f);
            _requiredPresses = EditorGUILayout.IntSlider("点击次数", _requiredPresses, 1, 10);
            _fallOnGather = EditorGUILayout.Toggle("砍倒后倒下", _fallOnGather);

            EditorGUILayout.Space();

            if (GUILayout.Button("放置到选中的物体位置"))
            {
                PlaceAtSelection();
            }

            if (GUILayout.Button("放置到场景视图中心"))
            {
                PlaceAtSceneViewCenter();
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "1. 在场景中选中一个物体(作为中心点)\n" +
                "2. 点击'放置到选中的物体位置'\n" +
                $"3. 将在该位置周围 {_radius} 范围内随机放置 {_count} 个 {_typeNames[(int)_selectedType]}",
                MessageType.Info);
        }

        void PlaceAtSelection()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                ShowNotification(new GUIContent("请先在场景中选中一个物体"));
                return;
            }

            PlaceMultiple(selected.transform.position);
        }

        void PlaceAtSceneViewCenter()
        {
            Vector3 center = Vector3.zero;
            Camera sceneCam = SceneView.lastActiveSceneView?.camera;
            if (sceneCam != null)
            {
                Vector3 pos = sceneCam.transform.position + sceneCam.transform.forward * 15f;
                if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 100f, LayerMask.GetMask("Land")))
                {
                    center = hit.point;
                }
            }

            PlaceMultiple(center);
        }

        void PlaceMultiple(Vector3 center)
        {
            string prefabPath = _prefabPaths[(int)_selectedType];
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                ShowNotification(new GUIContent($"无法加载预制体: {prefabPath}"));
                Debug.LogError($"无法加载预制体: {prefabPath}");
                return;
            }

            for (int i = 0; i < _count; i++)
            {
                // 随机位置
                Vector2 randomCircle = Random.insideUnitCircle * _radius;
                Vector3 pos = center + new Vector3(randomCircle.x, 0, randomCircle.y);

                // 向下投射到Land层
                if (Physics.Raycast(pos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, LayerMask.GetMask("Land")))
                {
                    pos = hit.point;
                }

                GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                go.transform.position = pos;
                go.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

                var gatherNode = go.AddComponent<Game.Portia.GatherNode>();
                gatherNode.SetRequiredPresses(_requiredPresses);
                gatherNode.SetFallOnGather(_fallOnGather);

                Undo.RegisterCreatedObjectUndo(go, "Place GatherNode");
            }

            Selection.activeGameObject = GameObject.FindObjectsOfType<GameObject>().Length > 0
                ? GameObject.FindObjectsOfType<GameObject>()[0] : null;

            ShowNotification(new GUIContent($"已放置 {_count} 个 {_typeNames[(int)_selectedType]}"));
            Debug.Log($"已放置 {_count} 个 {_typeNames[(int)_selectedType]} at {center}, radius={_radius}");
        }
    }
}
