using UnityEngine;

public class CheatManager : MonoBehaviour
{
    [Header("作弊设置")]
    [Tooltip("玩家对象（如果未指定，自动查找）")]
    public Transform player;
    
    private bool cheatTriggered = false;

    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        // 检测 F4 和 Y 是否同时被按住
        if (Input.GetKey(KeyCode.F4) && Input.GetKey(KeyCode.Y))
        {
            if (!cheatTriggered)
            {
                cheatTriggered = true;
                TeleportToPortal(); 
            }
        }
        else
        {
            // 只要有一个键松开，就重置触发标志
            cheatTriggered = false;
        }
    }

    private void TeleportToPortal()
    {
        GameObject portal = GameObject.FindGameObjectWithTag("Portal");
        if (portal == null)
        {
            Debug.LogWarning("[Cheat] 未找到标签为 'Portal' 的传送门");
            return;
        }

        Vector3 targetPos = portal.transform.position; // 可根据需要加偏移

        // 处理 CharacterController
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;          // 禁用控制器
            player.position = targetPos;
            cc.enabled = true;           // 重新启用
        }
        else
        {
            player.position = targetPos;
        }

        Debug.Log($"[Cheat] 已传送至传送门，新位置: {targetPos}");
    }
}