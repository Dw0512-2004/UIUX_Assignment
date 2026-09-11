using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.InputSystem;

public class SwipeUpTransition : MonoBehaviour {
    [Header("半圆黑色遮罩")]
    public Image semiCircleImage;       
    public GameObject semiCircleTargetPos; 
    public float targetAlpha = 1f;      

    [Header("需要往下移动的两个物体")]
    public Transform objectA;
    public Transform objectB;
    public float moveDownDistance = 50f; 

    [Header("动画时间")]
    public float duration = 0.5f;

    [Header("要切换的场景名字")]
    public string targetSceneName = "GameScene"; 

    [Header("选歌数据关联 (轮盘模式)")]
    public SongCarousel songCarousel;             
    public DifficultySelector difficultySelector; 

    [Header("上滑手势灵敏度设置")]
    public float minSwipeDistance = 80f; // 屏幕上往上拖拽超过多少像素判定为成功上滑

    private bool isTransitioning = false;
    private Vector2 swipeStartPos;       // 记录手指/鼠标按下的起始位置

    [Header("Input System 设置")]
    public InputActionAsset inputActionAsset; 
    private InputAction m_StartAction;

    private void OnEnable() {
        if (inputActionAsset != null) {
            inputActionAsset.Enable();
            m_StartAction = inputActionAsset.FindAction("Start");
            if (m_StartAction != null) m_StartAction.Enable();
        }
    }

    private void OnDisable() {
        if (inputActionAsset != null) {
            inputActionAsset.Disable();
        }
    }

    private void Update() {
        if (isTransitioning) return;

        // 1. 保留原本的按键触发（如 Start 键或测试用的空格键）
        if (m_StartAction != null && m_StartAction.WasPressedThisFrame()) {
            TriggerTransition();
            return;
        }

        // 2. 💡 纯新 Input System 方式的“往上滑动 (Swipe Up)”检测
        DetectSwipeUp();
    }

    // 检测鼠标或触控的往上滑动
    private void DetectSwipeUp() {
        Pointer currentPointer = Pointer.current;
        if (currentPointer == null) return;

        // 当按下鼠标左键或者手机屏幕开始触摸的一瞬间
        if (currentPointer.press.wasPressedThisFrame) {
            swipeStartPos = currentPointer.position.ReadValue();
        }

        // 当松开鼠标左键或者手指离开屏幕的一瞬间
        if (currentPointer.press.wasReleasedThisFrame) {
            Vector2 swipeEndPos = currentPointer.position.ReadValue();
            float distanceY = swipeEndPos.y - swipeStartPos.y; // 计算垂直方向的位移

            // 如果向上滑动的距离超过了设定的阈值，且向上的幅度大于向右的幅度（确保是明显的往上划）
            if (distanceY > minSwipeDistance) {
                Debug.Log($"[SwipeUp] 成功检测到上滑手势！垂直距离: {distanceY}");
                TriggerTransition();
            }
        }
    }

    // 执行过渡动画与跳转
    public void TriggerTransition() {
        if (isTransitioning) return;
        isTransitioning = true;

        SaveSelectedDataToManager();

        Vector3 objectAStartPos = objectA != null ? objectA.position : Vector3.zero;
        Vector3 objectBStartPos = objectB != null ? objectB.position : Vector3.zero;

        Sequence seq = DOTween.Sequence();

        if (semiCircleImage != null && semiCircleTargetPos != null) {
            seq.Join(semiCircleImage.rectTransform.DOMove(semiCircleTargetPos.transform.position, duration).SetEase(Ease.OutQuad));
            seq.Join(semiCircleImage.DOFade(targetAlpha, duration));
        }

        if (objectA != null) seq.Join(objectA.DOMove(objectAStartPos + Vector3.down * moveDownDistance, duration).SetEase(Ease.OutQuad));
        if (objectB != null) seq.Join(objectB.DOMove(objectBStartPos + Vector3.down * moveDownDistance, duration).SetEase(Ease.OutQuad));

        seq.OnComplete(() => {
            SceneManager.LoadScene(targetSceneName);
        });
    }

    private void SaveSelectedDataToManager() {
        if (songCarousel == null) {
            Debug.LogWarning("[SwipeUpTransition] 未绑定 SongCarousel！");
            return;
        }

        SongData chosenSong = songCarousel.GetCurrentSongData();
        if (chosenSong == null) {
            Debug.LogWarning("[SwipeUpTransition] 轮盘当前没有选中任何歌曲！");
            return;
        }

        string chosenDifficulty = "easy";
        if (difficultySelector != null) {
            chosenDifficulty = difficultySelector.GetCurrentDifficultyName().ToLower();
        }

        if (Manager.Instance != null) {
            Manager.Instance.selectedSong = chosenSong;
            Manager.Instance.selectedDifficulty = chosenDifficulty;
            Debug.Log($"[SwipeUpTransition] 成功保存轮盘数据 ➔ 歌名: {chosenSong.songName} | 难度: {chosenDifficulty}");
        }
    }
}