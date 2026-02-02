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
    public string nextID; // For automatic next dialogue without options
    public List<DialogueOption> options = new List<DialogueOption>();
}
