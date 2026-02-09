using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData_SO : ScriptableObject
{
    public List<DialoguePiece> dialoguePieces = new List<DialoguePiece>();
    public Dictionary<string,DialoguePiece> dialogueIndex = new Dictionary<string, DialoguePiece>();

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
            if (piece.speakerType == SpeakerType.Player)
            {
                piece.speakerName = "爱丽丝";
            }
        }
    }
    #endif
    
}
