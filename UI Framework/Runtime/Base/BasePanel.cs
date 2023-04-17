using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class BasePanel : MonoBehaviour
{
    /// <summary>
    /// 该面板下所有组件的父物体
    /// </summary>
    public Transform    parent;
    /// <summary>
    /// UI 字段
    /// </summary>
    public List<Text>   fields;
    /// <summary>
    /// UI 按钮
    /// </summary>
    public List<Button> buttons;
    public bool         initialized = false;
    public bool         debug       = false;

    /// <summary>
    /// 打开面板
    /// </summary>
    public abstract void Open();
    /// <summary>
    /// 关闭面板
    /// </summary>
    public abstract void Close();
    /// <summary>
    /// 初始化面板
    /// </summary>
    public abstract void Init();

    /// <summary>
    /// 新建该面板下的组件
    /// </summary>
    /// <param name="name">组件名</param>
    /// <param name="parent">组件父物体</param>
    /// <typeparam name="T">组件类型</typeparam>
    /// <returns></returns>
    public T NewComponent<T>(string name, Transform parent = null) where T : Component
    {
        parent = parent ? parent : this.transform;
        var component = Instantiate(new GameObject(), parent).AddComponent<T>();
        Init();
        return component;
    }
}
