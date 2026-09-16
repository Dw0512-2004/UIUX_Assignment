using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; 
using UnityEngine.UI; 

public class PanelController : MonoBehaviour
{
    [Header("各类 UI 面板")]
    public GameObject preloadingPanel;   // 预加载/开始面板
    public GameObject pauseMenuPanel;    // 暂停菜单面板
    public GameObject winMenuPanel;      // 胜利结算面板
    public GameObject loseMenuPanel;     // 失败结算面板

    [Header("=== 预加载面板 (Preloading) UI 元素 ===")]
    public TextMeshProUGUI preloadingSongNameText;  // 预加载界面的歌名
    public TextMeshProUGUI preloadingAuthorText;    // 预加载界面的作者
    public TextMeshProUGUI preloadingBestScoreText; // 预加载界面的最高分
    public Image[] preloadingStarImages;            // 大小为3，拖入预加载界面的三颗星星
    
    [Header("=== 胜利面板 (Win Menu) UI 元素 ===")]
    public TextMeshProUGUI winScoreText;    
    public TextMeshProUGUI winGradeText;    
    public GameObject newRecordBadge;       
    public Image[] winStarImages;           

    [Header("星星状态图片 (共用)")]
    public Sprite starBright;               // 亮起的星星图片
    public Sprite starGray;                 // 灰色的星星图片

    [Header("=== 失败面板 (Lose Menu) UI 元素 ===")]
    public TextMeshProUGUI loseScoreText;   

    // 单例模式，方便其他脚本随时调用
    public static PanelController Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 💡 游戏场景一启动，立刻读取数据并设置预加载界面
        SetupPreloadingPanel();
    }

    // ==========================================
    // 💡 新增：读取数据并展示到 Preloading Panel
    // ==========================================
    private void SetupPreloadingPanel()
    {
        if (Manager.Instance != null && Manager.Instance.selectedSong != null)
        {
            SongData song = Manager.Instance.selectedSong;
            string difficulty = Manager.Instance.selectedDifficulty;

            // 1. 设置歌名和作者
            if (preloadingSongNameText != null) preloadingSongNameText.text = song.songName;
            if (preloadingAuthorText != null) preloadingAuthorText.text = song.author;

            // 2. 从 SaveManager 读取当前歌曲、当前难度的最高分和星星
            if (SaveManager.Instance != null)
            {
                // 获取存档记录
                var record = SaveManager.Instance.GetRecord(song.songID, difficulty);

                // 设置最高分文字
                if (preloadingBestScoreText != null) 
                {
                    preloadingBestScoreText.text = record.score.ToString(); // 如果想加前缀可以写成 "Best: " + record.score
                }

                // 点亮历史获得的星星
                if (preloadingStarImages != null && preloadingStarImages.Length == 3)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        preloadingStarImages[i].sprite = (i < record.stars) ? starBright : starGray;
                    }
                }
            }
            
            Debug.Log($"[PanelController] 预加载界面已刷新 ➔ 歌名: {song.songName}, 难度: {difficulty}");
        }
        else
        {
            Debug.LogWarning("[PanelController] GameManager 中没有选中的歌曲数据，无法刷新预加载界面。");
        }
    }

    // 主题选择按钮方法
    public void SelectTheme(int themeIndex)
    {
        GameSettings.currentThemeIndex = themeIndex;
        TileSpawner spawner = FindAnyObjectByType<TileSpawner>();
        if (spawner != null) spawner.PreviewBackground(themeIndex);
    }

    // Pre-loading 专属：点击开始按钮时调用
    public void OnStartButtonClicked()
    {
        if (preloadingPanel != null) preloadingPanel.SetActive(false); 

        TileSpawner spawner = FindAnyObjectByType<TileSpawner>();
        if (spawner != null) spawner.StartGameWithMusic();
    }

    // ==========================================
    // 菜单控制 (暂停、恢复、胜利、失败) 
    // ==========================================
    public void ShowPauseMenu()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; 
        PauseAllAudio(); 
    }

    public void ResumeGame()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f; 
        ResumeAllAudio(); 
    }

    public void ShowWinMenu()
    {
        if (winMenuPanel != null) winMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        PauseAllAudio(); 

        if (ScoreManager.Instance != null)
        {
            if (winScoreText != null) winScoreText.text = ScoreManager.Instance.currentScore.ToString();
            if (winGradeText != null) winGradeText.text = ScoreManager.Instance.finalGrade;
            if (newRecordBadge != null) newRecordBadge.SetActive(ScoreManager.Instance.isNewRecord);

            if (winStarImages != null && winStarImages.Length == 3)
            {
                int starsGot = ScoreManager.Instance.finalStars;
                for (int i = 0; i < 3; i++)
                {
                    winStarImages[i].sprite = (i < starsGot) ? starBright : starGray;
                }
            }
        }
    }

    public void ShowLoseMenu()
    {
        if (loseMenuPanel != null) loseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        PauseAllAudio(); 

        if (ScoreManager.Instance != null && loseScoreText != null)
        {
            loseScoreText.text = ScoreManager.Instance.currentScore.ToString();
        }
    }

    // ==========================================
    // 全局音乐与音效控制方法
    // ==========================================
    private void PauseAllAudio()
    {
        AudioSource[] allAudio = FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude);
        foreach (AudioSource audio in allAudio)
        {
            if (audio.isPlaying) audio.Pause();
        }
    }

    private void ResumeAllAudio()
    {
        AudioSource[] allAudio = FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude);
        foreach (AudioSource audio in allAudio)
        {
            audio.UnPause();
        }
    }

    // ==========================================
    // 通用返回与重试方法
    // ==========================================
    public void RestartLevel()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Song Setting Scene");
    }
}