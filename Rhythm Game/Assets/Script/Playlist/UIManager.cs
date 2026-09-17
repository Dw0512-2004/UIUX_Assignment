using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public SongListPopulator listPopulator;       // 拖入你的 SongListPopulator
    public DifficultySelector difficultySelector; // 拖入你的 DifficultySelector
    public string gameSceneName = "GameScene";    // 你的游戏场景名字

    [Header("UI 点击音效设置")]
    public AudioClip playButtonSound;                 // 💡 新增：按下 Play 按鈕的點擊音效
    [Range(0f, 1f)] public float playButtonVolume = 0.5f; // 💡 新增：音量大小

    // 绑定到 Play 按钮的 OnClick 事件上
    public void OnPlayButtonClicked()
    {
        // 💡 新增：當玩家按下 Play 按鈕時，先播放點擊音效
        if (playButtonSound != null) {
            AudioSource.PlayClipAtPoint(playButtonSound, Camera.main.transform.position, playButtonVolume);
        }

        if (listPopulator == null)
        {
            Debug.LogError("未绑定 SongListPopulator！");
            return;
        }

        // 1. 获取当前选中的歌曲
        SongData chosenSong = listPopulator.GetSelectedSong();
        if (chosenSong == null)
        {
            Debug.LogWarning("请先在列表中选择一首歌！");
            return;
        }

        // 2. 获取当前选中的难度 (返回 "easy", "normal", 或 "hard")
        string chosenDifficulty = "easy";
        if (difficultySelector != null)
        {
            chosenDifficulty = difficultySelector.GetCurrentDifficultyName().ToLower();
        }

        // 3. 严格存入全局单例 GameManager 中
        if (Manager.Instance != null)
        {
            Manager.Instance.selectedSong = chosenSong;
            Manager.Instance.selectedDifficulty = chosenDifficulty;
            Debug.Log($"[LevelLoader] 成功存入 GameManager ➔ 歌名: {chosenSong.songName} | 难度: {chosenDifficulty}");
        }
        else
        {
            Debug.LogError("场景中找不到 GameManager 实例！请确保 GameManager 存在且带 DontDestroyOnLoad。");
            return;
        }

        // 💡 4. 跳转场景前，把选歌界面的预览音乐关掉
        if (listPopulator.previewAudioSource != null && listPopulator.previewAudioSource.isPlaying)
        {
            listPopulator.previewAudioSource.Stop();
        }

        // 5. 正式跳转场景
        SceneManager.LoadScene(gameSceneName);
    }
}