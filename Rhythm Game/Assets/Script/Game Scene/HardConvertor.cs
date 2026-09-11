using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq; // 引入 LINQ 以支持按时间排序
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;


public class HardConvertor : MonoBehaviour
{
    [Header("文件名称设置")]
    public string midiFileName; // 你的 MIDI 文件名
    public string outputJsonName; // 导出的 JSON 文件名

    [Header("映射与降难度参数")]
    public float slideThreshold = 0.25f; // 持续时间超过此值判定为长按
    
    [Tooltip("最小音符间隔（秒）：值越大过滤掉的密集音符越多，谱面越简单稀疏。Hard可设为0.1~0.15，Easy可设为0.4~0.5")]
    public float minNoteInterval = 0.25f; // 降难度核心过滤参数

    // 在 Unity 编辑器属性面板的组件名称旁右键点击即可执行
    [ContextMenu("MIDI - JSON")]
    public void ConvertMidi()
    {
        string inputPath = Path.Combine(Application.dataPath, midiFileName);
        string outputPath = Path.Combine(Application.dataPath, outputJsonName);

        if (!File.Exists(inputPath))
        {
            Debug.LogError($"找不到 MIDI 文件: {inputPath}，请确保它已放入 Assets 文件夹中！");
            return;
        }

        MidiFile midiFile = MidiFile.Read(inputPath);
        TempoMap tempoMap = midiFile.GetTempoMap();
        
        // 获取所有音符，并严格按绝对时间先后顺序排序，确保间隔过滤不会乱序
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
        
        // 刷新 Unity 资源库，让生成的 JSON 立即出现在项目面板中
        UnityEditor.AssetDatabase.Refresh();
        
        Debug.Log($"转换成功！经过密度过滤后，共生成 {newBeatmap.notes.Count} 个音符，已自动保存至: {outputPath}");
    }
}