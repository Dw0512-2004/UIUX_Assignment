using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SongListItem : MonoBehaviour {
    [Header("背景 (选中态)")]
    public Image background;
    public Sprite selectedColor;
    public Sprite unselectedColor;
    public Sprite lockedColor; // 新增:未解锁时的背景样式

    private bool isSelected = false; // 记录当前是否被选中,方便解锁状态变化时重新计算

    [Header("歌曲基本信息")]
    public Image albumImage;       // 音乐图片
    public Image albumBackground;  // 音乐图片的背景色
    public TextMeshProUGUI songNameText;
    public TextMeshProUGUI authorText;

    [Header("Like按钮")]
    public Image likeIcon;
    public Sprite likedSprite;
    public Sprite unlikedSprite;

    [Header("难度背景+文字 (显示当前选中难度)")]
    public Image difficultyBackground;
    public TextMeshProUGUI difficultyLevelText;

    [Header("三个Grade图片 (easy / normal / hard)")]
    public Image gradeEasyImage;
    public Image gradeNormalImage;
    public Image gradeHardImage;
    public Sprite gradeC, gradeB, gradeA, gradeS, gradeEmpty; // gradeEmpty = 灰色占位图

    [Header("锁头")]
    public GameObject lockIcon;

    [Header("星星 (跟随当前选中难度)")]
    public Image[] starImages; // Size = 3
    public Sprite starBright;
    public Sprite starGray;

    private string currentDifficulty = "easy"; // 记录目前是哪个难度,方便Like/Lock之外的刷新用

    private SongData songData;
    private System.Action<SongData> onClickCallback;

    // 初始化整个列表项(生成时调用一次)
    public void Setup(SongData data, System.Action<SongData> onClick) {
        songData = data;
        onClickCallback = onClick;

        albumImage.sprite = data.albumPlaylistArt;
        albumImage.SetNativeSize();
        albumBackground.color = data.backgroundColor;
        songNameText.text = data.songName;
        authorText.text = data.author;

        RefreshLikeIcon();
        RefreshLockIcon();
        RefreshGrades();
    }

    // 点击整个item时调用(Button的OnClick绑定这个,或者用EventTrigger)
    public void OnItemClicked() {
        onClickCallback?.Invoke(songData);
    }

    // 点击item内的Like按钮时调用(要单独一个Button,阻止事件冒泡到OnItemClicked)
    public void OnLikeClicked() {
        SaveManager.Instance.ToggleLiked(songData.songID);
        RefreshLikeIcon();
    }

    public void RefreshLikeIcon() {
        bool liked = SaveManager.Instance.IsLiked(songData.songID);
        likeIcon.sprite = liked ? likedSprite : unlikedSprite;
    }

    public void RefreshLockIcon() {
        bool unlocked = SaveManager.Instance.IsUnlocked(songData.songID);
        lockIcon.SetActive(!unlocked);
        RefreshBackground(); // 锁定状态变了,背景也要跟着重新判断
    }

    public void RefreshGrades() {
        SetGradeImage(gradeEasyImage, SaveManager.Instance.GetRecord(songData.songID, "easy").grade);
        SetGradeImage(gradeNormalImage, SaveManager.Instance.GetRecord(songData.songID, "normal").grade);
        SetGradeImage(gradeHardImage, SaveManager.Instance.GetRecord(songData.songID, "hard").grade);
    }

    private void SetGradeImage(Image target, string grade) {
        switch (grade) {
            case "S": target.sprite = gradeS; break;
            case "A": target.sprite = gradeA; break;
            case "B": target.sprite = gradeB; break;
            case "C": target.sprite = gradeC; break;
            default: target.sprite = gradeEmpty; break; // 没玩过 = 灰色
        }
    }

    public void SetSelected(bool selected) {
        isSelected = selected;
        RefreshBackground();
    }

    // 难度切换时(外部难度按钮触发),更新显示的号码/颜色
    public void UpdateDifficultyDisplay(string difficulty, Color bgColor) {
        currentDifficulty = difficulty;
        difficultyBackground.color = bgColor;

        switch (difficulty.ToLower()) {
            case "easy": difficultyLevelText.text = songData.easyLevel.ToString(); break;
            case "normal": difficultyLevelText.text = songData.normalLevel.ToString(); break;
            case "hard": difficultyLevelText.text = songData.hardLevel.ToString(); break;
        }

        RefreshStars();
    }

    private void RefreshStars() {
        int stars = SaveManager.Instance.GetRecord(songData.songID, currentDifficulty).stars;

        for (int i = 0; i < starImages.Length; i++) {
            starImages[i].sprite = (i < stars) ? starBright : starGray;
        }
    }

    private void RefreshBackground() {
        bool unlocked = SaveManager.Instance.IsUnlocked(songData.songID);

        if (!unlocked) {
            background.sprite = lockedColor;
        } else {
            background.sprite = isSelected ? selectedColor : unselectedColor;
        }
    }

    public string GetSongID() => songData.songID;
}