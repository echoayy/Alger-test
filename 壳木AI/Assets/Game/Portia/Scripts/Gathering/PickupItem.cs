using UnityEngine;
using Starter.Core;

namespace Game.Portia
{
    public class PickupItem : MonoBehaviour, IInteractable
    {
        [SerializeField] int _gid   = 1;
        [SerializeField] int _count = 1;

        PickupItemSpawner _spawner;
        int               _entryIndex = -1;
        Vector3           _respawnOrigin;
        bool              _canRespawn;

        public string PromptText =>
            $"[E] 拾取 {InventoryManager.GetItemName(_gid)} x{_count}";

        public void Interact(GameObject initiator)
        {
            InventoryManager.Instance?.Add(_gid, _count);
            EventBus.Emit(new ItemReceivedEvent { Gid = _gid, Count = _count });

            if (_canRespawn && _spawner != null && _entryIndex >= 0)
                _spawner.ScheduleRespawn(_entryIndex, _respawnOrigin);

            EventBus.Emit(new InteractTargetChangedEvent { Target = null });
            Destroy(gameObject);
        }

        public void Init(int gid, int count)
        {
            _gid   = gid;
            _count = count;
            _spawner = null;
            _entryIndex = -1;
            _respawnOrigin = Vector3.zero;
            _canRespawn = false;
        }

        public void InitRespawn(PickupItemSpawner spawner, int entryIndex, Vector3 respawnOrigin)
        {
            _spawner = spawner;
            _entryIndex = entryIndex;
            _respawnOrigin = respawnOrigin;
            _canRespawn = spawner != null && entryIndex >= 0;
        }
    }
}
