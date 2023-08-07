using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

namespace Unite.Framework.UI
{
    public class TableData : MonoBehaviour, IUIDataCollection<ListData>
    {
        public List<ListData> data;
        public List<ListData> Data => data;
        public int Count => data.Count;
        public bool IsReadOnly => true;
        public Transform parent { get; set; }
        public ListData this[int index]
        {
            get => data[index];
            set => data[index] = value;
        }

#if UNITY_EDITOR
        public ReorderableList drawList;
        public bool propertyFoldout = true;
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
        }

        public virtual int IndexOf(ListData value)
        {
            int index = 0;
            foreach (var info in this)
            {
                if (info.Equals(value)) return index;
                ++index;
            }
            return -1;
        }

        public virtual void Insert(int index, ListData value)
        {
            Add(value);

            data[this.Count - 1].rectTransform.SetSiblingIndex(index);
        }


        public virtual void RemoveAt(int index)
        {
            if (index < 0 || index > data.Count)
            {
                Debug.LogError("Index out of range.");
                return;
            }
            DestroyImmediate(data[index].gameObject);
            data.RemoveAt(index);
        }

        public virtual void Add(ListData value)
        {
            var rt = new GameObject("列表 " + data.Count).AddComponent<RectTransform>();
            print(parent.name);
            rt.SetParent(parent);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
        }

        public virtual void Clear()
        {
            data.Clear();
        }

        public virtual bool Contains(ListData value)
        {
            return IndexOf(value) != -1;
        }

        public virtual void CopyTo(ListData[] infor, int index)
        {
            if (infor.Length < data.Count + index)
            {
                return;
            }

            foreach (var info in this)
            {
                infor[index] = info;
                index += 1;
            }
        }

        public virtual bool Remove(ListData value)
        {
            int index = IndexOf(value);
            if (index == -1) return false;

            RemoveAt(index);
            return true;
        }

        public virtual IEnumerator<ListData> GetEnumerator() => new DataEnumerator<ListData, ListData>(
            list: data,
            GetCurrent: (in IList<ListData> list, in int index) => list[index]
        );

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
