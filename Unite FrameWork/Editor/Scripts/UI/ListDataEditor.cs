using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine.UI;

namespace Unite.Framework.UI
{
    [CustomEditor(typeof(ListData))]
    public class ListDataEditor : Editor
    {
        ListData list;
        bool apply;

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            Init();
            list.title = EditorGUILayout.TextField("标题", list.title);
            list.textParent = EditorGUILayout.ObjectField("父物体", list.textParent, typeof(Transform), true) as Transform;
            if (list.textParent == null) return;

            DrawDropdown();
            
            list.textList.DoLayoutList();
            if (GUILayout.Button("清空列表数据"))
            {
                list.textList.list.Clear();
                list.texts.ForEach(text => DestroyImmediate(text.gameObject));
                list.texts.Clear();
            }

            EditorGUILayout.BeginHorizontal();
            int selectIndex = list.textList.index;
            string selectElement = selectIndex >= 0 && selectIndex < list.Count ? list.texts[list.textList.index].name : "";
            var foldoutField = "属性: " + (list.drawSelect ? selectElement : "字段模板");
            var foldoutContnet = new GUIContent(foldoutField, "");
            list.propertyFoldout = EditorGUILayout.Foldout(list.propertyFoldout, foldoutContnet, true);
            if (!list.drawSelect && GUILayout.Button("应用"))
            {
                apply = true;
            }
            if (GUILayout.Button("切换"))
            {
                list.drawSelect = !list.drawSelect;
                list.alwaysUpdate = false;
            }
            EditorGUILayout.EndHorizontal();

            ++EditorGUI.indentLevel;
            if (list.propertyFoldout)
            {
                DrawProperty();
            }
            --EditorGUI.indentLevel;

        }

        void DrawDropdown()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("数据类型");
            list.typeDropdown = EditorGUILayout.DropdownButton(new GUIContent(list.dataType.ToString()), FocusType.Passive);
            if (list.typeDropdown)
            {
                var menu = new GenericMenu();
                var values = System.Enum.GetValues(typeof(ListData.DataType));
                foreach (var value in values)
                {
                    var type = (ListData.DataType)value;
                    menu.AddItem(new GUIContent(type.ToString()), false, () => { OnTypeChange(type); });
                }
                menu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();
        }

        void DrawProperty()
        {
            if (list.drawSelect)
            {
                DrawSelectedProperty();
            }
            else 
            {
                DrawTemplateProperty();
            }
        }

        void DrawSelectedProperty()
        {
            if (!list.propertyFoldout) return;
            var index = list.textList.index;
            var serializedObject = new SerializedObject(list.texts[index].GetComponent<RectTransform>());
            var rectTransform = serializedObject.GetIterator();
            rectTransform.NextVisible(true);
            do
            {
                EditorGUILayout.PropertyField(rectTransform);
            }
            while(rectTransform.NextVisible(false));
            serializedObject.ApplyModifiedProperties();

            serializedObject = new SerializedObject(list.texts[index]);
            var text = serializedObject.GetIterator();
            text.NextVisible(true);
            do
            {
                EditorGUILayout.PropertyField(text);
            }
            while(text.NextVisible(false));
            serializedObject.ApplyModifiedProperties();
        }

        void DrawTemplateProperty()
        {
            if (!list.propertyFoldout) return;
            list.alwaysUpdate = EditorGUILayout.Toggle("始终更新", list.alwaysUpdate);
            var rectTransform = list.templateRect.GetIterator();
            rectTransform.NextVisible(true);
            do
            {
                EditorGUILayout.PropertyField(rectTransform);
            }
            while(rectTransform.NextVisible(false));
            list.templateRect.ApplyModifiedProperties();

            var text = list.templateText.GetIterator();
            text.NextVisible(true);
            do
            {
                if (text.name == "m_Text") break;
                EditorGUILayout.PropertyField(text);
            }
            while(text.NextVisible(false));
            list.templateText.ApplyModifiedProperties();
            
            var sourceRect = list.template.GetComponent<RectTransform>();
            var sourceText = list.template.GetComponent<Text>();

            if (!(apply || list.alwaysUpdate))
            {
                return;
            }
            foreach (var target in list.texts)
            {
                var tx = target.text;
                var rt = target.GetComponent<RectTransform>();
                EditorUtility.CopySerialized(sourceRect, rt);
                EditorUtility.CopySerialized(sourceText, target);
                target.text = tx;
            }
            apply = false;
        }

        void Init()
        {
            if (list is null)
            {
                list = target as ListData;
            }

            if (list.texts is null)
            {
                list.texts = new List<Text>();
            }

            if (list.template is null)
            {
                var path = "Assets/Unite Framework/Editor/Prefabs/UITextTemplate.prefab";
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                list.template = PrefabUtility.InstantiatePrefab(obj) as GameObject;
            }
            if (list.templateRect is null)
            {
                list.templateRect = new SerializedObject(list.template.GetComponent<RectTransform>());
            }
            if (list.templateText is null)
            {
                list.templateText = new SerializedObject(list.template.GetComponent<Text>());
            }

            if (list.data is null) 
            {
                list.data = new List<string>();
            }

            if (list.textList is null) 
            {
                var textList = list.textList = new ReorderableList(list.data, typeof(string));
                
                textList.drawHeaderCallback = (rect) =>
                {
                    EditorGUI.LabelField(rect, "列表数据");
                };
                textList.drawElementCallback = (rect, index, isActived, isFocused) =>
                {
                    var nameRect = new Rect(rect);
                    nameRect.width /= 2;
                    var dataRect = new Rect(nameRect);
                    dataRect.x += dataRect.width;

                    EditorGUI.LabelField(nameRect, list.texts[index].name);
                    list[index] = EditorGUI.TextField(dataRect, list[index]);
                };
                textList.onAddCallback = (rList) =>
                {
                    list.Add();
                    rList.list.Add("");
                };
                textList.onRemoveCallback = (rList) =>
                {
                    list.RemoveAt(rList.index);
                    rList.list.RemoveAt(rList.index);
                };
                textList.onReorderCallbackWithDetails = (rList, oldIndex, newIndex) =>
                {
                    list.texts[oldIndex].transform.SetSiblingIndex(newIndex);

                    var temp = list.texts[oldIndex];
                    var step = oldIndex < newIndex ? 1 : -1;
                    while (oldIndex != newIndex)
                    {
                        list.texts[oldIndex] = list.texts[oldIndex + step];
                        oldIndex += step;
                    }
                    list.texts[newIndex] = temp;
                };
            }
        }

        public void OnTypeChange(ListData.DataType type) => list.dataType = type;
    }
}
