using ClassicUO.Renderer;
using UnityEngine;

namespace Microsoft.Xna.Framework.Graphics
{
    internal class MeshHolder
    {
        public readonly Mesh Mesh;

        private readonly UnityEngine.Vector3[] vertices;
        private readonly UnityEngine.Vector2[] uvs;
        private readonly UnityEngine.Vector3[] normals;

        public MeshHolder(int quadCount)
        {
            Mesh = new Mesh();
            //Mesh.MarkDynamic();

            quadCount = Mathf.NextPowerOfTwo(quadCount);
            int vCount = quadCount * 4;

            vertices = new UnityEngine.Vector3[vCount];
            uvs = new UnityEngine.Vector2[vCount];
            normals = new UnityEngine.Vector3[vCount];

            var triangles = new int[quadCount * 6];
            for (var i = 0; i < quadCount; i++)
            {
                /*
                 *  TL    TR
                 *   0----1 0,1,2,3 = index offsets for vertex indices
                 *   |   /| TL,TR,BL,BR are vertex references in SpriteBatchItem.
                 *   |  / |
                 *   | /  |
                 *   |/   |
                 *   2----3
                 *  BL    BR
                 */
                // Triangle 1
                triangles[i * 6] = i * 4;
                triangles[i * 6 + 1] = i * 4 + 1;
                triangles[i * 6 + 2] = i * 4 + 2;
                // Triangle 2
                triangles[i * 6 + 3] = i * 4 + 1;
                triangles[i * 6 + 4] = i * 4 + 3;
                triangles[i * 6 + 5] = i * 4 + 2;
            }

            Mesh.vertices = vertices;
            Mesh.uv = uvs;
            Mesh.triangles = triangles;
            Mesh.normals = normals;
        }

        internal void Populate(UltimaBatcher2D.PositionTextureColor4 vertex)
        {
            vertex.TextureCoordinate0.y = 1 - vertex.TextureCoordinate0.y;
            vertices[0] = vertex.Position0;
            uvs[0] = vertex.TextureCoordinate0;
            normals[0] = vertex.Normal0;

            vertex.TextureCoordinate1.y = 1 - vertex.TextureCoordinate1.y;
            vertices[1] = vertex.Position1;
            uvs[1] = vertex.TextureCoordinate1;
            normals[1] = vertex.Normal1;

            vertex.TextureCoordinate2.y = 1 - vertex.TextureCoordinate2.y;
            vertices[2] = vertex.Position2;
            uvs[2] = vertex.TextureCoordinate2;
            normals[2] = vertex.Normal2;

            vertex.TextureCoordinate3.y = 1 - vertex.TextureCoordinate3.y;
            vertices[3] = vertex.Position3;
            uvs[3] = vertex.TextureCoordinate3;
            normals[3] = vertex.Normal3;

            Mesh.vertices = vertices;
            Mesh.uv = uvs;
            Mesh.normals = normals;
        }
    }
}