using UnityEngine;
using Starter.Runtime;
using Game.System;

namespace Game.Test
{
    public class NpcSpawner : MonoBehaviour
    {
        [SerializeField, Tooltip("游戏配置")] TestGameConfig _config;
        [SerializeField, Tooltip("生成半径（围绕原点）")] float _spawnRadius = 12f;

        float _timer;

        void Update()
        {
            if (_config == null) return;
            if (GameManager.Instance?.State != GameState.Playing) return;

            _timer += Time.deltaTime;
            if (_timer >= _config.NpcSpawnInterval)
            {
                _timer = 0f;
                TrySpawn();
            }
        }

        void TrySpawn()
        {
            if (NpcController.ActiveCount >= _config.NpcMaxCount) return;

            var angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            var dist  = Random.Range(3f, _spawnRadius);
            // y=1：胶囊中心在地面上方一个单位
            var pos   = new Vector3(Mathf.Cos(angle) * dist, 1f, Mathf.Sin(angle) * dist);

            var go = PoolManager.Get(NpcController.PoolPath);
            go.transform.SetPositionAndRotation(pos, Quaternion.identity);
            go.GetComponent<NpcController>().Init(_config);
        }
    }
}
