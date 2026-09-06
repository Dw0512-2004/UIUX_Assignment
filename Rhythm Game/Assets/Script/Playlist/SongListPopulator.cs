using UnityEngine;
using System.Collections.Generic;

public class SongListPopulator : MonoBehaviour {
    public Transform contentParent;   // 挂了VerticalLayoutGroup的那个物体
    public GameObject songItemPrefab;

    [Header("手动拖入所有歌曲的SongData")]
    public List<SongData> songs = new List<SongData>(); // 改成public,自己在Inspector填

    private List<SongListItem> spawnedItems = new List<SongListItem>();
    private SongData selectedSong;

    void Start() {
        PopulateList(); // 不用LoadSongs()了,songs已经在Inspector填好了
    }

    private void LoadSongs() {
        // 如果SongData放在Resources/Songs文件夹下
        songs = new List<SongData>(Resources.LoadAll<SongData>("Songs"));
        Debug.Log("读取到歌曲数量: " + songs.Count);
    }

    private void PopulateList() {
        foreach (var data in songs) {
            GameObject itemObj = Instantiate(songItemPrefab, contentParent);
            SongListItem item = itemObj.GetComponent<SongListItem>();
            item.Setup(data, OnSongClicked);
            spawnedItems.Add(item);
        }

        // 默认选中第一首
        if (spawnedItems.Count > 0) {
            OnSongClicked(songs[0]);
        }
    }

    private void OnSongClicked(SongData data) {
        selectedSong = data;

        foreach (var item in spawnedItems) {
            item.SetSelected(item.GetSongID() == data.songID);
        }

        // 这里可以顺便通知其他UI(比如顶部要不要显示"当前选中的歌曲"预览之类)
    }

    public SongData GetSelectedSong() => selectedSong;


    public void UpdateAllItemsDifficulty(string difficulty, Color bgColor) {
        foreach (var item in spawnedItems) {
            item.UpdateDifficultyDisplay(difficulty, bgColor);
        }
    }
}