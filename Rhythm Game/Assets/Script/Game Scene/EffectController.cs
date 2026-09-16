using UnityEngine;
using DG.Tweening;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance { get; private set; }

    [Header("特效预制体")]
    public GameObject flameEffectPrefab;
    public GameObject shockwavePrefab;

    [Header("屏幕微震设置")]
    public Camera targetCamera;
    public float shakeDuration = 0.1f;
    public float shakeStrength = 0.12f;

    private Vector3 originalCamPos; // 💡 新增：记录摄像机初始位置，防止永久漂移

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        // 记录摄像机的原始绝对位置
        if (targetCamera != null)
        {
            originalCamPos = targetCamera.transform.position;
        }
    }

    public void PlayHitEffect(Vector3 spawnPosition)
    {
        if (flameEffectPrefab != null)
        {
            GameObject flame = Instantiate(flameEffectPrefab, spawnPosition, Quaternion.identity);
            ParticleSystem ps = flame.GetComponent<ParticleSystem>();
            if (ps != null) Destroy(flame, ps.main.duration + ps.main.startLifetime.constantMax);
            else Destroy(flame, 1f);
        }

        if (shockwavePrefab != null)
        {
            Instantiate(shockwavePrefab, spawnPosition, Quaternion.identity);
        }

        if (targetCamera != null)
        {
            targetCamera.transform.DOKill(true);
            targetCamera.transform.position = originalCamPos; // 💡 每次震动前，强行归位！
            
            // 💡 只震动 X 和 Y，Z 轴给 0，防止摄像机穿过 2D 场景！
            targetCamera.transform.DOShakePosition(shakeDuration, new Vector3(shakeStrength, shakeStrength, 0), 12, 90, false, true);
        }
    }
}