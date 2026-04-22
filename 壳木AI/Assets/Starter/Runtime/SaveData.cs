using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Starter.Runtime
{
    [Serializable]
    public class SaveMeta
    {
        public int    slot;
        public string saveTime;     // "yyyy-MM-dd HH:mm:ss"
        public float  totalPlayTime; // 累计游玩时长（秒）
        public string level;         // 存档时所在场景名
        public string version;       // Application.version
    }

    // 类型化键值存储容器，通过 ISerializationCallbackReceiver 桥接 Dict ↔ List
    // 支持 int / float / bool / string / Vector2 / Vector3 / 任意可 JsonUtility 序列化的对象
    [Serializable]
    public class SaveData : ISerializationCallbackReceiver
    {
        public SaveMeta meta = new();

        [Serializable]
        class KVPair { public string k; public string v; }

        [SerializeField] List<KVPair> _entries = new();
        [NonSerialized]  Dictionary<string, string> _dict = new();

        // ── ISerializationCallbackReceiver ────────────────────────────────────

        public void OnBeforeSerialize()
        {
            _entries.Clear();
            foreach (var kv in _dict)
                _entries.Add(new KVPair { k = kv.Key, v = kv.Value });
        }

        public void OnAfterDeserialize()
        {
            _dict ??= new();
            _dict.Clear();
            foreach (var e in _entries)
                if (e != null) _dict[e.k] = e.v;
        }

        // ── Setters ───────────────────────────────────────────────────────────

        public void Set(string key, int     v) => _dict[key] = v.ToString(CultureInfo.InvariantCulture);
        public void Set(string key, float   v) => _dict[key] = v.ToString("R", CultureInfo.InvariantCulture);
        public void Set(string key, bool    v) => _dict[key] = v ? "1" : "0";
        public void Set(string key, string  v) => _dict[key] = v ?? string.Empty;

        public void Set(string key, Vector2 v) =>
            _dict[key] = $"{v.x.ToString("R", CultureInfo.InvariantCulture)}|{v.y.ToString("R", CultureInfo.InvariantCulture)}";

        public void Set(string key, Vector3 v) =>
            _dict[key] = $"{v.x.ToString("R", CultureInfo.InvariantCulture)}|{v.y.ToString("R", CultureInfo.InvariantCulture)}|{v.z.ToString("R", CultureInfo.InvariantCulture)}";

        // 存储任意可被 JsonUtility 序列化的对象（需 [Serializable]）
        public void SetObject<T>(string key, T obj) => _dict[key] = JsonUtility.ToJson(obj);

        // ── Getters ───────────────────────────────────────────────────────────

        public int GetInt(string key, int def = 0) =>
            _dict.TryGetValue(key, out var s) && int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var r) ? r : def;

        public float GetFloat(string key, float def = 0f) =>
            _dict.TryGetValue(key, out var s) && float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var r) ? r : def;

        public bool GetBool(string key, bool def = false) =>
            _dict.TryGetValue(key, out var s) ? s == "1" : def;

        public string GetString(string key, string def = "") =>
            _dict.TryGetValue(key, out var s) ? s : def;

        public Vector2 GetVector2(string key, Vector2 def = default)
        {
            if (!_dict.TryGetValue(key, out var s)) return def;
            var p = s.Split('|');
            return p.Length == 2
                   && float.TryParse(p[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
                   && float.TryParse(p[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
                ? new Vector2(x, y) : def;
        }

        public Vector3 GetVector3(string key, Vector3 def = default)
        {
            if (!_dict.TryGetValue(key, out var s)) return def;
            var p = s.Split('|');
            return p.Length == 3
                   && float.TryParse(p[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
                   && float.TryParse(p[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
                   && float.TryParse(p[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var z)
                ? new Vector3(x, y, z) : def;
        }

        public T GetObject<T>(string key, T def = default) =>
            _dict.TryGetValue(key, out var s) ? JsonUtility.FromJson<T>(s) : def;

        // ── 工具 ──────────────────────────────────────────────────────────────

        public bool HasKey(string key)  => _dict.ContainsKey(key);
        public void Remove(string key)  => _dict.Remove(key);
        public void ClearValues()       => _dict.Clear();
    }
}
