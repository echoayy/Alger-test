using UnityEngine;
using UnityEngine.UI;
using Starter.UI;
using Game.System;

namespace Game.Test
{
    public class TestPausePanel : UIPanel
    {
        [SerializeField, Tooltip("继续游戏按钮")] Button _resumeButton;

        void Reset()
        {
            panelAttr.showBlur  = true;
            panelAttr.autoScale = false;
        }

        void Start()
        {
            if (_resumeButton != null)
                _resumeButton.onClick.AddListener(() => UIManager.Inst.PopPanel(this));
        }

        // UIManager 在 PopPanel 时调用，统一由此处恢复游戏
        public override void OnPop()
        {
            GameManager.Instance?.ResumeGame();
        }
    }
}
