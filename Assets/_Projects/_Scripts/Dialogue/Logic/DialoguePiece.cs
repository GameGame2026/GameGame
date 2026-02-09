using System.Collections.Generic;
using UnityEngine;

public enum SpeakerType
{
    NPC,
    Player
}

[System.Serializable]
public class DialoguePiece
{
    public string id;
    public SpeakerType speakerType;
    public string speakerName;
    public Sprite image;
    [TextArea]
    public string text;
    public string nextID; // 自动跳转到下一段对话（无需选项）
    public List<DialogueOption> options = new List<DialogueOption>();
    
    [Header("结束标记")]
    [Tooltip("勾选此项表示这是最后一段对话，将显示'按 I 关闭对话'提示")]
    public bool isLastPiece = false; // 是否为最后一段对话

}
