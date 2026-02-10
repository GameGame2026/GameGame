# 动画配置指南

本文档给出在本工程中配置角色（Player）与敌人（Enemy）动画的详细步骤、参数命名约定、动画事件（Animation Events）及与代码交互的函数签名示例。目的是让动画师/程序员能快速把动画资源挂到 Animator 上，并通过事件触发攻击、受击和伤害判断。

请先确认：
- 项目中 `EnemyBase.cs` 已使用的动画参数（在代码中有哈希化）：
  - Speed (float)
  - Attack (Trigger)
  - Hit (Trigger)
  - Dead (Trigger)

如果你的 Player/Enemy 脚本中存在其它参数，也请同步到 Animator 中。

---

## 一、Animator Controller 结构建议

1. State 组织
   - Base Layer（主层）
     - Idle
     - Walk / Run
     - Attack (可以使用多条 Attack 子状态或通过参数切换多个 Attack 动画)
     - Hit
     - Dead
     - Stunned / Friendly（如果需要）
   - Optional Layers
     - UpperBody（用于只播放上半身攻击动画，不影响下身移动）

2. 参数（在 Animator 窗口的 Parameters 面板添加）
   - Speed (float) —— 用于控制行走/跑步混合
   - Attack (Trigger) —— 普通攻击触发（如果你需要按序列播放 0,1,2,3，可用 Int 或 Trigger + 子状态）
   - AttackIndex (int) [可选] —— 如果你有 4 段攻击动画（Attack_0 .. Attack_3），可以在触发前设置这个参数，再触发 Attack
   - Hit (Trigger)
   - Dead (Trigger)
   - IsGrounded (bool) [可选，处理下落/坡面]

3. 过渡设置
   - 一般使用 Has Exit Time = false 来立即进入攻击/受击状态，且将过渡时间设短（例如 0.05）以免动画卡顿。
   - 对于受击/死亡等中断性状态，勾选 Interrupt Source（允许运行动画中断）或用层级优先级实现强制切换。

---

## 二、Attack 动画：4 段连招（0,1,2,3）的推荐做法

方案 A（较简单，代码控制序号）：
- 参数：
  - AttackIndex (int)
  - Attack (Trigger)
- 工作流：
  1. 在脚本里选择要播放的段数（0..3），先设置 Animator.SetInteger("AttackIndex", idx);
  2. 然后调用 Animator.SetTrigger("Attack");
  3. 在 Animator 中，用一个子状态机或 Blend Tree，根据 AttackIndex 选择对应的动画（Attack_0 .. Attack_3）。

方案 B（使用多个 Trigger）：
- 参数：Attack0 (Trigger), Attack1, Attack2, Attack3
- 脚本根据连段调用对应触发。优点是直观，缺点是参数多。

注：动画之间的连贯性可以由动画事件或脚本回调控制（例如在 Attack_0 的最后触发一个事件允许接下段攻击）。

---

## 三、禁用输入与攻击判定（Animation Events）

你提到“在攻击动画中，暂时不允许输入，动画结束后才允许输入”，并且你计划在 Animation 里做物理判断。推荐如下约定：

1. 在动画里添加 Animation Events（在 Unity 的 Animation 窗口中，时间轴上右键 -> Add Event）：
   - OnAttackBegin
   - OnAttackHit (可以携带参数或通过 Animator 的 AttackIndex 来区分)
   - OnAttackEnd

2. 在脚本里实现对应函数（函数签名示例，挂在 Animator 所在的 GameObject 上）：
   - public void OnAttackBegin()
     - 作用：设置一个局部状态 isInAttack = true，禁用玩家输入（或通知 InputController）。也可用于开启根运动或切换物理模式。
   - public void OnAttackHit()
     - 作用：在此处做检测（Physics.OverlapSphere / Raycast）或通知其他脚本触发伤害事件。例如调用 PlayerAttackHitTrigger()，由你在动画里自行决定击中时机。
   - public void OnAttackEnd()
     - 作用：isInAttack = false，重新允许输入，结束攻击连段计时器等。

示例函数签名（可直接作为 Animation Event 的目标）：
- void OnAttackBegin()
- void OnAttackHit()
- void OnAttackEnd()

提示：Animation Event 调用函数时只能传入 string/float/int/Object，若需传递更复杂信息，可通过 Animator.GetInteger("AttackIndex") 在 OnAttackHit 里读取。

---

## 四、受击与敌人/玩家的伤害触发（Animation Events）

建议统一使用以下 Animation Events 来触发受击/扣血：
- OnDamageTrigger
- OnDamageEnd [可选]

函数签名建议（挂在受击对象对应脚本上）：
- public void OnDamageTrigger()
  - 由 Animation Event 调用，内部调用 PlayerStats.TakeDamage(damage) 或 EnemyBase.TakeDamage(damage)；或者调用发起方（攻击者）的 OnDealDamageTrigger()，由其执行物理判断和目标扣血。

示例：对于 Player 的攻击动画，在攻击帧添加 OnDamageTrigger；该事件可以直接调用 player 脚本里暴露的公共方法：
- public void DealDamageEvent()
  - 作用：查找碰撞体（例如 Physics.OverlapSphere）并对每个 Enemy 调用 enemy.TakeDamage(playerAttackDamage);

对 Enemy 的攻击动画同理，添加 OnDamageTrigger，事件回调里对 Player 调用 TakeDamage。

---

## 五、Point（贴点）对象的使用说明

你提到不再 Instantiate 而使用 SetActive(true/false) 并且这是一个名为 `Point` 的 GameObject。建议：

1. 在场景或预制体中准备一个 `Point` GameObject（作为子对象或独立管理的对象池），初始 SetActive(false)。
2. 在需要 "贴点" 的时候，通过引用直接 SetActive(true)，并把 `Point.transform.position` 设为要贴的位置；回收时 SetActive(false)。

在动画中你可以在需要显示 Point 的帧使用 Animation Event 调用：
- public void ShowPoint() => pointGameObject.SetActive(true);
- public void HidePoint() => pointGameObject.SetActive(false);

确保在 Inspector 中拖入 `Point` 的引用（避免在运行时使用 Find）。

---

## 六、Attack 冷却（Cooldown）配置

你要求把攻击冷却时间放在 config 中。当前 `EnemyBase.cs` 中已有 `public float AttackCooldown = 1.5f;`。

建议：
- 对 Player 也添加类似字段（例如 PlayerController 或 PlayerStats）：public float attackCooldown = 0.5f;
- 在逻辑中维护一个计时器 lastAttackTime 或 cooldownTimer，在尝试发起攻击时判断是否到冷却时间：
  - if (Time.time - lastAttackTime < attackCooldown) return;
  - lastAttackTime = Time.time;

注意：如果动画里自行控制能否出击（例如只在动画事件中调用实际伤害），同样需要在发起攻击入口处检查冷却。

---

## 七、动画混合�� Root Motion

- Root Motion（勾选 Animator 的 Apply Root Motion）
  - 如果你的动画包含位移（例如向前冲锋），考虑启用 Root Motion 并在脚本里控制 Rigidbody 的速度/位置同步。
  - 如果使用 NavMeshAgent 或手写移动系统，通常禁用 Root Motion，由代码驱动位移并用动画只控制视觉。

- 动画混合（Blend Trees）
  - 对于移动使用 Speed 参数控制 Idle/Walk/Run 的混合。

---

## 八、建议的 Animation Event 与脚本接口清单（一览）

针对 Player（示例）：
- OnAttackBegin() — 禁用输入、开始攻击动作
- OnAttackHit() — 检查命中并调用 DealDamageEvent()
- OnAttackEnd() — 允许输入
- DealDamageEvent() — 物理检测并对被击中敌人调用 TakeDamage(float damage)

针对 Enemy（示例）：
- OnAttackBegin() — 可选：播放音效/特效
- OnDamageTrigger() — 在攻击动画命中帧调用，检测 Player 并调用 player.TakeDamage(attackDamage)

EnemyBase 中已有函数可直接调用：
- public virtual void TakeDamage(float damage)
- public virtual void PerformAttack()

---

## 九、调试技巧

- 在 Animator 窗口中开启 Parameters 面板，运行时手动更改参数以观察状态切换。
- 在 Animation 窗口里把关键帧的事件位置标注清楚（例如攻击判定帧），便于程序与动画师对齐。
- 使用 Gizmos 或在场景中绘制 Physics.OverlapSphere 的范围，调试命中检测。
- 把 `Point` 对象在 Scene 视图中保持可见，并用不同颜色表示激活状态。

---

## 十、常见问题与解决方案

- 问：攻击动画播放但并未触发伤害？
  - 检查 Animation Event 是否挂在正确的动画片段且事件指向的函数在对应的 GameObject 上（通常是挂有 Animator 的根节点）。
  - 检查 DealDamageEvent 内的物理层（LayerMask）和碰撞体是否正确设置。

- 问：输入没有在攻击结束后恢复？
  - 检查 OnAttackEnd 是否确实被调用；在 Animation Event 中确认事件名称和拼写。

- 问：AttackIndex 无效，始终播放同一段？
  - 确保在触发 Attack 之前正确设置 Animator.SetInteger("AttackIndex", idx) 并在 Animator 中用该参数做分支。

---

## 附录：推荐的参数与事件命名汇总

Parameters:
- Speed (float)
- Attack (Trigger)
- AttackIndex (int) [可选]
- Hit (Trigger)
- Dead (Trigger)
- IsGrounded (bool) [可选]

Animation Events (函数名示例):
- OnAttackBegin
- OnAttackHit
- OnAttackEnd
- OnDamageTrigger
- ShowPoint
- HidePoint

---

如果你希望，我可以：
- 帮你生成演示用的空函数模板（C#）并放入合适的脚本文件，或
- 将此文档集成到项目的 README 中，或
- 根据你现有的 Player 脚本和 EnemyBase 自动生成推荐的 Animation Event 函数签名。

告诉我你更希望哪个，我会继续按你选择执行。
