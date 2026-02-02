using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OptionUI : MonoBehaviour
{
    public TextMeshProUGUI optionText;
    public Image backgroundImage;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    
    private Button thisButton;
    private DialoguePiece currentPiece;
    private string nextPieceID;

    private void Awake()
    {
        thisButton = GetComponent<Button>();
        if (thisButton != null)
        {
            thisButton.onClick.AddListener(OnOptionClicked);
        }
        
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }
    }

    public void UpdateOption(DialoguePiece piece, DialogueOption option)
    {
        currentPiece = piece;
        optionText.text = option.text;
        nextPieceID = option.targetID;
        
        // Reset to normal state
        SetHighlight(false);
    }

    public void SetHighlight(bool highlight)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = highlight ? highlightColor : normalColor;
        }
        
        if (optionText != null)
        {
            optionText.fontStyle = highlight ? FontStyles.Bold : FontStyles.Normal;
        }
    }

    public void TriggerOption()
    {
        OnOptionClicked();
    }

    private void OnOptionClicked()
    {
        if (string.IsNullOrEmpty(nextPieceID))
        {
            DialogueUI.Instance.dialoguePanel.SetActive(false);
        }
        else if (DialogueUI.Instance.currentData.dialogueIndex.ContainsKey(nextPieceID))
        {
            DialogueUI.Instance.UpdateMainDialogue(DialogueUI.Instance.currentData.dialogueIndex[nextPieceID]);
        }
        else
        {
            Debug.LogWarning($"Dialogue piece with ID '{nextPieceID}' not found!");
            DialogueUI.Instance.dialoguePanel.SetActive(false);
        }
    }
    
}
