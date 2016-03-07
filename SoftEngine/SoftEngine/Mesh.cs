
using SharpDX;

namespace SoftEngine
{
    public class Mesh
    {
        public struct Face
        {
            public int A;
            public int B;
            public int C;
        }
        public string Name { get; set; }
        public Face[] Faces { get; set; }
        public Vector3[] Vertices { get; private set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public Mesh(string name, int verticesCount, int facecount)
        {
            Vertices = new Vector3[verticesCount];
            Name = name;
            Faces = new Face[facecount];
        }
    }
}
