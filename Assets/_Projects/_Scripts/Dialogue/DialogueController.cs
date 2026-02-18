using System;
using System.Collections;
using System.Collections.Generic;
using GamePlay.Controller;
using UnityEngine;
using _Projects.GamePlay;

public class DialogueController : MonoBehaviour
{
    public DialogueData_SO currentData;
    public PlayerInputHandler input;
    bool canTalk = false;
    private bool lastInteractInput = false;
    private InteractableNPC npc;
     [SerializeField]
    private bool ForcedToDialogue = false;

    // 2.18 静影：跨场景无法对话问题：增加自动拾取玩家信息
    private void Start()
    {
        // 查找玩家
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            input = playerObj.GetComponent<PlayerInputHandler>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && currentData != null)
        {
            canTalk = true;
        }

        // 2.15 静影：加入强制对话
        if (ForcedToDialogue)
        {
            OpenDialogue();
            ForcedToDialogue = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DialogueUI.Instance.dialoguePanel.SetActive(false);
            canTalk = false;
        }
    }

    private void Update()
    {
        // 边缘检测 - 只在按键按下时触发
        bool interactPressed = input.InteractInput;
        
        // 只有在可以对话 且 对话未激活 且 按键刚按下时才打开对话
        if (canTalk && !DialogueUI.Instance.IsDialogueActive && interactPressed && !lastInteractInput)
        {
            OpenDialogue();

            // 关掉npc对话提示符！！
            npc = GetComponent<InteractableNPC>();
            if (npc != null)
            {
                npc.MarkAsInteracted();
                npc.HideQuestMark();
            }
        }
        
        lastInteractInput = interactPressed;
    }

    private void OpenDialogue()
    {
        // 设置输入处理器（如果 DialogueUI 还没有的话）
        if (DialogueUI.Instance.inputHandler == null && input != null)
        {
            DialogueUI.Instance.inputHandler = input;
            Debug.Log("DialogueUI inputHandler 已设置");
        }
        
        DialogueUI.Instance.UpdateDialogueData(currentData);
        DialogueUI.Instance.UpdateMainDialogue(currentData.dialoguePieces[0]);
    }
}
