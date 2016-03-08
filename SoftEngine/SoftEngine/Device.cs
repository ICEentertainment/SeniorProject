using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using SharpDX;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SoftEngine
{
    public class Device
    {
        //Creating a backbuffer object
        private byte[] backBuffer;
        private readonly float[] depthBuffer;
        private object[] lockbuffer;
        private WriteableBitmap bmp;
        private readonly int renderWidth;
        private readonly int renderHeight;

        /*  This constructor takes in a bmp image with a declared size, our screen size,
            and inits our backbuffer with the amount of pixels to draw on the screen, wich 
            since we use V4s is 4 times the resolution. 
        */
        public Device(WriteableBitmap bmp)
        {
            this.bmp = bmp;
            renderHeight = bmp.PixelHeight;
            renderWidth = bmp.PixelWidth;
            //4times our resolution becuae each pixel has 4 properties
            backBuffer = new byte[renderWidth * renderHeight * 4];
            //We only want one property per pixel, the z, so dont have to multiply it
            depthBuffer = new float[renderWidth * renderHeight];
            lockbuffer = new object[renderWidth * renderHeight];
            for(var index =0; index < lockbuffer.Length; index++)
            {
                lockbuffer[index] = new object();
            }
        }

        /*  The Clear function does exactly what It says. It fills our back buffer with a certain color 
            this allows for an image to be swapped with a blank screen.
        */
        public void Clear(byte r, byte g, byte b, byte a) {
            //clear the backbuffer
            for (var index = 0; index < backBuffer.Length; index += 4)
            {
                
                backBuffer[index] = b;
                backBuffer[index + 1] = g;
                backBuffer[index + 2] = r;
                backBuffer[index + 3] = a;
            }
            //clear the depthbuffer
            for (var index = 0; index < depthBuffer.Length; index++)
            {
                depthBuffer[index] = float.MaxValue;
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
        public void PutPixel(int x, int y, float z, Color4 color)
        {
            var index = (x + y * renderWidth);
            var index4 = index * 4;
            
            //If the point is behind another point then forget it
            lock (lockbuffer[index])
            {
                if (depthBuffer[index] < z)
                {
                    return;
                }
                depthBuffer[index] = z;

                backBuffer[index4] = (byte)(color.Blue * 255);
                backBuffer[index4 + 1] = (byte)(color.Green * 255);
                backBuffer[index4 + 2] = (byte)(color.Red * 255);
                backBuffer[index4 + 3] = (byte)(color.Alpha * 255);
            }
        }

        /*  This is changes our points with the final transformation matrix 
        */
        public Vector3 Project(Vector3 coord, Matrix transMat)
        {
            
            var point = Vector3.TransformCoordinate(coord, transMat);
           
            var x = point.X * renderWidth + renderHeight / 2.0f;
            var y = -point.Y * renderHeight + renderWidth / 2.0f;
            return (new Vector3(x, y,point.Z));
        }

      /*    This takes our point and draws it relitive to our world view. The function "clips" any point that 
            falls out side our projection view since we dont want to waste time drawing points we are not going
            to see. 
      */
        public void DrawPoint(Vector3 point, Color4 color)
        {
           
            if (point.X >= 0 && point.Y >= 0 && point.X < renderWidth && point.Y < renderHeight)
            {
                
                PutPixel((int)point.X, (int)point.Y, point.Z, color);
            }
        }
        
        float Clamp(float value, float min = 0, float max = 1)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        // Interpolating the value between 2 vertices 
        // min is the starting point, max the ending point
        // and gradient the % between the 2 points
        float Interpolate(float min, float max, float gradient)
        {
            return min + (max - min) * Clamp(gradient);
        }

        //Draws the line from two points, papb to pcpd
        void ScanLine(int y, Vector3 pa, Vector3 pb, Vector3 pc, Vector3 pd, Color4 color)
        {
            
            var gradient1 = pa.Y != pb.Y ? (y - pa.Y) / (pb.Y - pa.Y) : 1;
            var gradient2 = pc.Y != pd.Y ? (y - pc.Y) / (pd.Y - pc.Y) : 1;

            int sx = (int)Interpolate(pa.X, pb.X, gradient1);
            int ex = (int)Interpolate(pc.X, pd.X, gradient2);

            float z1 = Interpolate(pa.Z, pb.Z, gradient1);
            float z2 = Interpolate(pc.Z, pd.Z, gradient2);
            
            for (var x = sx; x < ex; x++)
            {
                float gradient = (x - sx) / (float)(ex - sx);
                var z = Interpolate(z1, z2, gradient);
                DrawPoint(new Vector3(x, y,z), color);
            }
        }

        public void DrawTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Color4 color)
        {
            //Sorting the points so our triangles always has p1 as the lowest y and so on
            if (p1.Y > p2.Y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;
            }

            if (p2.Y > p3.Y)
            {
                var temp = p2;
                p2 = p3;
                p3 = temp;
            }

            if (p1.Y > p2.Y)
            {
                var temp = p2;
                p2 = p1;
                p1 = temp;
            }

            // finding the inverses
            float invP1P2, invP1P3;
            if (p2.Y - p1.Y > 0)
                invP1P2 = (p2.X - p1.X) / (p2.Y - p1.Y);
            else
                invP1P2 = 0;

            if (p3.Y - p1.Y > 0)
                invP1P3 = (p3.X - p1.X) / (p3.Y - p1.Y);
            else
                invP1P3 = 0;
            //If p2 is on the right we use this formula
            if (invP1P2 > invP1P3)
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    //Then we are doing the top to bottom triangle
                    if (y < p2.Y)
                    {
                        ScanLine(y, p1, p3, p1, p2, color);
                    }
                    //We are using the bottom up triangle
                    else
                    {
                        ScanLine(y, p1, p3, p2, p3, color);
                    }
                }
            }
            //P2 is on the left side
            else
            {
                for (var y = (int)p1.Y; y <= (int)p3.Y; y++)
                {
                    if (y < p2.Y)
                    {
                        ScanLine(y, p1, p2, p1, p3, color);
                    }
                    else
                    {
                        ScanLine(y, p2, p3, p1, p3, color);
                    }
                }
            }
        }

        /* I am using Blender as my 3D modeling software and am using a JSON exporter to output
            my scenes into JSON format (created by David Catuhe). This function loads that file
            and imports all the meshes in the scene into our array where we create a mesh object
            for each and return an array of mesh objects.  
        */
        public async Task<Mesh[]> LoadJSONMesh(String meshName)
        {
            // loading the meshes from the file passed in.
            var meshes = new List<Mesh>();
            var file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(meshName);
            var data = await Windows.Storage.FileIO.ReadTextAsync(file);
            dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(data);
            //loop through all the meshes from our file and load their values
            for (var Mindex = 0; Mindex < jsonObj.meshes.Count; Mindex++)
            {
                //Grab the vertices
                var verticesArray = jsonObj.meshes[Mindex].vertices;
                //Grab the faces
                var faceArray = jsonObj.meshes[Mindex].indices;
                //Grab the number of texture cooridnates per vertex
                var uvCount = jsonObj.meshes[Mindex].uvCount.Value;

                var step = 1;
                //Jumping 6,8 and 10 depending on the uvCount
                switch ((int)uvCount)
                {
                    case 0:
                        step = 6;
                        break;
                    case 1:
                        step = 8;
                        break;
                    case 2:
                        step = 10;
                        break;
                }
                var vertices = verticesArray.Count / step;
                //Number of faces is the face array divided by 3, this is because our face array
                //is filled with vertices that creates triangles. so divide by 3 tells us how many
                //triangles we have
                var faces = faceArray.Count / 3;
                var mesh = new Mesh(jsonObj.meshes[Mindex].name.Value, vertices, faces);
                //Now we fill our mesh object from the file
                for (var index = 0; index < faces; index++)
                {
                    //Grabing each vertice that creates the face
                    var a = (int)faceArray[index * 3].Value;
                    var b = (int)faceArray[index * 3 + 1].Value;
                    var c = (int)faceArray[index * 3 + 2].Value;
                    mesh.Faces[index] = new Mesh.Face { A = a, B = b, C = c };
                }
                for (var index = 0; index < vertices; index++)
                {
                    //Grabbing each vertice that we care about
                    var x = (float)verticesArray[index * step].Value;
                    var y = (float)verticesArray[index * step + 1].Value;
                    var z = (float)verticesArray[index * step + 2].Value;
                    mesh.Vertices[index] = new Vector3(x, y, z);
                }
                //Position that was set in blender
                var position = jsonObj.meshes[Mindex].position;
                //Set our mesh to that postion
                mesh.Position = new Vector3((float)position[0].Value, (float)position[1].Value, (float)position[2].Value);
                //Add that mesh to our array of meshes
                meshes.Add(mesh);

            }
            return meshes.ToArray();
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
                                                           (float)renderWidth / renderHeight, 
                                                           0.01f, 1.0f);

            foreach (Mesh mesh in meshes) 
            {
                //Moves our object from model space and puts it in the world view. This is so 
                // the object doesnt lose any data since it technically doesnt get moved. 
                var worldMatrix = Matrix.RotationYawPitchRoll(mesh.Rotation.Y, mesh.Rotation.X, mesh.Rotation.Z) * 
                                  Matrix.Translation(mesh.Position);

                //THE WVP of the matrices. This makes one god matrix that we apply to every vertice
                // That does all the transformations at once.
                var transformMatrix = worldMatrix * viewMatrix * projectionMatrix;

                Parallel.For(0, mesh.Faces.Length, faceIndex =>
                  {
                      var face = mesh.Faces[faceIndex];
                    // Drawing each triangles face. Obviously a triangle is made of three points
                    // Just grab the three and draw a line to each recursivly
                    var vertexA = mesh.Vertices[face.A];
                      var vertexB = mesh.Vertices[face.B];
                      var vertexC = mesh.Vertices[face.C];

                      var pixelA = Project(vertexA, transformMatrix);
                      var pixelB = Project(vertexB, transformMatrix);
                      var pixelC = Project(vertexC, transformMatrix);
                      var color = 155.0f + (faceIndex % mesh.Faces.Length) * .75f / mesh.Faces.Length;
                      DrawTriangle(pixelA, pixelB, pixelC, new Color4(color, color, color, 1));
                      faceIndex++;
                  });
            }
        }
    }
}
