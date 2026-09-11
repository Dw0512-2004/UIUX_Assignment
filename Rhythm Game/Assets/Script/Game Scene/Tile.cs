using UnityEngine;

public class Tile : MonoBehaviour
{
    private float moveSpeed;
    private bool isHit = false;

    // 由 TileSpawner 生成时调用
    public void Initialize(float speed)
    {
        moveSpeed = speed;
    }

    void Update()
    {
        // 游戏未在进行中或音符已被击中时停止移动
        if (GameManager.Instance.currentState != GameManager.GameState.Playing || isHit) return;

        // 音符向下移动
        transform.Translate(Vector3.down * moveSpeed * Time.deltaTime);

        // 如果音符漏掉，掉出屏幕底部（假设屏幕底部 Y 坐标约为 -6，可根据实际摄像机视野调整）
        if (transform.position.y < -6f) 
        {
            ScoreManager.Instance.Miss();
            Destroy(gameObject);
        }
    }

    // 被玩家成功点击时调用
    public void OnHit()
    {
        isHit = true;
        // 此处后续可实例化打击粒子特效或播放音效
        Destroy(gameObject);
    }
}