using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LableQueueEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var queue = target as LableQueue;
    }
}

public class LableQueue : MonoBehaviour
{
    public int maxSize;
    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
