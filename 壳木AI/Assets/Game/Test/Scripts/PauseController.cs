using UnityEngine;
using Starter.UI;
using Game.System;

namespace Game.Test
{
    // 挂在 --Managers-- 下，监听 ESC 键开关暂停面板
    public class PauseController : MonoBehaviour
    {
        const string PausePanelRes = "UI/TestPausePanel";

        TestPausePanel _current;

        void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;

            var state = GameManager.Instance?.State;

            if (state == GameState.Playing)
            {
                GameManager.Instance.PauseGame();
                _current = UIManager.Inst.PushPanel<TestPausePanel>(PausePanelRes);
            }
            else if (state == GameState.Paused && _current != null)
            {
                UIManager.Inst.PopPanel(_current); // 触发 OnPop → ResumeGame
                _current = null;
            }
        }
    }
}
