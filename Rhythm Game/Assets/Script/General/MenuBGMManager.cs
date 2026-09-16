using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuBGMManager : MonoBehaviour {
    private static MenuBGMManager instance;
    public static MenuBGMManager Instance => instance;

    private AudioSource bgmAudioSource;

    [Header("正式游戏场景的名字")]
    public string gameSceneName = "GameScene"; // 填入你的游戏场景名称

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject); // 跨场景不销毁
            bgmAudioSource = GetComponent<AudioSource>();
        } else {
            Destroy(gameObject); 
            return;
        }
    }

    private void OnEnable() {
        // 注册场景切换监听事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() {
        // 取消注册，防止内存泄漏
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 当任意新场景加载完成时触发
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        // 如果进入的是游戏场景 (GameScene)，就销毁自己并停止 BGM
        if (scene.name == gameSceneName) {
            Debug.Log($"[MenuBGMManager] 进入游戏场景 {scene.name}，销毁菜单 BGM。");
            Destroy(gameObject);
        }
    }

    // 暂停/静音菜单 BGM
    public void PauseBGM() {
        if (bgmAudioSource != null && bgmAudioSource.isPlaying) {
            bgmAudioSource.Pause();
        }
    }

    // 恢复播放菜单 BGM
    public void ResumeBGM() {
        if (bgmAudioSource != null && !bgmAudioSource.isPlaying) {
            bgmAudioSource.UnPause();
            if (!bgmAudioSource.isPlaying) {
                bgmAudioSource.Play();
            }
        }
    }
}