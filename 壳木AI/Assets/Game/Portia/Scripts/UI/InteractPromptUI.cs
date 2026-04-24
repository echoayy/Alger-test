using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Starter.Core;
using Game.System;

namespace Game.Portia
{
    public class InteractPromptUI : MonoBehaviour
    {
        Canvas        _canvas;
        RectTransform _promptRoot;
        Text          _promptLabel;
        RectTransform _keyHintRoot;
        Text          _keyHintLabel;

        void Awake()
        {
            BuildCanvas();
            BuildPrompt();
            BuildKeyHints();
            SetPromptVisible(false);
            _canvas.enabled = false;
            EventBus.On<InteractTargetChangedEvent>(OnTargetChanged);
            EventBus.On<GameStateChangedEvent>(OnGameStateChanged);
        }

        void OnDestroy()
        {
            EventBus.Off<InteractTargetChangedEvent>(OnTargetChanged);
            EventBus.Off<GameStateChangedEvent>(OnGameStateChanged);
            if (_keyHintRoot != null)
                Destroy(_keyHintRoot.gameObject);
        }

        void OnTargetChanged(InteractTargetChangedEvent e)
        {
            if (e.Target == null || string.IsNullOrEmpty(e.Target.PromptText))
            {
                SetPromptVisible(false);
                return;
            }

            _promptLabel.text = NormalizePromptText(e.Target.PromptText);
            SetPromptVisible(true);
        }

        void OnGameStateChanged(GameStateChangedEvent e)
        {
            _canvas.enabled = e.Current == GameState.Playing;
            if (e.Current != GameState.Playing) SetPromptVisible(false);
            if (_keyHintRoot != null)
                _keyHintRoot.gameObject.SetActive(e.Current == GameState.Playing);
        }

        void SetPromptVisible(bool visible) => _promptRoot.gameObject.SetActive(visible);

        void BuildCanvas()
        {
            _canvas              = gameObject.AddComponent<Canvas>();
            _canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 20;
            gameObject.AddComponent<CanvasScaler>();
        }

        void BuildPrompt()
        {
            var go = new GameObject("PromptRoot");
            go.transform.SetParent(transform, false);
            _promptRoot                  = go.AddComponent<RectTransform>();
            _promptRoot.anchorMin        = _promptRoot.anchorMax = new Vector2(0f, 1f);
            _promptRoot.pivot            = new Vector2(0f, 1f);
            _promptRoot.anchoredPosition = new Vector2(20f, -60f);
            _promptRoot.sizeDelta        = new Vector2(280f, 44f);

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(go.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = textRect.offsetMax = Vector2.zero;

            _promptLabel           = textGo.AddComponent<Text>();
            _promptLabel.fontSize  = 20;
            _promptLabel.color     = Color.white;
            _promptLabel.alignment = TextAnchor.MiddleCenter;
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                    ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font != null) _promptLabel.font = font;
        }

        void BuildKeyHints()
        {
            var go = new GameObject("KeyHintRoot");
            go.transform.SetParent(transform, false);
            _keyHintRoot                  = go.AddComponent<RectTransform>();
            _keyHintRoot.anchorMin        = _keyHintRoot.anchorMax = new Vector2(0f, 1f);
            _keyHintRoot.pivot            = new Vector2(0f, 1f);
            _keyHintRoot.anchoredPosition = new Vector2(20f, -20f);
            _keyHintRoot.sizeDelta        = new Vector2(400f, 30f);
            var textGo = new GameObject("Label");
            textGo.transform.SetParent(go.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = textRect.offsetMax = Vector2.zero;

            _keyHintLabel           = textGo.AddComponent<Text>();
            _keyHintLabel.fontSize  = 18;
            _keyHintLabel.color     = new Color(1f, 1f, 1f, 0.6f);
            _keyHintLabel.alignment = TextAnchor.MiddleCenter;
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                    ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font != null) _keyHintLabel.font = font;
            _keyHintLabel.text = "建造面板B键  |  背包tab键  |  采集E键";
        }

        static string NormalizePromptText(string rawPrompt)
        {
            if (string.IsNullOrWhiteSpace(rawPrompt)) return rawPrompt;

            var promptText = rawPrompt.Trim();
            if (!HasLegacyKeyHint(promptText)) return promptText;

            var actionText = Regex.Replace(promptText, "^\\s*\\u6309\\s+\\S+\\s*", string.Empty);
            actionText = Regex.Replace(actionText, @"^\s*\[\s*[^\]]+\s*\]\s*", string.Empty);
            actionText = actionText.Trim();

            return string.IsNullOrEmpty(actionText)
                ? $"\u6309 {InteractionDetector.CurrentInteractKey}"
                : $"\u6309 {InteractionDetector.CurrentInteractKey} {actionText}";
        }

        static bool HasLegacyKeyHint(string promptText) =>
            Regex.IsMatch(promptText, "^\\s*\\u6309\\s+\\S+") ||
            Regex.IsMatch(promptText, @"^\s*\[\s*[^\]]+\s*\]");
    }
}
