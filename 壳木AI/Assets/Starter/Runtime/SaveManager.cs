using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Starter.Core;

namespace Starter.Runtime
{
    // 存档事件（通过 EventBus 广播）
    public struct GameSavedEvent { public int Slot; public SaveData Data; }
    public struct GameLoadedEvent { public int Slot; public SaveData Data; }

    // 单机存档管理器
    // 初始化：SaveManager 无需手动 Init，随时可调用
    // 存档文件位置：Application.persistentDataPath/saves/slot_{n}.json
    public static class SaveManager
    {
        public const int MaxSlots = 3;

        static SaveData _current;
        static float    _sessionStart;

        static readonly List<ISaveable> _saveables = new();

        // 当前已加载的存档数据（New Game 或 Load 之后有值）
        public static SaveData Current => _current;

        // 存档/读档完成回调（也可用 EventBus 监听 GameSavedEvent / GameLoadedEvent）
        public static event Action<int> OnSaved;
        public static event Action<int> OnLoaded;

        // ── 新游戏 ────────────────────────────────────────────────────────────

        public static SaveData NewGame()
        {
            _current      = new SaveData();
            _sessionStart = Time.realtimeSinceStartup;
            return _current;
        }

        // ── 存档 ──────────────────────────────────────────────────────────────

        public static void Save(int slot = 0)
        {
            _current ??= new SaveData();

            // 更新元数据
            _current.meta.slot          = slot;
            _current.meta.saveTime      = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _current.meta.totalPlayTime += Time.realtimeSinceStartup - _sessionStart;
            _current.meta.level         = SceneManager.GetActiveScene().name;
            _current.meta.version       = Application.version;
            _sessionStart = Time.realtimeSinceStartup;  // 重置本次会话计时

            // 通知所有已注册的 ISaveable 写入数据
            foreach (var s in _saveables)
            {
                try { s.OnSave(_current); }
                catch (Exception e) { Debug.LogError($"[SaveManager] OnSave 异常 ({s.SaveID}): {e}"); }
            }

            WriteToDisk(_current, slot);

            EventBus.Emit(new GameSavedEvent { Slot = slot, Data = _current });
            OnSaved?.Invoke(slot);
            Debug.Log($"[SaveManager] 已保存 → slot {slot}  ({GetPath(slot)})");
        }

        // ── 读档 ──────────────────────────────────────────────────────────────

        // 读取存档数据到 Current，并通知场景中已注册的 ISaveable
        public static SaveData Load(int slot = 0)
        {
            var path = GetPath(slot);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[SaveManager] 存档不存在: slot {slot}");
                return null;
            }

            try
            {
                var json = File.ReadAllText(path);
                _current  = JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] 存档损坏: {e.Message}");
                return null;
            }

            _sessionStart = Time.realtimeSinceStartup;

            // 通知所有已注册的 ISaveable 读取数据
            foreach (var s in _saveables)
            {
                try { s.OnLoad(_current); }
                catch (Exception e) { Debug.LogError($"[SaveManager] OnLoad 异常 ({s.SaveID}): {e}"); }
            }

            EventBus.Emit(new GameLoadedEvent { Slot = slot, Data = _current });
            OnLoaded?.Invoke(slot);
            Debug.Log($"[SaveManager] 已读取 ← slot {slot}");
            return _current;
        }

        // ── 存档槽查询 ────────────────────────────────────────────────────────

        public static bool     HasSave(int slot = 0) => File.Exists(GetPath(slot));

        public static void     Delete(int slot = 0)
        {
            var path = GetPath(slot);
            if (File.Exists(path)) File.Delete(path);
        }

        // 读取所有槽的元数据（用于存档选择 UI），无存档的槽返回 null
        public static SaveMeta[] GetAllSlotMeta()
        {
            var metas = new SaveMeta[MaxSlots];
            for (int i = 0; i < MaxSlots; i++)
            {
                var path = GetPath(i);
                if (!File.Exists(path)) continue;
                try
                {
                    var json = File.ReadAllText(path);
                    metas[i] = JsonUtility.FromJson<SaveData>(json)?.meta;
                }
                catch { /* 损坏的存档跳过 */ }
            }
            return metas;
        }

        // ── ISaveable 注册 ────────────────────────────────────────────────────

        public static void Register(ISaveable s)
        {
            if (!_saveables.Contains(s))
                _saveables.Add(s);
        }

        public static void Unregister(ISaveable s) => _saveables.Remove(s);

        // ── 便捷接口 ──────────────────────────────────────────────────────────

        // 自动存档（默认写 slot 0）
        public static void AutoSave() { if (_current != null) Save(0); }

        // 开始计时（NewGame/Load 已自动调用，手动调用用于场景跳转后重置计时）
        public static void BeginSession() => _sessionStart = Time.realtimeSinceStartup;

        // ── 内部 ──────────────────────────────────────────────────────────────

        static void WriteToDisk(SaveData data, int slot)
        {
            var path = GetPath(slot);
            var dir  = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(path, JsonUtility.ToJson(data));
        }

        static string GetPath(int slot) =>
            Path.Combine(Application.persistentDataPath, "saves", $"slot_{slot}.json");
    }
}
