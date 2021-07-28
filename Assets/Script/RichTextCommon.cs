/***
 *              HuaHua
 *              2020-09-25
 *              富文本，一个drawcall 支持文字和图片混排
 **/
using System.Collections.Generic;

internal static class RichTextCommon
{
    public static void EnsureSizeEx<T>(this IList<T> list, int size)
    {
        if (null != list)
        {
            var count = list.Count;
            for (int i = count; i < size; ++i)
            {
                list.Add(default(T));
            }
        }
    }
}
