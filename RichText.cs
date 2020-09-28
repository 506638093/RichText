/***
 *              HuaHua
 *              2020-09-25
 *              富文本，一个drawcall 支持文字和图片混排
 **/

using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor.Graphing;
using UnityEngine;
using UnityEngine.U2D;
using XUnityCore;

namespace UnityEngine.UI
{
    [ExecuteInEditMode]
    public class RichText : Text
    {
        #region member
        public override string text
        {
            get
            {
                return m_Text;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    if (string.IsNullOrEmpty(m_Text))
                    {
                        return;
                    }
                    m_Text = string.Empty;

                    parseText();
                    SetVerticesDirty();
                }
                else if (m_Text != value)
                {
                    m_Text = value;

                    parseText();
                    SetVerticesDirty();
                    SetLayoutDirty();
                }
            }
        }

        [SerializeField]
        public Texture2D m_AtlasTexture;

        [SerializeField]
        public bool m_UiMode = false;

        [System.NonSerialized]
        private MeshRenderer m_meshRender;
        [System.NonSerialized]
        private MeshFilter m_meshFilter;
        [System.NonSerialized]
        private Mesh m_mesh;

        private string m_parseText;
        private readonly UIVertex[] m_tempVerts = new UIVertex[4];
        private readonly List<RichTextSprite> m_spriteList = new List<RichTextSprite>();
        [System.NonSerialized]
        private static readonly VertexHelper s_VertexHelper = new VertexHelper();
        #endregion //member

        #region public function
        public void SetSpriteFillAmount(string name, float amount)
        {
            for (int i = 0; i < m_spriteList.Count; ++i)
            {
                if(m_spriteList[i].GetName().Equals(name))
                {
                    m_spriteList[i].SetFillAmount(amount);
                    break;
                }
            }
        }
        #endregion //public function

        #region unity function

        protected override void UpdateGeometry()
        {
            if (m_UiMode)
            {
                base.UpdateGeometry();
            }
            else
            {
                if (m_meshRender == null)
                {
                    m_meshRender = gameObject.GetOrAddComponent<MeshRenderer>();
                    m_meshRender.sharedMaterial = material;

                    m_meshFilter = gameObject.GetOrAddComponent<MeshFilter>();

                    m_mesh = new Mesh();
                }

                DoMeshGeneration3D();
            }
        }

        private void DoMeshGeneration3D()
        {
            if (rectTransform != null && rectTransform.rect.width >= 0 && rectTransform.rect.height >= 0)
                OnPopulateMesh(s_VertexHelper);
            else
                s_VertexHelper.Clear(); // clear the vertex helper so invalid graphics dont draw.

            var components = ListPool<Component>.Get();
            GetComponents(typeof(IMeshModifier), components);

            for (var i = 0; i < components.Count; i++)
                ((IMeshModifier)components[i]).ModifyMesh(s_VertexHelper);

            ListPool<Component>.Release(components);

            s_VertexHelper.FillMesh(m_mesh);
            m_meshFilter.sharedMesh = m_mesh;
        }

        protected override void OnEnable()
        {
            this.supportRichText = true;

            parseText();

            m_Material.SetTexture("_MainTex", mainTexture);
            m_Material.SetTexture("_SpriteTex", m_AtlasTexture);

            SetVerticesDirty();

            base.OnEnable();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        private void LateUpdate()
        {
            if (rectTransform.hasChanged)
            {
                rectTransform.hasChanged = false;
                SetVerticesDirty();
            }
        }

        #endregion //unity function

        #region function
        private void parseText()
        {
            m_parseText = text;
            parseSprite(m_parseText);
        }

        private void parseSprite(string strText)
        {
            if (string.IsNullOrEmpty(strText) || -1 == strText.IndexOf("<quad"))
            {
                parseSprite(0);
                return;
            }

            int count = 0;
            foreach (Match match in RichTextSprite.GetMatches(strText))
            {
                RichTextSprite sprt = getRichSprite(count);
                if (sprt.SetValue(match))
                {
                    ++count;
                }
            }

            parseSprite(count);
        }

        private RichTextSprite getRichSprite(int index)
        {
            if (index >= 0)
            {
                m_spriteList.EnsureSizeEx(index + 1);
                RichTextSprite sprt = m_spriteList[index] ?? (m_spriteList[index] = new RichTextSprite(this));
                return sprt;
            }

            return null;
        }

        private void parseSprite(int startIndex)
        {
            var count = m_spriteList.Count;
            for (int i = startIndex; i < count; ++i)
            {
                var tag = m_spriteList[i];
                tag.Reset();
            }
        }

        private void handleSpriteTag(VertexHelper toFill)
        {
            var count = m_spriteList.Count;
            for (int i = 0; i < count; i++)
            {
                RichTextSprite richSprite = m_spriteList[i];
                var name = richSprite.GetName();
                var sprite = richSprite.GetSprite();
                if (string.IsNullOrEmpty(name) || null == sprite)
                {
                    continue;
                }

                setSpriteVertex_FillMethod_None(toFill, richSprite, sprite);
            }
        }

        private void setSpriteVertex_FillMethod_None(VertexHelper toFill, RichTextSprite richSprite, Sprite sprite)
        {
            UIVertex v = UIVertex.simpleVert;
            var vertexIndex = richSprite.GetVertexIndex() * 4;
            var fetchIndex = vertexIndex + 3;
            if (fetchIndex >= toFill.currentVertCount)
            {
                return;
            }

            toFill.PopulateUIVertex(ref v, fetchIndex);
            Vector3 textPos = v.position;
            var tagSize = richSprite.GetSize();

            var texture = sprite.texture;
            var textureWidthInv = 1.0f / texture.width;
            var textureHeightInv = 1.0f / texture.height;
            var uvRect = sprite.textureRect;
            uvRect = new Rect(uvRect.x * textureWidthInv, uvRect.y * textureHeightInv, uvRect.width * textureWidthInv, uvRect.height * textureHeightInv);

            // pos = (0, 0)
            var position = new Vector3(0, 0, 0) + textPos;
            var uv0 = new Vector2(uvRect.x, uvRect.y);
            setSpriteVertex(toFill, vertexIndex, position, uv0);

            var fillAmount = richSprite.GetFillAmount();

            // pos = (1, 0)
            position = new Vector3(tagSize.x * fillAmount, 0, 0) + textPos;
            uv0 = new Vector2(uvRect.x + uvRect.width * fillAmount, uvRect.y);
            setSpriteVertex(toFill, ++vertexIndex, position, uv0);

            // pos = (1, 1)
            position = new Vector3(tagSize.x * fillAmount, tagSize.y, 0) + textPos;
            uv0 = new Vector2(uvRect.x + uvRect.width * fillAmount, uvRect.y + uvRect.height);
            setSpriteVertex(toFill, ++vertexIndex, position, uv0);

            // pos = (0, 1)
            position = new Vector3(0, tagSize.y, 0) + textPos;
            uv0 = new Vector2(uvRect.x, uvRect.y + uvRect.height);
            setSpriteVertex(toFill, ++vertexIndex, position, uv0);
        }

        private void setSpriteVertex(VertexHelper toFill, int vertexIndex, Vector3 position, Vector2 uv0)
        {
            UIVertex v = new UIVertex();
            toFill.PopulateUIVertex(ref v, vertexIndex);
            v.position = position;
            v.uv0 = uv0;
            v.uv1 = new Vector2(0, 1.0f);
            toFill.SetUIVertex(v, vertexIndex);
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (null == font)
            {
                return;
            }

            // We don't care if we the font Texture changes while we are doing our Update.
            // The end result of cachedTextGenerator will be valid for this instance.
            // Otherwise we can get issues like Case 619238.
            m_DisableFontTextureRebuiltCallback = true;

            Vector2 extents = rectTransform.rect.size;

            var settings = GetGenerationSettings(extents);
            cachedTextGenerator.Populate(m_parseText, settings);

            Rect inputRect = rectTransform.rect;

            // get the text alignment anchor point for the text in local space
            Vector2 textAnchorPivot = GetTextAnchorPivot(alignment);
            Vector2 refPoint = Vector2.zero;
            refPoint.x = (textAnchorPivot.x == 1 ? inputRect.xMax : inputRect.xMin);
            refPoint.y = (textAnchorPivot.y == 0 ? inputRect.yMin : inputRect.yMax);

            // Determine fraction of pixel to offset text mesh.
            Vector2 roundingOffset = PixelAdjustPoint(refPoint) - refPoint;

            // Apply the offset to the vertices
            IList<UIVertex> verts = cachedTextGenerator.verts;
            float unitsPerPixel = 1 / pixelsPerUnit;
            //Last 4 verts are always a new line...
            int vertCount = verts.Count - 4;

            toFill.Clear();

            if (roundingOffset != Vector2.zero)
            {
                for (int i = 0; i < vertCount; ++i)
                {
                    int tempVertsIndex = i & 3;
                    m_tempVerts[tempVertsIndex] = verts[i];
                    m_tempVerts[tempVertsIndex].position *= unitsPerPixel;
                    m_tempVerts[tempVertsIndex].position.x += roundingOffset.x;
                    m_tempVerts[tempVertsIndex].position.y += roundingOffset.y;
                    m_tempVerts[tempVertsIndex].uv1 = new Vector2(1.0f, 0);

                    if (tempVertsIndex == 3)
                    {
                        toFill.AddUIVertexQuad(m_tempVerts);
                    }
                }
            }
            else
            {
                for (int i = 0; i < vertCount; ++i)
                {
                    int tempVertsIndex = i & 3;
                    m_tempVerts[tempVertsIndex] = verts[i];
                    m_tempVerts[tempVertsIndex].position *= unitsPerPixel;
                    m_tempVerts[tempVertsIndex].uv1 = new Vector2(1.0f, 0);

                    if (tempVertsIndex == 3)
                    {
                        toFill.AddUIVertexQuad(m_tempVerts);
                    }
                }
            }

            handleSpriteTag(toFill);
            m_DisableFontTextureRebuiltCallback = false;
        }


        #endregion //function

    }

}