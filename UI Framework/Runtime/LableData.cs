using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(LableData))]
public class LableDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var data = this.target as LableData;
        if (GUILayout.Button("注册所有标签文本")) 
        {
            data.GetLables();
        }
    }
}

public class LableData : MonoBehaviour
{
    /// <summary>
    /// 所有标签文本所在的父物体
    /// </summary>
    public Transform parent;
    /// <summary>
    /// 筛选 Tag，当物体 tag 不为空时只会获取该拥有该 tag 的物体
    /// </summary>
    public string    targetTag;
    public List<Text> information;
    void Start()
    {
    }

    /// <summary>
    /// 获取所有标签文本
    /// </summary>
    public virtual void GetLables()
    {
        if (information == null) information = new List<Text>();
        information.Clear();
        foreach (var text in parent.GetComponentsInChildren<Text>())
        {
            if (targetTag != "" && text.tag != targetTag) continue;
            information.Add(text);
        }
    }

    /// <summary>
    /// 更新所有标签
    /// 长度小于 information 时返回-1
    /// 长度大于 information 时返回0
    /// 长度等于 information 时返回1
    /// </summary>
    /// <param name="data">更新数据</param>
    /// <returns></returns>
    public virtual int UpdateInformation(string[] data)
    {
        if (data.Length < information.Count) return -1;
        else if (data.Length > information.Count) return 0;
        var index = 0;
        foreach (var info in information) info.text = data[index++];
        return 1;
    }
}
