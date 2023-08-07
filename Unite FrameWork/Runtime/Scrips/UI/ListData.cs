using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditorInternal;
using UnityEditor;
using UnityEditor.Build.Player;
using JetBrains.Annotations;

namespace Unite.Framework.UI
{
    public class ListData : MonoBehaviour, IUIDataCollection<string>
    {
        private class FormatStragety
        {
            Dictionary<int, System.Func<string, string>> stragety 
                            = new Dictionary<int, System.Func<string, string>>();

            public bool TryAddFormat(int type, System.Func<string, string> format)
            {
                if (type < 5) return false;

                return stragety.TryAdd(type, format);
            }

            public bool TryGetFormat(int type, out System.Func<string, string> format)
            {
                if (type < 5)
                {
                    format = null;
                    return true;
                }
                return stragety.TryGetValue(type, out format);
            }

            public bool TryRemoveFormat(int type)
            {
                if (!stragety.ContainsKey(type)) return false;

                stragety.Remove(type);
                return true;
            }
        }
        private FormatStragety formatStragety = new FormatStragety();

        public enum DataType : int
        {
            Field = 0,
            Custom = 5
        }
        public int data_type = (int)DataType.Field;
        public DataType dataType
        { 
            get
            {
                if (data_type >= 0 && data_type <= 5) 
                    return (DataType) data_type;
                
                return (DataType)data_type;
            }
            set
            {
                data_type = (data_type >= 0 && data_type <= 5) 
                            ? (int)value : (int)DataType.Custom;
                var index = 0;
                data.ForEach(info => texts[index++].text = Format(info));
            }
        }
        public List<Text> texts;
        public List<string> data;
        public List<string> Data => data;
        public string title;
        private RectTransform m_RectTransform;
        public RectTransform rectTransform
        {
            get
            {
                if (m_RectTransform is null) 
                    m_RectTransform = this.GetComponent<RectTransform>();
                return m_RectTransform;
            }
        }
        public Transform textParent;
        public Transform parent 
        {
            get => textParent;
            set => textParent = value;
        }

        public virtual string this[int index]
        {
            get => data[index];
            set
            {
                data[index] = value;
                texts[index].text = Format(data[index]);
            }
        }

        public int Count => texts.Count;

        public bool IsReadOnly { get; } = false;

#if UNITY_EDITOR
        public ReorderableList textList;
        public GameObject template;
        public SerializedObject templateRect;
        public SerializedObject templateText;
        public bool propertyFoldout = true;
        public bool drawSelect = false;
        public bool alwaysUpdate = false;
        public bool typeDropdown = false;
#endif

        void Start()
        {
            
        }

        void Update()
        {

        }

        public void UpdateData(object[] info, int index = 0)
        {
            if (info.Length - index < this.Count)
            {
                throw new System.Exception("Data length is not match.");
            }

            data.ForEach(text => 
            {
                text = (string)(info[index++]);
            });
        }

        public virtual int IndexOf(string value)
        {
            int index = 0;
            foreach (var info in this)
            {
                if (info.Equals(value)) return index;
                ++index;
            }
            return -1;
        }

        public virtual void Insert(int index, string value)
        {
            Add(value);

            texts[this.Count - 1].rectTransform.SetSiblingIndex(index);
        }


        public virtual void RemoveAt(int index)
        {
            if (index < 0 || index > texts.Count)
            {
                Debug.LogError("Index out of range.");
                return;
            }
            DestroyImmediate(texts[index].gameObject);
            texts.RemoveAt(index);
        }

        public virtual void Add(string value = "")
        {
            var rt = new GameObject("数据 " + texts.Count).AddComponent<RectTransform>();
            print(parent.name);
            rt.SetParent(parent);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);

            var tx = rt.gameObject.AddComponent<Text>();
            tx.text = value;
            tx.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            texts.Add(tx);
        }

        public virtual void Clear()
        {
            texts.Clear();
        }

        public virtual bool Contains(string value)
        {
            return IndexOf(value) != -1;
        }

        public virtual void CopyTo(string[] infor, int index)
        {
            if (infor.Length < texts.Count + index)
            {
                return;
            }

            foreach (var info in this)
            {
                infor[index] = info;
                index += 1;
            }
        }

        public virtual bool Remove(string value)
        {
            int index = IndexOf(value);
            if (index == -1) return false;

            RemoveAt(index);
            return true;
        }

        public virtual IEnumerator<string> GetEnumerator() =>
        new DataEnumerator<string, string>(
            list: data,
            GetCurrent: (in IList<string> list, in int index) => list[index]
        );

        public string Format(string value)
        {
            switch (data_type)
            {
                case (int)DataType.Field:
                    return value;
                default:
                    if (formatStragety.TryGetFormat(data_type, out var format))
                    {
                        return format?.Invoke(value);
                    }
                    return "#";
            }
        }

        public bool TryAddFormat(int type, System.Func<string, string> format)
        {
            return formatStragety.TryAddFormat(type, format);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
