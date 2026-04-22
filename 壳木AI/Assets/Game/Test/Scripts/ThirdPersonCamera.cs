using UnityEngine;
using Starter.Core;
using Game.System;

namespace Game.Test
{
    // 俯视固定相机：跟随玩家 XYZ 位置，自身旋转固定不变
    public class ThirdPersonCamera : MonoBehaviour
    {
        [SerializeField, Tooltip("跟随目标（Player Transform）")] Transform _target;
        [SerializeField, Tooltip("相机高于玩家的距离")] float _height     = 14f;
        [SerializeField, Tooltip("俯视倾斜角（90=正上方，60=斜俯视）")] float _pitch  = 70f;
        [SerializeField, Tooltip("世界 -Z 方向后退偏移，让视野偏向玩家前方")] float _backOffset = 3f;
        [SerializeField, Tooltip("位置跟随平滑系数")] float _smooth     = 10f;

        bool _isPlaying;

        void Start()
        {
            // 固定相机旋转，整个游戏中不变
            transform.rotation = Quaternion.Euler(_pitch, 0f, 0f);
            EventBus.On<GameStateChangedEvent>(OnGameStateChanged);
            if (GameManager.Instance != null)
                _isPlaying = GameManager.Instance.State == GameState.Playing;
        }

        void OnDestroy()
        {
            EventBus.Off<GameStateChangedEvent>(OnGameStateChanged);
        }

        void OnGameStateChanged(GameStateChangedEvent evt)
        {
            _isPlaying = evt.Current == GameState.Playing;
        }

        void LateUpdate()
        {
            if (_target == null || !_isPlaying) return;

            // 只跟随位置，旋转锁定不动
            var desired = _target.position
                + Vector3.up   * _height
                + Vector3.back * _backOffset;

            transform.position = Vector3.Lerp(transform.position, desired, _smooth * Time.deltaTime);
        }

        public void SetTarget(Transform target) => _target = target;
    }
}
