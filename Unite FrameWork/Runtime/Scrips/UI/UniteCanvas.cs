using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Unite.Framework;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace Unite.Framework.UI
{
    public class SourceHelper
    {
        public static void HandleLoad(IUIElement element, object content)
        {
            switch (element.sourceType)
            {
                case "ListData":
                    var json = JsonConvert.SerializeObject(content, Formatting.Indented);
                    Debug.Log(json);
                    var list = JsonConvert.DeserializeObject<string[]>(json);
                    element.LoadSource<string[]>(list);
                    break;
                case "TableData":
                    json = JsonConvert.SerializeObject(content);
                    var table = JsonConvert.DeserializeObject<string[][]>(json);
                    element.LoadSource<string[][]>(table);
                    break;
            }
        }
    }

    public class UniteCanvas : MonoBehaviour, IUIElement
    {
        public string sourceType { get; } = "Canvas";
        public string sourceReference { get; } = "";
        public bool Reloadable;
        public bool reloadable => Reloadable;
        public UISerializedObject sourceObject => Serialize();

        public bool LoadSource<T>(T content)
        {
            if (content is not UIRuntime) return false;

            var elementStack = new Stack<UIRuntime>();
            elementStack.Push(content as UIRuntime);
            UniteAlgorithm.TreeTraverse(
                root: this.transform, 
                leftToRight: false,
                PostOrder: (Transform node, int depth, out bool returnFlag) => {
                    returnFlag = false;
                    var element = elementStack.Pop();
                    Debug.Log(element.property.name);
                    if (node.gameObject.TryGetComponent<IDataUIElement>(out var data))
                    {
                        SourceHelper.HandleLoad(data, element);
                        return;
                    }
                    foreach (var child in element.children)
                    {
                        elementStack.Push(child);
                    }
                }
            );

            return true;
        }

        void Start()
        {
            var file = Application.dataPath + "/Canvas_Config/" + this.name + ".config";
            var json = File.ReadAllText(file);
            var content = JsonConvert.DeserializeObject<UIRuntime>(json);
            LoadSource<UIRuntime>(content);
        }

        void Update()
        {
            
        }

        UISerializedObject Serialize()
        {
            var content = new UIRuntime();
            var elementStack = new Stack<UIRuntime>();
            var parent = new List<UIRuntime>();
            elementStack.Push(content);
            UniteAlgorithm.TreeTraverse(
                root: this.transform,
                leftToRight: false,
                PreOrder: (Transform node, int depth, out bool returnFalg) =>
                {
                    returnFalg = false;
                    var element = elementStack.Pop();
                    element.property = new UISerializedObject() { name = node.name };
                    element.children = new List<UIRuntime>();
                    if (element != content && node.TryGetComponent<IUIElement>(out var data))
                    {
                        element.property = data.sourceObject;
                        returnFalg = true;
                        return;
                    }
                    //Debug.Log("have a baby" + node.name);
                    foreach (var child in node)
                    {
                        var obj = new UIRuntime();
                        element.children.Add(obj);
                        elementStack.Push(obj);
                    }
                }
            );
            return new UISerializedObject() { name = this.name, content = content };
        }
    }
}