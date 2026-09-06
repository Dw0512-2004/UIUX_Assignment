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

    private int currentIndex = 0;

    void Start() {
        RefreshDisplay();
        NotifyListUpdate();
    }

    // 按钮OnClick绑定这一个方法
    public void OnDifficultyButtonClicked() {
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
}