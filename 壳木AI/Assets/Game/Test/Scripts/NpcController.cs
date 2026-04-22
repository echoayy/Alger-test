using UnityEngine;
using Starter.Runtime;
using Game.System;

namespace Game.Test
{
    public class NpcController : MonoBehaviour
    {
        public const string PoolPath = "Game/Prefabs/NPC";

        // 活跃数量计数，由 OnEnable/OnDisable 维护
        public static int ActiveCount { get; private set; }

        float   _moveSpeed;
        float   _health;
        Vector3 _target;

        // ── 生命周期 ──────────────────────────────────────────────────────────

        void OnEnable()  => ActiveCount++;
        void OnDisable() => ActiveCount--;

        public void Init(TestGameConfig cfg)
        {
            _health    = cfg.NpcMaxHealth;
            _moveSpeed = cfg.NpcMoveSpeed;
            PickTarget();
        }

        void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
            MoveToTarget();
        }

        // ── 移动 ─────────────────────────────────────────────────────────────

        void MoveToTarget()
        {
            var dir = _target - transform.position;
            dir.y = 0f;
            if (dir.magnitude < 0.5f) { PickTarget(); return; }

            transform.position += dir.normalized * (_moveSpeed * Time.deltaTime);
            transform.rotation  = Quaternion.LookRotation(dir);
        }

        void PickTarget()
        {
            var angle  = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            var dist   = Random.Range(3f, 8f);
            var offset = new Vector3(Mathf.Cos(angle) * dist, 0f, Mathf.Sin(angle) * dist);
            _target    = transform.position + offset;
            _target.y  = transform.position.y;
            _target.x  = Mathf.Clamp(_target.x, -40f, 40f);
            _target.z  = Mathf.Clamp(_target.z, -40f, 40f);
        }

        // ── 受伤 / 死亡 ───────────────────────────────────────────────────────

        public void TakeDamage(float dmg)
        {
            _health -= dmg;
            if (_health <= 0f) PoolManager.Release(PoolPath, gameObject);
        }
    }
}
