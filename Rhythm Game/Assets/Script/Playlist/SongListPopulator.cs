using UnityEngine;
using System.Collections.Generic;

public class SongListPopulator : MonoBehaviour {
    public Transform contentParent;   // 挂了VerticalLayoutGroup的那个物体
    public GameObject songItemPrefab;

    [Header("手动拖入所有歌曲的SongData")]
    public List<SongData> songs = new List<SongData>(); 

    [Header("预览音乐播放器")]
    public AudioSource previewAudioSource; // 专门用来在选歌界面试听音乐的 AudioSource

    [Header("难度按钮 (用于初始化同步)")]
    public DifficultyToggleButton difficultyToggle; // 新增

    [Header("预览设置 (秒)")]
    public float previewDuration = 10f;    // 播放时长（10秒）
    public float previewStartTime = 30f;   // 从音乐的第几秒开始截取

    private List<SongListItem> spawnedItems = new List<SongListItem>();
    private SongData selectedSong;

    void Start() {
        PopulateList();

        if (difficultyToggle != null) {
            string name = difficultyToggle.GetCurrentDifficultyName();
            Color color = difficultyToggle.GetCurrentColor();
            Debug.Log($"[Populator] 初始化难度: {name}, 颜色: {color}");
            UpdateAllItemsDifficulty(name, color);
        }
    }

    void Update() {
        // 💡 核心修改：检测 10 秒预览是否播放完毕，播完后自动停止并恢复全局菜单 BGM
        if (previewAudioSource != null && previewAudioSource.isPlaying && previewAudioSource.clip != null) {
            float targetEndTime = previewStartTime + previewDuration;
            if (previewAudioSource.time >= targetEndTime) {
                StopSongPreview(); // 10秒到期，停止预览并恢复菜单 BGM
            }
        }
    }

    private void PopulateList() {
        foreach (var data in songs) {
            GameObject itemObj = Instantiate(songItemPrefab, contentParent);
            SongListItem item = itemObj.GetComponent<SongListItem>();
            item.Setup(data, OnSongClicked);
            spawnedItems.Add(item);
        }
    }

    // 当玩家按下指定的歌曲卡片按钮时触发
    public void OnSongClicked(SongData data) {
        // 如果点击的是当前已经选中且正在播放的歌，或者是新歌，先停止上一首的预览
        StopSongPreview();

        selectedSong = data;

        // 1. 更新UI的高亮选中状态
        foreach (var item in spawnedItems) {
            item.SetSelected(item.GetSongID() == data.songID);
        }

        // 2. 切换成该 SongData 里面的 mp3 并播放 10 秒片段
        if (previewAudioSource != null && data.easyDifficulty.musicClip != null) {
            previewAudioSource.clip = data.easyDifficulty.musicClip; 
            
            float clipLength = previewAudioSource.clip.length;
            float startTime = previewStartTime;
            if (startTime >= clipLength) {
                startTime = 0f; 
            }

            previewAudioSource.time = startTime; 
            previewAudioSource.Play(); 
            
            // 💡 联动：播放预览时，暂停全局菜单 BGM
            if (MenuBGMManager.Instance != null) {
                MenuBGMManager.Instance.PauseBGM();
            }

            Debug.Log($"[SongListPopulator] 播放 10 秒预览并暂停菜单 BGM: {data.songName}");
        }
    }

    private void StopSongPreview() {
        if (previewAudioSource != null && previewAudioSource.isPlaying) {
            previewAudioSource.Stop();
        }

        // 💡 联动：预览结束时，恢复全局菜单 BGM
        if (MenuBGMManager.Instance != null) {
            MenuBGMManager.Instance.ResumeBGM();
        }
    }

    public SongData GetSelectedSong() => selectedSong;

    public void UpdateAllItemsDifficulty(string difficulty, Color bgColor) {
        foreach (var item in spawnedItems) {
            item.UpdateDifficultyDisplay(difficulty, bgColor);
        }
    }
}