# 快速使用指南 - PlayerControllerConfig

## 立即开始使用新的配置系统

### 步骤 1：创建配置文件（在 Unity 编辑器中）

1. 在 Project 窗口中右键点击
2. 选择 `Create > GamePlay > Player Controller Config`
3. 命名为 `DefaultPlayerConfig`

### 步骤 2：配置参数

选中刚创建的配置文件，在 Inspector 中设置：

```
【移动设置】
Move Speed: 2.0
Sprint Speed: 5.335
Rotation Smooth Time: 0.12
Speed Change Rate: 10.0

【跳跃设置】
Jump Height: 1.2
Gravity: -15.0
Jump Timeout: 0.5
Fall Timeout: 0.15

【地面检测】
Grounded Offset: -0.14
Grounded Radius: 0.28
Ground Layers: (选择 Ground 层)

【相机设置】
Top Clamp: 70
Bottom Clamp: -30
Camera Angle Override: 0
Lock Camera Position: false

【音效设置】
Landing Audio Clip: (拖入落地音效)
Footstep Audio Clips: (拖入脚步音效数组)
Footstep Audio Volume: 0.5
```

### 步骤 3：配置角色

1. 选择场景中的玩家对象
2. 找到 `Third Person Controller` 组件
3. 将 `DefaultPlayerConfig` 拖入 `Config` 字段

### 完成！

现在你的角色已经使用新的配置系统了。

## 创建角色变体

### 快速角色
复制 `DefaultPlayerConfig`，命名为 `FastPlayerConfig`：
- Move Speed: **3.0** ⬆️
- Sprint Speed: **7.0** ⬆️
- Jump Height: **1.5** ⬆️
- Rotation Smooth Time: **0.08** ⬇️

### 重型角色
复制 `DefaultPlayerConfig`，命名为 `HeavyPlayerConfig`：
- Move Speed: **1.5** ⬇️
- Sprint Speed: **3.0** ⬇️
- Jump Height: **0.8** ⬇️
- Gravity: **-20.0** ⬇️
- Rotation Smooth Time: **0.2** ⬆️

### 月球重力
复制 `DefaultPlayerConfig`，命名为 `LowGravityConfig`：
- Jump Height: **3.0** ⬆️
- Gravity: **-5.0** ⬆️
- Fall Timeout: **0.5** ⬆️

## 运行时切换配置

```csharp
// 在代码中动态切换配置
public PlayerControllerConfig fastConfig;
public PlayerControllerConfig heavyConfig;

void SwitchToFastMode()
{
    GetComponent<ThirdPersonController>().config = fastConfig;
}

void SwitchToHeavyMode()
{
    GetComponent<ThirdPersonController>().config = heavyConfig;
}
```

## 常见问题

**Q: 为什么角色不动了？**
A: 检查是否设置了 `config` 引用。如果 config 为空，会导致空引用错误。

**Q: 如何快速测试不同配置？**
A: 在运行时，直接在 Inspector 中更改 ThirdPersonController 的 config 引用即可。

**Q: 配置文件存放在哪里？**
A: 建议在 `Assets/_Projects/Configs/PlayerController/` 目录下统一管理。

**Q: 能否在运行时修改配置值？**
A: 可以，但不推荐（ScriptableObject 是共享的）。建议创建新的配置文件。

## 提示

💡 使用配置文件可以快速平衡游戏，无需重新编译代码。
💡 为不同关卡创建不同配置，增加游戏多样性。
💡 配置文件支持版本控制，方便团队协作。

开始享受数据驱动的开发体验吧！ 🚀

