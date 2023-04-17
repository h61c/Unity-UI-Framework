using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(StandardPanel))]
public class StandardPanelEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var panel = this.target as StandardPanel;
        base.OnInspectorGUI();
        if (GUILayout.Button("格式化按钮文本")) {
            panel.FromatButtonText();
        }
    }
}

/// <summary>
/// 基础面板组件
/// </summary>
public class StandardPanel : BasePanel
{
    /// <summary>
    /// 打开事件
    /// </summary>
    public UnityEvent onOpen;
    /// <summary>
    /// 关闭事件
    /// </summary>
    public UnityEvent onClose;
    void Start()
    {
        
    }

    public override void Open()
    {
        onOpen.Invoke();
    }

    public override void Close()
    {
        onClose.Invoke();
    }

    public override void Init()
    {
        if (debug) print($"{this.name}: Initiatate.");
        fields  = parent.GetComponentsInChildren<Text>().ToList();
        buttons = parent.GetComponentsInChildren<Button>().ToList();
        initialized = true;
    }

    /// <summary>
    /// 格式化按钮作为父物体的 UI 字段名称
    /// </summary>
    public void FromatButtonText()
    {
        foreach (var field in parent.GetComponentsInChildren<Text>())
        {
            var parent = field.transform.parent;
            if (parent == null) continue;
            print(parent.name);
            if (parent.GetComponent<Button>() != null && !field.name.Contains(parent.name))
                field.name = parent.name + "/" + field.name;
        }
    }
}
