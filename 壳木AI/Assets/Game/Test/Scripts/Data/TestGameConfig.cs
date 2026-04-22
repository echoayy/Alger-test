using UnityEngine;

namespace Game.Test
{
    [CreateAssetMenu(fileName = "TestGameConfig", menuName = "壳木AI/Test/GameConfig")]
    public class TestGameConfig : ScriptableObject
    {
        [Header("枪械")]
        [Tooltip("子弹飞行速度（米/秒）")] public float BulletSpeed   = 25f;
        [Tooltip("每发伤害值")]             public float BulletDamage  = 50f;
        [Tooltip("射速（发/秒）")]           public float FireRate      = 8f;

        [Header("NPC")]
        [Tooltip("NPC 最大生命值")]          public float NpcMaxHealth     = 100f;
        [Tooltip("NPC 移动速度（米/秒）")]   public float NpcMoveSpeed     = 2.5f;
        [Tooltip("每隔几秒生成一个 NPC")]    public float NpcSpawnInterval = 3f;
        [Tooltip("场景中同时存在的最大 NPC 数")] public int NpcMaxCount    = 8;
    }
}
