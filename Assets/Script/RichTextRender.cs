/***
 *              HuaHua
 *              2020-09-25
 *              富文本，一个drawcall 支持文字和图片混排
 **/

using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.U2D;
using HuaHua;

namespace UnityEngine.UI
{
    [ExecuteInEditMode]
    public class RichTextRender : MonoBehaviour
    {
        [System.NonSerialized]
        private MeshRenderer m_meshRender;
        [System.NonSerialized]
        private MeshFilter m_meshFilter;
        [System.NonSerialized]
        private Mesh m_mesh;

        private bool m_dirty;

        internal static readonly HideFlags MeshHideflags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor | HideFlags.HideInInspector;

        struct MeshOrder
        {
            public Mesh mesh;
            public float z;
            public Matrix4x4 matrix;
        }

        public void MarkDirty()
        {
            m_dirty = true;
#if UNITY_EDITOR
            OnCombineMesh();
#endif
        }

        private void OnCombineMesh()
        {
            RichText[] texts = GetComponentsInChildren<RichText>(false);
            RichImage[] images = GetComponentsInChildren<RichImage>(false);
            var meshCount = texts.Length + images.Length;
            if (meshCount == 0)
            {
                if (m_meshRender)
                {
                    m_meshRender.enabled = false;
                }
                return;
            }

            if (m_meshRender == null)
            {
                m_meshRender = gameObject.GetOrAddComponent<MeshRenderer>();
                m_meshRender.hideFlags = MeshHideflags;

                m_meshFilter = gameObject.GetOrAddComponent<MeshFilter>();
                m_meshFilter.hideFlags = MeshHideflags;

                m_mesh = new Mesh();
                m_mesh.MarkDynamic();
                m_mesh.hideFlags = MeshHideflags;
            }            

            Material material = null;

            var meshes = ListPool<MeshOrder>.Get();

            var worldToLocalMatrix = this.transform.worldToLocalMatrix;
            for (int i = 0; i < images.Length; ++i)
            {
                var image = images[i];

                if (!image.IsActive())
                {
                    continue;
                }

                var mesh = image.Mesh();
                if (mesh == null)
                {
                    continue;
                }

                var meshOrder = new MeshOrder();
                meshOrder.mesh = mesh;
                meshOrder.matrix = worldToLocalMatrix * image.transform.localToWorldMatrix;
                meshOrder.z = image.transform.localPosition.z;

                meshes.Add(meshOrder);

                if (material == null)
                {
                    material = image.material;
                }
            }
            for (int j = 0; j < texts.Length; ++j)
            {
                var text = texts[j];

                if (!text.IsActive())
                {
                    continue;
                }

                var mesh = text.Mesh();
                if (mesh == null)
                {
                    continue;
                }

                var meshOrder = new MeshOrder();
                meshOrder.mesh = mesh;
                meshOrder.matrix = worldToLocalMatrix * text.transform.localToWorldMatrix;
                meshOrder.z = text.transform.localPosition.z;

                meshes.Add(meshOrder);

                if (material == null)
                {
                    material = text.material;
                }
            }

            if (meshes.Count == 0)
            {
                if (m_meshRender)
                {
                    m_meshRender.enabled = false;
                }
                return;
            }
            m_meshRender.enabled = true;

            meshes.Sort((lhs, rhs) => rhs.z.CompareTo(lhs.z));

            CombineInstance[] combine = new CombineInstance[meshes.Count];
            for (int i = 0; i < meshes.Count; ++i)
            {
                combine[i].mesh = meshes[i].mesh;
                combine[i].transform = meshes[i].matrix;
            }

            ListPool<MeshOrder>.Release(meshes);

            m_mesh.CombineMeshes(combine, true);
            m_meshFilter.sharedMesh = m_mesh;
            m_meshRender.sharedMaterial = material;
        }

        protected void OnEnable()
        {
            m_dirty = true;
        }

        private void LateUpdate()
        {
            if(m_dirty)
            {
                OnCombineMesh();

                m_dirty = false;
            }
        }

    }

}