using UnityEngine;
using Object = UnityEngine.Object;

namespace Starter.Runtime
{
    // 持有 prefab 引用的泛型对象池，基于 UnityEngine.Pool
    // 用法：var pool = new ObjectPool<Bullet>(bulletPrefab, preload: 10);
    //       var b = pool.Get(pos, rot);   pool.Return(b);
    public class ObjectPool<T> where T : Component
    {
        readonly T _prefab;
        readonly Transform _parent;
        readonly UnityEngine.Pool.ObjectPool<T> _pool;

        public int CountInactive => _pool.CountInactive;
        public int CountActive   => _pool.CountActive;

        public ObjectPool(T prefab, int preload = 0, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;
            _pool   = new UnityEngine.Pool.ObjectPool<T>(
                createFunc:       Create,
                actionOnGet:      OnGet,
                actionOnRelease:  OnRelease,
                actionOnDestroy:  obj => Object.Destroy(obj.gameObject),
                collectionCheck:  true
            );

            // 预热：创建后立即归还到池中
            for (int i = 0; i < preload; i++)
            {
                var obj = Create();
                _pool.Release(obj);
            }
        }

        public T Get(Vector3 position = default, Quaternion rotation = default)
        {
            var obj = _pool.Get();
            obj.transform.SetPositionAndRotation(position, rotation);
            return obj;
        }

        public void Return(T obj) => _pool.Release(obj);

        public void Clear() => _pool.Clear();

        // ── 内部回调 ──────────────────────────────────────────────────────────

        T Create()
        {
            var obj = Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            return obj;
        }

        void OnGet(T obj)
        {
            obj.transform.localScale    = Vector3.one;
            obj.transform.localRotation = Quaternion.identity;
            obj.gameObject.SetActive(true);
        }

        void OnRelease(T obj)
        {
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(_parent);
        }
    }
}
