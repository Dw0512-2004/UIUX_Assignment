using UnityEngine;
using TMPro; // 引入 TextMeshPro 命名空间

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private int currentScore = 0;
    private int combo = 0;
    private int multiplier = 1;

    [Header("UI References")]
    public TextMeshProUGUI scoreText;     // 对应顶部的分数
    public TextMeshProUGUI comboText;     // 对应连击数
    public TextMeshProUGUI judgmentText;  // 对应判定文本 ("Perfect", "Miss" 等)

    [Header("Fail Settings (失败判定)")]
    public int maxMissCount = 5;          // 允许的最大 Miss 次数
    private int currentMissCount = 0;     // 当前已累计的 Miss 次数

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 当玩家成功击中音符时调用
    public void AddScore(int baseScore, string judgment)
    {
        combo++;
        // 每连续击中10个音符，乘数+1
        multiplier = (combo / 10) + 1; 
        currentScore += baseScore * multiplier;

        UpdateUI(judgment);
    }

    // 当玩家漏掉音符或点错时调用
    public void Miss()
    {
        combo = 0;
        multiplier = 1;

        // 累加 Miss 次数
        currentMissCount++;
        Debug.Log($"Miss 触发！当前累计 Miss: {currentMissCount} / {maxMissCount}");

        UpdateUI("Miss");

        // 检查是否达到失败条件
        if (currentMissCount >= maxMissCount)
        {
            TriggerLose();
        }
    }

    // 触发游戏失败逻辑
    private void TriggerLose()
    {
        Debug.Log("游戏失败！弹出 Lose Menu");

        // 1. 将 GameManager 状态设为 GameOver，停止音符下落和输入检测
        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentState = GameManager.GameState.GameOver;
        }

        // 2. 停止背景音乐播放
        AudioSource bgm = FindAnyObjectByType<AudioSource>();
        if (bgm != null)
        {
            bgm.Stop();
        }

        // 3. 通过单例调用 PanelController 弹出失败面板
        if (PanelController.Instance != null)
        {
            PanelController.Instance.ShowLoseMenu();
        }
    }

    private void UpdateUI(string judgment)
    {
        if (scoreText != null) scoreText.text = currentScore.ToString();
        if (comboText != null) comboText.text = "x" + combo.ToString();
        
        if (judgmentText != null) 
        {
            judgmentText.text = judgment;
        }
    }
}