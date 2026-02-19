using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearAfterDialogue : MonoBehaviour
{
    private void OnEnable()
    {
        if (DialogueUI.Instance != null)
            DialogueUI.Instance.OnDialogueClosed += OnDialogueEnded;
    }

    private void OnDisable()
    {
        if (DialogueUI.Instance != null)
            DialogueUI.Instance.OnDialogueClosed -= OnDialogueEnded;
    }

    private void OnDialogueEnded(DialogueData_SO last_dlg)
    {
        // 判断这个对话是否是After_fight
        if (last_dlg != null && last_dlg.name == "After_fight")
        gameObject.SetActive(false);
        Debug.Log($"{name} 因对话结束而消失");
    }
}
