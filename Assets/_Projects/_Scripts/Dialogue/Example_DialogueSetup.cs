using UnityEngine;

/// <summary>
/// 对话系统使用示例
/// 这个脚本展示了如何创建和配置对话数据
/// </summary>
public class Example_DialogueSetup : MonoBehaviour
{
    [Header("示例：如何在代码中创建对话")]
    public DialogueData_SO exampleDialogue;

    // 这只是示例代码，展示对话数据的结构
    // 实际使用时，应该在 Unity Editor 中通过 ScriptableObject 创建对话数据
    
    void ShowExampleStructure()
    {
        // ====== 示例 1: 简单的 NPC 对话链（使用 nextID） ======
        // 
        // Dialogue Piece 1:
        // - id: "npc_greeting"
        // - speakerType: NPC
        // - speakerName: "商人"
        // - image: [NPC头像Sprite]
        // - text: "你好，欢迎来到我的商店！"
        // - nextID: "npc_ask"
        // - options: [] (空)
        //
        // 玩家看到对话后，文本完整显示，会出现"按 I 进行下一步对话"
        // 按 I 后会自动跳转到 "npc_ask"
        
        // ====== 示例 2: 带选项的对话 ======
        //
        // Dialogue Piece 2:
        // - id: "npc_ask"
        // - speakerType: NPC
        // - speakerName: "商人"
        // - image: [NPC头像Sprite]
        // - text: "你想买点什么？"
        // - nextID: "" (可以留空，因为有选项)
        // - options:
        //     [0] text: "我想买武器", targetID: "buy_weapon"
        //     [1] text: "我想买药水", targetID: "buy_potion"
        //     [2] text: "只是看看", targetID: "just_looking"
        //     [3] text: "离开", targetID: "" (空表示结束对话)
        //
        // 文本显示完成后，会显示选项面板
        // 玩家可以用 W/S 选择，或按 1/2/3/4，或用鼠标点击
        
        // ====== 示例 3: 玩家回应 ======
        //
        // Dialogue Piece 3:
        // - id: "buy_weapon"
        // - speakerType: Player
        // - speakerName: "冒险者"
        // - image: [Player头像Sprite]
        // - text: "给我看看你最好的武器。"
        // - nextID: "show_weapon"
        // - options: []
        //
        // 这时会显示 PlayerPanel（左侧），隐藏 NPCPanel
        
        // ====== 示例 4: 对话结束 ======
        //
        // Dialogue Piece 4:
        // - id: "just_looking"
        // - speakerType: Player
        // - speakerName: "冒险者"
        // - image: [Player头像Sprite]
        // - text: "我只是随便看看。"
        // - nextID: "" (空)
        // - options: []
        //
        // 因为 nextID 为空且没有选项，对话会停在这里
        // 玩家可以按 I 键关闭对话（走出触发器范围也会关闭）
    }
    
    /*
     * ===== 在 Unity Editor 中的设置步骤 =====
     * 
     * 1. 创建对话数据：
     *    - 右键 Project 窗口 -> Create -> Dialogue -> Dialogue Data
     *    - 命名为你的对话名称，如 "Merchant_Dialogue"
     * 
     * 2. 配置对话片段：
     *    - 选中创建的 DialogueData_SO
     *    - 在 Inspector 中添加 Dialogue Pieces
     *    - 为每个片段设置：
     *      * ID（唯一标识）
     *      * Speaker Type（NPC 或 Player）
     *      * Speaker Name（说话者名字）
     *      * Image（头像）
     *      * Text（对话内容）
     *      * Next ID（下一段对话的ID，如果没有选项）
     *      * Options（选项列表，如果需要选择）
     * 
     * 3. 设置 DialogueController：
     *    - 在场景中的 NPC 物体上添加 DialogueController 组件 // 还要加上InteractableNPC组件！不然根本检测不到可交互！！
     *    - 添加 Collider（设置为 Trigger）
     *    - 将创建的 DialogueData_SO 拖到 Current Data 字段
     *    - 将玩家的 PlayerInputHandler 拖到 Input 字段
     * 
     * 4. 设置 DialogueUI（应该已经在场景中）：
     *    - 确保所有 UI 元素都已连接：
     *      * NPC Panel（右侧面板）
     *        - NPC Icon
     *        - NPC Name Text
     *      * Player Panel（左侧面板）
     *        - Player Icon
     *        - Player Name Text
     *      * Main Text（对话内容文本）
     *      * Next Hint Panel（提示面板）
     *      * Option Panel（选项面板）
     *      * Option Prefab（选项预制体）
     *    - 设置 Input Handler 引用
     * 
     * 5. 设置 Option Prefab：
     *    - 创建一个 Button UI 对象
     *    - 添加 OptionUI 组件
     *    - 设置：
     *      * Option Text（TextMeshProUGUI）
     *      * Background Image（用于高亮）
     *      * Normal Color（正常颜色）
     *      * Highlight Color（高亮颜色）
     *    - 保存为 Prefab
     * 
     * ===== 操作说明 =====
     * 
     * 玩家操作：
     * - 走进 NPC 触发范围
     * - 按 I 键开启对话
     * - 如果文本在打字中，按 I 跳过打字效果
     * - 如果显示了选项：
     *   * W/S 键上下选择
     *   * 1/2/3/4 直接选择对应选项
     *   * 鼠标点击选项
     *   * I 键确认当前选中的选项
     * - 如果没有选项但有下一段对话：
     *   * 按 I 进入下一段
     * - 走出触发范围会关闭对话
     */
}

