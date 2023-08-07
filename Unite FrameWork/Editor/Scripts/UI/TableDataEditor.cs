using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Runtime.InteropServices;
using System.Drawing.Printing;
using System.Data;
using System.Linq;
using Unity.VisualScripting;
using System.Reflection.Emit;

namespace Unite.Framework.UI
{
    [CustomEditor(typeof(TableData))]
    public class TableDataEditor : Editor
    {
        TableData table;
        Editor previourEditor;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            Init();
            table.parent = EditorGUILayout.ObjectField("父物体", table.parent, typeof(Transform), true) as Transform;
            if (table.parent is null) return;
            table.drawList.DoLayoutList();
            DrawSelect();
        }

        void DrawSelect()
        {
            if (table.Count == 0) return;

            var index = table.drawList.index;
            Editor.CreateCachedEditor(table[index], typeof(ListDataEditor), ref previourEditor);
            ++EditorGUI.indentLevel;
            if (table.propertyFoldout = EditorGUILayout.Foldout(table.propertyFoldout, "属性:", true))
                previourEditor.OnInspectorGUI();
            --EditorGUI.indentLevel;
        }

        void Init()
        {
            if (table is null)
            {
                table = target as TableData;
                
            }

            if (table.data is null)
            {
                table.data = new List<ListData>();
            }
            
            if (table.drawList is null)
            {
                table.drawList = new ReorderableList(table.data, typeof(ListData));

                table.drawList.drawHeaderCallback = (rect) =>
                {
                    if (table.Count == 0) return;
                    var curRect = new Rect(rect);
                    var width = curRect.width /= table[0].Count + 1;
                    curRect.x = width;
                    EditorGUILayout.BeginHorizontal();
                    for (int i = 0; i < table[0].Count; ++i)
                    {
                        EditorGUI.LabelField(curRect, i.ToString());
                        curRect.x += width;
                    }
                    EditorGUILayout.EndHorizontal();
                };
                table.drawList.drawElementCallback = (rect, index, isActived, isFocusd) =>
                {
                    var curRect = new Rect(rect);
                    var width = curRect.width /= table[0].Count + 1;
                    EditorGUI.LabelField(curRect, table[index].title);

                    curRect.x = width;
                    for (int i = 0; i < table[index].Count; ++i)
                    {
                        table[index][i] = EditorGUI.TextField(curRect, table[index][i]);
                        curRect.x += width;
                    }
                };
                table.drawList.onReorderCallbackWithDetails = (list, oldIndex, newIndex) =>
                {
                    table[newIndex].transform.SetSiblingIndex(newIndex);
                };
            }
        }
    }
}