using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class RecordSlot : MonoBehaviour {
    public Image albumImage;
    public CanvasGroup canvasGroup;
    public Button slotButton; // 💡 新增：卡片上的按钮组件

    [Header("难度相关图案")]
    public Image outlineImage;
    public Image circleInImage;
    public Image circleOutImage;

    [Header("成绩/状态显示")]
    public TextMeshProUGUI scoreText;      
    public TextMeshProUGUI gradeText;
    public GameObject[] starIcons; 
    public Image likeIcon;         
    public Sprite likedSprite;
    public Sprite unlikedSprite;
    public GameObject lockOverlay; 

    private string currentSongID;
    private SongData songData; // 💡 记录当前槽位绑定的 SongData
    private System.Action<RecordSlot> onClickCallback; // 点击回调

    public void Populate(SongData data, System.Action<RecordSlot> onClickAction = null) {
        songData = data;
        currentSongID = data.songID;
        onClickCallback = onClickAction;

        albumImage.sprite = data.albumArt;
        if (outlineImage != null) outlineImage.sprite = data.outlineSprite;
        if (circleInImage != null) circleInImage.sprite = data.circleInSprite;
        if (circleOutImage != null) circleOutImage.sprite = data.circleOutSprite;

        RefreshPlayerInfo();

        // 绑定按钮事件
        if (slotButton != null) {
            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(() => {
                onClickCallback?.Invoke(this); // 点击时通知轮盘
            });
        }
    }

    public SongData GetSongData() => songData;

    public void RefreshPlayerInfo() {
        bool unlocked = SaveManager.Instance.IsUnlocked(currentSongID);
        //if (lockOverlay != null) lockOverlay.SetActive(!unlocked);

        DifficultyRecord record = SaveManager.Instance.GetRecord(currentSongID, currentDifficulty);
        if (scoreText != null) scoreText.text = record.score.ToString();

        bool liked = SaveManager.Instance.IsLiked(currentSongID);
        if (likeIcon != null) likeIcon.sprite = liked ? likedSprite : unlikedSprite;
    }

    private string currentDifficulty = "easy";

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
        if (outlineImage != null) outlineImage.gameObject.SetActive(visible);
        if (circleInImage != null) circleInImage.gameObject.SetActive(visible);
        if (circleOutImage != null) circleOutImage.gameObject.SetActive(visible);
    }
}