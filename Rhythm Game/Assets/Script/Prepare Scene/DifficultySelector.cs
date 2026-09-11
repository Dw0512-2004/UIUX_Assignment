using UnityEngine;
using UnityEngine.UI;

public class DifficultySelector : MonoBehaviour {
    [System.Serializable]
    public class DifficultyOption {
        public Image image;         
        public Sprite emptySprite; 
        public Sprite fullSprite; 
        public Color themeColor = Color.white; // 💡 顺便支持每个难度专属的背景色(如 Easy绿, Normal蓝, Hard红)
    }

    public DifficultyOption[] options; // 0: Easy, 1: Normal, 2: Hard
    private int currentSelectedIndex = 0;

    [Header("关联的选歌组件 (根据你的场景二选一拖入)")]
    public SongCarousel songCarousel;       // 如果用轮盘，拖入这个
    public SongListPopulator songListPopulator; // 如果用列表，拖入这个

    public ScoreDisplayController scoreDisplay;

    void Start() {
        // 默认选中 Easy (索引 0)
        SelectDifficulty(0);
    }

    public int GetSelectedDifficulty() {
        return currentSelectedIndex;
    }

    // 绑定到每个难度按钮的 OnClick 事件上 (参数分别填 0, 1, 2)
    public void SelectDifficulty(int index) {
        currentSelectedIndex = index;

        // 1. 刷新难度按钮的图标高亮
        for (int i = 0; i < options.Length; i++) {
            if (options[i].image != null) {
                options[i].image.sprite = (i == index) ? options[i].fullSprite : options[i].emptySprite;
            }
        }

        // 2. 获取当前的 SongID 和难度名称
        string songID = GetCurrentSongID();
        string diffName = GetCurrentDifficultyName();

        // 3. 刷新分数显示
        if (scoreDisplay != null && !string.IsNullOrEmpty(songID)) {
            scoreDisplay.RefreshDisplay(songID, diffName);
        }

        // 4. 💡 核心联动：如果场景里用的是列表模式，通知歌单刷新所有项的难度等级和星星
        if (songListPopulator != null) {
            Color currentBgColor = (index < options.Length) ? options[index].themeColor : Color.white;
            songListPopulator.UpdateAllItemsDifficulty(diffName, currentBgColor);
        }

        Debug.Log($"已切换难度为: {diffName} (针对歌曲 ID: {songID})");
    }

    // 自动适配轮盘或列表，安全地拿到当前选中的歌名 ID
    private string GetCurrentSongID() {
        if (songCarousel != null) {
            return songCarousel.GetCurrentSongID();
        }
        else if (songListPopulator != null && songListPopulator.GetSelectedSong() != null) {
            return songListPopulator.GetSelectedSong().songID;
        }
        return "";
    }

    public string GetCurrentDifficultyName() {
        switch (currentSelectedIndex) {
            case 0: return "easy";
            case 1: return "normal";
            case 2: return "hard";
            default: return "easy";
        }
    }
}