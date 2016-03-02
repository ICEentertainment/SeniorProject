using Windows.UI.Xaml.Media.Imaging;
using SharpDX.Mathematics.Interop;
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
        public void PutPixel(int x, int y, RawColor4 color)
        {
            //Need to convert our 2D coordinates, X and Y to 1d for our back buffer
            var index = (x + y * bmp.PixelWidth) + 4;

            backBuffer[index] = (byte)(color.B * 255);
            backBuffer[index + 1] = (byte)(color.G * 255);
            backBuffer[index + 2] = (byte)(color.R * 255);
            backBuffer[index + 3] = (byte)(color.A * 255);
        }   

        //Pass in a 3D coordinate and it returns a 2D Vector
        public RawVector2 Project(RawVector3 coord, RawMatrix transMat)
        {
            //Transform the coordinates
            var point = TransformCoordinate(coord, transMat);
            //Coordinates are now transformed according to the center of the screen.
            // Need to transform again to top left
            var x = point.X * bmp.PixelWidth + bmp.PixelWidth / 2.0f;
            var y = point.Y * bmp.PixelHeight + bmp.PixelHeight / 2.0f;
            return (new RawVector2(x, y));
        }

        /*We are drawing a single point with a specfic color*/
        public void drawPoint(RawVector2 point)
        {
            if(point.X >= 0 && point.Y >= 0 && point.X < bmp.PixelWidth && point.Y < bmp.PixelHeight)
        }
        public void Render(Camera camera, params Mesh[] Meshes)
        {
            var viewMatrix = 
        }



        public static RawVector3 TransformCoordinate(RawVector3 coordinate, RawMatrix transform)
        {
            RawVector3 result;
            TransformCoordinate(ref coordinate, ref transform, out result);
            return result;
        }
        public static void TransformCoordinate(ref RawVector3 coordinate, ref RawMatrix transform, out RawVector3 result)
        {
            RawVector4 vector = new RawVector4();
            vector.X = (coordinate.X * transform.M11) + (coordinate.Y * transform.M21) + (coordinate.Z * transform.M31) + transform.M41;
            vector.Y = (coordinate.X * transform.M12) + (coordinate.Y * transform.M22) + (coordinate.Z * transform.M32) + transform.M42;
            vector.Z = (coordinate.X * transform.M13) + (coordinate.Y * transform.M23) + (coordinate.Z * transform.M33) + transform.M43;
            vector.W = 1f / ((coordinate.X * transform.M14) + (coordinate.Y * transform.M24) + (coordinate.Z * transform.M34) + transform.M44);

            result = new RawVector3(vector.X * vector.W, vector.Y * vector.W, vector.Z * vector.W);
        }
    }
}
