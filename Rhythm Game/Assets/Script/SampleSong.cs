using System.Collections.Generic;

[System.Serializable]
public class NoteInfo
{
    public float time;
    public int track;
    public NoteType type;
    public float holdDuration; // 普通音符为0
}

public enum NoteType
{
    Normal,
    Hold
}

public static class SampleSong
{
    public static List<NoteInfo> GetNotes()
    {
        List<NoteInfo> notes = new List<NoteInfo>();

        float bpm = 120f;
        float interval = 60f / bpm / 4f; // 16分音符间隔 = 0.125秒
        float startTime = 2f; // 从第2秒开始

        // 生成一段4小节的简单节奏，每个音随机分配轨道
        for (int i = 0; i < 64; i++) // 64个16分音符 = 4小节*16
        {
            float t = startTime + i * interval;
            int track = i % 4;  // 简单循环轨道
            notes.Add(new NoteInfo
            {
                time = t,
                track = track,
                type = NoteType.Normal,
                holdDuration = 0
            });
        }

        // 加一个 Hold 音符测试
        notes.Add(new NoteInfo
        {
            time = 6.0f,
            track = 2,
            type = NoteType.Hold,
            holdDuration = 1.5f
        });

        return notes;
    }
}