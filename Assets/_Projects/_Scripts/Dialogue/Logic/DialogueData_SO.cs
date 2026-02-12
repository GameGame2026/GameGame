using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData_SO : ScriptableObject
{
    public List<DialoguePiece> dialoguePieces = new List<DialoguePiece>();
    public Dictionary<string,DialoguePiece> dialogueIndex = new Dictionary<string, DialoguePiece>();

    // 运行时初始化字典
    public void InitializeDialogue()
    {
        dialogueIndex.Clear();
        foreach (var piece in dialoguePieces)
        {
            if (!string.IsNullOrEmpty(piece.id) && !dialogueIndex.ContainsKey(piece.id))
            {
                dialogueIndex.Add(piece.id, piece);
            }
            else if (string.IsNullOrEmpty(piece.id))
            {
                Debug.LogWarning($"对话片段缺少 ID: {piece.text}");
            }
            else if (dialogueIndex.ContainsKey(piece.id))
            {
                Debug.LogWarning($"重复的对话 ID: {piece.id}");
            }
        }
        Debug.Log($"对话数据初始化完成，共 {dialogueIndex.Count} 个对话片段");
    }

    #if UNITY_EDITOR
    private void OnValidate()
    {
        dialogueIndex.Clear();
        foreach (var piece in dialoguePieces)
        {
            if (!dialogueIndex.ContainsKey(piece.id))
            {
                dialogueIndex.Add(piece.id, piece);
            }

            //尝试偷偷加一段
            if (piece.speakerType == SpeakerType.Player && string.IsNullOrEmpty(piece.speakerName) )
            {
                piece.speakerName = "爱丽丝";
            }
        }
    }
    #endif
    
}
