using UnityEngine;
using Starter.Runtime;

namespace Game.Test
{
    public class BulletBehaviour : MonoBehaviour
    {
        [SerializeField, Tooltip("默认飞行速度（被 Launch 覆盖）")] float _speed = 25f;

        string _poolName;
        float  _maxRange;
        float  _damage;
        float  _traveled;

        public void Launch(string poolName, float maxRange, float speed, float damage)
        {
            _poolName = poolName;
            _maxRange = maxRange;
            _speed    = speed;
            _damage   = damage;
            _traveled = 0f;
        }

        void Update()
        {
            float step = _speed * Time.deltaTime;
            transform.Translate(Vector3.forward * step, Space.Self);
            _traveled += step;
            if (_traveled >= _maxRange) ReturnToPool();
        }

        void OnTriggerEnter(Collider other)
        {
            // 命中 NPC 则扣血
            var npc = other.GetComponent<NpcController>();
            if (npc != null) npc.TakeDamage(_damage);
            ReturnToPool();
        }

        void ReturnToPool()
        {
            if (!string.IsNullOrEmpty(_poolName))
                PoolManager.Release(_poolName, gameObject);
        }
    }
}
