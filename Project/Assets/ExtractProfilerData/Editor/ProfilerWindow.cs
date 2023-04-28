
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
#if UNITY_2019_4_OR_NEWER
using UnityEngine.Profiling;
#else
using UnityEditorInternal;
#endif
using UnityEngine;

public static class ProfilerWindow
{
    private static List<Dynamic> _windows = null;

    private static Dynamic _GetWindow(ProfilerArea area)
    {
        if (null == _windows)
        {
            var dynamicType = new DynamicType(typeof(EditorWindow));
            var type = dynamicType.GetType("UnityEditor.ProfilerWindow");
            var list = type.PrivateStaticField<IList>("m_ProfilerWindows");
            _windows = new List<Dynamic>();
            foreach (var window in list)
            {
                _windows.Add(new Dynamic(window));
            }
        }
        foreach (var dynamic in _windows)
        {
            var val = (ProfilerArea)dynamic.PrivateInstanceField("m_CurrentArea");
            if (val == area)
            {
                return dynamic;
            }
        }
        return null;
    }
    

    public static MemoryElement GetMemoryDetailRoot(int filterDepth, float filterSize)
    {
        var windowDynamic = _GetWindow(ProfilerArea.Memory);
        var modules = (System.Array)windowDynamic?.PrivateInstanceField("m_ProfilerModules");
        var memoryProfilerModule = modules.GetValue((int)ProfilerArea.Memory);
        var memoryProfilerModuleDy = new Dynamic(memoryProfilerModule); 
        var m_ReferenceListView = memoryProfilerModuleDy.PrivateInstanceField("m_MemoryListView");
        var m_ReferenceListViewDy = new Dynamic(m_ReferenceListView); 
        var rootDynamic = m_ReferenceListViewDy.PrivateInstanceField("m_Root");
        return rootDynamic != null ? MemoryElement.Create(new Dynamic(rootDynamic), 0, filterDepth, filterSize) : null;
    }

    public static void WriteMemoryDetail(StreamWriter writer, MemoryElement root)
    {
        if (null == root) return;
        writer.WriteLine(root.ToString());
        foreach (var element in root.children)
        {
            if (null != element)
            {
                WriteMemoryDetail(writer, element);
            }
        }
    }

    public static void RefreshMemoryData()
    {
        var windowDynamic = _GetWindow(ProfilerArea.Memory);
        var modules = (System.Array)windowDynamic?.PrivateInstanceField("m_ProfilerModules");
        var memoryProfilerModule = modules.GetValue((int)ProfilerArea.Memory);
        var memoryProfilerModuleDy = new Dynamic(memoryProfilerModule); 
        
        if (null != memoryProfilerModuleDy)
        {
            memoryProfilerModuleDy.CallPrivateInstanceMethod("RefreshMemoryData");
        }
        else
        {
            Debug.Log("请打开Profiler 窗口的 Memory 视图");
        }
    }

    #region CPU Usage

    
    public static List<CpuUsageElement> GetCpuUsageDetailRoot()
    {
        var windowDynamic = _GetWindow(ProfilerArea.CPU);
        var modules = (System.Array)windowDynamic?.PrivateInstanceField("m_ProfilerModules");
        var module = modules.GetValue((int)ProfilerArea.CPU);
        var moduleDy = new Dynamic(module); 
        var view = moduleDy.PrivateInstanceField("m_FrameDataHierarchyView");//ProfilerFrameDataHierarchyView
        var viewDy = new Dynamic(view);
        var treeView = viewDy.PrivateInstanceField("m_TreeView");//ProfilerFrameDataTreeView
        var treeViewDy = new Dynamic(treeView);
        var m_DataSource =  GetFieldInType(treeViewDy.InnerType, treeViewDy.InnerObject, "m_DataSource", "TreeView");
        var m_DataSourceDy = new Dynamic(m_DataSource);
        var root =  (IList)GetFieldInType(m_DataSourceDy.InnerType, m_DataSourceDy.InnerObject, "m_Rows", "TreeViewDataSource");

        List<CpuUsageElement> list = new List<CpuUsageElement>();
        foreach (var item in root) //FrameDataTreeViewItem
        {
            CpuUsageElement element = CpuUsageElement.Create(new Dynamic(item));
            list.Add(element);
        }

        return list;
    }
    
    public static object GetFieldInType(Type type, object obj, string methodName, string typeStr)
    {
        int maxLoop = 10;
        int loopIndex = 0;
        while (++loopIndex < maxLoop && type != null)
        {
            if (type.Name == typeStr)
            {
                var field = type.GetField(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.GetProperty| BindingFlags.Public);
                var ret =  field != null ? field.GetValue(obj) : null;
                return ret;
            }
            type = type.BaseType;
        }

        return null;
    }
    
    public static object GetMethodInType(Type type, object obj, string methodName, string typeStr)
    {
        int maxLoop = 10;
        int loopIndex = 0;
        while (++loopIndex < maxLoop && type != null)
        {
            if (type.Name == typeStr)
            {
                var methodInfo = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.GetProperty| BindingFlags.Public);
                var ret =  methodInfo != null ? methodInfo.Invoke(obj, null) : null;
                return ret;
            }
            type = type.BaseType;
        }

        return null;
    }
    
    public static void WriteCpuDetail(StreamWriter writer, CpuUsageElement root)
    {
        if (null == root) return;
        writer.WriteLine(root.ToString());
    }
    #endregion
}

public class DynamicType
{
    private readonly Assembly _assembly;

    public DynamicType(Type type)
    {
        _assembly = type.Assembly;
    }

    public Dynamic GetType(string typeName)
    {
        return new Dynamic(_assembly.GetType(typeName));
    }
}
