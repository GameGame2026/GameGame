using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowAfterDialogue : MonoBehaviour
{
    public  DialogueData_SO theDialogue;

    public void Start()
    {
        gameObject.SetActive(false);

        if (DialogueUI.Instance != null)
            DialogueUI.Instance.OnDialogueClosed += OnDialogueEnded;
    }
    

    private void OnDestroy()
    {
        if (DialogueUI.Instance != null)
            DialogueUI.Instance.OnDialogueClosed -= OnDialogueEnded;
    }

    private void OnDialogueEnded(DialogueData_SO last_dlg)
    {
        // 判断这个对话是否是After_fight
        if (last_dlg != null && last_dlg == theDialogue)
        gameObject.SetActive(true);
        Debug.Log($"{name} 因对话结束而出现");
    }
}
