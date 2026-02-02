using System;
using System.Collections;
using System.Collections.Generic;
using GamePlay.Controller;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    public DialogueData_SO currentData;
    public PlayerInputHandler input;
    bool canTalk = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && currentData != null)
        {
            canTalk = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DialogueUI.Instance.dialoguePanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (canTalk && input.InteractInput)
        {
            OpenDialogue();
        }
    }

    private void OpenDialogue()
    {
        DialogueUI.Instance.UpdateDialogueData(currentData);
        DialogueUI.Instance.UpdateMainDialogue(currentData.dialoguePieces[0]);
    }
}
