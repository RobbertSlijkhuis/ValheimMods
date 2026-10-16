using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_Armors.Helpers
{
    internal class TransformHelper
    {
        /**
         * Depth-first search by exact name in the same order as vanilla Utils.FindChild (first child first, parents before children).
         * Everything under (and including) the exclude transform is skipped, so the search never ends up inside the object that is looking for a bone.
         */
        public static Transform FindChildDepthFirst(Transform root, string name, Transform exclude = null)
        {
            if (root == null)
                return null;

            Stack<Transform> stack = new Stack<Transform>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                Transform current = stack.Pop();

                if (exclude != null && current == exclude)
                    continue;

                if (current != root && current.name == name)
                    return current;

                for (int i = current.childCount - 1; i >= 0; i--)
                    stack.Push(current.GetChild(i));
            }

            return null;
        }

        public static Quaternion generateRotation(Vector3 vector)
        {
            Quaternion rotation = new Quaternion(0, 0, 0, 0);
            rotation.eulerAngles = vector;
            return rotation;
        }
    }
}
