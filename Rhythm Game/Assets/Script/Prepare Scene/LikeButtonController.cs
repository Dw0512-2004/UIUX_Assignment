using UnityEngine;
using UnityEngine.UI;

public class LikeButtonController : MonoBehaviour {
    public Image heartImage;
    public Sprite emptyHeart;
    public Sprite fullHeart;

    private string currentSongID;

    // 每次中间歌曲切换时,SongCarousel调用这个方法
    public void RefreshForSong(string songID) {
        currentSongID = songID;
        bool liked = SaveManager.Instance.IsLiked(currentSongID);
        heartImage.sprite = liked ? fullHeart : emptyHeart;
    }

    // Like按钮的OnClick绑定这个方法(只需要绑定一次,因为这个UI是固定不动的)
    public void OnLikeButtonClicked() {
        SaveManager.Instance.ToggleLiked(currentSongID);
        RefreshForSong(currentSongID); // 立刻刷新图标
    }
}