using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour {
    private static SaveManager _instance;

    public static SaveManager Instance {
        get {
            if (_instance == null) {
                // 如果场景里没有,自动创建一个
                GameObject go = new GameObject("SaveManager");
                _instance = go.AddComponent<SaveManager>();
                DontDestroyOnLoad(go);
                _instance.Load();
            }
            return _instance;
        }
    }

    private SaveData saveData;
    private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    void Awake() {
        if (_instance == null) {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        } else if (_instance != this) {
            Destroy(gameObject);
        }
    }

    public void Load() {
        if (File.Exists(SavePath)) {
            string json = File.ReadAllText(SavePath);
            saveData = JsonUtility.FromJson<SaveData>(json);
        } else {
            saveData = new SaveData();
        }
    }

    public void Save() {
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);
    }

    // 找到某首歌的记录,没有就自动新建一条(新歌默认未解锁、未收藏)
    private SongRecord GetOrCreateRecord(string songID) {
        SongRecord record = saveData.records.Find(r => r.songID == songID);
        if (record == null) {
            record = new SongRecord { songID = songID, isUnlocked = false, isLiked = false };
            saveData.records.Add(record);
        }
        return record;
    }

    // ---------- 成绩相关 ----------

    // 结算画面调用:提交成绩(打破记录才更新)
    public void SubmitScore(string songID, string difficulty, int score, string grade, int stars) {
        SongRecord record = GetOrCreateRecord(songID);
        DifficultyRecord target = GetDifficultyRecord(record, difficulty);

        if (score > target.score) {
            target.score = score;
            target.grade = grade;
            target.stars = stars;
        }

        Save();
    }

    // 选歌界面调用:查询某首歌某难度的最佳成绩
    public DifficultyRecord GetRecord(string songID, string difficulty) {
        SongRecord record = saveData.records.Find(r => r.songID == songID);
        if (record == null) return new DifficultyRecord(); // 还没玩过,返回空成绩
        return GetDifficultyRecord(record, difficulty);
    }

    private DifficultyRecord GetDifficultyRecord(SongRecord record, string difficulty) {
        switch (difficulty.ToLower()) {
            case "easy": return record.easy;
            case "normal": return record.normal;
            case "hard": return record.hard;
            default: return record.easy;
        }
    }

    // ---------- 收藏(Like)相关 ----------

    public void SetLiked(string songID, bool liked) {
        SongRecord record = GetOrCreateRecord(songID);
        record.isLiked = liked;
        Save();
    }

    public void ToggleLiked(string songID) {
        SongRecord record = GetOrCreateRecord(songID);
        record.isLiked = !record.isLiked;
        Save();
    }

    public bool IsLiked(string songID) {
        SongRecord record = saveData.records.Find(r => r.songID == songID);
        return record != null && record.isLiked;
    }

    // ---------- 解锁(Unlock)相关 ----------

    public void SetUnlocked(string songID, bool unlocked) {
        SongRecord record = GetOrCreateRecord(songID);
        record.isUnlocked = unlocked;
        Save();
    }

    public bool IsUnlocked(string songID) {
        SongRecord record = saveData.records.Find(r => r.songID == songID);
        return record != null && record.isUnlocked;
    }






    // --------------Profile-----------------//
    // 统计:玩家总共获得多少颗星星(把所有歌曲、所有难度的星星加起来)
    public int GetTotalStars() {
        int total = 0;
        foreach (var record in saveData.records) {
            total += record.easy.stars;
            total += record.normal.stars;
            total += record.hard.stars;
        }
        return total;
    }

    // 统计:玩家解锁了多少首歌
    public int GetUnlockedSongCount() {
        int count = 0;
        foreach (var record in saveData.records) {
            if (record.isUnlocked) count++;
        }
        return count;
    }

    // 统计:玩家总共拿了多少个S/A/B (跨所有歌曲、所有难度)
    public (int sCount, int aCount, int bCount) GetGradeCounts() {
        int sCount = 0, aCount = 0, bCount = 0;

        foreach (var record in saveData.records) {
            CountGrade(record.easy.grade, ref sCount, ref aCount, ref bCount);
            CountGrade(record.normal.grade, ref sCount, ref aCount, ref bCount);
            CountGrade(record.hard.grade, ref sCount, ref aCount, ref bCount);
        }

        return (sCount, aCount, bCount);
    }

    private void CountGrade(string grade, ref int s, ref int a, ref int b) {
        switch (grade) {
            case "S": s++; break;
            case "A": a++; break;
            case "B": b++; break;
                // C不记录,其他情况(比如空字符串,代表还没玩过)也不算
        }
    }
}