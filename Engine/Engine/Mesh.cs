using SharpDX.Mathematics.Interop;
namespace Engine
{
    class Mesh
    {
        public string NAME { get; set; }
        public RawVector3[] Vertices { get; set; }
        public RawVector3 Position { get; set; }
        public RawVector3 Rotation { get; set; }

        public Mesh(string name, int verticesCount)
        {
            Vertices = new RawVector3[verticesCount];
            this.NAME = name;
        }
    }
    
}
