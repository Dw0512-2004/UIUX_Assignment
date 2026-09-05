using UnityEngine;

[CreateAssetMenu(fileName = "NewSong", menuName = "Song/SongData")]
public class SongData : ScriptableObject {
    [Header("基本信息")]
    public string songName;
    public string author;
    public Sprite albumArt;          // 唱片封面图
    public Color backgroundColor;    // 背景颜色

    [Header("难度数值")]
    public int easyLevel;     // 例如 3
    public int normalLevel;   // 例如 5
    public int hardLevel;     // 例如 8

    [Header("难度相关图案 (随歌曲变化)")]
    public Sprite outlineSprite;    // outline图
    public Sprite circleInSprite;   // circle in图
    public Sprite circleOutSprite;  // circle out图

    [Header("分数")]
    public int score;
}