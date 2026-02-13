using UnityEngine;

namespace _Projects._Scripts.GamePlay.UI
{
    /// <summary>
    /// 调试辅助工具 - 显示当前鼠标和时间状态
    /// 添加到场景中的任何GameObject上用于调试
    /// </summary>
    public class UIDebugHelper : MonoBehaviour
    {
        [Header("调试设置")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.F1;
        
        private void Update()
        {
            // 切换调试信息显示
            if (Input.GetKeyDown(toggleKey))
            {
                showDebugInfo = !showDebugInfo;
            }
        }

        private void OnGUI()
        {
            if (!showDebugInfo) return;
            
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 16;
            style.normal.textColor = Color.yellow;
            
            int y = 10;
            int lineHeight = 25;
            
            GUI.Label(new Rect(10, y, 500, 20), $"Time Scale: {Time.timeScale}", style);
            y += lineHeight;
            
            GUI.Label(new Rect(10, y, 500, 20), $"Cursor Visible: {Cursor.visible}", style);
            y += lineHeight;
            
            GUI.Label(new Rect(10, y, 500, 20), $"Cursor Lock State: {Cursor.lockState}", style);
            y += lineHeight;
            
            GUI.Label(new Rect(10, y, 500, 20), $"Mouse Position: {Input.mousePosition}", style);
            y += lineHeight;
            
            // 显示当前是否有按钮在鼠标下
            var eventSystem = UnityEngine.EventSystems.EventSystem.current;
            if (eventSystem != null)
            {
                var pointerData = new UnityEngine.EventSystems.PointerEventData(eventSystem);
                pointerData.position = Input.mousePosition;
                var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
                eventSystem.GetComponent<UnityEngine.EventSystems.EventSystem>().RaycastAll(pointerData, results);
                
                if (results.Count > 0)
                {
                    GUI.Label(new Rect(10, y, 500, 20), $"Under Mouse: {results[0].gameObject.name}", style);
                }
                else
                {
                    GUI.Label(new Rect(10, y, 500, 20), "Under Mouse: Nothing", style);
                }
            }
            y += lineHeight;
            
            GUI.Label(new Rect(10, y, 500, 20), $"Press {toggleKey} to toggle debug info", style);
        }
    }
}

