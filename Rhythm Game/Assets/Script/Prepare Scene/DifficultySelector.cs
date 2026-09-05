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

    void Start() {
        SelectDifficulty(0);
    }

    public void SelectDifficulty(int index) {
        currentSelectedIndex = index;

        for (int i = 0; i < options.Length; i++) {
            if (i == index) {
                options[i].image.sprite = options[i].fullSprite;
            } else {
                options[i].image.sprite = options[i].emptySprite;
            }
        }
    }

    public int GetSelectedDifficulty() {
        return currentSelectedIndex;
    }
}