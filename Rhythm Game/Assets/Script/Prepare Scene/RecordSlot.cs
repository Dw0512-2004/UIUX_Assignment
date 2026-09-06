using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class RecordSlot : MonoBehaviour {
    public Image albumImage;
    public CanvasGroup canvasGroup;

    [Header("难度相关图案")]
    public Image outlineImage;
    public Image circleInImage;
    public Image circleOutImage;

    [Header("成绩/状态显示 (可选,看UI设计)")]
    public TextMeshProUGUI scoreText;      // 或者TMP_Text,看你用哪种
    public TextMeshProUGUI gradeText;
    public GameObject[] starIcons; // 比如3颗星的图标,根据stars数量显示/隐藏
    public Image likeIcon;         // 收藏爱心图标(实心/空心切换,呼应你最早问的那个)
    public Sprite likedSprite;
    public Sprite unlikedSprite;
    public GameObject lockOverlay; // 如果没解锁,盖一层锁的图标/遮罩

    private string currentSongID;
    private string currentDifficulty = "easy"; // 假设默认显示easy的成绩,实际应跟你的难度选择联动

    public void Populate(SongData data) {
        currentSongID = data.songID;

        albumImage.sprite = data.albumArt;
        outlineImage.sprite = data.outlineSprite;
        circleInImage.sprite = data.circleInSprite;
        circleOutImage.sprite = data.circleOutSprite;

        RefreshPlayerInfo();
    }

    // 刷新玩家相关的数据显示(成绩、收藏、解锁状态)
    public void RefreshPlayerInfo() {
        bool unlocked = SaveManager.Instance.IsUnlocked(currentSongID);
        //lockOverlay.SetActive(!unlocked);

        DifficultyRecord record = SaveManager.Instance.GetRecord(currentSongID, currentDifficulty);
        if (scoreText != null) scoreText.text = record.score.ToString();
        //if (gradeText != null) gradeText.text = string.IsNullOrEmpty(record.grade) ? "-" : record.grade;

        /*
        for (int i = 0; i < starIcons.Length; i++) {
            starIcons[i].SetActive(i < record.stars);
        }
        */

        bool liked = SaveManager.Instance.IsLiked(currentSongID);
        likeIcon.sprite = liked ? likedSprite : unlikedSprite;
    }


    public Tween AnimateToCenter(Vector3 targetPos, float bigScale, float duration) {
        Sequence seq = DOTween.Sequence();
        seq.Join(transform.DOMove(targetPos, duration));
        seq.Join(transform.DOScale(bigScale, duration));
        seq.Join(canvasGroup.DOFade(1f, duration));
        seq.OnComplete(() => SetExtraImagesVisible(true));
        return seq;
    }

    public Tween AnimateToSide(Vector3 targetPos, float smallScale, float duration) {
        SetExtraImagesVisible(false);
        Sequence seq = DOTween.Sequence();
        seq.Join(transform.DOMove(targetPos, duration));
        seq.Join(transform.DOScale(smallScale, duration));
        seq.Join(canvasGroup.DOFade(0.5f, duration));
        return seq;
    }

    public void SetInstantState(Vector3 pos, float scale, float alpha, bool showExtra) {
        transform.position = pos;
        transform.localScale = Vector3.one * scale;
        canvasGroup.alpha = alpha;
        SetExtraImagesVisible(showExtra);
    }

    private void SetExtraImagesVisible(bool visible) {
        outlineImage.gameObject.SetActive(visible);
        circleInImage.gameObject.SetActive(visible);
        circleOutImage.gameObject.SetActive(visible);
    }
}