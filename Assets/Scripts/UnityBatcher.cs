using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UnityEngine;
using UnityEngine.Rendering;
using BlendState = Microsoft.Xna.Framework.Graphics.BlendState;
using Color = UnityEngine.Color;
using CompareFunction = Microsoft.Xna.Framework.Graphics.CompareFunction;
using Quaternion = UnityEngine.Quaternion;
using Texture2D = Microsoft.Xna.Framework.Graphics.Texture2D;
using UnityTexture = UnityEngine.Texture;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Vector4 = UnityEngine.Vector4;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using XnaVector3 = Microsoft.Xna.Framework.Vector3;

namespace ClassicUO.Renderer
{
    internal sealed class UltimaBatcher2D : IDisposable
    {
        private readonly RasterizerState _rasterizerState;
        private BlendState _blendState;
        private bool _started;
        private DepthStencilState _stencil;
        private bool _useScissor;
        private int _numSprites;

        private Material hueMaterial;
        private Material xbrMaterial;

        private MeshHolder reusedMesh = new MeshHolder(1);

        public float scale = 1;
        
        public bool UseGraphicsDrawTexture;

        private Mesh draw2DMesh;
        private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
        private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
        private static readonly int Hue = Shader.PropertyToID("_Hue");
        private static readonly int HueTex1 = Shader.PropertyToID("_HueTex1");
        private static readonly int HueTex2 = Shader.PropertyToID("_HueTex2");
        private static readonly int UvMirrorX = Shader.PropertyToID("_uvMirrorX");
        private static readonly int Scissor = Shader.PropertyToID("_Scissor");
        private static readonly int ScissorRect = Shader.PropertyToID("_ScissorRect");
        private static readonly int TextureSize = Shader.PropertyToID("textureSize");

        public UltimaBatcher2D(GraphicsDevice device)
        {
            GraphicsDevice = device;
            _blendState = BlendState.AlphaBlend;
            _rasterizerState = RasterizerState.CullNone;

            _rasterizerState = new RasterizerState
            {
                CullMode = _rasterizerState.CullMode,
                DepthBias = _rasterizerState.DepthBias,
                FillMode = _rasterizerState.FillMode,
                MultiSampleAntiAlias = _rasterizerState.MultiSampleAntiAlias,
                SlopeScaleDepthBias = _rasterizerState.SlopeScaleDepthBias,
                ScissorTestEnable = true
            };

            _stencil = Stencil;
            DefaultEffect = new IsometricEffect(device);

            hueMaterial = new Material(UnityEngine.Resources.Load<Shader>("HueShader"));
            xbrMaterial = new Material(UnityEngine.Resources.Load<Shader>("XbrShader"));
        }

        private MatrixEffect DefaultEffect { get; }

        private Effect CustomEffect;

        private DepthStencilState Stencil { get; } = new DepthStencilState
        {
            StencilEnable = false,
            DepthBufferEnable = false,
            StencilFunction = CompareFunction.NotEqual,
            ReferenceStencil = 1,
            StencilMask = 1,
            StencilFail = StencilOperation.Keep,
            StencilDepthBufferFail = StencilOperation.Keep,
            StencilPass = StencilOperation.Keep
        };

        public GraphicsDevice GraphicsDevice { get; }

        public void SetBrightlight(float f)
        {
            ((IsometricEffect)DefaultEffect).Brighlight.SetValue(f);
        }

        public void DrawString(SpriteFont spriteFont, string text, int x, int y, ref XnaVector3 color)
        {
            if (String.IsNullOrEmpty(text))
                return;

            Texture2D textureValue = spriteFont.Texture;
            List<Rectangle> glyphData = spriteFont.GlyphData;
            List<Rectangle> croppingData = spriteFont.CroppingData;
            List<XnaVector3> kerning = spriteFont.Kerning;
            List<char> characterMap = spriteFont.CharacterMap;

            XnaVector2 curOffset = XnaVector2.Zero;
            bool firstInLine = true;

            XnaVector2 baseOffset = XnaVector2.Zero;
            float axisDirX = 1;
            float axisDirY = 1;

            foreach (char c in text)
            {
                // Special characters
                if (c == '\r') continue;

                if (c == '\n')
                {
                    curOffset.X = 0.0f;
                    curOffset.Y += spriteFont.LineSpacing;
                    firstInLine = true;

                    continue;
                }

                /* Get the List index from the character map, defaulting to the
				 * DefaultCharacter if it's set.
				 */
                int index = characterMap.IndexOf(c);

                if (index == -1)
                {
                    if (!spriteFont.DefaultCharacter.HasValue)
                    {
                        throw new ArgumentException(
                            "Text contains characters that cannot be" +
                            " resolved by this SpriteFont.",
                            "text"
                        );
                    }

                    index = characterMap.IndexOf(
                        spriteFont.DefaultCharacter.Value
                    );
                }

                /* For the first character in a line, always push the width
				 * rightward, even if the kerning pushes the character to the
				 * left.
				 */
                XnaVector3 cKern = kerning[index];

                if (firstInLine)
                {
                    curOffset.X += Math.Abs(cKern.X);
                    firstInLine = false;
                }
                else
                    curOffset.X += (spriteFont.Spacing + cKern.X);

                // Calculate the character origin
                Rectangle cCrop = croppingData[index];
                Rectangle cGlyph = glyphData[index];

                float offsetX = baseOffset.X + (
                                    curOffset.X + cCrop.X
                                ) * axisDirX;

                float offsetY = baseOffset.Y + (
                                    curOffset.Y + cCrop.Y
                                ) * axisDirY;


                Draw2D(textureValue,
                    Mathf.RoundToInt((x + (int) offsetX)), Mathf.RoundToInt((y + (int) offsetY)),
                    cGlyph.X, cGlyph.Y, cGlyph.Width, cGlyph.Height,
                    ref color);

                curOffset.X += cKern.Y + cKern.Z;
            }
        }

        [MethodImpl(256)]
        public bool DrawSprite(Texture2D texture, int x, int y, bool mirror, ref XnaVector3 hue)
        {
            if (texture.UnityTexture == null)
            {
                return false;
            }
            
            int w = texture.Width;
            int h = texture.Height;

            if (UseGraphicsDrawTexture)
            {
                var rect = new Rect(x * scale, y * scale, w * scale, h * scale);
                hueMaterial.SetColor(Hue, new Color(hue.X,hue.Y,hue.Z));
                hueMaterial.SetFloat(UvMirrorX, mirror ? 1 : 0);
                Graphics.DrawTexture(rect,
                    texture.UnityTexture,new Rect(0,0,1,1),
                    0, 0,0,0, hueMaterial);
            }
            else
            {
                var vertex = new PositionTextureColor4();

                if (mirror)
                {
                    vertex.Position0.x = x + w;
                    vertex.Position0.y = y + h;
                    vertex.Position0.z = 0;
                    vertex.Normal0.x = 0;
                    vertex.Normal0.y = 0;
                    vertex.Normal0.z = 1;
                    vertex.TextureCoordinate0.x = 0;
                    vertex.TextureCoordinate0.y = 1;
                    vertex.TextureCoordinate0.z = 0;

                    vertex.Position1.x = x;
                    vertex.Position1.y = y + h;
                    vertex.Position0.z = 0;
                    vertex.Normal1.x = 0;
                    vertex.Normal1.y = 0;
                    vertex.Normal1.z = 1;
                    vertex.TextureCoordinate1.x = 1;
                    vertex.TextureCoordinate1.y = 1;
                    vertex.TextureCoordinate1.z = 0;

                    vertex.Position2.x = x + w;
                    vertex.Position2.y = y;
                    vertex.Position2.z = 0;
                    vertex.Normal2.x = 0;
                    vertex.Normal2.y = 0;
                    vertex.Normal2.z = 1;
                    vertex.TextureCoordinate2.x = 0;
                    vertex.TextureCoordinate2.y = 0;
                    vertex.TextureCoordinate2.z = 0;

                    vertex.Position3.x = x;
                    vertex.Position3.y = y;
                    vertex.Position3.z = 0;
                    vertex.Normal3.x = 0;
                    vertex.Normal3.y = 0;
                    vertex.Normal3.z = 1;
                    vertex.TextureCoordinate3.x = 1;
                    vertex.TextureCoordinate3.y = 0;
                    vertex.TextureCoordinate3.z = 0;
                }
                else
                {
                    vertex.Position0.x = x;
                    vertex.Position0.y = y + h;
                    vertex.Position0.z = 0;
                    vertex.Normal0.x = 0;
                    vertex.Normal0.y = 0;
                    vertex.Normal0.z = 1;
                    vertex.TextureCoordinate0.x = 0;
                    vertex.TextureCoordinate0.y = 1;
                    vertex.TextureCoordinate0.z = 0;

                    vertex.Position1.x = x + w;
                    vertex.Position1.y = y + h;
                    vertex.Position1.z = 0;
                    vertex.Normal1.x = 0;
                    vertex.Normal1.y = 0;
                    vertex.Normal1.z = 1;
                    vertex.TextureCoordinate1.x = 1;
                    vertex.TextureCoordinate1.y = 1;
                    vertex.TextureCoordinate1.z = 0;

                    vertex.Position2.x = x;
                    vertex.Position2.y = y;
                    vertex.Position2.z = 0;
                    vertex.Normal2.x = 0;
                    vertex.Normal2.y = 0;
                    vertex.Normal2.z = 1;
                    vertex.TextureCoordinate2.x = 0;
                    vertex.TextureCoordinate2.y = 0;
                    vertex.TextureCoordinate2.z = 0;

                    vertex.Position3.x = x + w;
                    vertex.Position3.y = y;
                    vertex.Position3.z = 0;
                    vertex.Normal3.x = 0;
                    vertex.Normal3.y = 0;
                    vertex.Normal3.z = 1;
                    vertex.TextureCoordinate3.x = 1;
                    vertex.TextureCoordinate3.y = 0;
                    vertex.TextureCoordinate3.z = 0;
                }

                vertex.Hue0 = vertex.Hue1 = vertex.Hue2 = vertex.Hue3 = hue;
            
                RenderVertex(vertex, texture, hue);
            }

            return true;
        }

        [MethodImpl(256)]
        public void DrawSpriteRotated(Texture2D texture, int x, int y, int destX, int destY, ref XnaVector3 hue, float angle)
        {
            if (texture.UnityTexture == null)
            {
                return;
            }
            
            float ww = texture.Width * 0.5f;
            float hh = texture.Height * 0.5f;

            float startX = x - (destX - 44 + ww);
            float startY = y - (destY + hh);

            float sin = (float) Math.Sin(angle);
            float cos = (float) Math.Cos(angle);

            float sinx = sin * ww;
            float cosx = cos * ww;
            float siny = sin * hh;
            float cosy = cos * hh;

            var vertex = new PositionTextureColor4();

            vertex.Position0.x = startX;
            vertex.Position0.y = startY;
            vertex.Position0.x += cosx - -siny;
            vertex.Position0.y -= sinx + -cosy;
            vertex.TextureCoordinate0.x = 0;
            vertex.TextureCoordinate0.y = 0;
            vertex.TextureCoordinate0.z = 0;

            vertex.Position1.x = startX;
            vertex.Position1.y = startY;
            vertex.Position1.x += cosx - siny;
            vertex.Position1.y += -sinx + -cosy;
            vertex.TextureCoordinate1.x = 0;
            vertex.TextureCoordinate1.y = 1;
            vertex.TextureCoordinate1.z = 0;

            vertex.Position2.x = startX;
            vertex.Position2.y = startY;
            vertex.Position2.x += -cosx - -siny;
            vertex.Position2.y += sinx + cosy;
            vertex.TextureCoordinate2.x = 1;
            vertex.TextureCoordinate2.y = 0;
            vertex.TextureCoordinate2.z = 0;

            vertex.Position3.x = startX;
            vertex.Position3.y = startY;
            vertex.Position3.x += -cosx - siny;
            vertex.Position3.y += sinx + -cosy;
            vertex.TextureCoordinate3.x = 1;
            vertex.TextureCoordinate3.y = 1;
            vertex.TextureCoordinate3.z = 0;

            vertex.Hue0 = vertex.Hue1 = vertex.Hue2 = vertex.Hue3 = hue;

            RenderVertex(vertex, texture, hue);
        }

        [MethodImpl(256)]
        public bool DrawSpriteLand(Texture2D texture, int x, int y, ref Rectangle rect, ref XnaVector3 normal0, ref XnaVector3 normal1, ref XnaVector3 normal2, ref XnaVector3 normal3, ref XnaVector3 hue)
        {
            if (texture.UnityTexture == null)
            {
                return false;
            }
            
            var vertex = new PositionTextureColor4();

            vertex.TextureCoordinate0.x = 0;
            vertex.TextureCoordinate0.y = 0;
            vertex.TextureCoordinate0.z = 0;

            vertex.TextureCoordinate1.x = 1;
            vertex.TextureCoordinate1.y = vertex.TextureCoordinate1.z = 0;

            vertex.TextureCoordinate2.x = vertex.TextureCoordinate2.z = 0;
            vertex.TextureCoordinate2.y = 1;

            vertex.TextureCoordinate3.x = vertex.TextureCoordinate3.y = 1;
            vertex.TextureCoordinate3.z = 0;

            vertex.Position0.x = x + 22;
            vertex.Position0.y = y - rect.Left;
            vertex.Position0.z = 0;

            vertex.Position1.x = x + 44;
            vertex.Position1.y = y + (22 - rect.Bottom);
            vertex.Position1.z = 0;

            vertex.Position2.x = x;
            vertex.Position2.y = y + (22 - rect.Top);
            vertex.Position2.z = 0;

            vertex.Position3.x = x + 22;
            vertex.Position3.y = y + (44 - rect.Right);
            vertex.Position3.z = 0;

            vertex.Hue0 = vertex.Hue1 = vertex.Hue2 = vertex.Hue3 = hue;

            vertex.Normal0 = normal0;
            vertex.Normal1 = normal1;
            vertex.Normal3 = normal2; // right order!
            vertex.Normal2 = normal3;

            RenderVertex(vertex, texture, hue);

            return true;
        }

        [MethodImpl(256)]
        public void DrawSpriteShadow(Texture2D texture, int x, int y, bool flip)
        { 
            if (texture.UnityTexture == null)
            {
                return;
            }
            
            var vertex = new PositionTextureColor4();

            float width = texture.Width;
            float height = texture.Height * 0.5f;

            float translatedY = y + height * 0.75f;

            float ratio = height / width;

            if (flip)
            {
                vertex.Position0.x = x + width;
                vertex.Position0.y = translatedY + height;
                vertex.Position0.z = 0;
                vertex.TextureCoordinate0.x = 0;
                vertex.TextureCoordinate0.y = 1;
                vertex.TextureCoordinate0.z = 0;

                vertex.Position1.x = x;
                vertex.Position1.y = translatedY + height;
                vertex.TextureCoordinate1.x = 1;
                vertex.TextureCoordinate1.y = 1;
                vertex.TextureCoordinate1.z = 0;

                vertex.Position2.x = x + width * (ratio + 1f);
                vertex.Position2.y = translatedY;
                vertex.TextureCoordinate2.x = 0;
                vertex.TextureCoordinate2.y = 0;
                vertex.TextureCoordinate2.z = 0;

                vertex.Position3.x = x + width * ratio;
                vertex.Position3.y = translatedY;
                vertex.TextureCoordinate3.x = 1;
                vertex.TextureCoordinate3.y = 0;
                vertex.TextureCoordinate3.z = 0;
            }
            else
            {
                vertex.Position0.x = x;
                vertex.Position0.y = translatedY + height;
                vertex.Position0.z = 0;
                vertex.TextureCoordinate0.x = 0;
                vertex.TextureCoordinate0.y = 1;
                vertex.TextureCoordinate0.z = 0;

                vertex.Position1.x = x + width;
                vertex.Position1.y = translatedY + height;
                vertex.TextureCoordinate1.x = 1;
                vertex.TextureCoordinate1.y = 1;
                vertex.TextureCoordinate1.z = 0;

                vertex.Position2.x = x + width * ratio;
                vertex.Position2.y = translatedY;
                vertex.TextureCoordinate2.x = 0;
                vertex.TextureCoordinate2.y = 0;
                vertex.TextureCoordinate2.z = 0;

                vertex.Position3.x = x + width * (ratio + 1f);
                vertex.Position3.y = translatedY;
                vertex.TextureCoordinate3.x = 1;
                vertex.TextureCoordinate3.y = 0;
                vertex.TextureCoordinate3.z = 0;
            }

            vertex.Hue0.z =
                vertex.Hue1.z =
                    vertex.Hue2.z =
                        vertex.Hue3.z =
                            vertex.Hue0.x =
                                vertex.Hue1.x =
                                    vertex.Hue2.x =
                                        vertex.Hue3.x = 0;

            vertex.Hue0.y =
                vertex.Hue1.y =
                    vertex.Hue2.y =
                        vertex.Hue3.y = ShaderHuesTraslator.SHADER_SHADOW;

            RenderVertex(vertex, texture, vertex.Hue0);
        }

        private void RenderVertex(PositionTextureColor4 vertex, Texture2D texture, Vector3 hue)
        {
            vertex.Position0 *= scale;
            vertex.Position1 *= scale;
            vertex.Position2 *= scale;
            vertex.Position3 *= scale;

            reusedMesh.Populate(vertex);

            var mat = hueMaterial;
            mat.mainTexture = texture.UnityTexture;
            mat.SetColor(Hue, new Color(hue.x,hue.y,hue.z));
            mat.SetPass(0);

            Graphics.DrawMeshNow(reusedMesh.Mesh, Vector3.zero, Quaternion.identity);
        }

        [MethodImpl(256)]
        public bool DrawCharacterSitted(Texture2D texture, int x, int y, bool mirror, float h3mod, float h6mod, float h9mod, ref XnaVector3 hue)
        { 
            float width = texture.Width;
            float height = texture.Height;


            float h03 = height * h3mod;
            float h06 = height * h6mod;
            float h09 = height * h9mod;

            const float SITTING_OFFSET = 8.0f;

            float widthOffset = width + SITTING_OFFSET;


            if (mirror)
            {
                if (h3mod != 0.0f)
                {
                    var vertex = new PositionTextureColor4();

                    vertex.Position0.x = x + width;
                    vertex.Position0.y = y;
                    vertex.Position0.z = 0;
                    vertex.TextureCoordinate0.x = 0;
                    vertex.TextureCoordinate0.y = 0;
                    vertex.TextureCoordinate0.z = 0;

                    vertex.Position1.x = x;
                    vertex.Position1.y = y;
                    vertex.Position1.z = 0;
                    vertex.TextureCoordinate1.x = 1;
                    vertex.TextureCoordinate1.y = 0;
                    vertex.TextureCoordinate1.z = 0;

                    vertex.Position2.x = x + width;
                    vertex.Position2.y = y + h03;
                    vertex.Position2.z = 0;
                    vertex.TextureCoordinate2.x = 0;
                    vertex.TextureCoordinate2.y = h3mod;
                    vertex.TextureCoordinate2.z = 0;

                    vertex.Position3.x = x;
                    vertex.Position3.y = y + h03;
                    vertex.Position3.z = 0;
                    vertex.TextureCoordinate3.x = 1;
                    vertex.TextureCoordinate3.y = h3mod;
                    vertex.TextureCoordinate3.z = 0;

                    vertex.Hue0 = vertex.Hue1 = vertex.Hue2 = vertex.Hue3 = hue;

                    RenderVertex(vertex, texture, hue);
                }

                if (h6mod != 0.0f)
                {

                    var vertex = new PositionTextureColor4();

                    vertex.Position0.x = x + width;
                    vertex.Position0.y = y + h03;
                    vertex.Position0.z = 0;
                    vertex.TextureCoordinate0.x = 0;
                    vertex.TextureCoordinate0.y = h3mod;
                    vertex.TextureCoordinate0.z = 0;

                    vertex.Position1.x = x;
                    vertex.Position1.y = y + h03;
                    vertex.Position1.z = 0;
                    vertex.TextureCoordinate1.x = 1;
                    vertex.TextureCoordinate1.y = h3mod;
                    vertex.TextureCoordinate1.z = 0;

                    vertex.Position2.x = x + widthOffset;
                    vertex.Position2.y = y + h06;
                    vertex.Position2.z = 0;
                    vertex.TextureCoordinate2.x = 0;
                    vertex.TextureCoordinate2.y = h6mod;
                    vertex.TextureCoordinate2.z = 0;

                    vertex.Position3.x = x + SITTING_OFFSET;
                    vertex.Position3.y = y + h06;
                    vertex.Position3.z = 0;
                    vertex.TextureCoordinate3.x = 1;
                    vertex.TextureCoordinate3.y = h6mod;
                    vertex.TextureCoordinate3.z = 0;

                    vertex.Hue0 = vertex.Hue1 = vertex.Hue2 = vertex.Hue3 = hue;

                    RenderVertex(vertex, texture, hue);
                }

                if (h9mod != 0.0f)
                {
                    var vertex = new PositionTextureColor4();

                    vertex.Position0.x = x + widthOffset;
                    vertex.Position0.y = y + h06;
                    vertex.Position0.z = 0;
                    vertex.TextureCoordinate0.x = 0;
                    vertex.TextureCoordinate0.y = h6mod;
                    vertex.TextureCoordinate0.z = 0;

                    vertex.Position1.x = x + SITTING_OFFSET;
                    vertex.Position1.y = y + h06;
                    vertex.Position1.z = 0;
                    vertex.TextureCoordinate1.x = 1;
                    vertex.TextureCoordinate1.y = h6mod;
                    vertex.TextureCoordinate1.z = 0;

                    vertex.Position2.x = x + widthOffset;
                    vertex.Position2.y = y + h09;
                    vertex.Position2.z = 0;
                    vertex.TextureCoordinate2.x = 0;
                    vertex.TextureCoordinate2.y = 1;
                    vertex.TextureCoordinate2.z = 0;

                    vertex.Position3.x = x + SITTING_OFFSET;
                    vertex.Position3.y = y + h09;
                    vertex.Position3.z = 0;
                    vertex.TextureCoordinate3.x = 1;
                    vertex.TextureCoordinate3.y = 1;
                    vertex.TextureCoordinate3.z = 0;

                    vertex.Hue0 = vertex.Hue1 = vertex.Hue2 = vertex.Hue3 = hue;

                    RenderVertex(vertex, texture, hue);
                }
            }
            else
            {
                if (h3mod != 0.0f)
                {
                    var vertex = new PositionTextureColor4();

                    vertex.Position0.x = x + SITTING_OFFSET;
                    vertex.Position0.y = y;
                    vertex.Position0.z = 0;
                    vertex.TextureCoordinate0.x = 0;
                    vertex.TextureCoordinate0.y = 0;
                    vertex.TextureCoordinate0.z = 0;

                    vertex.Position1.x = x + widthOffset;
                    vertex.Position1.y = y;
                    vertex.Position1.z = 0;
                    vertex.TextureCoordinate1.x = 1;
                    vertex.TextureCoordinate1.y = 0;
                    vertex.TextureCoordinate1.z = 0;

                    vertex.Position2.x = x + SITTING_OFFSET;
                    vertex.Position2.y = y + h03;
                    vertex.Position2.z = 0;
                    vertex.TextureCoordinate2.x = 0;
                    vertex.TextureCoordinate2.y = h3mod;
                    vertex.TextureCoordinate2.z = 0;

                    vertex.Position3.x = x + widthOffset;
                    vertex.Position3.y = y + h03;
                    vertex.Position3.z = 0;
                    vertex.TextureCoordinate3.x = 1;
                    vertex.TextureCoordinate3.y = h3mod;
                    vertex.TextureCoordinate3.z = 0;

                    vertex.Hue0 = vertex.Hue1 = vertex.Hue2 = vertex.Hue3 = hue;

                    RenderVertex(vertex, texture, hue);
                }

                if (h6mod != 0.0f)
                {
                    if (h3mod == 0.0f)
                    {

                    }
                    var vertex = new PositionTextureColor4();

                    vertex.Position0.x = x + SITTING_OFFSET;
                    vertex.Position0.y = y + h03;
                    vertex.Position0.z = 0;
                    vertex.TextureCoordinate0.x = 0;
                    vertex.TextureCoordinate0.y = h3mod;
                    vertex.TextureCoordinate0.z = 0;

                    vertex.Position1.x = x + widthOffset;
                    vertex.Position1.y = y + h03;
                    vertex.Position1.z = 0;
                    vertex.TextureCoordinate1.x = 1;
                    vertex.TextureCoordinate1.y = h3mod;
                    vertex.TextureCoordinate1.z = 0;

                    vertex.Position2.x = x;
                    vertex.Position2.y = y + h06;
                    vertex.Position2.z = 0;
                    vertex.TextureCoordinate2.x = 0;
                    vertex.TextureCoordinate2.y = h6mod;
                    vertex.TextureCoordinate2.z = 0;

                    vertex.Position3.x = x + width;
                    vertex.Position3.y = y + h06;
                    vertex.Position3.z = 0;
                    vertex.TextureCoordinate3.x = 1;
                    vertex.TextureCoordinate3.y = h6mod;
                    vertex.TextureCoordinate3.z = 0;

                    vertex.Hue0 = vertex.Hue1 = vertex.Hue2 = vertex.Hue3 = hue;

                    RenderVertex(vertex, texture, hue);
                }

                if (h9mod != 0.0f)
                {
                    if (h6mod == 0.0f)
                    {

                    }

                    var vertex = new PositionTextureColor4();

                    vertex.Position0.x = x;
                    vertex.Position0.y = y + h06;
                    vertex.Position0.z = 0;
                    vertex.TextureCoordinate0.x = 0;
                    vertex.TextureCoordinate0.y = h6mod;
                    vertex.TextureCoordinate0.z = 0;

                    vertex.Position1.x = x + width;
                    vertex.Position1.y = y + h06;
                    vertex.Position1.z = 0;
                    vertex.TextureCoordinate1.x = 1;
                    vertex.TextureCoordinate1.y = h6mod;
                    vertex.TextureCoordinate1.z = 0;

                    vertex.Position2.x = x;
                    vertex.Position2.y = y + h09;
                    vertex.Position2.z = 0;
                    vertex.TextureCoordinate2.x = 0;
                    vertex.TextureCoordinate2.y = 1;
                    vertex.TextureCoordinate2.z = 0;

                    vertex.Position3.x = x + width;
                    vertex.Position3.y = y + h09;
                    vertex.Position3.z = 0;
                    vertex.TextureCoordinate3.x = 1;
                    vertex.TextureCoordinate3.y = 1;
                    vertex.TextureCoordinate3.z = 0;

                    vertex.Hue0 = vertex.Hue1 = vertex.Hue2 = vertex.Hue3 = hue;

                    RenderVertex(vertex, texture, hue);
                }
            }

            return true;
        }

        [MethodImpl(256)]
        public bool Draw2D(Texture2D texture, int x, int y, ref XnaVector3 hue)
        {
            if (texture.UnityTexture == null)
            {
                return false;
            }
            
            if (UseGraphicsDrawTexture)
            {
                var rect = new Rect(x * scale, y * scale, texture.Width * scale, texture.Height * scale);
                hueMaterial.SetColor(Hue, new Color(hue.X,hue.Y,hue.Z));
                hueMaterial.SetFloat(UvMirrorX, 0);
                Graphics.DrawTexture(rect,
                    texture.UnityTexture,new Rect(0,0,1,1),
                    0, 0,0,0, hueMaterial);
            }
            else
            {
                var vertex = new PositionTextureColor4();

                vertex.Position0.x = x;
                vertex.Position0.y = y;
                vertex.Position0.z = 0;
                vertex.TextureCoordinate0.x = 0;
                vertex.TextureCoordinate0.y = 0;
                vertex.TextureCoordinate0.z = 0;

                vertex.Position1.x = x + texture.Width;
                vertex.Position1.y = y;
                vertex.Position1.z = 0;
                vertex.TextureCoordinate1.x = 1;
                vertex.TextureCoordinate1.y = 0;
                vertex.TextureCoordinate1.z = 0;

                vertex.Position2.x = x;
                vertex.Position2.y = y + texture.Height;
                vertex.Position2.z = 0;
                vertex.TextureCoordinate2.x = 0;
                vertex.TextureCoordinate2.y = 1;
                vertex.TextureCoordinate2.z = 0;

                vertex.Position3.x = x + texture.Width;
                vertex.Position3.y = y + texture.Height;
                vertex.Position3.z = 0;
                vertex.TextureCoordinate3.x = 1;
                vertex.TextureCoordinate3.y = 1;
                vertex.TextureCoordinate3.z = 0;

                vertex.Hue0 = vertex.Hue1 = vertex.Hue2 = vertex.Hue3 = hue;
                
                RenderVertex(vertex, texture, hue);
            }

            return true;
        }

        [MethodImpl(256)]
        public bool Draw2D(Texture2D texture, int x, int y, int sx, int sy, float swidth, float sheight, ref XnaVector3 hue)
        { 
            if (texture.UnityTexture == null)
            {
                return false;
            }
            
            float minX = sx / (float) texture.Width;
            float maxX = (sx + swidth) / texture.Width;
            float minY = sy / (float) texture.Height;
            float maxY = (sy + sheight) / texture.Height;

            if (UseGraphicsDrawTexture)
            {
                hueMaterial.SetColor(Hue, new Color(hue.X,hue.Y,hue.Z));
                hueMaterial.SetFloat(UvMirrorX, 0);
                //NOTE: given sourceRect needs to be flipped vertically for some reason
                Graphics.DrawTexture(new Rect(x * scale, y * scale, swidth * scale, sheight * scale),
                    texture.UnityTexture,new Rect(minX, 1 - maxY, maxX - minX, maxY - minY),
                    0, 0,0,0, hueMaterial);
            }
            else
            {
                var vertex = new PositionTextureColor4();

                vertex.Position0.x = x;
                vertex.Position0.y = y;
                vertex.Position0.z = 0;
                vertex.Normal0.x = 0;
                vertex.Normal0.y = 0;
                vertex.Normal0.z = 1;
                vertex.TextureCoordinate0.x = minX;
                vertex.TextureCoordinate0.y = minY;
                vertex.TextureCoordinate0.z = 0;
                vertex.Position1.x = x + swidth;
                vertex.Position1.y = y;
                vertex.Position1.z = 0;
                vertex.Normal1.x = 0;
                vertex.Normal1.y = 0;
                vertex.Normal1.z = 1;
                vertex.TextureCoordinate1.x = maxX;
                vertex.TextureCoordinate1.y = minY;
                vertex.TextureCoordinate1.z = 0;
                vertex.Position2.x = x;
                vertex.Position2.y = y + sheight;
                vertex.Position2.z = 0;
                vertex.Normal2.x = 0;
                vertex.Normal2.y = 0;
                vertex.Normal2.z = 1;
                vertex.TextureCoordinate2.x = minX;
                vertex.TextureCoordinate2.y = maxY;
                vertex.TextureCoordinate2.z = 0;
                vertex.Position3.x = x + swidth;
                vertex.Position3.y = y + sheight;
                vertex.Position3.z = 0;
                vertex.Normal3.x = 0;
                vertex.Normal3.y = 0;
                vertex.Normal3.z = 1;
                vertex.TextureCoordinate3.x = maxX;
                vertex.TextureCoordinate3.y = maxY;
                vertex.TextureCoordinate3.z = 0;
                vertex.Hue0 = vertex.Hue1 = vertex.Hue2 = vertex.Hue3 = hue;
                
                RenderVertex(vertex, texture, hue);
            }

            return true;
        }

        [MethodImpl(256)]
        public bool Draw2D(Texture2D texture, float dx, float dy, float dwidth, float dheight, int sx, int sy, float swidth, float sheight, ref XnaVector3 hue, float angle = 0.0f)
        {
            if (texture.UnityTexture == null)
            {
                return false;
            }
            
            float minX = sx / (float) texture.Width, maxX = (sx + swidth) / texture.Width;
            float minY = sy / (float) texture.Height, maxY = (sy + sheight) / texture.Height;

            var vertex = new PositionTextureColor4();

            float x = dx;
            float y = dy;
            float w = dx + dwidth;
            float h = dy + dheight;

            if (angle != 0.0f)
            {
                angle = (float)(angle * Math.PI) / 180.0f;

                float ww = dwidth * 0.5f;
                float hh = dheight * 0.5f;

                float sin = (float)Math.Sin(angle);
                float cos = (float)Math.Cos(angle);

                float tempX = -ww;
                float tempY = -hh;
                float rotX = tempX * cos - tempY * sin;
                float rotY = tempX * sin + tempY * cos;
                rotX += dx + ww;
                rotY += dy + hh;

                vertex.Position0.x = rotX;
                vertex.Position0.y = rotY;

                tempX = dwidth - ww;
                tempY = -hh;
                rotX = tempX * cos - tempY * sin;
                rotY = tempX * sin + tempY * cos;
                rotX += dx + ww;
                rotY += dy + hh;

                vertex.Position1.x = rotX;
                vertex.Position1.y = rotY;

                tempX = -ww;
                tempY = dheight - hh;
                rotX = tempX * cos - tempY * sin;
                rotY = tempX * sin + tempY * cos;
                rotX += dx + ww;
                rotY += dy + hh;

                vertex.Position2.x = rotX;
                vertex.Position2.y = rotY;

                tempX = dwidth - ww;
                tempY = dheight - hh;
                rotX = tempX * cos - tempY * sin;
                rotY = tempX * sin + tempY * cos;
                rotX += dx + ww;
                rotY += dy + hh;

                vertex.Position3.x = rotX;
                vertex.Position3.y = rotY;
            }
            else
            {
                vertex.Position0.x = x;
                vertex.Position0.y = y;

                vertex.Position1.x = w;
                vertex.Position1.y = y;

                vertex.Position2.x = x;
                vertex.Position2.y = h;

                vertex.Position3.x = w;
                vertex.Position3.y = h;

                if (UseGraphicsDrawTexture)
                {
                    hueMaterial.SetColor(Hue, new Color(hue.X,hue.Y,hue.Z));
                    hueMaterial.SetFloat(UvMirrorX, 0);
                    //NOTE: given sourceRect needs to be flipped vertically for some reason
                    Graphics.DrawTexture(new Rect(x * scale, y * scale, dwidth * scale, dheight * scale),
                        texture.UnityTexture, new Rect(minX, 1 - maxY, maxX - minX, maxY - minY),
                        0, 0, 0, 0, hueMaterial);
                    return true;
                }
            }

            vertex.Position0.z = 0;
            vertex.Normal0.x = 0;
            vertex.Normal0.y = 0;
            vertex.Normal0.z = 1;
            vertex.TextureCoordinate0.x = minX;
            vertex.TextureCoordinate0.y = minY;
            vertex.TextureCoordinate0.z = 0;

            vertex.Position1.z = 0;
            vertex.Normal1.x = 0;
            vertex.Normal1.y = 0;
            vertex.Normal1.z = 1;
            vertex.TextureCoordinate1.x = maxX;
            vertex.TextureCoordinate1.y = minY;
            vertex.TextureCoordinate1.z = 0;

            vertex.Position2.z = 0;
            vertex.Normal2.x = 0;
            vertex.Normal2.y = 0;
            vertex.Normal2.z = 1;
            vertex.TextureCoordinate2.x = minX;
            vertex.TextureCoordinate2.y = maxY;
            vertex.TextureCoordinate2.z = 0;

            vertex.Position3.z = 0;
            vertex.Normal3.x = 0;
            vertex.Normal3.y = 0;
            vertex.Normal3.z = 1;
            vertex.TextureCoordinate3.x = maxX;
            vertex.TextureCoordinate3.y = maxY;
            vertex.TextureCoordinate3.z = 0;
            vertex.Hue0 = vertex.Hue1 = vertex.Hue2 = vertex.Hue3 = hue;

            RenderVertex(vertex, texture, hue);

            return true;
        }

        [MethodImpl(256)]
        public bool Draw2D(Texture2D texture, int x, int y, float width, float height, ref XnaVector3 hue)
        {
            if (texture.UnityTexture == null)
            {
                return false;
            }
            
            if (UseGraphicsDrawTexture)
            {
                if (CustomEffect is XBREffect xbrEffect)
                {
                    xbrMaterial.SetVector(TextureSize, new Vector4(xbrEffect._vectorSize.X, xbrEffect._vectorSize.Y));
                    Graphics.DrawTexture(new Rect(x * scale, y * scale, width * scale, height * scale), texture.UnityTexture, xbrMaterial);
                }
                else
                {
                    hueMaterial.SetColor(Hue, new Color(hue.X,hue.Y,hue.Z));
                    hueMaterial.SetFloat(UvMirrorX, 0);
                    Graphics.DrawTexture(new Rect(x * scale, y * scale, width * scale, height * scale), texture.UnityTexture, hueMaterial);
                }
            }
            else
            {
                var vertex = new PositionTextureColor4();

                vertex.Position0.x = x;
                vertex.Position0.y = y;
                vertex.Position0.z = 0;
                vertex.Normal0.x = 0;
                vertex.Normal0.y = 0;
                vertex.Normal0.z = 1;
                vertex.TextureCoordinate0.x = 0;
                vertex.TextureCoordinate0.y = 0;
                vertex.TextureCoordinate0.z = 0;

                vertex.Position1.x = x + width;
                vertex.Position1.y = y;
                vertex.Position1.z = 0;
                vertex.Normal1.x = 0;
                vertex.Normal1.y = 0;
                vertex.Normal1.z = 1;
                vertex.TextureCoordinate1.x = 1;
                vertex.TextureCoordinate1.y = 0;
                vertex.TextureCoordinate1.z = 0;

                vertex.Position2.x = x;
                vertex.Position2.y = y + height;
                vertex.Position2.z = 0;
                vertex.Normal2.x = 0;
                vertex.Normal2.y = 0;
                vertex.Normal2.z = 1;
                vertex.TextureCoordinate2.x = 0;
                vertex.TextureCoordinate2.y = 1;
                vertex.TextureCoordinate2.z = 0;

                vertex.Position3.x = x + width;
                vertex.Position3.y = y + height;
                vertex.Position3.z = 0;
                vertex.Normal3.x = 0;
                vertex.Normal3.y = 0;
                vertex.Normal3.z = 1;
                vertex.TextureCoordinate3.x = 1;
                vertex.TextureCoordinate3.y = 1;
                vertex.TextureCoordinate3.z = 0;

                vertex.Hue0 = vertex.Hue1 = vertex.Hue2 = vertex.Hue3 = hue;
                
                RenderVertex(vertex, texture, hue);
            }

            return true;
        }

        [MethodImpl(256)]
        public bool Draw2DTiled(Texture2D texture, int dx, int dy, float dwidth, float dheight, ref XnaVector3 hue)
        {
            if (texture.UnityTexture == null)
            {
                return false;
            }
            
            int y = dy;
            int h = (int) dheight;

            while (h > 0)
            {
                int x = dx;
                int w = (int) dwidth;

                int rw = texture.Width;
                int rh = h < texture.Height ? h : texture.Height;

                while (w > 0)
                {
                    if (w < texture.Width)
                        rw = w;
                    Draw2D(texture, x, y, 0, 0, rw, rh, ref hue);
                    w -= texture.Width;
                    x += texture.Width;
                }

                h -= texture.Height;
                y += texture.Height;
            }

            return true;
        }

        [MethodImpl(256)]
        public bool DrawRectangle(Texture2D texture, int x, int y, int width, int height, ref XnaVector3 hue)
        {
            if (texture.UnityTexture == null)
            {
                return false;
            }
            
            Draw2D(texture, x, y, width, 1, ref hue);
            Draw2D(texture, x + width, y, 1, height + 1, ref hue);
            Draw2D(texture, x, y + height, width, 1, ref hue);
            Draw2D(texture, x, y, 1, height, ref hue);

            return true;
        }

        [MethodImpl(256)]
        public void DrawLine(Texture2D texture, int startX, int startY, int endX, int endY, int originX, int originY)
        {
            if (texture.UnityTexture == null)
            {
                return;
            }
            
            var vertex = new PositionTextureColor4();

            const int WIDTH = 1;
            XnaVector2 begin = new XnaVector2(startX, startY);
            XnaVector2 end = new XnaVector2(endX, endY);

            Rectangle r = new Rectangle((int)begin.X, (int)begin.Y, (int)(end - begin).Length() + WIDTH, WIDTH);

            float angle = (float)(Math.Atan2(end.Y - begin.Y, end.X - begin.X) * 57.295780);
            angle = -(float)(angle * Math.PI) / 180.0f;


            float ww = r.Width * 0.5f;
            float hh = r.Height * 0.5f;


            float rotSin = (float) Math.Sin(angle);
            float rotCos = (float) Math.Cos(angle);


            float sinx = rotSin * ww;
            float cosx = rotCos * ww;
            float siny = rotSin * hh;
            float cosy = rotCos * hh;


            vertex.Position0.x = originX;
            vertex.Position0.y = originY;
            vertex.Position0.x += cosx - -siny;
            vertex.Position0.y -= sinx + -cosy;
            vertex.TextureCoordinate0.x = 0;
            vertex.TextureCoordinate0.y = 0;
            vertex.TextureCoordinate0.z = 0;

            vertex.Position1.x = originX;
            vertex.Position1.y = originY;
            vertex.Position1.x += cosx - siny;
            vertex.Position1.y += -sinx + -cosy;
            vertex.TextureCoordinate1.x = 0;
            vertex.TextureCoordinate1.y = 1;
            vertex.TextureCoordinate1.z = 0;

            vertex.Position2.x = originX;
            vertex.Position2.y = originY;
            vertex.Position2.x += -cosx - -siny;
            vertex.Position2.y += sinx + cosy;
            vertex.TextureCoordinate2.x = 1;
            vertex.TextureCoordinate2.y = 0;
            vertex.TextureCoordinate2.z = 0;

            vertex.Position3.x = originX;
            vertex.Position3.y = originY;
            vertex.Position3.x += -cosx - siny;
            vertex.Position3.y += sinx + -cosy;
            vertex.TextureCoordinate3.x = 1;
            vertex.TextureCoordinate3.y = 1;
            vertex.TextureCoordinate3.z = 0;

            vertex.Hue0 = vertex.Hue1 = vertex.Hue2 = vertex.Hue3 = Vector3.zero;

            RenderVertex(vertex, texture, Vector3.zero);
        }

        [MethodImpl(256)]
        public void Begin()
        {
            hueMaterial.SetTexture(HueTex1, GraphicsDevice.Textures[1].UnityTexture);
            hueMaterial.SetTexture(HueTex2, GraphicsDevice.Textures[2].UnityTexture);
        }

        public void Begin(Effect effect)
        {
            CustomEffect = effect;
        }

        [MethodImpl(256)]
        public void End()
        {
            CustomEffect = null;
        }

        //Because XNA's Blend enum starts with 1, we duplicate BlendMode.Zero for 0th index
        //and also for indexes 12-15 where Unity's BlendMode enum doesn't have a match to XNA's Blend enum
        //and we don't need those anyways
        private static readonly BlendMode[] BlendModesMatchingXna =
        {
            BlendMode.Zero,
            BlendMode.Zero,
            BlendMode.One,
            BlendMode.SrcColor,
            BlendMode.OneMinusSrcColor,
            BlendMode.SrcAlpha,
            BlendMode.OneMinusSrcAlpha,
            BlendMode.DstAlpha,
            BlendMode.OneMinusDstAlpha,
            BlendMode.DstColor,
            BlendMode.OneMinusDstColor,
            BlendMode.SrcAlphaSaturate,
            BlendMode.Zero,
            BlendMode.Zero,
            BlendMode.Zero,
            BlendMode.Zero
        };

        private static void SetMaterialBlendState(Material mat, BlendState blendState)
        {
            var src = BlendModesMatchingXna[(int) blendState.ColorSourceBlend];
            var dst = BlendModesMatchingXna[(int) blendState.ColorDestinationBlend];
            SetMaterialBlendState(mat, src, dst);
        }

        private static void SetMaterialBlendState(Material mat, BlendMode src, BlendMode dst)
        {
            mat.SetFloat(SrcBlend, (float) src);
            mat.SetFloat(DstBlend, (float) dst);
        }

        private void ApplyStates()
        {
            // GraphicsDevice.BlendState = _blendState;
            SetMaterialBlendState(hueMaterial, _blendState);

            GraphicsDevice.DepthStencilState = _stencil;

            // GraphicsDevice.RasterizerState = _useScissor ? _rasterizerState : RasterizerState.CullNone;
            hueMaterial.SetFloat(Scissor, _useScissor ? 1 : 0);
            if (_useScissor)
            {
                var scissorRect = GraphicsDevice.ScissorRectangle;
                var scissorVector4 = new Vector4(scissorRect.X * scale,
                    scissorRect.Y * scale,
                    scissorRect.X * scale + scissorRect.Width * scale,
                    scissorRect.Y * scale + scissorRect.Height * scale);
                hueMaterial.SetVector(ScissorRect, scissorVector4);
            }

            DefaultEffect.ApplyStates();
        }

        [MethodImpl(256)]
        public void EnableScissorTest(bool enable)
        {
            if (enable == _useScissor)
                return;

            if (!enable && _useScissor && ScissorStack.HasScissors)
                return;

            _useScissor = enable;
            ApplyStates();
        }

        [MethodImpl(256)]
        public void SetBlendState(BlendState blend)
        {
            _blendState = blend ?? BlendState.AlphaBlend;
            ApplyStates();
        }

        [MethodImpl(256)]
        public void SetStencil(DepthStencilState stencil)
        {
            _stencil = stencil ?? Stencil;
            ApplyStates();
        }

        public void Dispose()
        {
            DefaultEffect?.Dispose();
        }

        private class IsometricEffect : MatrixEffect
        {
            private Vector2 _viewPort;
            private Matrix _matrix = Matrix.Identity;

            public IsometricEffect(GraphicsDevice graphicsDevice) : base(graphicsDevice, Resources.IsometricEffect)
            {
                WorldMatrix = Parameters["WorldMatrix"];
                Viewport = Parameters["Viewport"];
                //NOTE: Since we don't parse the mojoshader to read the properties, Brightlight doesn't exist as a key in the Parameters dictionary
                Parameters.Add("Brightlight", new EffectParameter());
                Brighlight = Parameters["Brightlight"];

                CurrentTechnique = Techniques["HueTechnique"];
            }

            protected IsometricEffect(Effect cloneSource) : base(cloneSource)
            {
            }


            public EffectParameter WorldMatrix { get; }
            public EffectParameter Viewport { get; }
            public EffectParameter Brighlight { get; }


            public override void ApplyStates()
            {
                WorldMatrix.SetValue(_matrix);

                _viewPort.x = GraphicsDevice.Viewport.Width;
                _viewPort.y = GraphicsDevice.Viewport.Height;
                Viewport.SetValue(_viewPort);

                base.ApplyStates();
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PositionTextureColor4
        {
            public Vector3 Position0;
            public Vector3 TextureCoordinate0;
            public Vector3 Hue0;
            public Vector3 Normal0;

            public Vector3 Position1;
            public Vector3 TextureCoordinate1;
            public Vector3 Hue1;
            public Vector3 Normal1;

            public Vector3 Position2;
            public Vector3 TextureCoordinate2;
            public Vector3 Hue2;
            public Vector3 Normal2;

            public Vector3 Position3;
            public Vector3 TextureCoordinate3;
            public Vector3 Hue3;
            public Vector3 Normal3;
        }
    }
}