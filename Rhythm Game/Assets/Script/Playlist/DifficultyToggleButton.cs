using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DifficultyToggleButton : MonoBehaviour {
    [System.Serializable]
    public class DifficultyOption {
        public string name; // "easy" / "normal" / "hard"
        public Color backgroundColor;
    }

    [Header("这个按钮本身的背景+文字 (显示当前难度)")]
    public Image difficultyBackground;
    public TextMeshProUGUI difficultyText;

    public DifficultyOption[] options; // Size = 3,依次 easy, normal, hard

    public SongListPopulator songListPopulator;

    [Header("UI 点击音效设置")]
    public AudioClip toggleClickSound;                 // 💡 新增：切換難度按鈕的點擊音效
    [Range(0f, 1f)] public float toggleClickVolume = 0.5f; // 💡 新增：音量大小

    private int currentIndex = 0;

    // DifficultyToggleButton.cs
    void Start() {
        Debug.Log($"[ToggleButton] Start执行, currentIndex = {currentIndex}, 颜色 = {options[currentIndex].backgroundColor}");
        RefreshDisplay();
    }

    // 按钮OnClick绑定这一个方法
    public void OnDifficultyButtonClicked() {
        // 💡 新增：當玩家點擊按鈕切換難度時，先播放點擊音效
        if (toggleClickSound != null) {
            AudioSource.PlayClipAtPoint(toggleClickSound, Camera.main.transform.position, toggleClickVolume);
        }

        currentIndex = (currentIndex + 1) % options.Length;
        RefreshDisplay();
        NotifyListUpdate();
    }

    private void RefreshDisplay() {
        difficultyBackground.color = options[currentIndex].backgroundColor;
        difficultyText.text = options[currentIndex].name.ToUpper();
    }

    private void NotifyListUpdate() {
        songListPopulator.UpdateAllItemsDifficulty(GetCurrentDifficultyName(), options[currentIndex].backgroundColor);
    }

    public string GetCurrentDifficultyName() {
        return options[currentIndex].name;
    }

    public Color GetCurrentColor() {
        return options[currentIndex].backgroundColor;
    }
}