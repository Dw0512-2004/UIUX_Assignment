using UnityEngine;
using System.Collections.Generic;

public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance;

    [Header("Prefab & Parent")]
    public GameObject notePrefab;
    public Transform tracksParent;   // Tracks 物体

    [Header("Track Settings")]
    public float[] trackXPositions = { -300, -100, 100, 300 };  // 四条轨道的X坐标
    public float hitLineY = -700f;   // 判定线Y坐标
    public float speed = 600f;       // 像素/秒（可微调手感）

    [Header("Music")]
    public AudioSource musicSource;

    // 谱面数据（这里固定一首歌，后面可替换）
    private List<NoteInfo> noteList;
    private List<NoteObject> activeNotes = new List<NoteObject>();
    private Queue<NoteObject> pool = new Queue<NoteObject>();

    private double dspStartTime;
    private bool songStarted = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 生成对象池
        for (int i = 0; i < 30; i++)
        {
            CreateNewPooledNote();
        }

        // 获取谱面
        noteList = SampleSong.GetNotes();
        // 按时间排序
        noteList.Sort((a, b) => a.time.CompareTo(b.time));
    }

    void CreateNewPooledNote()
    {
        GameObject obj = Instantiate(notePrefab, tracksParent);
        obj.SetActive(false);
        NoteObject note = obj.GetComponent<NoteObject>();
        pool.Enqueue(note);
    }

    public void StartSong()
    {
        if (songStarted) return;

        double startDsp = AudioSettings.dspTime + 0.1;
        musicSource.PlayScheduled(startDsp);
        dspStartTime = startDsp;
        songStarted = true;
    }

    void Update()
    {
        if (!songStarted) return;

        double currentTime = AudioSettings.dspTime - dspStartTime;

        // 生成即将出现的音符（提前2秒）
        while (noteList.Count > 0 && noteList[0].time <= currentTime + 2.0f)
        {
            SpawnNote(noteList[0]);
            noteList.RemoveAt(0);
        }

        // 更新所有活跃音符的位置
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            if (activeNotes[i] != null && activeNotes[i].gameObject.activeSelf)
                activeNotes[i].UpdatePosition((float)currentTime, speed, hitLineY);
            else
                activeNotes.RemoveAt(i);
        }
    }

    void SpawnNote(NoteInfo info)
    {
        NoteObject note = GetFromPool();
        if (note == null)
        {
            CreateNewPooledNote();
            note = GetFromPool();
        }

        // 设置初始X坐标
        RectTransform rect = note.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(trackXPositions[info.track], hitLineY + 2000f);
        note.Setup(info.time, info.track, info.type, info.holdDuration);
        activeNotes.Add(note);
    }

    NoteObject GetFromPool()
    {
        if (pool.Count > 0) return pool.Dequeue();
        return null;
    }

    public void ReturnToPool(NoteObject note)
    {
        if (note == null) return;
        note.gameObject.SetActive(false);
        pool.Enqueue(note);
    }

    /// <summary>
    /// 提供给 TrackInput 查找轨道上最近未判定的音符
    /// </summary>
    public NoteObject GetClosestUnjudged(int trackIndex)
    {
        NoteObject closest = null;
        float minDist = float.MaxValue;
        float currentTime = (float)(AudioSettings.dspTime - dspStartTime);

        foreach (var note in activeNotes)
        {
            if (note == null || !note.gameObject.activeSelf)
                continue;
            if (note.trackIndex != trackIndex)
                continue;

            float dist = Mathf.Abs(note.targetTime - currentTime);
            if (dist < minDist)
            {
                minDist = dist;
                closest = note;
            }
        }
        return closest;
    }

    /// <summary>
    /// 给 TrackInput 使用的当前歌曲时间
    /// </summary>
    public float CurrentSongTime =>
        songStarted ? (float)(AudioSettings.dspTime - dspStartTime) : 0f;
}