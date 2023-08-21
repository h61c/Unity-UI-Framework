using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unite.Framework
{
    public delegate void TreeTraverseDelegate(Transform node, int level, out bool returnFlag);
    public class UniteAlgorithm
    {
        public static void TreeTraverse(Transform root, TreeTraverseDelegate PreOrder = null, 
                                    TreeTraverseDelegate PostOrder = null, int level = 0, bool leftToRight = true)
        {
            bool returnFlag = false;
            PreOrder?.Invoke(root, level, out returnFlag);
            if (returnFlag) return;

            var cur = leftToRight ? 0 : root.childCount - 1;
            var end = leftToRight ? root.childCount : -1;
            while (root.childCount > 0 && cur != end)
            {
                TreeTraverse(root.GetChild(cur), PreOrder, PostOrder, level + 1, leftToRight);
                cur += leftToRight ? 1 : -1;
            }
            PostOrder?.Invoke(root, level, out returnFlag);
        }
    }
}