using UnityEngine;

public class Manager : MonoBehaviour
{
    private static Manager _instance;
    public static Manager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<Manager>();
                if (_instance == null)
                {
                    GameObject managerObj = new GameObject("Manager (Auto-Created)");
                    _instance = managerObj.AddComponent<Manager>();
                    DontDestroyOnLoad(managerObj);
                    Debug.Log("[Manager] 场景中未找到实例，已自动创建并设为 DontDestroyOnLoad。");
                }
            }
            return _instance;
        }
    }

    [Header("当前选中的局内数据 (跨场景传递)")]
    public SongData selectedSong;       
    public string selectedDifficulty = "easy"; 

    [Header("Debug / 编辑器测试用 (直接在 GameScene 运行)")]
    public SongData debugDefaultSong;   

    // 💡 确保这里包含了 Paused
    public enum GameState { PreLoading, Playing, Paused, GameOver }
    public GameState currentState = GameState.PreLoading;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }

        if (selectedSong == null && debugDefaultSong != null)
        {
            selectedSong = debugDefaultSong;
            Debug.Log("[Manager] 检测到未经过选歌界面，已自动加载 Debug 默认歌曲用于测试！");
        }
    }

    public void StartGame()
    {
        currentState = GameState.Playing;
        Debug.Log("游戏开始！");
    }

    public void PauseGame()
    {
        currentState = GameState.Paused;
    }

    public void EndGame()
    {
        currentState = GameState.GameOver;
        Debug.Log("游戏结束！");
    }
}