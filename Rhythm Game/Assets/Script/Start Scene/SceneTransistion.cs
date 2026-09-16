using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransistion : MonoBehaviour
{
    [Header("目标场景名称")]
    public string songSettingSceneName = "Song Setting Scene"; 

    [Header("音效相关")]
    public AudioSource audioSource;    // 专门用来播放点击按钮音效的 AudioSource
    public AudioClip clickSfx;         // 按下按钮时的音效 (Sound Effect)

    [Header("动画相关 (二选一或结合使用)")]
    public Animator transitionAnimator; // 如果你用的是 Animator 动画
    public string triggerName = "StartTransition"; // Animator 里的 Trigger 动画参数名
    public float animationDuration = 1.0f; // 如果没有用 Animator，或者想用纯延时等待动画播完的时间（秒）

    private bool isTransitioning = false;

    // 绑定给 UI Button 的 OnClick 事件
    public void GoToSongSetting()
    {
        if (isTransitioning) return; // 防止玩家连续狂点
        isTransitioning = true;

        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        // 1. 播放点击音效
        if (audioSource != null && clickSfx != null)
        {
            audioSource.PlayOneShot(clickSfx);
        }

        // 2. 触发过场动画
        if (transitionAnimator != null)
        {
            transitionAnimator.SetTrigger(triggerName);
        }

        // 3. 等待动画播放完毕（你可以根据你的动画实际长度調整 animationDuration，或者等待指定秒数）
        yield return new WaitForSeconds(animationDuration);

        // 4. 動畫播完後，跳轉到下一個場景
        SceneManager.LoadScene(songSettingSceneName);
    }
}