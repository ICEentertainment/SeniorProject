using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;
using SharpDX;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Engine
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Device device;
        Mesh mesh = new Mesh("Cube", 8);
        Camera mera = new Camera();
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Choose the back buffer resolution here
            WriteableBitmap bmp = new WriteableBitmap(640, 480);

            device = new Device(bmp);

            // Our Image XAML control
            frontBuffer.Source = bmp;

            mesh.Vertices[0] = new Vector3(-1, 1, 1);
            mesh.Vertices[1] = new Vector3(1, 1, 1);
            mesh.Vertices[2] = new Vector3(-1, -1, 1);
            mesh.Vertices[3] = new Vector3(-1, -1, -1);
            mesh.Vertices[4] = new Vector3(-1, 1, -1);
            mesh.Vertices[5] = new Vector3(1, 1, -1);
            mesh.Vertices[6] = new Vector3(1, -1, 1);
            mesh.Vertices[7] = new Vector3(1, -1, -1);

            camera.Position = new Vector3(0, 0, 10.0f);
            camera.Target = Vector3.Zero;

            // Registering to the XAML rendering loop
            CompositionTarget.Rendering += Comp_Rendering;
        }
        void Comp_Rendering(object sender, object e)
        {
            device.Clear(0, 0, 0, 255);

            //Rotate the cube every call
            mesh.Rotation = new Vector3(mesh.Rotation.Y + 0.01f, mesh.Rotation.X + 0.01f, mesh.Rotation.Z);

            device.Render(camera, mesh);

            device.Present();
        }
        public MainPage()
        {
            this.InitializeComponent();
        }
    }
}
