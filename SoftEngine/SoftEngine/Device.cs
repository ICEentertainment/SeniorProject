using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using SharpDX;

namespace SoftEngine
{
    public class Device
    {
        //Creating a backbuffer object
        private byte[] backBuffer;
        private WriteableBitmap bmp;

        /*  This constructor takes in a bmp image with a declared size, our screen size,
            and inits our backbuffer with the amount of pixels to draw on the screen, wich 
            since we use V4s is 4 times the resolution. 
        */
        public Device(WriteableBitmap bmp)
        {
            this.bmp = bmp;
         
            backBuffer = new byte[bmp.PixelWidth * bmp.PixelHeight * 4];
        }

        /*  The Clear function does exactly what It says. It fills our back buffer with a certain color 
            this allows for an image to be swapped with a blank screen.
        */
        public void Clear(byte r, byte g, byte b, byte a) {
            for (var index = 0; index < backBuffer.Length; index += 4)
            {
                
                backBuffer[index] = b;
                backBuffer[index + 1] = g;
                backBuffer[index + 2] = r;
                backBuffer[index + 3] = a;
            }
        }

       /*   This is where we swap our front buffer with the back buffer. Now since we are only
            using one buffer we are not actually "swapping" the buffers but instead flushing
            the back buffer into the front so that our image is displayed and so we can start
            filling up the buffer with another image. 
       */
        public void Present()
        {
            // Open a stream to start writing to.
            using (var stream = bmp.PixelBuffer.AsStream())
            {
                
                stream.Write(backBuffer, 0, backBuffer.Length);
            }
            //Clear our back buffer so we can start filling it again.
            bmp.Invalidate();
        }

        /*  PutPixel takes a point and a color and writes into our back bufffer. This
            is how we populate our backbuffer with a specific color.
        */
        public void PutPixel(int x, int y, Color4 color)
        {
            var index = (x + y * bmp.PixelWidth) * 4;

            backBuffer[index] = (byte)(color.Blue * 255);
            backBuffer[index + 1] = (byte)(color.Green * 255);
            backBuffer[index + 2] = (byte)(color.Red * 255);
            backBuffer[index + 3] = (byte)(color.Alpha * 255);
        }

        /*  This is changes our points with the final transformation matrix 
        */
        public Vector2 Project(Vector3 coord, Matrix transMat)
        {
            
            var point = Vector3.TransformCoordinate(coord, transMat);
           
            var x = point.X * bmp.PixelWidth + bmp.PixelWidth / 2.0f;
            var y = -point.Y * bmp.PixelHeight + bmp.PixelHeight / 2.0f;
            return (new Vector2(x, y));
        }

      /*    This takes our point and draws it relitive to our world view. The function "clips" any point that 
            falls out side our projection view since we dont want to waste time drawing points we are not going
            to see. 
      */
        public void DrawPoint(Vector2 point)
        {
           
            if (point.X >= 0 && point.Y >= 0 && point.X < bmp.PixelWidth && point.Y < bmp.PixelHeight)
            {
                
                PutPixel((int)point.X, (int)point.Y, new Color4(1.0f, 1.0f, 0.0f, 1.0f));
            }
        }

        /* This is where the real magic happens. This is the most important function in the whole device.
        */
        public void Render(Camera camera, params Mesh[] meshes)
        {
            // The view matrix takes our world view and makes it relitive to our camera view. 
            // Since it is LH rule the postive Z axis is going up while the Y axis is going into 
            // the page and the X axis is going to the right. 
            var viewMatrix = Matrix.LookAtLH(camera.Position, camera.Target, Vector3.UnitY);
                
            // The projection matrix is what decides what in our view will be displayed.
            // The camera view sets up our view looking at our world matrix and the 
            // projection matrix decides what we can see in the view. 
            var projectionMatrix = Matrix.PerspectiveFovRH(0.78f, 
                                                           (float)bmp.PixelWidth / bmp.PixelHeight, 
                                                           0.01f, 1.0f);

            foreach (Mesh mesh in meshes) 
            {
                //Moves our object from model space and puts it in the world view. This is so 
                // the object doesnt lose any data since it technically doesnt get moved. 
                var worldMatrix = Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z) * 
                                  Matrix.Translation(mesh.Position);

                var transformMatrix = worldMatrix * viewMatrix * projectionMatrix;

                foreach (var vertex in mesh.Vertices)
                {
                   
                    var point = Project(vertex, transformMatrix);
                    
                    DrawPoint(point);
                }
            }
        }
    }
}
