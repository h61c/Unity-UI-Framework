using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public abstract class BasePanelManager<T_Panel> : MonoBehaviour where T_Panel : Component
{
    #region JsonDefine
    public class JsonData
    {
        public class Data {
            public Dictionary<string, string> field = new();
            public void Add(string name, string value) { field.Add(name, value); }
            public string GetValue(string name)
            {
                if (!field.ContainsKey(name)) return null;
                return field[name];
            }
        }
        public Dictionary<string, Data> dict = new();
        public Data AddField(string name) {
            var data = new Data();
            dict.Add(name, data);
            return data;
        }
        public Data GetField(string name)
        {
            if (!dict.ContainsKey(name)) return null;
            return dict[name];
        }
    }
    #endregion
    protected static BasePanelManager<T_Panel>     instance { set; get; }
    protected static Dictionary<string, T_Panel>   panelDict{ set; get; } = new();
    [HideInInspector] public string uiFieldPath;
    public Canvas canvas;
    public bool awakeInitPanel = false;
    public bool debug = false;

    /// <summary>
    /// 以 Json 格式保存所有 UI 字段
    /// </summary>
    public virtual void SaveDataFromJson() { SaveDataFromJson(uiFieldPath); }
    
    /// <summary>
    /// 以 Json 格式保存所有字段
    /// </summary>
    /// <param name="path">保存位置</param>
    /// <returns></returns>
    public virtual int SaveDataFromJson(string path)
    {
        if (!File.Exists(path)) return -1;
        JsonData jsonData = new();
        JsonSerializerSettings setting = new();
        setting.Formatting = Formatting.Indented;
        foreach (var value in panelDict.Values)
        {
            var panel = value as BasePanel;
            var data = jsonData.AddField(panel.name);
            foreach (var text in panel.fields) data.Add(text.name, text.text);
        }
        string json = JsonConvert.SerializeObject(jsonData, setting);
        File.WriteAllText(path, json);
        Debug.Log("Save Json:\n" + json);
        return 1;
    }

    /// <summary>
    /// 以 Json 格式读取所有 UI 字段
    /// </summary>
    /// <param name="LoadDataFromJson(uiFieldPath"></param>
    public virtual void LoadDataFromJson() { LoadDataFromJson(uiFieldPath); }

    /// <summary>
    /// 以 Json 格式读取所有 UI 字段
    /// </summary>
    /// <param name="path">读取路径</param>
    /// <returns></returns>
    public virtual int LoadDataFromJson(string path)
    {
        if (!File.Exists(path)) return -1;
        string json  = File.ReadAllText(path);
        Debug.Log("Load Json:\n" + json);
        var jsonData = JsonConvert.DeserializeObject<JsonData>(json);
        foreach (var value in panelDict.Values)
        {
            var panel = value as BasePanel;
            var data  = jsonData.GetField(panel.name);
            foreach (var text in panel.fields) text.text = data.GetValue(text.name);
        }
        return 1;
    }

    /// <summary>
    /// 将面板注册到管理器
    /// </summary>
    /// <param name="panel"></param>
    /// <returns></returns>
    public static int Regist(T_Panel panel)
    {
        string _name = (panel as Object).name;
        if (panelDict.ContainsKey(_name))
             return -1;
        else panelDict.Add(_name, panel);
        return 1;
    }

    /// <summary>
    /// 打开面板
    /// </summary>
    /// <param name="panelName">面板名</param>
    /// <returns></returns>
    public static int Open(string panelName)
    {
        if (!panelDict.ContainsKey(panelName)) return 0;
        if (panelDict[panelName] == null)      return -1;
        var panel = panelDict[panelName] as BasePanel;
        panel.Open();
        return 1;
    }

    /// <summary>
    /// 关闭面板
    /// </summary>
    /// <param name="panelName">面板名</param>
    /// <returns></returns>
    public static int Close(string panelName)
    {
        if (!panelDict.ContainsKey(panelName)) return 0;
        if (panelDict[panelName] == null)      return -1;
        var panel = panelDict[panelName] as BasePanel;
        panel.Close();
        return 1;
    }

    /// <summary>
    /// 创建面板
    /// </summary>
    /// <param name="name">面板名</param>
    /// <param name="parent">面板父物体</param>
    /// <returns></returns>
    public static T_Panel NewPanel(string name, Transform parent = null)
    {
        parent = parent ? parent : instance.canvas.transform;
        var panel = Instantiate(new GameObject(name), parent).AddComponent<T_Panel>();
        Regist(panel);
        return panel;
    }

    protected void Awake()
    {
        if (awakeInitPanel)
        {
            GetPanels();
            InitPanels();
        }
    }

    /// <summary>
    /// 初始化所有面板
    /// </summary>
    public abstract void InitPanels();

    /// <summary>
    /// 注册所有面板
    /// </summary>
    public abstract void GetPanels();
}