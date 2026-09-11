using UnityEngine;

[CreateAssetMenu(fileName = "NewSong", menuName = "Song/SongData")]
public class SongData : ScriptableObject {
    [Header("唯一识别码 (不要重复!)")]
    public string songID; // 例如 "song_001",自己起名字,保证每首歌不同就行

    [Header("基本信息")]
    public string songName;
    public string author;
    public Sprite albumArt;          // 唱片封面图
    public Sprite albumPlaylistArt;          // 唱片封面图
    public Color backgroundColor;    // 背景颜色

    [System.Serializable]
    public struct DifficultyData 
    {
        public int level;                // 难度等级数字 (例如 3, 5, 8)
        public AudioClip musicClip;      // 该难度的音乐片段
        public TextAsset beatmapJson;    // 该难度的 JSON 谱面文件
    }

    [Header("各难度详细配置")]
    public DifficultyData easyDifficulty;
    public DifficultyData normalDifficulty;
    public DifficultyData hardDifficulty;

    [Header("难度相关图案 (随歌曲变化)")]
    public Sprite outlineSprite;    // outline图
    public Sprite circleInSprite;   // circle in图
    public Sprite circleOutSprite;  // circle out图

    [Header("分数")]
    public int score;
}