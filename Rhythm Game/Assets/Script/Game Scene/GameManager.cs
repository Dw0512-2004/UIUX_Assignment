using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 单例模式，方便全局访问
    public static GameManager Instance { get; private set; }

    // 游戏状态枚举
    public enum GameState { Ready, Playing, Paused, GameOver }
    public GameState currentState = GameState.Ready;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    public void StartGame()
    {
        currentState = GameState.Playing;
        // 此处可以添加触发音频播放的逻辑
        Debug.Log("游戏开始！");
    }

    public void PauseGame()
    {
        currentState = GameState.Paused;
        // 此处可以添加暂停音频的逻辑
    }

    public void EndGame()
    {
        currentState = GameState.GameOver;
        Debug.Log("游戏结束！");
    }
}