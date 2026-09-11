using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// 包装整个谱面的 JSON 数据结构
[System.Serializable]
public class BeatmapData
{
    public string songName;
    public float bpm;
    public List<NoteData> notes;
}

// 单个音符的数据结构
[System.Serializable]
public class NoteData
{
    public float hitTime;    // 准确到达判定线的时间(秒)
    public int laneIndex;    // 轨道索引 (0, 1, 2, 3)
    public int noteType;     // 0 = 单按 (Tap), 1 = 长按 (Slide)
    public float duration;   // 长按滑条的持续时间 (秒)
}

// 主题数据结构
[System.Serializable]
public struct GameThemeData
{
    public string themeName;           
    public GameObject tapTilePrefab;   
    public GameObject slideTilePrefab; 
    public Sprite backgroundSprite;    
}

public class TileSpawner : MonoBehaviour
{
    [Header("Theme Settings (4套主题资源配置)")]
    public GameThemeData[] themes = new GameThemeData[4]; 
    public SpriteRenderer backgroundRenderer;             

    [Header("Fallback Prefabs (默认/备用预制体)")]
    public GameObject tapTilePrefab;   
    public GameObject slideTilePrefab; 

    [Header("Setup")]
    public Transform[] spawnPoints;    

    [Header("Audio Setup")]
    public AudioSource bgmAudioSource; 
    public double startDelay = 2.0;    

    [Header("Beatmap File")]
    public TextAsset beatmapJsonFile;  

    [Header("Game Settings")]
    public float noteSpeed = 8f;       
    public float spawnOffsetTime = 2f; 

    private Queue<NoteData> noteQueue = new Queue<NoteData>();
    private double audioStartDSPTime;
    
    private bool hasGameStarted = false; 
    private bool isGameEnded = false;    

    void Start()
    {
        LoadSongAndBeatmapFromManager();
        LoadBeatmapFromJSON();
    }

    // 从全局 GameManager 读取选中的歌曲和难度对应的音乐与谱面
    void LoadSongAndBeatmapFromManager()
    {
        if (Manager.Instance != null && Manager.Instance.selectedSong != null)
        {
            SongData song = Manager.Instance.selectedSong;
            string diff = Manager.Instance.selectedDifficulty.ToLower();

            AudioClip targetClip = null;
            TextAsset targetJson = null;

            // 根据难度类型精准匹配对应的音乐和谱面
            switch (diff)
            {
                case "normal":
                    targetClip = song.normalDifficulty.musicClip;
                    targetJson = song.normalDifficulty.beatmapJson;
                    break;
                case "hard":
                    targetClip = song.hardDifficulty.musicClip;
                    targetJson = song.hardDifficulty.beatmapJson;
                    break;
                case "easy":
                default:
                    targetClip = song.easyDifficulty.musicClip;
                    targetJson = song.easyDifficulty.beatmapJson;
                    break;
            }

            // 赋值给 TileSpawner 自身的音频源和 JSON 文件引用
            if (bgmAudioSource != null && targetClip != null)
            {
                bgmAudioSource.clip = targetClip;
            }
            else
            {
                Debug.LogWarning($"[TileSpawner] 选中的歌曲 [{song.songName}] 的 {diff} 难度音频为空！");
            }

            if (targetJson != null)
            {
                beatmapJsonFile = targetJson;
            }
            else
            {
                Debug.LogWarning($"[TileSpawner] 选中的歌曲 [{song.songName}] 的 {diff} 难度 JSON 谱面为空！");
            }

            Debug.Log($"[TileSpawner] 成功加载游戏数据 ➔ 歌名: {song.songName} | 难度: {diff}");
        }
        else
        {
            Debug.LogWarning("[TileSpawner] 未检测到 GameManager 或所选歌曲，将使用 Inspector 默认配置。");
        }
    }

    // 菜单选主题时实时预览背景
    public void PreviewBackground(int index)
    {
        if (themes != null && themes.Length > 0)
        {
            if (index < 0 || index >= themes.Length) index = 0;
            GameThemeData currentTheme = themes[index];

            if (backgroundRenderer != null && currentTheme.backgroundSprite != null)
            {
                backgroundRenderer.sprite = currentTheme.backgroundSprite;
                Debug.Log($"实时预览背景已切换为: {currentTheme.themeName}");
            }
        }
    }

    // 游戏正式开始时完整应用主题（Prefab + 背景）
    void ApplySelectedTheme()
    {
        int index = GameSettings.currentThemeIndex;

        if (themes != null && themes.Length > 0)
        {
            if (index < 0 || index >= themes.Length) index = 0;
            GameThemeData currentTheme = themes[index];
            Debug.Log($"成功应用主题: {currentTheme.themeName} (索引: {index})");

            if (currentTheme.tapTilePrefab != null) tapTilePrefab = currentTheme.tapTilePrefab;
            if (currentTheme.slideTilePrefab != null) slideTilePrefab = currentTheme.slideTilePrefab;

            if (backgroundRenderer != null && currentTheme.backgroundSprite != null)
            {
                backgroundRenderer.sprite = currentTheme.backgroundSprite;
            }
        }
    }

    void LoadBeatmapFromJSON()
    {
        if (beatmapJsonFile == null)
        {
            Debug.LogError("未绑定 JSON 谱面文件！");
            return;
        }

        BeatmapData mapData = JsonUtility.FromJson<BeatmapData>(beatmapJsonFile.text);
        
        if (mapData != null && mapData.notes != null)
        {
            mapData.notes.Sort((a, b) => a.hitTime.CompareTo(b.hitTime));

            foreach (var note in mapData.notes)
            {
                noteQueue.Enqueue(note);
            }
            Debug.Log($"成功加载谱面: {mapData.songName}, 总音符数: {mapData.notes.Count}");
        }
    }

    // 点击 Start 按钮时触发
    public void StartGameWithMusic()
    {
        if (bgmAudioSource == null || bgmAudioSource.clip == null)
        {
            Debug.LogError("请检查 AudioSource 或 AudioClip 是否为空！");
            return;
        }

        ApplySelectedTheme(); // 正式开玩前锁定并加载主题

        audioStartDSPTime = AudioSettings.dspTime + startDelay;
        bgmAudioSource.PlayScheduled(audioStartDSPTime);
        
        hasGameStarted = true;
        isGameEnded = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }

    void Update()
    {
        if (!hasGameStarted || GameManager.Instance == null) return;
        if (GameManager.Instance.currentState != GameManager.GameState.Playing) return;

        double currentMusicTime = AudioSettings.dspTime - audioStartDSPTime;

        // 1. 生成音符
        while (noteQueue.Count > 0 && currentMusicTime >= (noteQueue.Peek().hitTime - spawnOffsetTime))
        {
            NoteData nextNote = noteQueue.Dequeue();
            SpawnNote(nextNote);
        }

        // 2. 检查胜利条件：音符空了且音乐播完
        if (!isGameEnded && noteQueue.Count == 0)
        {
            if (bgmAudioSource.clip != null && currentMusicTime >= bgmAudioSource.clip.length)
            {
                TriggerWin();
            }
        }
    }

    void SpawnNote(NoteData data)
    {
        GameObject prefab = (data.noteType == 0) ? tapTilePrefab : slideTilePrefab;
        Transform spawnPt = spawnPoints[data.laneIndex];

        GameObject newTile = Instantiate(prefab, spawnPt.position, Quaternion.identity);
        
        if (data.noteType == 0)
        {
            Tile tapTile = newTile.GetComponent<Tile>();
            if (tapTile != null) tapTile.Initialize(noteSpeed);
        }
        else if (data.noteType == 1)
        {
            LongTile slideTile = newTile.GetComponent<LongTile>();
            if (slideTile != null)
            {
                float slideDuration = data.duration > 0 ? data.duration : 0.5f;
                float slideHeight = slideDuration * noteSpeed;
                slideTile.Initialize(noteSpeed, data.laneIndex, slideHeight);
            }
        }
    }

    private void TriggerWin()
    {
        isGameEnded = true;
        Debug.Log("音乐播完且音符生成完毕，游戏胜利！弹出 Win Menu");

        GameManager.Instance.currentState = GameManager.GameState.GameOver;

        if (PanelController.Instance != null)
        {
            PanelController.Instance.ShowWinMenu();
        }
    }
}