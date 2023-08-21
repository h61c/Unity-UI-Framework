using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Unite.Framework;

namespace Unite.Framework.UI
{
    [CustomEditor(typeof(UIManager), true)]
    public class UIManagerEditor : Editor
    {
        UIManager uiManager;
        ReorderableList canvas;
        ReorderableList popup;
        List<bool> treeFoldout;
        public override void OnInspectorGUI()
        {
            uiManager = target as UIManager;

            Init();
            if (treeFoldout is null) treeFoldout = new List<bool>();
            uiManager.configPath = EditorGUILayout.TextField("配置文件路径", uiManager.configPath);
            canvas.DoLayoutList();
            popup.DoLayoutList();
            //DrawTree(canvas[0].canvas.transform);
        }

        void Init()
        {
            if (uiManager.canvas is null)   uiManager.canvas = new List<UIManager.ConfigPathData>();
            if (uiManager.popup is null)    uiManager.popup = new  List<UIManager.ConfigPathData>();
            if (canvas is null) canvas = new ReorderableList(uiManager.canvas, typeof(Canvas)) {                
                drawHeaderCallback = (rect) =>
                {
                    EditorGUI.LabelField(rect, "UI界面");
                },
                drawElementCallback = (rect, index, isActived, isFocused) =>
                {
                    if (index >= uiManager.canvas.Count) return;
                    var width = rect.width / 4;
                    var curCanvas = uiManager.canvas[index];
                    var curRect = new Rect(rect);
                    curRect.width = width;
                    GUILayout.BeginHorizontal();
                    if (curCanvas.canvas != null)
                        curCanvas.canvas.name = EditorGUI.TextField(curRect, curCanvas.canvas.name);
                    else EditorGUI.TextField(curRect, "");
                    curRect.x += width;
                    curRect.width = width;
                    uiManager.canvas[index].canvas = EditorGUI.ObjectField(curRect, curCanvas.canvas, typeof(Canvas), true) as Canvas;
                    curRect.x += width;
                    curRect.width = rect.width - curRect.x;
                    
                    GUILayout.EndHorizontal();
                },
                onAddCallback = (list) =>
                {
                    uiManager.canvas.Add(new UIManager.ConfigPathData());
                },
                onRemoveCallback = (list) =>
                {
                    if (list.index >= uiManager.canvas.Count) return;
                    uiManager.canvas.RemoveAt(list.index);
                },
            };
            if (popup is null) popup = new ReorderableList(uiManager.popup, typeof(Canvas)) {

            };
        }
        
        void DrawTree(Transform root)
        {
            var index = 0;
            //  root 根节点
            //  Order 遍历方式
            //      - node当前节点
            //      - level 当前层级
            //      - 退出当前节点遍历标志
            UniteAlgorithm.TreeTraverse(root, PreOrder: (Transform node, int level, out bool returnFlag) =>
            {
                returnFlag = false;
                EditorGUI.indentLevel = level;

                var content = node.name;
                var isDataUIElement = false;
                if (isDataUIElement = node.gameObject.TryGetComponent<IUIElement>(out var element))
                {
                    content += " *reloadable*";
                }

                if (index >= treeFoldout.Count)
                {
                    treeFoldout.Add(false);
                }

                if (node.childCount > 0 && !isDataUIElement)
                {
                    bool foldout = treeFoldout[index] = EditorGUILayout.Foldout(treeFoldout[index], content);
                    if (!foldout) returnFlag = true;
                }
                else
                {
                    EditorGUILayout.LabelField(content);
                    returnFlag = isDataUIElement; 
                } 
                ++index;
            });
        }

        [MenuItem("GameObject/UniteFramework/UI/UIManager", false, 21)]
        public static void Create()
        {
            var element = new GameObject().AddComponent<UIManager>();
            element.transform.parent = Selection.activeTransform;
            element.name = "UIManager";
        }
    }
}