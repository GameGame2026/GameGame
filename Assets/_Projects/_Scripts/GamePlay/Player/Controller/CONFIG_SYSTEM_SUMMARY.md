# ScriptableObject 配置文件系统重构

## 完成时间
2026年2月2日

## 重构目标
将 ThirdPersonController 的所有配置参数提取到 ScriptableObject 中，实现数据与逻辑分离。

## 新增文件

### PlayerControllerConfig.cs
**位置：** `Assets\_Projects\_Scripts\GamePlay\Player\Controller\PlayerControllerConfig.cs`

**类型：** ScriptableObject 配置文件

**功能：** 存储所有角色控制器的可配置参数

**包含的配置项：**

#### 移动设置
- `moveSpeed` - 移动速度（默认：2.0）
- `sprintSpeed` - 冲刺速度（默认：5.335）
- `rotationSmoothTime` - 转向速度（默认：0.12）
- `speedChangeRate` - 加速/减速速率（默认：10.0）

#### 跳跃设置
- `jumpHeight` - 跳跃高度（默认：1.2）
- `gravity` - 重力值（默认：-15.0）
- `jumpTimeout` - 跳跃冷却（默认：0.5）
- `fallTimeout` - 下落冷却（默认：0.15）

#### 地面检测
- `groundedOffset` - 地面检测偏移（默认：-0.14）
- `groundedRadius` - 地面检测半径（默认：0.28）
- `groundLayers` - 地面层级

#### 相机设置
- `topClamp` - 相机上仰最大角度（默认：70）
- `bottomClamp` - 相机下俯最大角度（默认：-30）
- `cameraAngleOverride` - 相机角度覆盖（默认：0）
- `lockCameraPosition` - 锁定相机（默认：false）

#### 音效设置
- `landingAudioClip` - 落地音效
- `footstepAudioClips` - 脚步音效数组
- `footstepAudioVolume` - 脚步音量（默认：0.5）

## 修改的文件

### ThirdPersonController.cs
**改动：**
- ❌ 移除了所有配置字段（60+ 行代码删除）
- ✅ 添加了 `PlayerControllerConfig config` 引用
- ✅ 所有方法都通过 `config.参数名` 访问配置
- ✅ 添加了配置空检查（OnDrawGizmosSelected）

## 使用方法

### 1. 创建配置文件
在 Unity 中：
1. 右键 Project 窗口
2. 选择 `Create > GamePlay > Player Controller Config`
3. 命名配置文件（例如：`DefaultPlayerConfig`）
4. 在 Inspector 中配置各项参数

### 2. 配置角色控制器
1. 选择玩家对象
2. 在 ThirdPersonController 组件中
3. 将创建的配置文件拖入 `Config` 字段

### 3. 创建多种配置
可以为不同角色/场景创建不同配置：
- `FastPlayerConfig` - 快速移动配置
- `HeavyPlayerConfig` - 重型角色配置
- `LowGravityConfig` - 低重力配置
等等...

## 优势

### 1. 数据与逻辑分离
- **配置文件**：纯数据，可视化编辑
- **控制器**：纯逻辑，不包含魔法数字

### 2. 可复用性强
- 一个配置文件可被多个角色共享
- 轻松创建角色变体（快速版、重型版等）
- 无需修改代码即可调整参数

### 3. 便于平衡调整
- 设计师可以独立调整参数
- 快速切换不同配置进行对比测试
- 配置文件可以版本控制

### 4. 易于扩展
添加新配置只需：
```csharp
// 1. 在 PlayerControllerConfig 中添加字段
public float newParameter = 1.0f;

// 2. 在 ThirdPersonController 中使用
float value = config.newParameter;
```

### 5. 减少组件臃肿
- ThirdPersonController Inspector 界面更简洁
- 只显示运行时状态和必要引用
- 配置参数都在独立的 ScriptableObject 中

## 配置示例

### 快速角色配置
```
moveSpeed: 3.0
sprintSpeed: 7.0
jumpHeight: 1.5
gravity: -12.0
rotationSmoothTime: 0.08
```

### 重型角色配置
```
moveSpeed: 1.5
sprintSpeed: 3.0
jumpHeight: 0.8
gravity: -20.0
rotationSmoothTime: 0.2
```

### 低重力配置
```
moveSpeed: 2.0
sprintSpeed: 5.0
jumpHeight: 3.0
gravity: -5.0
fallTimeout: 0.5
```

## 迁移指南

### 对于现有场景：
1. 创建一个 `DefaultPlayerConfig` 配置文件
2. 将原 ThirdPersonController 的参数值复制到配置文件
3. 在 ThirdPersonController 中引用新配置文件
4. 原参数值会自动失效（已从代码中移除）

### 注意事项
⚠️ **重要：** 所有使用 ThirdPersonController 的场景都需要设置 `config` 引用，否则会出现空引用错误。

## 文件清单

### 新增：
- ✅ `PlayerControllerConfig.cs` - 配置文件类

### 修改：
- ✅ `ThirdPersonController.cs` - 使用配置文件

### 代码统计：
- 删除代码：约 60 行（配置字段）
- 新增代码：约 75 行（ScriptableObject）
- 净增加：约 15 行
- 可维护性：大幅提升 ⬆️

## 编译状态
✅ 所有文件编译通过
✅ 仅有命名空间风格警告（不影响功能）

## 下一步建议
1. 为不同场景/角色创建专属配置
2. 考虑添加配置预设切换功能
3. 可以添加配置文件的验证逻辑
4. 考虑为配置添加编辑器脚本（自定义 Inspector）

重构完成！🎉

