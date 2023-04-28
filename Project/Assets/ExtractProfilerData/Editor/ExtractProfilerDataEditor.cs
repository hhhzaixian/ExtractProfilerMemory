using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class ExtractProfilerDataEditor: EditorWindow
{

    private float _memorySize = 1f;
    private int _memoryDepth = 1;

    private float _cpuUsage = 0.0001f;
    private int _cpuUsageDepth = -1;

    public static ExtractProfilerDataEditor Window;

    [MenuItem("Window/Extract Profiler Data")]
    public static void ShowWindow()
    {
        EditorApplication.ExecuteMenuItem("Window/Analysis/Profiler");
        if (Window == null)
        {
            Window = CreateInstance<ExtractProfilerDataEditor>();
        }
        Window.Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField("Current Target: " + ProfilerDriver.GetConnectionIdentifier(ProfilerDriver.connectedProfiler));

        if (GUILayout.Button("Take Sample"))
        {
            TakeSample();
        }

        _memorySize = EditorGUILayout.FloatField("Memory Size(MB) >= ", _memorySize);
        _memoryDepth = EditorGUILayout.IntField("Memory Depth(>=1)", _memoryDepth);

        if (GUILayout.Button("Extract Memory"))
        {
            if (_memoryDepth <= 0 )
            {
                _memoryDepth = 1;
            }
            ExtractMemory(_memorySize, _memoryDepth - 1);
        }

        EditorGUILayout.EndVertical();
        
        EditorGUILayout.BeginVertical();
        _cpuUsage = EditorGUILayout.FloatField("Cpu Usage(百分数) >= ", _cpuUsage);
        _cpuUsageDepth = EditorGUILayout.IntField("Cpu Usage Depth(>=1)", _cpuUsageDepth);
        if (GUILayout.Button("Extract Cpu Usage"))
        {
            ExtractCpuUsage(_cpuUsage, _cpuUsageDepth);
        }
        EditorGUILayout.EndVertical();
    }
    
    private MemoryElement _memoryElementRoot;
    private void ExtractMemory(float memSize, int memDepth)
    {
        var filterSize = memSize * 1024 * 1024;
        var parent = Directory.GetParent(Application.dataPath);
        var outputPath = string.Format("{0}/TempCaches/MemoryDetailed{1:yyyy_MM_dd_HH_mm_ss}.txt", parent.FullName, DateTime.Now);
        var outputDir = Path.GetDirectoryName(outputPath);
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }
        
        File.Create(outputPath).Dispose();
        _memoryElementRoot = ProfilerWindow.GetMemoryDetailRoot(memDepth, filterSize);

        if (null != _memoryElementRoot)
        {
            var writer = new StreamWriter(outputPath);
            writer.WriteLine("Memory Size: >= {0}MB", _memorySize);
            writer.WriteLine("Memory Depth: {0}", _memoryDepth);
            writer.WriteLine("Current Target: {0}", ProfilerDriver.GetConnectionIdentifier(ProfilerDriver.connectedProfiler));
            writer.WriteLine("**********************");
            ProfilerWindow.WriteMemoryDetail(writer, _memoryElementRoot);
            writer.Flush();
            writer.Close();
        }
        
        Process.Start(outputPath);
    }
    
    private List<CpuUsageElement> _cpuUsageRows;
    private void ExtractCpuUsage(float minTimeMs, int depth)
    {
        var parent = Directory.GetParent(Application.dataPath);
        var outputPath = string.Format("{0}/TempCaches/CpuUsageDetailed{1:yyyy_MM_dd_HH_mm_ss}.txt", parent.FullName, DateTime.Now);
        var outputDir = Path.GetDirectoryName(outputPath);
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }
        File.Create(outputPath).Dispose();
        _cpuUsageRows = ProfilerWindow.GetCpuUsageDetailRoot();

        if (null != _cpuUsageRows)
        {
            var writer = new StreamWriter(outputPath);
            writer.WriteLine("CpuUsage Size: >= {0}MB", _cpuUsage);
            writer.WriteLine("CpuUsage Depth: {0}", _cpuUsageDepth);
            writer.WriteLine("Current Target: {0}", ProfilerDriver.GetConnectionIdentifier(ProfilerDriver.connectedProfiler));
            writer.WriteLine("**********************");
            for (int i = 0; i < _cpuUsageRows.Count; i++)
            {
                var element = _cpuUsageRows[i];
                if (element.TimeMs > minTimeMs && (depth < 0 || element.m_Depth < depth))
                {
                    ProfilerWindow.WriteCpuDetail(writer, element);
                }
            }
            writer.Flush();
            writer.Close();
        }
        
        Process.Start(outputPath);
    }

    private static void TakeSample()
    {
        ProfilerWindow.RefreshMemoryData();
    }
}
