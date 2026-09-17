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
    public Image backgroundImage;       
    public float backgroundFadeDuration = 0.4f;

    public TextMeshProUGUI SongName;
    public TextMeshProUGUI AuthorName;
    public TextMeshProUGUI EasyText;
    public TextMeshProUGUI NormalText;
    public TextMeshProUGUI HardText;

    public LikeButtonController likeButtonController; 
    public ScoreDisplayController scoreDisplay; 
    public DifficultySelector difficultySelector; 

    [Header("预览音乐播放器")]
    public AudioSource previewAudioSource; 
    [Header("预览设置 (秒)")]
    public float previewDuration = 10f;    // 播放时长（10秒）
    public float previewStartTime = 30f;   // 从第几秒开始截取

    [Header("左右滑动切歌手势灵敏度")]
    public float minSwipeDistance = 60f;  
    private Vector2 swipeStartPos;         

    [Header("UI 点击音效设置")]
    public AudioClip slotClickSound;                 // 轮盘卡片点击音效
    [Range(0f, 1f)] public float slotClickVolume = 0.5f; 

    [Header("随机抽歌按鈕点击音效设置")]
    public AudioClip shuffleClickSound;                 // 💡 新增：隨機抽歌按鈕點擊音效
    [Range(0f, 1f)] public float shuffleClickVolume = 0.5f; // 💡 新增：音量大小

    void Start() {
        if (songs == null || songs.Count == 0) return;

        InitializeSlots();
        OnCenterSongChanged(songs[centerIndex], false); 
    }

    private void InitializeSlots() {
        int leftIndex = GetLeftIndex();
        int rightIndex = GetRightIndex();

        leftSlot.Populate(songs[leftIndex], OnSlotClicked);
        centerSlot.Populate(songs[centerIndex], OnSlotClicked);
        rightSlot.Populate(songs[rightIndex], OnSlotClicked);

        leftSlot.SetInstantState(leftPos.position, sideScale, 0.5f, false);
        centerSlot.SetInstantState(centerPos.position, centerScale, 1f, true);
        rightSlot.SetInstantState(rightPos.position, sideScale, 0.5f, false);
    }

    private void OnSlotClicked(RecordSlot clickedSlot) {
        if (isAnimating || isSpinning) return;

        if (slotClickSound != null) {
            AudioSource.PlayClipAtPoint(slotClickSound, Camera.main.transform.position, slotClickVolume);
        }

        if (clickedSlot == rightSlot) {
            ShowNext(true); 
        } else if (clickedSlot == leftSlot) {
            ShowPrevious(true); 
        } else if (clickedSlot == centerSlot) {
            PlaySongPreview(songs[centerIndex]); 
        }
    }

    void Update() {
        if (previewAudioSource != null && previewAudioSource.isPlaying && previewAudioSource.clip != null) {
            float targetEndTime = previewStartTime + previewDuration;
            if (previewAudioSource.time >= targetEndTime) {
                StopSongPreview(); 
            }
        }

        if (isAnimating || isSpinning) return;
        DetectSwipeGesture();
    }

    private void DetectSwipeGesture() {
        Pointer currentPointer = Pointer.current;
        if (currentPointer == null) return;

        if (currentPointer.press.wasPressedThisFrame) {
            swipeStartPos = currentPointer.position.ReadValue();
        }

        if (currentPointer.press.wasReleasedThisFrame) {
            Vector2 swipeEndPos = currentPointer.position.ReadValue();
            float distanceX = swipeEndPos.x - swipeStartPos.x; 
            float distanceY = Mathf.Abs(swipeEndPos.y - swipeStartPos.y); 

            if (Mathf.Abs(distanceX) > minSwipeDistance && Mathf.Abs(distanceX) > distanceY) {
                StopSongPreview(); 

                if (distanceX > 0) {
                    ShowPrevious(false); 
                } else {
                    ShowNext(false); 
                }
            }
        }
    }

    public void ShowPrevious(bool playAudioAfterMove = false) {
        if (isAnimating) return;
        StopSongPreview();
        isAnimating = true;

        int newCenterIndex = GetLeftIndex();
        int newLeftIndex = (newCenterIndex - 1 + songs.Count) % songs.Count;

        RecordSlot recycled = rightSlot;
        recycled.Populate(songs[newLeftIndex], OnSlotClicked); 
        recycled.SetInstantState(offLeftPos.position, sideScale, 0.5f, false);

        Sequence seq = DOTween.Sequence();
        seq.Join(leftSlot.AnimateToCenter(centerPos.position, centerScale, duration));
        seq.Join(centerSlot.AnimateToSide(rightPos.position, sideScale, duration));
        seq.Join(recycled.transform.DOMove(leftPos.position, duration));
        seq.Join(recycled.transform.DOScale(sideScale, duration));
        seq.Join(recycled.canvasGroup.DOFade(0.5f, duration));

        seq.OnComplete(() => {
            RecordSlot oldCenter = centerSlot;
            centerSlot = leftSlot;
            rightSlot = oldCenter;
            leftSlot = recycled;

            centerIndex = newCenterIndex;
            OnCenterSongChanged(songs[centerIndex], playAudioAfterMove); 
            isAnimating = false;
        });
    }

    public void ShowNext(bool playAudioAfterMove = false) {
        if (isAnimating) return;
        StopSongPreview();
        isAnimating = true;

        int newCenterIndex = GetRightIndex();
        int newRightIndex = (newCenterIndex + 1) % songs.Count;

        RecordSlot recycled = leftSlot;
        recycled.Populate(songs[newRightIndex], OnSlotClicked); 
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
            OnCenterSongChanged(songs[centerIndex], playAudioAfterMove); 
            isAnimating = false;
        });
    }

    private int GetLeftIndex() => (centerIndex - 1 + songs.Count) % songs.Count;
    private int GetRightIndex() => (centerIndex + 1) % songs.Count;

    private void OnCenterSongChanged(SongData data, bool playAudio) {
        if (backgroundImage != null) {
            backgroundImage.DOColor(data.backgroundColor, backgroundFadeDuration);
        }

        if (SongName != null) SongName.text = data.songName;
        if (AuthorName != null) AuthorName.text = data.author;

        if (EasyText != null) EasyText.text = data.easyDifficulty.level.ToString();
        if (NormalText != null) NormalText.text = data.normalDifficulty.level.ToString();
        if (HardText != null) HardText.text = data.hardDifficulty.level.ToString();

        if (likeButtonController != null) likeButtonController.RefreshForSong(data.songID);

        if (difficultySelector != null) {
            string difficultyName = difficultySelector.GetCurrentDifficultyName();
            if (scoreDisplay != null) scoreDisplay.RefreshDisplay(data.songID, difficultyName);
        }

        if (playAudio) {
            PlaySongPreview(data);
        } else {
            StopSongPreview(); 
        }
    }

    private void PlaySongPreview(SongData data) {
        if (previewAudioSource != null && data.easyDifficulty.musicClip != null) {
            previewAudioSource.Stop();
            previewAudioSource.clip = data.easyDifficulty.musicClip;

            float clipLength = previewAudioSource.clip.length;
            float startTime = previewStartTime;
            if (startTime >= clipLength) {
                startTime = 0f; 
            }

            previewAudioSource.time = startTime;
            previewAudioSource.Play();

            if (MenuBGMManager.Instance != null) {
                MenuBGMManager.Instance.PauseBGM();
            }
        }
    }

    private void StopSongPreview() {
        if (previewAudioSource != null && previewAudioSource.isPlaying) {
            previewAudioSource.Stop();
        }

        if (MenuBGMManager.Instance != null) {
            MenuBGMManager.Instance.ResumeBGM();
        }
    }

    public bool isSpinning = false;

    public void OnRandomButtonPressed() {
        if (isAnimating || isSpinning) return;

        // 💡 新增：當玩家點擊隨機抽歌按鈕時，播放點擊音效
        if (shuffleClickSound != null) {
            AudioSource.PlayClipAtPoint(shuffleClickSound, Camera.main.transform.position, shuffleClickVolume);
        }

        StopSongPreview(); 
        int targetIndex = GetRandomTargetIndex();
        StartCoroutine(SpinToTarget(targetIndex));
    }

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
        PlaySongPreview(songs[centerIndex]); 
    }

    private IEnumerator DoOneStep(float duration, bool isLastStep) {
        bool done = false;

        int newCenterIndex = GetRightIndex();
        int newRightIndex = (newCenterIndex + 1) % songs.Count;

        RecordSlot recycled = leftSlot;
        recycled.Populate(songs[newRightIndex], OnSlotClicked);
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
            OnCenterSongChanged(songs[centerIndex], false); 
            done = true;
        });

        yield return new WaitUntil(() => done);
    }

    public string GetCurrentSongID() {
        return songs[centerIndex].songID;
    }

    public SongData GetCurrentSongData() {
        if (songs != null && songs.Count > 0) {
            return songs[centerIndex];
        }
        return null;
    }
}