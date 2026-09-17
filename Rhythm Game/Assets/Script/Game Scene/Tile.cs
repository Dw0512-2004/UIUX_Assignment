using UnityEngine;

public class Tile : MonoBehaviour
{
    private float moveSpeed;
    public int laneIndex; 
    private bool isHit = false;
    
    public bool IsHit => isHit; 

    [Header("Audio Settings")]
    public AudioClip tapSound; 
    [Range(0f, 1f)] public float tapVolume = 0.5f; // 💡 控制短按音量，預設為 50%

    public void Initialize(float speed, int lane) 
    {
        moveSpeed = speed;
        laneIndex = lane;
    }

    void Update()
    {
        if (Manager.Instance.currentState != Manager.GameState.Playing || isHit) return;

        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);

        if (transform.position.y < -6f) 
        {
            ScoreManager.Instance.Miss();
            Destroy(gameObject);
        }
    }

    public void OnHit()
    {
        if (isHit) return;
        isHit = true;

        // 💡 播放短按音效並套用音量設定
        if (tapSound != null)
        {
            AudioSource.PlayClipAtPoint(tapSound, Camera.main.transform.position, tapVolume);
        }

        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.PlayHitEffect(transform.position);
        }

        Destroy(gameObject);
    }
}