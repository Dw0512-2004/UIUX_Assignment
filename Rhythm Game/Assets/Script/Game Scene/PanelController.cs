using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelController : MonoBehaviour
{
    [Header("各类 UI 面板")]
    public GameObject preloadingPanel;   // 预加载/开始面板
    public GameObject pauseMenuPanel;    // 暂停菜单面板
    public GameObject winMenuPanel;      // 胜利结算面板
    public GameObject loseMenuPanel;     // 失败结算面板

    // 单例模式，方便其他脚本随时调用
    public static PanelController Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 主题选择按钮方法
    public void SelectTheme(int themeIndex)
    {
        GameSettings.currentThemeIndex = themeIndex;
        Debug.Log($"已通过 PanelController 成功选择主题: Theme {themeIndex + 1}");

        TileSpawner spawner = FindAnyObjectByType<TileSpawner>();
        if (spawner != null)
        {
            spawner.PreviewBackground(themeIndex);
        }
    }

    // Pre-loading 专属：点击开始按钮时调用
    public void OnStartButtonClicked()
    {
        if (preloadingPanel != null)
        {
            preloadingPanel.SetActive(false); 
        }

        TileSpawner spawner = FindAnyObjectByType<TileSpawner>();
        if (spawner != null)
        {
            spawner.StartGameWithMusic();
        }
    }

    // ==========================================
    // 菜单控制 (暂停、恢复、胜利、失败) - 加入音乐控制
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
    }

    public void ShowLoseMenu()
    {
        if (loseMenuPanel != null) loseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        PauseAllAudio(); 
    }

    // ==========================================
    // 全局音乐与音效控制方法 (已适配最新 Unity API)
    // ==========================================
    private void PauseAllAudio()
    {
        // 使用 FindObjectsInactive.Exclude 替代被废弃的 FindObjectsSortMode
        AudioSource[] allAudio = FindObjectsByType<AudioSource>(FindObjectsInactive.Exclude);
        foreach (AudioSource audio in allAudio)
        {
            if (audio.isPlaying)
            {
                audio.Pause();
            }
        }
    }

    private void ResumeAllAudio()
    {
        // 使用 FindObjectsInactive.Exclude 替代被废弃的 FindObjectsSortMode
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