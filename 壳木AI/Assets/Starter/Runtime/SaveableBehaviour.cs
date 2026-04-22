using UnityEngine;

namespace Starter.Runtime
{
    // 继承此类可免去手动注册/注销 ISaveable 的样板代码
    // 用法：public class PlayerSave : SaveableBehaviour { ... }
    [DisallowMultipleComponent]
    public abstract class SaveableBehaviour : MonoBehaviour, ISaveable
    {
        public abstract string SaveID { get; }
        public abstract void OnSave(SaveData data);
        public abstract void OnLoad(SaveData data);

        protected virtual void OnEnable()  => SaveManager.Register(this);
        protected virtual void OnDisable() => SaveManager.Unregister(this);
    }
}
