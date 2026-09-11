using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransistion : MonoBehaviour
{
    [Header("目标场景名称")]
    public string songSettingSceneName = "Song Setting Scene"; // 请确保这里的名字和你的场景名完全一致

    // 绑定给 UI Button 的 OnClick 事件
    public void GoToSongSetting()
    {
        SceneManager.LoadScene(songSettingSceneName);
    }
}