// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
// #endregion License
// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.Xna.Framework.Graphics
{
    public class EffectTechnique
    {
        public List<EffectPass> Passes = new List<EffectPass>();
    }
    public class EffectParameter
    {
        internal void SetValue<T>( T val )
        {
           // throw new NotImplementedException();
        }
        public void Apply()
        {

        }

        public unsafe void SetValueRef(ref Matrix value)
        {
        }

    }

    public sealed class EffectAnnotationCollection : IEnumerable<EffectAnnotation>, IEnumerable
    {
        private List<EffectAnnotation> elements;

        public int Count
        {
            get
            {
                return this.elements.Count;
            }
        }

        public EffectAnnotation this[int index]
        {
            get
            {
                return this.elements[index];
            }
        }

        public EffectAnnotation this[string name]
        {
            get
            {
                foreach (EffectAnnotation element in this.elements)
                {
                    if (name.Equals(element.Name))
                        return element;
                }
                return (EffectAnnotation) null;
            }
        }

        internal EffectAnnotationCollection(List<EffectAnnotation> value)
        {
            this.elements = value;
        }

        public IEnumerator GetEnumerator()
        {
            return this.elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) this.elements.GetEnumerator();
        }

        IEnumerator<EffectAnnotation> IEnumerable<EffectAnnotation>.GetEnumerator()
        {
            return (IEnumerator<EffectAnnotation>) this.elements.GetEnumerator();
        }
    }

    public sealed class EffectPass
    {
        private Effect parentEffect;
        private uint pass;

        public string Name { get; private set; }

        public EffectAnnotationCollection Annotations { get; private set; }

        internal EffectPass(
            string name,
            EffectAnnotationCollection annotations,
            Effect parent,
            uint passIndex)
        {
            this.Name = name;
            this.Annotations = annotations;
            this.parentEffect = parent;
            this.pass = passIndex;
        }

        public void Apply()
        {
            this.parentEffect.OnApply();
            this.parentEffect.INTERNAL_applyEffect(this.pass);
        }
    }

    public enum EffectParameterClass
    {
        Scalar,
        Vector,
        Matrix,
        Object,
        Struct,
    }

    public enum EffectParameterType
    {
        Void,
        Bool,
        Int32,
        Single,
        String,
        Texture,
        Texture1D,
        Texture2D,
        Texture3D,
        TextureCube,
    }

    public sealed class EffectAnnotation
  {
    private IntPtr values;

    public string Name { get; private set; }

    public string Semantic { get; private set; }

    public int RowCount { get; private set; }

    public int ColumnCount { get; private set; }

    public EffectParameterClass ParameterClass { get; private set; }

    public EffectParameterType ParameterType { get; private set; }

    internal EffectAnnotation(
      string name,
      string semantic,
      int rowCount,
      int columnCount,
      EffectParameterClass parameterClass,
      EffectParameterType parameterType,
      IntPtr data)
    {
      this.Name = name;
      this.Semantic = semantic;
      this.RowCount = rowCount;
      this.ColumnCount = columnCount;
      this.ParameterClass = parameterClass;
      this.ParameterType = parameterType;
      this.values = data;
    }

    public unsafe bool GetValueBoolean()
    {
      return (uint) *(int*) (void*) this.values > 0U;
    }

    public unsafe int GetValueInt32()
    {
      return *(int*) (void*) this.values;
    }

    public unsafe Matrix GetValueMatrix()
    {
      float* values = (float*) (void*) this.values;
      return new Matrix(*values, values[4], values[8], values[12], values[1], values[5], values[9], values[13], values[2], values[6], values[10], values[14], values[3], values[7], values[11], values[15]);
    }

    public unsafe float GetValueSingle()
    {
      return *(float*) (void*) this.values;
    }

    public string GetValueString()
    {
      throw new NotImplementedException("effect->objects[?]");
    }

    public unsafe Vector2 GetValueVector2()
    {
      float* values = (float*) (void*) this.values;
      return new Vector2(*values, values[1]);
    }

    public unsafe Vector3 GetValueVector3()
    {
      float* values = (float*) (void*) this.values;
      return new Vector3(*values, values[1], values[2]);
    }

    public unsafe Vector4 GetValueVector4()
    {
      float* values = (float*) (void*) this.values;
      return new Vector4(*values, values[1], values[2], values[3]);
    }
  }

    public class Effect : GraphicsResource
    {
        public Dictionary<string, EffectParameter> Parameters = new Dictionary<string, EffectParameter>();
        public Dictionary<string, EffectTechnique> Techniques = new Dictionary<string, EffectTechnique>();

        public Effect( GraphicsDevice graphicsDevice, byte[] v ) : this( graphicsDevice )
        {
            Parameters.Add("MatrixTransform", new EffectParameter());
            Parameters.Add("WorldMatrix", new EffectParameter());
            Parameters.Add("Viewport", new EffectParameter());
            Techniques.Add("HueTechnique", new EffectTechnique());

        }

        internal Effect(GraphicsDevice graphicsDevice) : base(graphicsDevice)
		{
			
        }

        protected Effect(Effect cloneSource)
        {
            
        }

        public EffectTechnique CurrentTechnique { get; internal set; }

        internal unsafe void INTERNAL_applyEffect(uint pass)
        {
        }

        protected internal virtual void OnApply()
        {
        }
    }

    public class BasicEffect : Effect
    {
        public Matrix World;
        public Matrix View;
        public Matrix Projection;
        public bool TextureEnabled;
        public Texture2D Texture;
        public bool VertexColorEnabled;

        public BasicEffect(GraphicsDevice graphicsDevice, byte[] v) : base(graphicsDevice, v)
        {
        }

        internal BasicEffect(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {
        }

        protected BasicEffect(Effect cloneSource) : base(cloneSource)
        {
        }
    }
}
