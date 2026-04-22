# TestScene · 俯视角射击验证文档

> 目的：在 `Assets/Game/Test/` 内用最小可运行 Demo 验证工程基础设施层（UIManager、EventBus、GameManager、PoolManager），同时建立一套可扩展的俯视角射击原型。

---

## 一、场景概述

| 项目 | 内容 |
|------|------|
| 场景路径 | `Assets/Game/Test/Scenes/TestScene.unity` |
| 生成方式 | Unity 菜单 **壳木AI → 创建 TestScene** 一键生成 |
| 视角风格 | 俯视固定相机（高度 14，俯仰 70°），相机不随鼠标转动 |
| 游戏流程 | 开始界面 → 点击"开始游戏" → 玩家从高空落地 → 俯视射击 NPC |

---

## 二、控制方案

| 输入 | 效果 |
|------|------|
| `WASD` | 相对玩家朝向移动 |
| `Space` | 跳跃 |
| `鼠标移动` | 准星（十字叉丝）跟随，玩家模型实时转向准星方向 |
| `鼠标左键（按住）` | 连发射击，射速由配置文件控制 |
| `ESC` | 打开/关闭暂停菜单 |

---

## 三、目录结构

```
Assets/Game/Test/
├── Config/
│   └── TestGameConfig.asset        ← ScriptableObject 配置（可在 Inspector 实时调整）
├── Editor/
│   └── TestSceneSetup.cs           ← 一键生成场景的 Editor 脚本
├── Materials/
│   ├── Ground.mat                  ← 地面灰色材质
│   └── NPC_Red.mat                 ← NPC 红色材质
├── Resources/
│   ├── UI/
│   │   ├── TestStartPanel.prefab   ← 开始界面面板
│   │   └── TestPausePanel.prefab   ← 暂停菜单面板
│   └── Game/Prefabs/
│       ├── Bullet.prefab           ← 子弹（对象池）
│       └── NPC.prefab              ← NPC（对象池）
├── Scenes/
│   └── TestScene.unity
└── Scripts/
    ├── Data/
    │   └── TestGameConfig.cs
    ├── UI/
    │   ├── TestStartPanel.cs
    │   └── TestPausePanel.cs
    ├── BulletBehaviour.cs
    ├── CrosshairUI.cs
    ├── NpcController.cs
    ├── NpcSpawner.cs
    ├── PauseController.cs
    ├── TestSceneBootstrap.cs
    ├── ThirdPersonCamera.cs
    └── WeaponController.cs
```

---

## 四、脚本职责速查

| 脚本 | 挂载位置 | 核心职责 |
|------|----------|----------|
| `TestSceneBootstrap` | --Managers-- | 场景入口：隐藏玩家 → 推开始面板 → 监听 Playing → 激活玩家 |
| `ThirdPersonCamera` | Main Camera | 俯视固定相机，仅跟随玩家位置，不跟旋转 |
| `CrosshairUI` | Main Camera | 自建 Canvas，十字准星跟随鼠标；Playing 时隐藏系统光标 |
| `WeaponController` | Player | 鼠标射线 → 旋转玩家 → 连发逻辑 → 从对象池取子弹 |
| `BulletBehaviour` | Bullet prefab | 飞行 + 射程限制 + 命中 NPC 扣血 + 归还对象池 |
| `NpcController` | NPC prefab | 随机游走 + `TakeDamage()` + 血量归零归还对象池 |
| `NpcSpawner` | --Managers-- | 每 N 秒在随机位置生成 NPC，不超过最大数量 |
| `PauseController` | --Managers-- | ESC 开关暂停面板，调用 GameManager.PauseGame / ResumeGame |
| `TestStartPanel` | 预制体 | 点击"开始游戏" → PopPanel + GameManager.StartGame() |
| `TestPausePanel` | 预制体 | 点击"继续游戏" → PopPanel（OnPop 自动 ResumeGame） |

---

## 五、配置文件 TestGameConfig

路径：`Assets/Game/Test/Config/TestGameConfig.asset`

| 字段 | 默认值 | 说明 |
|------|--------|------|
| `BulletSpeed` | 25 m/s | 子弹飞行速度 |
| `BulletDamage` | 50 | 每发伤害 |
| `FireRate` | 8 发/秒 | 连发射速 |
| `NpcMaxHealth` | 100 | NPC 初始血量 |
| `NpcMoveSpeed` | 2.5 m/s | NPC 移动速度 |
| `NpcSpawnInterval` | 3 秒 | 每几秒生成一个 NPC |
| `NpcMaxCount` | 8 | 场景中同时存在的 NPC 上限 |

> 直接在 Unity Inspector 中修改后**无需重新生成场景**，下次 Play 即生效。

---

## 六、系统交互流程

```
玩家按下开始
    └─ GameManager.StartGame() → GameState = Playing
           ├─ TestSceneBootstrap  → 激活 Player（从 y=5 下落）
           ├─ ThirdPersonCamera   → 开始跟随
           ├─ CrosshairUI         → 隐藏系统光标，显示十字准星
           └─ NpcSpawner          → 开始计时，定时生成 NPC

鼠标移动
    └─ WeaponController.GetAimPoint() → 射线打到地面
           └─ RotatePlayerTowardCursor() → 玩家模型即时转向

按住左键
    └─ WeaponController.Fire()
           ├─ PoolManager.Get("Game/Prefabs/Bullet")
           └─ BulletBehaviour.Launch(speed, damage)
                  └─ OnTriggerEnter(NPC) → NpcController.TakeDamage()
                         └─ 血量 ≤ 0 → PoolManager.Release(NPC)

按下 ESC（Playing 状态）
    └─ GameManager.PauseGame() → GameState = Paused (timeScale = 0)
           ├─ PauseController → PushPanel(TestPausePanel)
           └─ CrosshairUI     → 显示系统光标

点击"继续游戏" / 再按 ESC
    └─ UIManager.PopPanel() → TestPausePanel.OnPop()
           └─ GameManager.ResumeGame() → GameState = Playing (timeScale = 1)
```

---

## 七、基础设施层验证结果

| 系统 | 验证状态 | 验证方式 |
|------|----------|----------|
| `UIManager.PushPanel / PopPanel` | ✅ | 开始面板、暂停面板均正常推入/弹出 |
| `EventBus.On / Emit` | ✅ | GameStateChangedEvent 驱动 Camera、CrosshairUI、NpcSpawner |
| `GameManager` 状态机 | ✅ | MainMenu → Playing → Paused → Playing 流转正常 |
| `PoolManager.Get / Release` | ✅ | 子弹与 NPC 均通过对象池创建回收 |
| `Singleton<T>` | ✅ | GameManager.Instance 跨脚本访问正常 |

---

## 八、已知局限 / 后续扩展方向

1. **NPC 无寻路**：当前仅随机游走，后续可接入 NavMesh 或 A*
2. **无伤害反馈**：击中 NPC 无特效/声音，可接入 VFX 和 AudioManager
3. **无分数/计分**：可通过 EventBus 发出 `NpcDiedEvent`，接入 HUD 计分
4. **无玩家血量**：HealthComponent 已存在于基础设施，后续接入
5. **NPC 不攻击玩家**：可在 NpcController 中添加追踪 + 攻击逻辑
6. **子弹无消耗特效**：命中点可添加对象池管理的粒子效果

---

*文档版本：v1.0 · 2026-04-22*
