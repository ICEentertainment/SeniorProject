using Windows.UI.Xaml.Media.Imaging;
using SharpDX;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Engine
{
    class Device
    {
        private byte[] backBuffer;
        private WriteableBitmap bmp;
        
        public Device(WriteableBitmap bmp)
        {
            this.bmp = bmp;
            // Pixel width * height + 4 color properties
            backBuffer = new byte[bmp.PixelWidth * bmp.PixelHeight * 4];
        }
        //Create backbuffer with a specific color
        public void Clear(byte r, byte g, byte b, byte a)
        {
            for(var i= 0; i<backBuffer.Length; i += 4)
            {
                backBuffer[i] = b;
                backBuffer[i + 1] = g;
                backBuffer[i + 2] = r;
                backBuffer[i + 3] = a;
            }
        }
        //Flush the back buffer and put all into the front buffer
        public void Present()
        {
            using (var stream = bmp.PixelBuffer.AsStream()){
                //Writing our bmp into our backbuffer
                stream.Write(backBuffer, 0, backBuffer.Length);
            }
            //We are done with our bmp
            bmp.Invalidate();
        }
        //Color a specifc pixel
        public void PutPixel(int x, int y, Color4 color)
        {
            //Need to convert our 2D coordinates, X and Y to 1d for our back buffer
            var index = (x + y * bmp.PixelWidth) + 4;

            backBuffer[index] = (byte)(color.Blue* 255);
            backBuffer[index + 1] = (byte)(color.Green * 255);
            backBuffer[index + 2] = (byte)(color.Red * 255);
            backBuffer[index + 3] = (byte)(color.Alpha * 255);
        }   

        //Pass in a 3D coordinate and it returns a 2D Vector
        public Vector2 Project(Vector3 coord, Matrix transMat)
        {
            //Transform the coordinates
            var point = Vector3.TransformCoordinate(coord, transMat);
            //Coordinates are now transformed according to the center of the screen.
            // Need to transform again to top left
            var x = point.X * bmp.PixelWidth + bmp.PixelWidth / 2.0f;
            var y = point.Y * bmp.PixelHeight + bmp.PixelHeight / 2.0f;
            return (new Vector2(x, y));
        }

        /*We are drawing a single point with a specfic color*/
        public void drawPoint(Vector2 point)
        {
            if (point.X >= 0 && point.Y >= 0 && point.X < bmp.PixelWidth && point.Y < bmp.PixelHeight)
            {
                PutPixel((int)point.X, (int)point.Y, new Color4(1.0f, 1.0f, 0.0f, 1.0f));
            }
        }
        public void Render(Camera camera, params Mesh[] Meshes)
        {
            var viewMatrix = Matrix.LookAtLH(camera.Position, camera.Target, Vector3.UnitY);
            var projectionMatrix = Matrix.PerspectiveFovRH(.78f, (float)bmp.PixelWidth / bmp.PixelHeight, 0.01f, 0.01f);

            foreach(Mesh mesh in Meshes)
            {
                var worldMatrix = Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z);
                var transformMatirx = worldMatrix * viewMatrix * projectionMatrix;

                foreach (var vertex in mesh.Vertices)
                {
                    var point = Project(vertex, transformMatirx);
                    drawPoint(point);
                }
            }
        }


    
    }
}
