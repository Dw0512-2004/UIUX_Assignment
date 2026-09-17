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

    [Header("Audio Settings")]
    public AudioClip headHitSound;  
    public AudioClip completeSound; 
    [Range(0f, 1f)] public float hitVolume = 0.5f; // 💡 控制長按音量，預設為 50%

    private float initialHeight;      
    private float totalHoldDuration;  
    private float elapsedHoldTime = 0f; 

    private float currentHitLineY; 

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

        // 💡 播放頭部擊中音效並套用音量設定
        if (headHitSound != null)
        {
            AudioSource.PlayClipAtPoint(headHitSound, Camera.main.transform.position, hitVolume);
        }

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

        // 💡 播放長按成功結束的音效並套用音量設定
        if (completeSound != null)
        {
            AudioSource.PlayClipAtPoint(completeSound, Camera.main.transform.position, hitVolume);
        }

        Destroy(gameObject);
    }
}