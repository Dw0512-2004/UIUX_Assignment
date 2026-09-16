using UnityEngine;

public class LongTile : MonoBehaviour
{
    private float moveSpeed;
    private bool isHeadHit = false;   
    private bool isCompleted = false; 
    private int laneIndex;            

    public bool IsHit => isHeadHit; 

    [Header("Visual Elements")]
    public Transform tailTransform;         
    public SpriteRenderer bodyRenderer;     

    private float initialHeight;      
    private float totalHoldDuration;  
    private float elapsedHoldTime = 0f; 

    private float currentHitLineY; // 记录对齐的 Y 坐标

    public void Initialize(float speed, int lane, float height)
    {
        moveSpeed = speed;
        laneIndex = lane;
        initialHeight = height;
        totalHoldDuration = height / speed;
        UpdateVisualHeight(height);
    }

    public int LaneIndex => laneIndex;

    void Update()
    {
        if (Manager.Instance.currentState != Manager.GameState.Playing) return;

        if (!isHeadHit)
        {
            transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);
            
            // 💡 修复：和单按音符统一，掉出屏幕底端（-6f）才算漏按，再也不会过早判定 Miss！
            if (transform.position.y < -6f)
            {
                ScoreManager.Instance.Miss();
                Destroy(gameObject);
            }
        }
        else if (isHeadHit && !isCompleted)
        {
            elapsedHoldTime += Time.deltaTime;
            float progressRatio = 1f - (elapsedHoldTime / totalHoldDuration);
            progressRatio = Mathf.Clamp01(progressRatio);
            UpdateVisualHeight(initialHeight * progressRatio);

            if (elapsedHoldTime >= totalHoldDuration) CompleteHold();
        }
    }

    private void UpdateVisualHeight(float height)
    {
        if (bodyRenderer != null)
        {
            Vector3 scale = bodyRenderer.transform.localScale;
            scale.y = height;
            bodyRenderer.transform.localScale = scale;
            bodyRenderer.transform.localPosition = new Vector3(0, height / 2f, 0); 
        }
        if (tailTransform != null) tailTransform.localPosition = new Vector3(0, height, 0);
    }

    public void OnHeadHit(float targetY)
    {
        isHeadHit = true;
        currentHitLineY = targetY; 
        
        Vector3 pos = transform.position;
        pos.y = currentHitLineY; 
        transform.position = pos;

        if (EffectManager.Instance != null) EffectManager.Instance.PlayHitEffect(transform.position);
    }

    public void BreakHold()
    {
        if (isHeadHit && !isCompleted)
        {
            ScoreManager.Instance.Miss();
            Destroy(gameObject);
        }
    }

    private void CompleteHold()
    {
        isCompleted = true;
        ScoreManager.Instance.AddScore(200, "Perfect"); 
        Destroy(gameObject);
    }
}