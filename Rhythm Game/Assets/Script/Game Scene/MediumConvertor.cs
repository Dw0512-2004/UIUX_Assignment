using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

public class MediumMidiToJson : MonoBehaviour
{
    [Header("文件名称设置")]
    public string midiFileName;
    public string outputJsonName; // 导出 Medium 谱面

    [Header("Medium 难度过滤参数")]
    public float slideThreshold = 0.3f; 
    
    [Tooltip("Medium 难度音符间距较大，过滤掉密集杂音")]
    public float minNoteInterval = 0.25f; // 间距设为 0.25 秒

    [ContextMenu("MIDI - JSON")]
    public void ConvertMidi()
    {
        string inputPath = Path.Combine(Application.dataPath, midiFileName);
        string outputPath = Path.Combine(Application.dataPath, outputJsonName);

        if (!File.Exists(inputPath))
        {
            Debug.LogError($"找不到 MIDI 文件: {inputPath}");
            return;
        }

        MidiFile midiFile = MidiFile.Read(inputPath);
        TempoMap tempoMap = midiFile.GetTempoMap();
        var allNotes = midiFile.GetNotes().OrderBy(n => n.TimeAs<MetricTimeSpan>(tempoMap).TotalMicroseconds);

        BeatmapData newBeatmap = new BeatmapData();
        newBeatmap.songName = midiFileName;
        newBeatmap.bpm = 120f; 
        newBeatmap.notes = new List<NoteData>();

        float lastValidTime = -1f;
        float[] laneAvailableTimes = new float[4] { 0f, 0f, 0f, 0f };
        float laneSafetyBuffer = 0.15f;

        foreach (Note note in allNotes)
        {
            MetricTimeSpan timeSpan = note.TimeAs<MetricTimeSpan>(tempoMap);
            float hitTime = (float)timeSpan.TotalMicroseconds / 1000000f;

            MetricTimeSpan lengthSpan = note.LengthAs<MetricTimeSpan>(tempoMap);
            float duration = (float)lengthSpan.TotalMicroseconds / 1000000f;

            int lane = note.NoteNumber % 4;
            int type = (duration >= slideThreshold) ? 1 : 0;

            // 【核心修复】：检查当前轨道是否被之前的长按滑条占用
            if (hitTime < laneAvailableTimes[lane])
            {
                continue; // 如果这个轨道还在跑长按，直接跳过当前音符，避免重叠！
            }

            // 全局最小时间间隔过滤（控制整体密度，保持你原有的降难度逻辑）
            if (lastValidTime != -1f && (hitTime - lastValidTime) < minNoteInterval)
            {
                continue; 
            }

            // 计算这个音符会占用轨道多久
            // 如果是长按(type=1)，占用时间 = hitTime + duration；如果是单点(type=0)，占用一个极短判定时间
            float occupyDuration = (type == 1) ? duration : 0.1f;
            laneAvailableTimes[lane] = hitTime + occupyDuration + laneSafetyBuffer;

            // 添加进谱面数据
            newBeatmap.notes.Add(new NoteData
            {
                hitTime = hitTime,
                laneIndex = lane,
                noteType = type,
                duration = duration
            });

            lastValidTime = hitTime; 
        }

        string jsonOutput = JsonUtility.ToJson(newBeatmap, true);
        File.WriteAllText(outputPath, jsonOutput);
        UnityEditor.AssetDatabase.Refresh();
        
        Debug.Log($"Medium 谱面生成成功！共生成 {newBeatmap.notes.Count} 个音符 -> {outputPath}");
    }
}