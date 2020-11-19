using UnityEngine;
using UnityEngine.UI;

namespace HuaHua
{
    /// <summary>
    /// unity扩展类
    /// </summary>
    public static class UnityExtend
    {
        /// <summary>
        /// 获取或者添加一个脚本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            T t = obj.GetComponent<T>();
            if (t == null)
            {
                t = obj.AddComponent<T>();
            }
            return t;
        }

        /// <summary>
        /// 获取或者添加一个脚本
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static T GetOrAddComponent<T>(this Transform t) where T : Component
        {
            T newT = t.GetComponent<T>();
            if (newT == null)
            {
                newT = t.gameObject.AddComponent<T>();
            }
            return newT;
        }

        public static void SetColor(this Graphic graphic, int r, int g, int b, int a)
        {
            graphic.color = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
        }

        public static void SetColor(this Graphic graphic, int r, int g, int b)
        {
            graphic.color = new Color32((byte)r, (byte)g, (byte)b, 255);
        }

        /// <summary>
        /// 获取对象在Hierarchy中的节点路径
        /// </summary>
        public static string GetHierarchyPath(this Transform tran, Transform root = null)
        {
            return GetHierarchPathLoop(tran, root);
        }
        /// <summary>
        /// 获取对象在Hierarchy中的节点路径
        /// </summary>
        public static string GetHierarchyPath(this GameObject obj, Transform root = null)
        {
            return GetHierarchyPath(obj.transform, root);
        }
        /// <summary>
        /// 获取对象在Hierarchy中的节点路径
        /// </summary>
        public static string GetHierarchyPath(this Component obj, Transform root = null)
        {
            return GetHierarchyPath(obj.transform, root);
        }
        private static string GetHierarchPathLoop(Transform t, Transform root = null, string path = null)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = t.gameObject.name;
            }
            else
            {
                path = t.gameObject.name + "/" + path;
            }
            if (t.parent != null && t.parent != root)
            {
                return GetHierarchPathLoop(t.parent, root, path);
            }
            else
            {
                return path;
            }
        }
    }
}