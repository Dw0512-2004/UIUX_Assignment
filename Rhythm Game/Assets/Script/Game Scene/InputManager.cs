using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class InputManager : MonoBehaviour
{
    // 記錄【觸控ID或鼠標】當前正在按住的【HitZone 判定區】
    private Dictionary<int, Hitzone> activeTouches = new Dictionary<int, Hitzone>();

    void Update()
    {
        if (Manager.Instance.currentState != Manager.GameState.Playing) return;

        // 1. 處理電腦端鼠標 (使用保持狀態 isPressed，而不是單帧事件，避免誤判)
        if (Mouse.current != null)
        {
            int mouseId = -1;
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                ProcessTouchDown(Mouse.current.position.ReadValue(), mouseId);
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                ProcessTouchRelease(mouseId);
            }
        }

        // 2. 處理手機端多點觸控
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                int touchId = touch.touchId.ReadValue();
                var phase = touch.phase.ReadValue();

                if (phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    ProcessTouchDown(touch.position.ReadValue(), touchId);
                }
                else if (phase == UnityEngine.InputSystem.TouchPhase.Ended || phase == UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    ProcessTouchRelease(touchId);
                }
            }
        }
    }

    private void ProcessTouchDown(Vector2 screenPosition, int touchId)
    {
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(screenPosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(worldPoint, Vector2.zero);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider != null && hit.collider.CompareTag("HitZone"))
            {
                Hitzone zone = hit.collider.GetComponent<Hitzone>();
                if (zone != null)
                {
                    // 💡 核心修復：如果這個 ID 之前有殘留記錄，先安全移除，絕對不在此處呼叫 OnRelease() 
                    if (activeTouches.ContainsKey(touchId))
                    {
                        activeTouches.Remove(touchId);
                    }

                    zone.OnPress();               
                    activeTouches[touchId] = zone; // 成功記錄當前按下的區域
                    break; 
                }
            }
        }
    }

    private void ProcessTouchRelease(int touchId)
    {
        // 只有當玩家【真正松開】時，才通知對應的 HitZone 釋放
        if (activeTouches.ContainsKey(touchId))
        {
            Hitzone zone = activeTouches[touchId];
            if (zone != null)
            {
                zone.OnRelease();
            }
            activeTouches.Remove(touchId);
        }
    }
}