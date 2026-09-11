using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class SongCarousel : MonoBehaviour {
    public List<SongData> songs;

    public RecordSlot leftSlot, centerSlot, rightSlot;
    public Transform leftPos, centerPos, rightPos, offLeftPos, offRightPos;

    public float centerScale = 1f;
    public float sideScale = 0.7f;
    public float duration = 0.4f;

    private int centerIndex = 0;
    private bool isAnimating = false;

    [Header("其他需要跟着歌曲切换的UI")]
    public Image backgroundImage;       // 背景图片,要跟着换颜色
    public float backgroundFadeDuration = 0.4f;

    public TextMeshProUGUI SongName;
    public TextMeshProUGUI AuthorName;
    public TextMeshProUGUI EasyText;
    public TextMeshProUGUI NormalText;
    public TextMeshProUGUI HardText;

    public LikeButtonController likeButtonController; // 拖入场景里固定的这个UI
    public ScoreDisplayController scoreDisplay; // 拖入场景里固定的这个UI
    public DifficultySelector difficultySelector; // 拖入难度选择器,拿到当前选中难度

    public InputActionAsset inputAction;
    private InputAction m_Next;
    private InputAction m_Previous;

    private void Awake() {
        m_Next = InputSystem.actions.FindAction("Next");
        m_Previous = InputSystem.actions.FindAction("Previous");
    }

    void Start() {
        int leftIndex = GetLeftIndex();
        int rightIndex = GetRightIndex();

        leftSlot.Populate(songs[leftIndex]);
        centerSlot.Populate(songs[centerIndex]);
        rightSlot.Populate(songs[rightIndex]);

        leftSlot.SetInstantState(leftPos.position, sideScale, 0.5f, false);
        centerSlot.SetInstantState(centerPos.position, centerScale, 1f, true);
        rightSlot.SetInstantState(rightPos.position, sideScale, 0.5f, false);

        OnCenterSongChanged(songs[centerIndex]); // 通知其他UI(歌名/作者/背景色/分数)更新
    }

    private void Update() {
        if (m_Next.WasPerformedThisFrame()) {
            ShowNext();
        }
        if (m_Previous.WasPerformedThisFrame()) {
            ShowPrevious();
        }
    }

    // 从左往右滑(左边来到中间) = 上一首
    public void ShowPrevious() {
        if (isAnimating) return;
        isAnimating = true;

        int newCenterIndex = GetLeftIndex();
        int newLeftIndex = (newCenterIndex - 1 + songs.Count) % songs.Count;

        // 原本的right slot要被recycle成新的left slot
        RecordSlot recycled = rightSlot;
        recycled.Populate(songs[newLeftIndex]);
        recycled.SetInstantState(offLeftPos.position, sideScale, 0.5f, false);

        Sequence seq = DOTween.Sequence();
        seq.Join(leftSlot.AnimateToCenter(centerPos.position, centerScale, duration));
        seq.Join(centerSlot.AnimateToSide(rightPos.position, sideScale, duration));
        seq.Join(recycled.transform.DOMove(leftPos.position, duration));
        seq.Join(recycled.transform.DOScale(sideScale, duration));
        seq.Join(recycled.canvasGroup.DOFade(0.5f, duration));

        seq.OnComplete(() => {
            // 重新指定三个槽位的角色
            RecordSlot oldCenter = centerSlot;
            centerSlot = leftSlot;
            rightSlot = oldCenter;
            leftSlot = recycled;

            centerIndex = newCenterIndex;
            OnCenterSongChanged(songs[centerIndex]);
            isAnimating = false;
        });
    }

    // 从右往左滑(右边来到中间) = 下一首
    public void ShowNext() {
        if (isAnimating) return;
        isAnimating = true;

        int newCenterIndex = GetRightIndex();
        int newRightIndex = (newCenterIndex + 1) % songs.Count;

        RecordSlot recycled = leftSlot;
        recycled.Populate(songs[newRightIndex]);
        recycled.SetInstantState(offRightPos.position, sideScale, 0.5f, false);

        Sequence seq = DOTween.Sequence();
        seq.Join(rightSlot.AnimateToCenter(centerPos.position, centerScale, duration));
        seq.Join(centerSlot.AnimateToSide(leftPos.position, sideScale, duration));
        seq.Join(recycled.transform.DOMove(rightPos.position, duration));
        seq.Join(recycled.transform.DOScale(sideScale, duration));
        seq.Join(recycled.canvasGroup.DOFade(0.5f, duration));

        seq.OnComplete(() => {
            RecordSlot oldCenter = centerSlot;
            centerSlot = rightSlot;
            leftSlot = oldCenter;
            rightSlot = recycled;

            centerIndex = newCenterIndex;
            OnCenterSongChanged(songs[centerIndex]);
            isAnimating = false;
        });
    }

    private int GetLeftIndex() => (centerIndex - 1 + songs.Count) % songs.Count;
    private int GetRightIndex() => (centerIndex + 1) % songs.Count;

    // 其他UI(歌名文字、作者文字、背景色、分数)在这里统一更新
    private void OnCenterSongChanged(SongData data) {
        // 背景颜色平滑过渡
        if (backgroundImage != null) {
            backgroundImage.DOColor(data.backgroundColor, backgroundFadeDuration);
        }

        if (SongName != null) SongName.text = data.songName;
        if (AuthorName != null) AuthorName.text = data.author;

        // 💡 适配新版 SongData 结构：从嵌套结构中读取难度等级
        if (EasyText != null) EasyText.text = data.easyDifficulty.level.ToString();
        if (NormalText != null) NormalText.text = data.normalDifficulty.level.ToString();
        if (HardText != null) HardText.text = data.hardDifficulty.level.ToString();

        if (likeButtonController != null) likeButtonController.RefreshForSong(data.songID);

        if (difficultySelector != null) {
            string difficultyName = difficultySelector.GetCurrentDifficultyName();
            if (scoreDisplay != null) scoreDisplay.RefreshDisplay(data.songID, difficultyName);
        }
    }

    // shuffle
    public bool isSpinning = false;

    // 随机按钮绑定这个方法
    public void OnRandomButtonPressed() {
        if (isAnimating || isSpinning) return;

        int targetIndex = GetRandomTargetIndex();
        StartCoroutine(SpinToTarget(targetIndex));
    }

    // 避免抽到跟当前一样的歌
    private int GetRandomTargetIndex() {
        if (songs.Count <= 1) return centerIndex;

        int newIndex;
        do {
            newIndex = Random.Range(0, songs.Count);
        } while (newIndex == centerIndex);

        return newIndex;
    }

    private IEnumerator SpinToTarget(int targetIndex) {
        isSpinning = true;

        int extraLoops = Random.Range(1, 3);
        int directSteps = (targetIndex - centerIndex + songs.Count) % songs.Count;
        int totalSteps = directSteps + extraLoops * songs.Count;

        if (totalSteps < 8) totalSteps += songs.Count;

        for (int i = 0; i < totalSteps; i++) {
            float t = (float)i / totalSteps; 
            float stepDuration = Mathf.Lerp(0.08f, 0.35f, t * t); 

            yield return StartCoroutine(DoOneStep(stepDuration, isLastStep: i == totalSteps - 1));
        }

        isSpinning = false;
    }

    private IEnumerator DoOneStep(float duration, bool isLastStep) {
        bool done = false;

        int newCenterIndex = GetRightIndex();
        int newRightIndex = (newCenterIndex + 1) % songs.Count;

        RecordSlot recycled = leftSlot;
        recycled.Populate(songs[newRightIndex]);
        recycled.SetInstantState(offRightPos.position, sideScale, 0.5f, false);

        Sequence seq = DOTween.Sequence();
        seq.Join(rightSlot.AnimateToCenter(centerPos.position, centerScale, duration));
        seq.Join(centerSlot.AnimateToSide(leftPos.position, sideScale, duration));
        seq.Join(recycled.transform.DOMove(rightPos.position, duration));
        seq.Join(recycled.transform.DOScale(sideScale, duration));
        seq.Join(recycled.canvasGroup.DOFade(0.5f, duration));

        if (isLastStep) {
            seq.Append(rightSlot.transform.DOPunchScale(Vector3.one * 0.1f, 0.25f, 4, 0.5f));
        }

        seq.OnComplete(() => {
            RecordSlot oldCenter = centerSlot;
            centerSlot = rightSlot;
            leftSlot = oldCenter;
            rightSlot = recycled;

            centerIndex = newCenterIndex;
            OnCenterSongChanged(songs[centerIndex]);
            done = true;
        });

        yield return new WaitUntil(() => done);
    }

    public string GetCurrentSongID() {
        return songs[centerIndex].songID;
    }

    // 💡 额外提供一个便捷方法：获取当前中心歌曲的完整 SongData 数据
    public SongData GetCurrentSongData() {
        if (songs != null && songs.Count > 0) {
            return songs[centerIndex];
        }
        return null;
    }
}