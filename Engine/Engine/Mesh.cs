using SharpDX;
namespace Engine
{
    class Mesh
    {
        public string NAME { get; set; }
        public Vector3[] Vertices { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public Mesh(string name, int verticesCount)
        {
            Vertices = new Vector3[verticesCount];
            this.NAME = name;
        }
    }
    
}
