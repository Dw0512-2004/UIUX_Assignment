using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplayController : MonoBehaviour {
    [Header("分数文字")]
    public TextMeshProUGUI scoreText;

    [Header("Grade图片 (C/B/A/S)")]
    public Image gradeImage;
    public Sprite gradeC;
    public Sprite gradeB;
    public Sprite gradeA;
    public Sprite gradeS;

    [Header("星星图片 (3颗)")]
    public Image[] starImages; // Size = 3
    public Sprite starGray;
    public Sprite starYellow;

    private string currentSongID;
    private string currentDifficulty = "easy"; // 默认easy,跟难度按钮同步

    // 歌曲切换 或 难度切换 时,都调用这个方法刷新显示
    public void RefreshDisplay(string songID, string difficulty) {
        currentSongID = songID;
        currentDifficulty = difficulty;

        DifficultyRecord record = SaveManager.Instance.GetRecord(currentSongID, currentDifficulty);

        // 分数
        scoreText.text = record.score.ToString();

        // Grade图片
        if (GetGradeSprite(record.grade) == null) {
            gradeImage.color = new Color(0, 0, 0, 0);
        } else {
            gradeImage.color = Color.white;
            gradeImage.sprite = GetGradeSprite(record.grade);
            gradeImage.SetNativeSize();
        }

        // 星星:根据record.stars决定几颗是黄色,其余是灰色
        for (int i = 0; i < starImages.Length; i++) {
            starImages[i].sprite = (i < record.stars) ? starYellow : starGray;
        }
    }

    private Sprite GetGradeSprite(string grade) {
        switch (grade) {
            case "S": return gradeS;
            case "A": return gradeA;
            case "B": return gradeB;
            case "C": return gradeC;
            default: return null; // 没有记录(没玩过),可以留空或者放一张"未评级"占位图
        }
    }
}