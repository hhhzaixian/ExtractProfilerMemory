using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class CpuUsageElement : IComparable<CpuUsageElement>
{
    public int id;
    public string displayName;
    public string[] m_StringProperties;
    public int m_Depth;
    
    public string GCAlloc;
    public float TimeMs;
    public float SelfMs;

    private CpuUsageElement()
    {
    }

    public void ParseColumnValue(Dynamic srcElement)
    {
        id = System.Convert.ToInt32(ProfilerWindow.GetFieldInType(srcElement.InnerType, srcElement.InnerObject,"m_ID", "TreeViewItem"));
        displayName = System.Convert.ToString(ProfilerWindow.GetFieldInType(srcElement.InnerType, srcElement.InnerObject,"m_DisplayName", "TreeViewItem"));
        m_Depth = System.Convert.ToInt32(ProfilerWindow.GetFieldInType(srcElement.InnerType, srcElement.InnerObject,"m_Depth", "TreeViewItem"));
        m_StringProperties = (string[])ProfilerWindow.GetFieldInType(srcElement.InnerType, srcElement.InnerObject,"m_StringProperties", "FrameDataTreeViewItem");

        // foreach (var fieldInfo in srcElement.InnerType.GetFields())
        // {
        //     Debug.Log($"fieldInfo: {fieldInfo.Name} {fieldInfo.Attributes}");
        // }
        // foreach (var methodInfo in srcElement.InnerType.GetMethods())
        // {
        //     Debug.Log($"methodInfo: {methodInfo.Name} {methodInfo.Attributes}");
        // }
        
        if (m_StringProperties != null && m_StringProperties.Length > 6)
        {
            GCAlloc = m_StringProperties[4];
            TimeMs = float.Parse(m_StringProperties[5]);
            SelfMs = float.Parse(m_StringProperties[6]);
        }
    }

    public static CpuUsageElement Create(Dynamic srcElement)
    {
        var dstElement = new CpuUsageElement { m_Depth = 0 };
        dstElement.ParseColumnValue(srcElement);

        return dstElement;
    }


    public override string ToString()
    {
        var resultString = string.Format(new string('\t', m_Depth) + "{4}, {0}, {1}, {2}, {3}", displayName, TimeMs, SelfMs, GCAlloc, m_Depth);
        return resultString;
    }

    public int CompareTo(CpuUsageElement other)
    {
        if (other.id != id)
        {
            return (int) (other.id - id);
        }

        if (string.IsNullOrEmpty(displayName)) return -1;
        return !string.IsNullOrEmpty(other.displayName) ? string.Compare(displayName, other.displayName, StringComparison.Ordinal) : 1;

    }
}