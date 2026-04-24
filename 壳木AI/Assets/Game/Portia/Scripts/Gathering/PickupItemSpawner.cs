using System;
using System.Collections;
using UnityEngine;

namespace Game.Portia
{
    public class PickupItemSpawner : MonoBehaviour
    {
        [Serializable]
        public struct Entry
        {
            public GameObject prefab;
            public int        gid;
            public int        itemCount;
            public int        spawnCount;
            public float      scale;
            public float      respawnDelay;
            public float      respawnRadius;
        }

        [SerializeField] Entry[] _entries;
        [SerializeField] float   _spawnRadius      = 40f;
        [SerializeField] float   _minRadius        = 4f;
        [SerializeField] float   _raycastFromY     = 200f;
        [SerializeField] float   _collisionPadding = 0.35f;
        [SerializeField] float   _respawnRetryDelay = 5f;

        int _spawnMask;

        void Awake()
        {
            _spawnMask = ~LayerMask.GetMask("Player", "Ignore Raycast");
            ApplyEntryDefaults();
        }

        void Start()
        {
            var player = GameObject.FindWithTag("Player");
            var center = player != null ? player.transform.position : Vector3.zero;

            for (int i = 0; i < _entries.Length; i++)
                SpawnBatch(_entries[i], i, center, _spawnRadius, _minRadius);
        }

        public void ScheduleRespawn(int entryIndex, Vector3 respawnOrigin)
        {
            if (entryIndex < 0 || entryIndex >= _entries.Length) return;
            StartCoroutine(RespawnRoutine(entryIndex, respawnOrigin));
        }

        IEnumerator RespawnRoutine(int entryIndex, Vector3 respawnOrigin)
        {
            var entry = _entries[entryIndex];
            float delay = entry.respawnDelay > 0f ? entry.respawnDelay : 0f;
            if (delay > 0f) yield return new WaitForSeconds(delay);

            while (!TrySpawnSingle(entry, entryIndex, respawnOrigin, entry.respawnRadius, 0f))
                yield return new WaitForSeconds(_respawnRetryDelay);
        }

        void SpawnBatch(Entry entry, int entryIndex, Vector3 center, float radius, float minRadius)
        {
            if (entry.prefab == null) return;

            int spawned = 0;
            int attempts = Mathf.Max(entry.spawnCount * 12, 12);

            while (spawned < entry.spawnCount && attempts-- > 0)
            {
                if (TrySpawnSingle(entry, entryIndex, center, radius, minRadius))
                    spawned++;
            }

            if (spawned < entry.spawnCount)
            {
                Debug.LogWarning(
                    $"[PickupItemSpawner] {entry.prefab.name}: only spawned {spawned}/{entry.spawnCount} items.");
            }
        }

        bool TrySpawnSingle(Entry entry, int entryIndex, Vector3 center, float radius, float minRadius)
        {
            if (entry.prefab == null) return false;

            int attempts = 20;
            while (attempts-- > 0)
            {
                if (!TryFindGroundPoint(center, radius, minRadius, out var spawnPoint))
                    continue;

                if (IsBlocked(spawnPoint, EstimateBlockRadius(entry)))
                    continue;

                var respawnAnchor = minRadius > 0f ? spawnPoint : center;
                SpawnPickup(entry, entryIndex, respawnAnchor, spawnPoint);
                return true;
            }

            return false;
        }

        bool TryFindGroundPoint(Vector3 center, float radius, float minRadius, out Vector3 spawnPoint)
        {
            radius = Mathf.Max(radius, 0.01f);

            var rand2d = UnityEngine.Random.insideUnitCircle * radius;
            if (rand2d.sqrMagnitude < minRadius * minRadius)
            {
                spawnPoint = default;
                return false;
            }

            var origin = new Vector3(center.x + rand2d.x, center.y + _raycastFromY, center.z + rand2d.y);
            if (!Physics.Raycast(origin, Vector3.down, out var hit, _raycastFromY * 2f,
                    _spawnMask, QueryTriggerInteraction.Ignore))
            {
                spawnPoint = default;
                return false;
            }

            spawnPoint = hit.point;
            return true;
        }

        void SpawnPickup(Entry entry, int entryIndex, Vector3 respawnOrigin, Vector3 spawnPoint)
        {
            var go = Instantiate(entry.prefab, spawnPoint,
                Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f));
            go.transform.localScale = Vector3.one * entry.scale;

            EnsureTrigger(go);

            var pickup = go.GetComponent<PickupItem>() ?? go.AddComponent<PickupItem>();
            pickup.Init(entry.gid, entry.itemCount);
            pickup.InitRespawn(this, entryIndex, respawnOrigin);
        }

        void ApplyEntryDefaults()
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                if (_entries[i].respawnDelay <= 0f)
                    _entries[i].respawnDelay = 20f;
                if (_entries[i].respawnRadius <= 0f)
                    _entries[i].respawnRadius = 3f;
            }
        }

        float EstimateBlockRadius(Entry entry)
        {
            return Mathf.Max(0.45f, entry.scale * 0.75f) + _collisionPadding;
        }

        bool IsBlocked(Vector3 position, float radius)
        {
            var center = position + Vector3.up * Mathf.Max(radius, 0.2f);
            var hits = Physics.OverlapSphere(center, radius, _spawnMask, QueryTriggerInteraction.Ignore);
            foreach (var hit in hits)
            {
                if (hit == null || !hit.enabled || hit.isTrigger) continue;
                if (hit is TerrainCollider) continue;
                return true;
            }

            return false;
        }

        static void EnsureTrigger(GameObject go)
        {
            foreach (var c in go.GetComponentsInChildren<Collider>())
                if (c.isTrigger) return;

            var sc = go.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 0.6f;
            sc.center = new Vector3(0f, 0.3f, 0f);
        }
    }
}
