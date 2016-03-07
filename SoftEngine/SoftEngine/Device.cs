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
        public void DrawPoint2(Vector2 point)
        {

            if (point.X >= 0 && point.Y >= 0 && point.X < bmp.PixelWidth && point.Y < bmp.PixelHeight)
            {

                PutPixel((int)point.X, (int)point.Y, new Color4(1.0f, 0.0f, 0.0f, 1.0f));
            }
        }

        /* Using Bresenhams algorithm to draw the lines. 
        */
        public void DrawLine(Vector2 point1, Vector2 point2)
        {
            var x0 = (int)point1.X;
            var y0 = (int)point1.Y;
            var x1 = (int)point2.X;
            var y1 = (int)point2.Y;

            var dx = Math.Abs(x1 - x0);
            var dy = Math.Abs(y1 - y0);
            var sx = (x0 < x1) ? 1 : -1;
            var sy = (y0 < y1) ? 1 : -1;

            var err = dx - dy;

            while (true)
            {
                DrawPoint(new Vector2(x0, y0));

                if ((x0 == x1) && (y0 == y1)) break;
                var err2 = 2 * err;
                if (err2 > -dy) { err -= dy; x0 += sx; }
                if (err2 < dx) { err += dx; y0 += sy; }    
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
                                                           (float)bmp.PixelWidth / bmp.PixelHeight, 
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

                foreach (var vertex in mesh.Vertices)
                {

                    var point = Project(vertex, transformMatrix);
                    //Going to color the sides red just for fun
                    DrawPoint2(point);
                }
                //for (int i =0; i < mesh.Vertices.Length -1 ; i++)
                //{
                //    //var point1 = Project(mesh.Vertices[i], transformMatrix);
                //    //var point2 = Project(mesh.Vertices[i + 1], transformMatrix);
                //    //DrawLine(point1, point2);
                    
                //}
                foreach(var face in mesh.Faces)
                {   
                    // Drawing each triangles face. Obviously a triangle is made of three points
                    // Just grab the three and draw a line to each recursivly
                    var vertexA = mesh.Vertices[face.A];
                    var vertexB = mesh.Vertices[face.B];
                    var vertexC = mesh.Vertices[face.C];

                    var pixelA = Project(vertexA, transformMatrix);
                    var pixelB = Project(vertexB, transformMatrix);
                    var pixelC = Project(vertexC, transformMatrix);

                    DrawLine(pixelA, pixelB);
                    DrawLine(pixelB, pixelC);
                    DrawLine(pixelC, pixelA);
                }
            }
        }
    }
}
