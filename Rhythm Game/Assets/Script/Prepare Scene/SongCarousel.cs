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
        // 背景颜色平滑过渡(用DOTween一起做,比直接赋值瞬间变色更好看)
        if (backgroundImage != null) {
            backgroundImage.DOColor(data.backgroundColor, backgroundFadeDuration);
        }

        SongName.text = data.songName;
        AuthorName.text = data.author;

        EasyText.text = data.easyLevel.ToString();
        NormalText.text = data.normalLevel.ToString();
        HardText.text = data.hardLevel.ToString();

        likeButtonController.RefreshForSong(data.songID);

        string difficultyName = difficultySelector.GetCurrentDifficultyName(); // 见下方
        scoreDisplay.RefreshDisplay(data.songID, difficultyName);

        // 歌名、作者、分数这些之后一样在这里更新
        // songNameText.text = data.songName;
        // authorText.text = data.author;
        // scoreText.text = data.score.ToString();

        // 三个难度号码,假设你有三个难度按钮上的文字/或者当前选中难度对应的号码
        // 之前做的DifficultySelector,可以在这里通知它更新号码显示
        // difficultySelector.UpdateLevels(data.easyLevel, data.normalLevel, data.hardLevel);
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

        // 额外空转的圈数,让随机感更强(可依需求调整,比如1~2圈)
        int extraLoops = Random.Range(1, 3);

        // 从当前位置往"下一首"方向走到目标要走几步
        int directSteps = (targetIndex - centerIndex + songs.Count) % songs.Count;
        int totalSteps = directSteps + extraLoops * songs.Count;

        // 至少保证有个基本步数,不会太快就停(比如太近的时候也要有点转的感觉)
        if (totalSteps < 8) totalSteps += songs.Count;

        for (int i = 0; i < totalSteps; i++) {
            float t = (float)i / totalSteps; // 0~1的进度
            // 前面快、后面慢:duration从很短逐渐变长
            float stepDuration = Mathf.Lerp(0.08f, 0.35f, t * t); // t*t让减速更明显

            yield return StartCoroutine(DoOneStep(stepDuration, isLastStep: i == totalSteps - 1));
        }

        isSpinning = false;
    }

    // 执行"下一首"这个方向的单步动画,duration可自定义
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

        // 最后一步加一个"落地弹一下"的效果,增强停止感
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
}