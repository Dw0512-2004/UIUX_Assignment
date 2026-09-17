using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScreen : MonoBehaviour
{
    // [Header("UI 点击音效设置")]
    public AudioClip clickSound;                 // 💡 新增：切換場景按鈕的點擊音效
    [Range(0f, 1f)] public float clickVolume = 0.5f; // 💡 新增：音量大小

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeScene(string TargetSceneName) {
        // 💡 新增：當呼叫切換場景時，先播放點擊音效
        if (clickSound != null) {
            AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position, clickVolume);
        }

        SceneManager.LoadScene(TargetSceneName);
    }
}