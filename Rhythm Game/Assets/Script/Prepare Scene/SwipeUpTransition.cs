using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.InputSystem;

public class SwipeUpTransition : MonoBehaviour {
    [Header("半圆黑色遮罩")]
    public Image semiCircleImage;      // 半透明黑色半圆的Image组件
    public GameObject semiCircleTargetPos; // 半圆要移动到的最终位置(比如屏幕上方)
    public float targetAlpha = 1f;      // 目标透明度(完全不透明)

    [Header("需要往下移动的两个物体")]
    public Transform objectA;
    public Transform objectB;
    public float moveDownDistance = 50f; // 往下移动的距离

    [Header("动画时间")]
    public float duration = 0.5f;

    [Header("要切换的场景名字")]
    public string targetSceneName = "StartScene";

    private bool isTransitioning = false;

    public InputActionAsset inputAction;
    private InputAction m_Start;

    private void Awake() {
        m_Start = InputSystem.actions.FindAction("Start");
    }

    private void Update() {
        if (m_Start.WasPressedThisFrame()) {
            TriggerTransition();
        }
    }

    // 检测到玩家往上滑的时候调用这个方法
    public void TriggerTransition() {
        if (isTransitioning) return;
        isTransitioning = true;

        Vector3 objectAStartPos = objectA.position;
        Vector3 objectBStartPos = objectB.position;

        Sequence seq = DOTween.Sequence();

        // 半圆:移动到目标位置 + 颜色alpha变成不透明
        seq.Join(semiCircleImage.rectTransform.DOMove(semiCircleTargetPos.transform.position, duration).SetEase(Ease.OutQuad));
        seq.Join(semiCircleImage.DOFade(targetAlpha, duration));

        // 两个物体往下移动一小段
        seq.Join(objectA.DOMove(objectAStartPos + Vector3.down * moveDownDistance, duration).SetEase(Ease.OutQuad));
        seq.Join(objectB.DOMove(objectBStartPos + Vector3.down * moveDownDistance, duration).SetEase(Ease.OutQuad));

        // 动画结束后切场景
        seq.OnComplete(() => {
            SceneManager.LoadScene(targetSceneName);
        });
    }
}