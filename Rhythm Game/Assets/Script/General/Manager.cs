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
                // 1. 先尝试在场景中找一个
                _instance = FindAnyObjectByType<Manager>();

                // 2. 如果场景里真的没有，自动在后台创建一个！
                if (_instance == null)
                {
                    GameObject managerObj = new GameObject("Manager (Auto-Created)");
                    _instance = managerObj.AddComponent<Manager>();
                    DontDestroyOnLoad(managerObj); // 保证跨场景不销毁
                    Debug.Log("[Manager] 场景中未找到实例，已自动创建并设为 DontDestroyOnLoad。");
                }
            }
            return _instance;
        }
    }

    [Header("当前选中的局内数据")]
    public SongData selectedSong;       
    public string selectedDifficulty = "easy"; 

    public enum GameState { PreLoading, Playing, GameOver }
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
            Destroy(gameObject); // 防止场景中出现重复的 GameManager
        }
    }

    public void StartGame()
    {
        currentState = GameState.Playing;
    }
}