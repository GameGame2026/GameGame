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
    private bool lastInteractInput = false;
    
    // 对话框关闭冷却时间
    private float dialogueCloseCooldown = 0f;
    private const float COOLDOWN_TIME = 0.2f;

    // 对话是否激活（包括冷却时间检查）
    public bool IsDialogueActive => (dialoguePanel != null && dialoguePanel.activeSelf) || dialogueCloseCooldown > 0;

    protected override void Awake()
    {
        base.Awake();
        
        // 如果没有手动赋值 inputHandler，尝试自动查找
        if (inputHandler == null)
        {
            inputHandler = FindObjectOfType<PlayerInputHandler>();
            if (inputHandler != null)
            {
                Debug.Log("DialogueUI 自动找到 PlayerInputHandler");
            }
            else
            {
                Debug.LogWarning("DialogueUI 未找到 PlayerInputHandler，对话交互可能无法正常工作！");
            }
        }
        
        if (nextHintPanel != null)
            nextHintPanel.SetActive(false);
        if (optionPanel != null)
            optionPanel.gameObject.SetActive(false);
    }

    private void Update()
    {
        // 更新冷却时间
        if (dialogueCloseCooldown > 0)
        {
            dialogueCloseCooldown -= Time.deltaTime;
        }
        
        // 检查 inputHandler 是否存在
        if (inputHandler == null)
        {
            // 尝试再次查找
            inputHandler = FindObjectOfType<PlayerInputHandler>();
            if (inputHandler == null)
            {
                return; // 没有输入处理器，无法处理输入
            }
        }
        
        // 始终更新 lastInteractInput，即使对话框未激活，防止重新打开
        bool interactPressed = inputHandler.InteractInput;
        
        if (!dialoguePanel.activeSelf)
        {
            lastInteractInput = interactPressed; // 保持状态同步，不要重置
            return;
        }
        

        // 处理交互输入（E 键）- 只在按键按下时触发（边缘检测）
        if (interactPressed && !lastInteractInput)
        {
            
            if (isTyping)
            {
                SkipTypewriter();
            }
            else if (textFullyDisplayed)
            {
                // 文本已完全显示后的处理
                if (currentOptions.Count > 0)
                {
                    if (selectedOptionIndex >= 0 && selectedOptionIndex < currentOptions.Count)
                    {
                        currentOptions[selectedOptionIndex].TriggerOption();
                    }
                }
                else if (currentPiece != null && currentPiece.isLastPiece)
                {
                    CloseDialogue();
                }
                else if (!string.IsNullOrEmpty(currentPiece?.nextID))
                {
                    ContinueDialogue();
                }
                else
                {
                    CloseDialogue();
                }
            }
        }
        lastInteractInput = interactPressed;

        // 使用 WS 键处理选项选择
        if (currentOptions.Count > 0 && !isTyping)
        {
            HandleOptionSelection();
        }
    }
    
    // 新增：统一的关闭对话方法
    private void CloseDialogue()
    {
        dialoguePanel.SetActive(false);
        dialogueCloseCooldown = COOLDOWN_TIME; // 设置冷却时间
    }

    private void HandleOptionSelection()
    {
        // WS 键导航
        if (Input.GetKeyDown(KeyCode.W))
        {
            SelectPreviousOption();
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            SelectNextOption();
        }
        
        // 数字键选择（1-4）
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

        // 取消选中之前的选项
        if (selectedOptionIndex >= 0 && selectedOptionIndex < currentOptions.Count)
        {
            currentOptions[selectedOptionIndex].SetHighlight(false);
        }

        // 选中新的选项
        selectedOptionIndex = index;
        currentOptions[selectedOptionIndex].SetHighlight(true);
        
        // 为 UI 导航设置 EventSystem 选择
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
        {
            return;
        }

        if (currentPiece != null && !string.IsNullOrEmpty(currentPiece.nextID))
        {
            
            if (currentData == null)
            {
                CloseDialogue();
                return;
            }
            
            if (currentData.dialogueIndex == null)
            {
                currentData.InitializeDialogue();
            }
            
            if (currentData.dialogueIndex.ContainsKey(currentPiece.nextID))
            {
                UpdateMainDialogue(currentData.dialogueIndex[currentPiece.nextID]);
            }
            else
            {
                // 如果找不到 nextID，结束对话
                CloseDialogue();
            }
        }
        else
        {
            // 如果没有 nextID，结束对话
            CloseDialogue();
        }
    }

    public void UpdateDialogueData(DialogueData_SO data)
    {
        currentData = data;
        if (currentData != null)
        {
            currentData.InitializeDialogue(); // 运行时初始化对话索引
        }
    }

    public void UpdateMainDialogue(DialoguePiece piece)
    {
        if (piece == null)
        {
            return;
        }
        
        dialoguePanel.SetActive(true);
        currentPiece = piece;
        textFullyDisplayed = false;
        isTyping = false;
        
        // 停止任何现有的打字机效果
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
        
        // 清空文本
        if (mainText != null)
        {
            mainText.text = "";
        }
        
        // 清除当前选项
        ClearOptions();
        
        // 隐藏提示和按钮（等待打字机效果完成后再显示）
        ShowNextHint(false);
        
        // 根据说话者类型更新 UI
        UpdateSpeakerUI(piece);
        
        
        typewriterCoroutine = StartCoroutine(TypewriterEffect(piece.text));
    }
    
    private void UpdateSpeakerUI(DialoguePiece piece)
    {
        if (piece.speakerType == SpeakerType.NPC)
        {
            // 显示 NPC 面板，隐藏玩家面板
            if (NPCPanel != null)
                NPCPanel.SetActive(true);
            if (PlayerPanel != null)
                PlayerPanel.SetActive(false);
            
            // 设置 NPC 图标和名字
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
        else // 玩家
        {
            // 显示玩家面板，隐藏 NPC 面板
            if (PlayerPanel != null)
                PlayerPanel.SetActive(true);
            if (NPCPanel != null)
                NPCPanel.SetActive(false);
            
            // 设置玩家图标和名字
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
        // 优先检查是否为最后一段对话
        if (currentPiece.isLastPiece)
        {
            // 最后一段对话，显示关闭提示
            ShowNextHint(true, "按 E 关闭对话");
            return;
        }
        
        // 如果有选项可选，则显示选项
        if (currentPiece.options != null && currentPiece.options.Count > 0)
        {
            CreateOptions(currentPiece);
            ShowNextHint(false);
            
            // 自动选中第一个选项
            SelectOption(0);
        }
        else if (!string.IsNullOrEmpty(currentPiece.nextID))
        {
            // 如果有下一段对话，显示提示和按钮
            ShowNextHint(true);
        }
        else
        {
            // 没有选项也没有下一段对话，显示关闭对话的提示
            ShowNextHint(true, "按 E 关闭对话");
        }
    }
    
    private void ShowNextHint(bool show, string customText = null)
    {
        if (nextHintPanel != null)
        {
            nextHintPanel.SetActive(show);
        }
        
        if (show && nextHintText != null)
        {
            nextHintText.text = customText ?? "按 E 继续对话";
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
        
        // 显示选项面板
        if (optionPanel != null)
            optionPanel.gameObject.SetActive(true);
        
        // 创建选项按钮
        for (int i = 0; i < piece.options.Count; i++)
        {
            var option = Instantiate(optionPrefab, optionPanel);
            option.UpdateOption(piece, piece.options[i], i + 1); // 传递序号（从1开始）
            currentOptions.Add(option);
        }
    }
    
    private void ClearOptions()
    {
        // 清空选项列表
        currentOptions.Clear();
        selectedOptionIndex = -1;
        
        // 销毁现有的选项 GameObject
        if (optionPanel != null && optionPanel.childCount > 0)
        {
            for (int i = optionPanel.childCount - 1; i >= 0; i--)
            {
                Destroy(optionPanel.GetChild(i).gameObject);
            }
        }
    }
}
