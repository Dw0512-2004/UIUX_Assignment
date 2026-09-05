using UnityEngine;
using UnityEngine.UI;

public class HeartToggle : MonoBehaviour
{
    public Image heartImage;      
    public Sprite emptyHeart;      
    public Sprite fullHeart;       

    private bool isFull = false;

    public void ToggleHeart() {
        isFull = !isFull;
        heartImage.sprite = isFull ? fullHeart : emptyHeart;
    }
}
