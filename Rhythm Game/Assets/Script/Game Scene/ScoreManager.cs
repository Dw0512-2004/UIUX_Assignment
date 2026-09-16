using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Current Run Data (这局的数据)")]
    public int currentScore = 0;
    public int combo = 0;
    public int multiplier = 1;
    
    [Header("Result Data (结算数据)")]
    public int finalStars = 0;        // 最终获得的星星数 (0~3)
    public string finalGrade = "C";   // 最终评级 (S, A, B, C, F)
    public bool isNewRecord = false;  // 是否打破了最高分记录

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
        currentMissCount++;
        
        Debug.Log($"Miss 触发！当前累计 Miss: {currentMissCount} / {maxMissCount}");
        UpdateUI("Miss");

        // 检查是否达到失败条件
        if (currentMissCount >= maxMissCount)
        {
            TriggerLose();
        }
    }

    // ==========================================
    // 💡 结算与保存系统 (完美对接 SaveManager)
    // ==========================================
    
    // 无论是胜利还是失败，都会调用这个方法来计算数据并保存
    public void SaveGameData(bool isWin)
    {
        // 1. 计算星星和评级 (根据失误次数判定)
        if (!isWin)
        {
            finalStars = 0;
            finalGrade = "F"; // 失败直接 F 级，0 颗星
        }
        else
        {
            if (currentMissCount == 0) 
            { 
                finalStars = 3; finalGrade = "S"; // 完美通关 (Full Combo)
            }
            else if (currentMissCount <= maxMissCount / 2) 
            { 
                finalStars = 2; finalGrade = "A"; // 失误较少
            }
            else 
            { 
                finalStars = 1; finalGrade = "B"; // 惊险通关
            }
        }

        // 2. 获取当前游玩的歌曲信息
        if (Manager.Instance != null && Manager.Instance.selectedSong != null)
        {
            string songID = Manager.Instance.selectedSong.songID;
            string difficulty = Manager.Instance.selectedDifficulty;

            // 3. 从 SaveManager 拿旧记录比对
            var oldRecord = SaveManager.Instance.GetRecord(songID, difficulty);
            
            // 4. 判断是否打破了最高分记录
            if (currentScore > oldRecord.score)
            {
                isNewRecord = true;
                Debug.Log("🎉 恭喜！创造了新的最高分！");
            }
            else
            {
                isNewRecord = false;
            }

            // 5. 执行保存 (调用你的 SubmitScore 方法)
            // 你的 SubmitScore 内部已经写了 score > target.score 才覆盖的逻辑，所以直接传进去即可
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SubmitScore(songID, difficulty, currentScore, finalGrade, finalStars);
            }
            
            Debug.Log($"结算完毕 ➔ 状态: {(isWin ? "胜利" : "失败")}, 分数: {currentScore}, 星星: {finalStars}, 评级: {finalGrade}");
        }
        else
        {
            Debug.LogWarning("[ScoreManager] 找不到 GameManager 里的当前歌曲数据，无法保存分数！");
        }
    }

    // ==========================================

    private void TriggerLose()
    {
        Debug.Log("游戏失败！弹出 Lose Menu");

        // 失败时，触发保存并计算 0 星数据
        SaveGameData(isWin: false);

        // 停止游戏逻辑
        if (Manager.Instance != null)
        {
            Manager.Instance.currentState = Manager.GameState.GameOver;
        }

        // 停止背景音乐播放
        AudioSource bgm = FindAnyObjectByType<AudioSource>();
        if (bgm != null) bgm.Stop();

        // 弹出失败面板
        if (PanelController.Instance != null)
        {
            PanelController.Instance.ShowLoseMenu();
        }
    }

    private void UpdateUI(string judgment)
    {
        if (scoreText != null) scoreText.text = currentScore.ToString();
        if (comboText != null) comboText.text = "x" + combo.ToString();
        if (judgmentText != null) judgmentText.text = judgment;
    }
}