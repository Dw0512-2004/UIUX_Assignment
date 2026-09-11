using UnityEngine;

public class LongTile : MonoBehaviour
{
    private float moveSpeed;
    private bool isHeadHit = false;   // 头部是否已被按住
    private bool isCompleted = false; // 是否完整滑完
    private int laneIndex;            // 记录属于哪个轨道

    [Header("Visual Elements")]
    public Transform tailTransform;       // 尾巴的 Transform
    public SpriteRenderer bodyRenderer;   // 滑条中间身体的 SpriteRenderer
    private BoxCollider2D boxCollider;    // 碰撞体组件

    private float initialHeight;      // 初始总高度
    private float totalHoldDuration;  // 总持续时间（秒）
    private float elapsedHoldTime = 0f; // 已经按住的时间

    private float hitLineY = -4f;     // 判定线 Y 坐标

    void Awake()
    {
        // 自动获取身上的 BoxCollider2D
        boxCollider = GetComponent<BoxCollider2D>();

        if (boxCollider != null)
        {
            // 1. 强制将碰撞体中心锁定在物体 Pivot（即滑条最底部的头部）
            boxCollider.offset = Vector2.zero; 
            
            // 2. 强制设置头部判定区的大小（这里以宽1、高0.5为例，你可以根据实际按键大小微调）
            boxCollider.size = new Vector2(1f, 0.5f); 
        }
    }

    // 初始化时接收速度、轨道、以及计算好的高度
    public void Initialize(float speed, int lane, float height)
    {
        moveSpeed = speed;
        laneIndex = lane;
        initialHeight = height;
        
        totalHoldDuration = height / speed;

        // 初始化视觉高度（碰撞体保持固定，不随 height 改变）
        UpdateVisualHeight(height);
    }

    public int LaneIndex => laneIndex;

    void Update()
    {
        if (GameManager.Instance.currentState != GameManager.GameState.Playing) return;

        // 阶段 1：头部还没被按到，正常从上往下掉落
        if (!isHeadHit)
        {
            transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);

            // 只有当滑条头部明显低于判定线下方时，才判定为 Miss
            if (transform.position.y < (hitLineY - 1.5f))
            {
                ScoreManager.Instance.Miss();
                Destroy(gameObject);
            }
        }
        // 阶段 2：头部已经被按住，进入“边按边消耗”的过程
        else if (isHeadHit && !isCompleted)
        {
            elapsedHoldTime += Time.deltaTime;

            // 计算剩余比例 (从 1 慢慢递减到 0)
            float progressRatio = 1f - (elapsedHoldTime / totalHoldDuration);
            progressRatio = Mathf.Clamp01(progressRatio);

            // 动态计算当前剩下的视觉高度（身体从上往下缩短）
            float currentHeight = initialHeight * progressRatio;
            UpdateVisualHeight(currentHeight);

            // 当时间耗尽，说明滑条已经被完全“吃完”了，判定成功
            if (elapsedHoldTime >= totalHoldDuration)
            {
                CompleteHold();
            }
        }
    }

    // 只更新视觉图片和尾巴，【绝对不修改 Collider 的大小】
    private void UpdateVisualHeight(float height)
    {
        // 1. 更新视觉 Sprite 的拉伸与偏移
        if (bodyRenderer != null)
        {
            Vector3 scale = bodyRenderer.transform.localScale;
            scale.y = height;
            bodyRenderer.transform.localScale = scale;

            // 让身体底部对准原点（头部），顶部向上延伸
            bodyRenderer.transform.localPosition = Vector3.zero;
        }

        // 2. 更新尾巴位置在身体的最顶端
        if (tailTransform != null)
        {
            tailTransform.localPosition = new Vector3(0, height, 0);
        }

        // 【注意】：这里故意删掉了所有修改 boxCollider.size 和 offset 的代码！
        // 让 BoxCollider2D 永远保持你在 Prefab 里调好的固定大小（只覆盖头部）
    }

    // 当玩家在判定线按下头部时调用
    public void OnHeadHit()
    {
        isHeadHit = true;
        
        // 固定在判定线上
        Vector3 pos = transform.position;
        pos.y = hitLineY; 
        transform.position = pos;

        // 【优化】头部被击中后，关闭碰撞体，防止后续误触发点击
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }

        Debug.Log("长按开始，滑动消耗中...");
    }

    // 玩家中途松手调用（中断失败）
    public void BreakHold()
    {
        if (isHeadHit && !isCompleted)
        {
            ScoreManager.Instance.Miss();
            Destroy(gameObject);
        }
    }

    // 顺利按完
    private void CompleteHold()
    {
        isCompleted = true;
        ScoreManager.Instance.AddScore(200, "Perfect"); 
        Destroy(gameObject);
    }
}