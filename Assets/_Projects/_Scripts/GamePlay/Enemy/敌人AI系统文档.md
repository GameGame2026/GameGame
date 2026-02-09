# 敌人AI系统 - 完整文档

## 📁 文件结构

```
Assets/_Projects/_Scripts/GamePlay/Enemy/
├── EnemyBase.cs              # 敌人基类
├── EnemyStateMachine.cs      # 状态机系统
├── Enemy.cs                  # 简单敌人类（旧版）
├── EnemyAttackTrigger.cs     # 攻击触发器
└── Enemies/
    ├── HeartPokerEnemy.cs    # 红心扑克
    ├── ClubPokerEnemy.cs     # 梅花扑克
    ├── GhostEnemy.cs         # 鬼魂敌人
    ├── TeapotEnemy.cs        # 茶壶敌人
    └── TeacupEnemy.cs        # 茶杯敌人
```

---

## 🎮 状态机系统

### 状态类型 (EnemyStateType)
| 状态 | 说明 |
|------|------|
| Idle | 待机状态，随机时间后进入巡逻 |
| Patrol | 巡逻状态，在出生点附近移动 |
| Alert | 警戒状态，发现玩家后短暂停顿 |
| Chase | 追击状态，追向玩家 |
| Attack | 攻击状态，对玩家造成伤害 |
| Stunned | 眩晕/被贴点状态 |
| Dead | 死亡状态 |
| Friendly | 友方状态（茶杯变身后） |

### 状态流转
```
Idle → Patrol → Alert → Chase → Attack
           ↑      ↓        ↓
           └──────┴────────┘
                  
任何状态 → Stunned（被贴点）→ Idle（点被回收）
任何状态 → Dead（血量为0）
Stunned → Friendly（茶杯特殊）
```

---

## 🃏 敌人类型详解

### 1. 红心扑克 (HeartPokerEnemy)
**特殊机制：**
- 贴点后进入「治愈模式」
- 治愈模式下被攻击时，为玩家恢复1点血量
- 点被回收后恢复正常敌对状态

**属性：**
- `healAmount` - 治愈量（默认1）
- `normalMaterial` - 正常态材质
- `healingMaterial` - 治愈态材质

---

### 2. 梅花扑克 (ClubPokerEnemy)
**特殊机制：**
- 攻击力2点
- 贴点后变为半透明，附着到最近的场景物体表面
- 附着后停止移动，但仍可被攻击

**属性：**
- `attachSearchRadius` - 贴附搜索范围
- `attachableLayers` - 可贴附的层
- `transparentAlpha` - 半透明度

---

### 3. 鬼魂敌人 (GhostEnemy)
**特殊机制：**
- 初始状态：半透明、不可被攻击
- 贴点后：变为不透明、可被攻击
- 空中漂浮，无视地面虚实状态

**属性：**
- `floatHeight` - 漂浮高度
- `floatAmplitude` - 漂浮幅度
- `floatFrequency` - 漂浮频率
- `ghostAlpha` - 半透明度

---

### 4. 茶壶敌人 (TeapotEnemy) - 远程
**特殊机制：**
- 远程攻击：吐出抛物线运动的泡泡
- 泡泡可被玩家贴点
- 贴点后泡泡原路返回，命中茶壶造成伤害

**属性：**
- `bubblePrefab` - 泡泡预制体
- `firePoint` - 发射点
- `bubbleForce` - 发射力度
- `bubbleAngle` - 发射角度
- `bubbleDamage` - 泡泡伤害

---

### 5. 茶杯敌人 (TeacupEnemy) - 辅助型
**特殊机制：**
- 贴点前：大范围泼水攻击（2点伤害）
- 贴点后：旋转变身，变为友方跟随单位
- 友方AI：自动攻击进入范围的敌人（1点伤害）
- 点被回收后恢复敌对状态

**属性：**
- `splashRange` - 泼水范围
- `splashAngle` - 泼水角度
- `splashDamage` - 泼水伤害
- `friendlyAttackDamage` - 友方攻击伤害
- `followDistance` - 跟随距离
- `transformSpinSpeed` - 变身旋转速度

---

## 🔧 EnemyBase 基类属性

### 基础属性
| 属性 | 说明 | 默认值 |
|------|------|--------|
| maxHealth | 最大血量 | 100 |
| attackDamage | 攻击力 | 10 |
| AttackCooldown | 攻击冷却 | 1.5s |

### 移动设置
| 属性 | 说明 | 默认值 |
|------|------|--------|
| PatrolSpeed | 巡逻速度 | 2 |
| chaseSpeed | 追击速度 | 4 |
| rotationSpeed | 转向速度 | 5 |

### 检测范围
| 属性 | 说明 | 默认值 |
|------|------|--------|
| detectionRange | 警戒范围 | 8 |
| chaseRange | 追击范围 | 15 |
| attackRange | 攻击范围 | 2 |
| patrolRadius | 巡逻半径 | 10 |

### 音效
| 属性 | 说明 |
|------|------|
| hitSound | 受击音效 |
| attackSound | 攻击音效 |
| deathSound | 死亡音效 |

---

## 💡 使用方法

### 创建新敌人
1. 创建空对象，添加所需组件（Rigidbody, Collider）
2. 添加对应的敌人脚本（如 HeartPokerEnemy）
3. 可选：添加 NavMeshAgent 以使用寻路
4. 配置 Inspector 中的属性
5. 设置子对象的攻击触发器

### 贴点系统接口
```csharp
// 外部调用贴点
enemy.SetPointAttached(true);  // 贴上点
enemy.SetPointAttached(false); // 回收点
```

### 重要说明
- **敌人被贴点不等于死亡**，点被回收后继续攻击
- **友方单位只能在攻击范围内活动**
- 所有敌人都在 `_Projects.GamePlay` 命名空间下

---

## 🎯 Gizmos 显示

在 Scene 视图中选中敌人可以看到：
- 🟡 黄色圆圈 - 警戒范围
- 🟠 橙色圆圈 - 追击范围
- 🔴 红色圆圈 - 攻击范围
- 🟢 绿色圆圈 - 巡逻范围

---

## ✅ 完成状态

- [x] EnemyBase 基类
- [x] EnemyStateMachine 状态机
- [x] 所有基础状态（Idle/Patrol/Alert/Chase/Attack/Stunned/Dead/Friendly）
- [x] HeartPokerEnemy 红心扑克
- [x] ClubPokerEnemy 梅花扑克
- [x] GhostEnemy 鬼魂敌人
- [x] TeapotEnemy 茶壶敌人 + TeapotBubble 泡泡
- [x] TeacupEnemy 茶杯敌人

