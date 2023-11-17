// Pcx - Point cloud importer & renderer for Unity
// https://github.com/keijiro/Pcx

using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.Runtime.InteropServices;

namespace Pcx
{
    /// A container class optimized for compute buffer.
    public sealed class PointCloudDataExtended : ScriptableObject
    {
        #region Public properties

        /// Number of points.
        public int pointCountExt
        {
            get { return _pointDataExt.Length; }
        }

        [SerializeField] int _propsCount;
        public int propsCount  
        {
            get { return _propsCount; } 
            set { _propsCount = value; }
        } 

        [SerializeField] bool _importNormal=false;
        public bool importNormal 
        {
            get { return _importNormal; } 
            set { _importNormal = value; }
        } 

        #endregion

        #region ScriptableObject implementation

        #endregion

        #region Serialized data members

        [SerializeField] public float[] _pointDataExt;

        #endregion

        #region Editor functions

#if UNITY_EDITOR

        static uint EncodeColor(Color c)
        {
            const float kMaxBrightness = 16;

            var y = Mathf.Max(Mathf.Max(c.r, c.g), c.b);
            y = Mathf.Clamp(Mathf.Ceil(y * 255 / kMaxBrightness), 1, 255);

            var rgb = new Vector3(c.r, c.g, c.b);
            rgb *= 255 * 255 / (y * kMaxBrightness);

            return ((uint)rgb.x) |
                   ((uint)rgb.y << 8) |
                   ((uint)rgb.z << 16) |
                   ((uint)y << 24);
        }

        public void Initialize(List<Vector3> positions, List<Color32> colors, List<Vector3> normals, List<List<float>> property_s) 
        {
            int numfloatdat;
            if (_importNormal)
                numfloatdat = 7;
            else
                numfloatdat = 4;
           _pointDataExt = new float[positions.Count * (numfloatdat + property_s.Count)];
            propsCount = property_s.Count;
            int indpoint = 0;
            for (var i = 0; i < ((positions.Count-1) * (numfloatdat + propsCount)); i=i+ numfloatdat + propsCount)
            {
                float[] prop_pack = new float[propsCount];
                for (int indexal = 0; indexal < propsCount; indexal++)
                {
                    prop_pack[indexal] = property_s[indexal][indpoint];
                }
                int numelements = numfloatdat + propsCount;
                _pointDataExt[i + 0] = positions[indpoint].x;
                _pointDataExt[i + 1] = positions[indpoint].y;
                _pointDataExt[i + 2] = positions[indpoint].z;
                uint coloruint = EncodeColor(colors[indpoint]);
                _pointDataExt[i + 3] = System.BitConverter.ToSingle(System.BitConverter.GetBytes(coloruint), 0);
                if (_importNormal)
                {
                    _pointDataExt[i + 4] = normals[indpoint].x;
                    _pointDataExt[i + 5] = normals[indpoint].y;
                    _pointDataExt[i + 6] = normals[indpoint].z;
                }
                for (int indul = numfloatdat; indul < numelements; indul++)
                {
                    _pointDataExt[i + indul] = prop_pack[indul - numfloatdat];
                }
                indpoint++;
            }
        }
#endif
        #endregion
    }
}
