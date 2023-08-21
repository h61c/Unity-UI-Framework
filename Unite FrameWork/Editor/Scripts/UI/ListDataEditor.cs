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
            Init();
            list.title = EditorGUILayout.TextField("标题", list.title);
            list.layoutType = (LayoutType)EditorGUILayout.EnumPopup("布局类型", list.layoutType);
            DrawDataTypeDropdown();
            
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
            string foldoutField = "属性: " + (list.drawSelect ? selectElement : "字段模板");
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

        void DrawMap()
        {
            EditorGUILayout.LabelField("Map");
            ++EditorGUI.indentLevel;
            foreach (var pair in list.map)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("key", pair.Key);
                EditorGUILayout.LabelField("value", pair.Value.ToString());
                GUILayout.EndHorizontal();
            }
            --EditorGUI.indentLevel;
        }

        void DrawDataTypeDropdown()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("数据类型");
            list.typeDropdown = EditorGUILayout.DropdownButton(new GUIContent(list.dataType.ToString()), FocusType.Passive);
            if (list.typeDropdown)
            {
                var menu = new GenericMenu();
                var values = System.Enum.GetValues(typeof(DataType));
                foreach (var value in values)
                {
                    var type = (DataType)value;
                    menu.AddItem(new GUIContent(type.ToString()), false, () => { OnTypeChange(type); });
                }
                menu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();
        }

        void DrawProperty()
        {
            try 
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
            catch
            {
                EditorGUILayout.LabelField("属性序列化出错");
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
            if (!list.propertyFoldout || list.Count is 0) return;
            list.alwaysUpdate = EditorGUILayout.Toggle("始终更新", list.alwaysUpdate);
            var rtSerializedObject = new SerializedObject(list.texts[0].rectTransform);
            var rectTransform = rtSerializedObject.GetIterator();
            rectTransform.NextVisible(true);
            do
            {
                EditorGUILayout.PropertyField(rectTransform);
            }
            while(rectTransform.NextVisible(false));
            rtSerializedObject.ApplyModifiedProperties();

            var textSerializedObject = new SerializedObject(list.texts[0]);
            var text = textSerializedObject.GetIterator();
            text.NextVisible(true);
            do
            {
                if (text.name == "m_Text") break;
                EditorGUILayout.PropertyField(text);
            }
            while(text.NextVisible(false));
            textSerializedObject.ApplyModifiedProperties();
            
            var sourceRect = list.texts[0].GetComponent<RectTransform>();
            var sourceText = list.texts[0];

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
            if (list is null) list = target as ListData;

            if (list.textList is not null) return;

            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i] is not null) continue;
                list.RemoveAt(i);
                break;
            }

            var textList = list.textList = new ReorderableList(list.data, typeof(string))
            {
                drawHeaderCallback = (rect) =>
                {
                    EditorGUI.LabelField(rect, "列表数据");
                },
                drawElementCallback = (rect, index, isActived, isFocused) =>
                {
                    var nameRect = new Rect(rect);
                    nameRect.width /= 2;
                    var dataRect = new Rect(nameRect);
                    dataRect.x += dataRect.width;

                    nameRect.width /= 3;
                    list.texts[index].name = EditorGUI.TextField(nameRect, list.texts[index].name);
                    list.OnPrimaryChange(list.texts[index].name, index);
                    list[index] = EditorGUI.TextField(dataRect, list[index]);
                },
                onAddCallback = (rList) =>
                {
                    list.Add();
                },
                onRemoveCallback = (rList) =>
                {
                    list.RemoveAt(rList.index);
                },
                onReorderCallbackWithDetails = (rList, oldIndex, newIndex) =>
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
                }
            };
            
        }

        public void OnTypeChange(DataType type) => list.dataType = type;

        [MenuItem("GameObject/UniteFramework/UI/ListData", false, 11)]
        public static void Create()
        {
            var element = new GameObject().AddComponent<ListData>();
            element.transform.parent = Selection.activeTransform;
            element.name = "ListData";
        }
    }
}
