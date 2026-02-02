using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GamePlay.Controller;

public class DialogueUI : Singleton<DialogueUI>
{
    [Header("Basic Elements")] 
    public Image NPCIcon;
    public GameObject NPCPanel;

    public Image PlayerIcon;
    public GameObject PlayerPanel;

    public TextMeshProUGUI NPCNameText;
    public TextMeshProUGUI PlayerNameText;
    public TextMeshProUGUI mainText;
    public Button nextButton;
    
    public GameObject dialoguePanel;
    
    [Header("Next Hint")]
    public GameObject nextHintPanel;
    public TextMeshProUGUI nextHintText;
    
    [Header("Options")]
    public RectTransform optionPanel;
    public OptionUI optionPrefab;

    [Header("Typewriter Settings")]
    public float typewriterSpeed = 0.05f;

    [Header("Data")] 
    public DialogueData_SO currentData;
    public PlayerInputHandler inputHandler;

    private DialoguePiece currentPiece;
    private bool isTyping;
    private bool textFullyDisplayed;
    private Coroutine typewriterCoroutine;
    
    private List<OptionUI> currentOptions = new List<OptionUI>();
    private int selectedOptionIndex = -1;

    protected override void Awake()
    {
        base.Awake();
        nextButton.onClick.AddListener(ContinueDialogue);
        
        if (nextHintPanel != null)
            nextHintPanel.SetActive(false);
        if (optionPanel != null)
            optionPanel.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!dialoguePanel.activeSelf)
            return;

        // Handle interact input (I key)
        if (inputHandler != null && inputHandler.InteractInput)
        {
            if (isTyping)
            {
                // Skip typewriter effect
                SkipTypewriter();
            }
            else if (currentOptions.Count > 0)
            {
                // Trigger selected option
                if (selectedOptionIndex >= 0 && selectedOptionIndex < currentOptions.Count)
                {
                    currentOptions[selectedOptionIndex].TriggerOption();
                }
            }
            else if (textFullyDisplayed && !string.IsNullOrEmpty(currentPiece?.nextID))
            {
                // Continue to next dialogue
                ContinueDialogue();
            }
        }

        // Handle option selection with WS keys
        if (currentOptions.Count > 0 && !isTyping)
        {
            HandleOptionSelection();
        }
    }

    private void HandleOptionSelection()
    {
        // WS navigation
        if (Input.GetKeyDown(KeyCode.W))
        {
            SelectPreviousOption();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            SelectNextOption();
        }
        
        // Number key selection (1-4)
        for (int i = 0; i < Mathf.Min(4, currentOptions.Count); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectOption(i);
                currentOptions[i].TriggerOption();
            }
        }
    }

    private void SelectOption(int index)
    {
        if (index < 0 || index >= currentOptions.Count)
            return;

        // Deselect previous
        if (selectedOptionIndex >= 0 && selectedOptionIndex < currentOptions.Count)
        {
            currentOptions[selectedOptionIndex].SetHighlight(false);
        }

        // Select new
        selectedOptionIndex = index;
        currentOptions[selectedOptionIndex].SetHighlight(true);
        
        // Set EventSystem selection for UI navigation
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(currentOptions[selectedOptionIndex].gameObject);
        }
    }

    private void SelectNextOption()
    {
        if (currentOptions.Count == 0)
            return;

        int newIndex = (selectedOptionIndex + 1) % currentOptions.Count;
        SelectOption(newIndex);
    }

    private void SelectPreviousOption()
    {
        if (currentOptions.Count == 0)
            return;

        int newIndex = selectedOptionIndex - 1;
        if (newIndex < 0)
            newIndex = currentOptions.Count - 1;
        SelectOption(newIndex);
    }

    private void ContinueDialogue()
    {
        if (!textFullyDisplayed)
            return;

        if (currentPiece != null && !string.IsNullOrEmpty(currentPiece.nextID))
        {
            if (currentData.dialogueIndex.ContainsKey(currentPiece.nextID))
            {
                UpdateMainDialogue(currentData.dialogueIndex[currentPiece.nextID]);
            }
            else
            {
                // End dialogue if nextID not found
                dialoguePanel.SetActive(false);
            }
        }
        else
        {
            // End dialogue if no nextID
            dialoguePanel.SetActive(false);
        }
    }

    public void UpdateDialogueData(DialogueData_SO data)
    {
        currentData = data;
    }

    public void UpdateMainDialogue(DialoguePiece piece)
    {
        dialoguePanel.SetActive(true);
        currentPiece = piece;
        textFullyDisplayed = false;
        
        // Stop any existing typewriter
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }
        
        // Clear current options
        ClearOptions();
        
        // Update speaker UI based on speaker type
        UpdateSpeakerUI(piece);
        
        // Start typewriter effect
        typewriterCoroutine = StartCoroutine(TypewriterEffect(piece.text));
    }
    
    private void UpdateSpeakerUI(DialoguePiece piece)
    {
        if (piece.speakerType == SpeakerType.NPC)
        {
            // Show NPC panel, hide Player panel
            if (NPCPanel != null)
                NPCPanel.SetActive(true);
            if (PlayerPanel != null)
                PlayerPanel.SetActive(false);
            
            // Set NPC icon and name
            if (piece.image != null && NPCIcon != null)
            {
                NPCIcon.enabled = true;
                NPCIcon.sprite = piece.image;
            }
            else if (NPCIcon != null)
            {
                NPCIcon.enabled = false;
            }
            
            if (NPCNameText != null)
            {
                NPCNameText.text = piece.speakerName;
            }
        }
        else // Player
        {
            // Show Player panel, hide NPC panel
            if (PlayerPanel != null)
                PlayerPanel.SetActive(true);
            if (NPCPanel != null)
                NPCPanel.SetActive(false);
            
            // Set Player icon and name
            if (piece.image != null && PlayerIcon != null)
            {
                PlayerIcon.enabled = true;
                PlayerIcon.sprite = piece.image;
            }
            else if (PlayerIcon != null)
            {
                PlayerIcon.enabled = false;
            }
            
            if (PlayerNameText != null)
            {
                PlayerNameText.text = piece.speakerName;
            }
        }
    }
    
    private IEnumerator TypewriterEffect(string fullText)
    {
        isTyping = true;
        mainText.text = "";
        
        foreach (char c in fullText)
        {
            mainText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }
        
        isTyping = false;
        textFullyDisplayed = true;
        OnTextFullyDisplayed();
    }
    
    private void SkipTypewriter()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
        
        isTyping = false;
        mainText.text = currentPiece.text;
        textFullyDisplayed = true;
        OnTextFullyDisplayed();
    }
    
    private void OnTextFullyDisplayed()
    {
        // Show options if available
        if (currentPiece.options != null && currentPiece.options.Count > 0)
        {
            CreateOptions(currentPiece);
            ShowNextHint(false);
            
            // Default: no option selected
            selectedOptionIndex = -1;
        }
        else if (!string.IsNullOrEmpty(currentPiece.nextID))
        {
            // Show next hint if there's a next dialogue
            ShowNextHint(true);
        }
        else
        {
            // No options and no next, dialogue will end
            ShowNextHint(false);
        }
    }
    
    private void ShowNextHint(bool show)
    {
        if (nextHintPanel != null)
        {
            nextHintPanel.SetActive(show);
        }
        
        if (show && nextHintText != null)
        {
            nextHintText.text = "按 I 进行下一步对话";
        }
    }

    private void CreateOptions(DialoguePiece piece)
    {
        ClearOptions();
        
        if (piece.options == null || piece.options.Count == 0)
        {
            if (optionPanel != null)
                optionPanel.gameObject.SetActive(false);
            return;
        }
        
        // Show option panel
        if (optionPanel != null)
            optionPanel.gameObject.SetActive(true);
        
        // Create option buttons
        for (int i = 0; i < piece.options.Count; i++)
        {
            var option = Instantiate(optionPrefab, optionPanel);
            option.UpdateOption(piece, piece.options[i]);
            currentOptions.Add(option);
        }
    }
    
    private void ClearOptions()
    {
        // Clear option list
        currentOptions.Clear();
        selectedOptionIndex = -1;
        
        // Destroy existing option GameObjects
        if (optionPanel != null && optionPanel.childCount > 0)
        {
            for (int i = optionPanel.childCount - 1; i >= 0; i--)
            {
                Destroy(optionPanel.GetChild(i).gameObject);
            }
        }
    }
}
