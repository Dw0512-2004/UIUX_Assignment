using System.Collections.Generic;

// 单个难度的成绩
[System.Serializable]
public class DifficultyRecord {
    public int score;
    public string grade;   // 比如 "S", "A", "B"
    public int stars;      // 拿了几颗星 (0~3或0~5,看你设计)
}

// 一首歌的完整存档记录
[System.Serializable]
public class SongRecord {
    public string songID;
    public bool isLiked;      // 有没有被收藏
    public bool isUnlocked;   // 有没有被解锁

    public DifficultyRecord easy = new DifficultyRecord();
    public DifficultyRecord normal = new DifficultyRecord();
    public DifficultyRecord hard = new DifficultyRecord();
}

// 整个存档文件
[System.Serializable]
public class SaveData {
    public List<SongRecord> records = new List<SongRecord>();
}