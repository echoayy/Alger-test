using UnityEngine;
using UnityEngine.UI;
using Starter.Core;
using Game.System;

namespace Game.Test
{
    // 独立 Canvas 的准星 UI：Playing 时隐藏系统光标，绘制十字准星跟随鼠标
    public class CrosshairUI : MonoBehaviour
    {
        [SerializeField, Tooltip("准星臂长（像素）")] float _armLength = 10f;
        [SerializeField, Tooltip("准星粗细（像素）")] float _thickness  = 2f;
        [SerializeField, Tooltip("中心间隙（像素）")] float _gap        = 4f;
        [SerializeField, Tooltip("准星颜色")] Color _color = Color.white;

        RectTransform _crosshairRoot;
        bool _isPlaying;

        void Start()
        {
            BuildCanvas();
            EventBus.On<GameStateChangedEvent>(OnGameStateChanged);
            if (GameManager.Instance != null)
                _isPlaying = GameManager.Instance.State == GameState.Playing;
            ApplyCursorState();
        }

        void OnDestroy()
        {
            EventBus.Off<GameStateChangedEvent>(OnGameStateChanged);
        }

        void OnGameStateChanged(GameStateChangedEvent evt)
        {
            _isPlaying = evt.Current == GameState.Playing;
            ApplyCursorState();
        }

        void ApplyCursorState()
        {
            Cursor.visible = !_isPlaying;
            if (_crosshairRoot != null)
                _crosshairRoot.gameObject.SetActive(_isPlaying);
        }

        void Update()
        {
            if (!_isPlaying || _crosshairRoot == null) return;
            _crosshairRoot.position = Input.mousePosition;
        }

        void BuildCanvas()
        {
            var canvasGo = new GameObject("[CrosshairCanvas]");
            var canvas   = canvasGo.AddComponent<Canvas>();
            canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;

            // 准星根节点（跟随鼠标）
            var root       = new GameObject("Crosshair");
            root.transform.SetParent(canvasGo.transform, false);
            _crosshairRoot = root.AddComponent<RectTransform>();
            _crosshairRoot.sizeDelta = Vector2.zero;

            // 上
            AddArm("Top",    new Vector2(_thickness, _armLength),
                new Vector2(0f,  _gap * 0.5f + _armLength * 0.5f));
            // 下
            AddArm("Bottom", new Vector2(_thickness, _armLength),
                new Vector2(0f, -(_gap * 0.5f + _armLength * 0.5f)));
            // 左
            AddArm("Left",   new Vector2(_armLength, _thickness),
                new Vector2(-(_gap * 0.5f + _armLength * 0.5f), 0f));
            // 右
            AddArm("Right",  new Vector2(_armLength, _thickness),
                new Vector2(_gap * 0.5f + _armLength * 0.5f, 0f));
        }

        void AddArm(string name, Vector2 size, Vector2 offset)
        {
            var go   = new GameObject(name);
            go.transform.SetParent(_crosshairRoot, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta       = size;
            rect.anchoredPosition = offset;
            var img  = go.AddComponent<Image>();
            img.color = _color;
        }
    }
}
