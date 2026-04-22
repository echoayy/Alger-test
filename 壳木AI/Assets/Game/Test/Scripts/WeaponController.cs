using UnityEngine;
using Starter.Runtime;
using Game.System;
using Game.Player;

namespace Game.Test
{
    [RequireComponent(typeof(PlayerController))]
    public class WeaponController : MonoBehaviour
    {
        [SerializeField, Tooltip("主相机（用于鼠标位置→世界坐标射线）")] Camera _camera;
        [SerializeField, Tooltip("枪挂点 Transform")] Transform _gunMount;
        [SerializeField, Tooltip("枪口 Transform，子弹由此生成")] Transform _muzzle;
        [SerializeField, Tooltip("子弹射程（米）")] float _maxRange = 30f;
        [SerializeField, Tooltip("游戏配置")] TestGameConfig _config;

        const string BulletPath = "Game/Prefabs/Bullet";

        PlayerController _playerCtrl;
        float            _nextFireTime;

        void Awake()
        {
            _playerCtrl = GetComponent<PlayerController>();
            _playerCtrl.ExternalRotation = true;
        }

        void Update()
        {
            if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;

            var aimPoint = GetAimPoint();
            RotatePlayerTowardCursor(aimPoint);

            // 连发：按住左键持续射击
            if (Input.GetMouseButton(0) && Time.time >= _nextFireTime)
            {
                Fire();
                float rate    = _config != null ? _config.FireRate : 8f;
                _nextFireTime = Time.time + 1f / rate;
            }
        }

        Vector3 GetAimPoint()
        {
            if (_camera == null) return transform.position + transform.forward * _maxRange;
            var ray = _camera.ScreenPointToRay(Input.mousePosition);
            return Physics.Raycast(ray, out var hit, _maxRange + 100f)
                ? hit.point
                : ray.GetPoint(_maxRange);
        }

        void RotatePlayerTowardCursor(Vector3 aimPoint)
        {
            var dir = aimPoint - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f) return;
            transform.rotation = Quaternion.LookRotation(dir);
        }

        void Fire()
        {
            if (_muzzle == null) return;
            var bullet = PoolManager.Get(BulletPath);
            if (bullet == null) return;

            bullet.transform.SetPositionAndRotation(_muzzle.position, _muzzle.rotation);

            float speed  = _config != null ? _config.BulletSpeed  : 25f;
            float damage = _config != null ? _config.BulletDamage : 50f;
            bullet.GetComponent<BulletBehaviour>().Launch(BulletPath, _maxRange, speed, damage);
        }
    }
}
