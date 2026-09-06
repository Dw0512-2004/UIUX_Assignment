using UnityEngine;
using UnityEngine.UI;

public class DifficultySelector : MonoBehaviour {
    [System.Serializable]
    public class DifficultyOption {
        public Image image;        
        public Sprite emptySprite; 
        public Sprite fullSprite; 
    }

    public DifficultyOption[] options; 

    private int currentSelectedIndex = 0;

    public SongCarousel songCarousel; // 拖入,用来拿到当前中间是哪首歌
    public ScoreDisplayController scoreDisplay;


    void Start() {
        SelectDifficulty(0);
    }

    public int GetSelectedDifficulty() {
        return currentSelectedIndex;
    }

    public void SelectDifficulty(int index) {
        currentSelectedIndex = index;

        for (int i = 0; i < options.Length; i++) {
            options[i].image.sprite = (i == index) ? options[i].fullSprite : options[i].emptySprite;
        }

        // 难度变了,刷新分数显示
        string songID = songCarousel.GetCurrentSongID(); // 见下方
        scoreDisplay.RefreshDisplay(songID, GetCurrentDifficultyName());
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
