using UnityEngine;

public class Tile : MonoBehaviour
{
    private float moveSpeed;
    public int laneIndex; 
    private bool isHit = false;
    
    public bool IsHit => isHit; // 💡 新增：暴露自身状态

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

        if (EffectManager.Instance != null)
        {
            EffectManager.Instance.PlayHitEffect(transform.position);
        }

        Destroy(gameObject);
    }
}