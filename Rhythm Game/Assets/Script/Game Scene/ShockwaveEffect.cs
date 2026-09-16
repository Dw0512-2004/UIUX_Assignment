using UnityEngine;
using DG.Tweening;

public class ShockwaveEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [Header("动画参数")]
    public float duration = 0.2f;       // 持续时间
    public float maxScale = 1.8f;       // 瞬间放大到的最大倍数

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // 1. 瞬间放大并淡出
        transform.DOScale(Vector3.one * maxScale, duration).SetEase(Ease.OutQuad);

        if (spriteRenderer != null)
        {
            // 逐渐变透明，并在动画结束后自动销毁
            spriteRenderer.DOFade(0f, duration).OnComplete(() => Destroy(gameObject));
        }
        else
        {
            Destroy(gameObject, duration);
        }
    }
}