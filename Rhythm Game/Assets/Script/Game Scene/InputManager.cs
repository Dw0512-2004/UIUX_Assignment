using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public float hitLineY = -3f; // 判定线 Y 坐标
    
    // 追踪当前正在被按住的长按滑条（按轨道索引保存）
    private LongTile[] activeHoldTiles = new LongTile[4]; 

    void Update()
    {
        if (GameManager.Instance.currentState != GameManager.GameState.Playing) return;

        // 1. 检测刚按下的瞬间 (Press Down)
        bool isPressedDown = false;
        Vector2 screenPos = Vector2.zero;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            screenPos = Mouse.current.position.ReadValue();
            isPressedDown = true;
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
            isPressedDown = true;
        }

        if (isPressedDown)
        {
            ProcessTouchDown(screenPos);
        }

        // 2. 检测手指松开的瞬间 (Release Up) - 用于中断长按
        bool isReleased = false;
        if (Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            isReleased = true;
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
        {
            isReleased = true;
        }

        if (isReleased)
        {
            ProcessTouchRelease();
        }
    }

    private void ProcessTouchDown(Vector2 screenPosition)
    {
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(screenPosition);
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Note"))
        {
            // 检查是不是普通块 (Tile)
            Tile hitTile = hit.collider.GetComponent<Tile>();
            if (hitTile != null)
            {
                float distance = Mathf.Abs(hitTile.transform.position.y - hitLineY);
                if (distance <= 1.2f) // 判定范围内
                {
                    ScoreManager.Instance.AddScore(100, "Perfect");
                    hitTile.OnHit();
                }
            }

            // 检查是不是长按块 (LongTile)
            LongTile longTile = hit.collider.GetComponent<LongTile>();
            if (longTile != null)
            {
                // 通过反射或直接在 LongTile 里获取 laneIndex，或者在射线中获取
                // 这里假设 LongTile 也有判定或直接触发 OnHeadHit
                longTile.OnHeadHit();
                
                // 记录当前轨道正在按住（这里假设可以通过组件获取轨道，或者简化处理）
                // 提示：你可以给 LongTile 增加一个公共属性 public int LaneIndex => laneIndex;
                int lane = longTile.LaneIndex; // 需要在 LongTile 里把 laneIndex 设为 public 属性
                activeHoldTiles[lane] = longTile;
            }
        }
    }

    private void ProcessTouchRelease()
    {
        // 当玩家松开手指时，中断所有正在进行的 Hold 状态
        for (int i = 0; i < activeHoldTiles.Length; i++)
        {
            if (activeHoldTiles[i] != null)
            {
                activeHoldTiles[i].BreakHold(); // 玩家中途松手，判定失败
                activeHoldTiles[i] = null;
            }
        }
    }
}