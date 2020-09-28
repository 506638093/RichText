/***
 *              HuaHua
 *              2020-09-25
 *              富文本，一个drawcall 支持文字和图片混排
 **/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using XUnityCore;


namespace UnityEngine.UI
{
    public class RichTextSprite
    {
        private RichText m_richText;
        private string m_name;
        private int m_vertexIndex;
        private Vector2 m_size;
        private Sprite m_sprite;
        private float m_fillAmount = 1.0f;

        public RichTextSprite(RichText richText)
        {
            if (null == richText)
            {
                throw new ArgumentNullException("RichText is null.");
            }

            m_richText = richText;
        }

        public bool SetValue(Match match)
        {
            if (null == match)
            {
                return false;
            }

            var keyCaptures = match.Groups[1].Captures;
            var valCaptures = match.Groups[2].Captures;

            var count = keyCaptures.Count;
            if (count != valCaptures.Count)
            {
                return false;
            }

            for (int i = 0; i < count; ++i)
            {
                var key = keyCaptures[i].Value;
                var val = valCaptures[i].Value;
                checkSetValue(match, key, val);
            }

            return true;
        }

        public void SetName(string name)
        {
            m_name = name;
        }

        public string GetName()
        {
            return m_name;
        }

        public void Reset()
        {
            SetName(null);
        }

        private void setSprite(string path)
        {
            ResourceManager.Instance.LoadSprite(path, (sprite, abPath) => {
                m_sprite = sprite;
            }, true);
        }

        public Sprite GetSprite()
        {
            return m_sprite;
        }

        public int GetVertexIndex()
        {
            return m_vertexIndex;
        }

        public Vector2 GetSize()
        {
            return m_size;
        }

        private void checkSetValue(Match match, string key, string val)
        {
            if (key == "name")
            {
                SetName(val);
                m_vertexIndex = match.Index;
            }
            else if (key == "src")
            {
                setSprite(val);
            }
            else if (key == "width")
            {
                float width;
                float.TryParse(val, out width);
                m_size.x = width;
            }
            else if (key == "height")
            {
                float height;
                float.TryParse(val, out height);
                m_size.y = height;
            }
        }

        public void SetFillAmount(float amount)
        {
            amount = Mathf.Clamp01(amount);

            float eps = 0.001f;
            var delta = m_fillAmount - amount;
            if (delta > eps || delta < -eps)
            {
                m_fillAmount = amount;
                m_richText.SetVerticesDirty();
            }
        }

        public float GetFillAmount()
        {
            return m_fillAmount;
        }

        public static MatchCollection GetMatches(string strText)
        {
            return _spriteTagRegex.Matches(strText);
        }

        private static readonly string _spriteTagPattern = @"<quad(?:\s+(\w+)\s*=\s*(?<quota>['""]?)([\w\/]+)\k<quota>)+\s*\/>";
        private static readonly Regex _spriteTagRegex = new Regex(_spriteTagPattern, RegexOptions.Singleline);
    }
}
