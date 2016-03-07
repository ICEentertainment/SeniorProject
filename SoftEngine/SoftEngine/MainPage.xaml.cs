using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
/*  Created By Joshua Brooks 
    Since this project is for learning there are massive ammounts of comments for each function.
    My process was to write the code then remind myself every aspect of what I wrote to double
    check my self. I understand to many comments is a bad thing and that is why I am adding this 
    disclaimer so one understands why there are so many comments.    
*/

namespace SoftEngine
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Device device;
        Mesh[] meshes;
        Camera mera = new Camera();
        DateTime previousDate;
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Choose the back buffer resolution here
            WriteableBitmap bmp = new WriteableBitmap(640, 480);

            // Our Image XAML control
            frontBuffer.Source = bmp;

            device = new Device(bmp);
            meshes = await device.LoadJSONMesh("Test2.babylon");
            mera.Position = new Vector3(0.0f, 0.0f, 13.0f);
            mera.Target = Vector3.Zero;

            // Registering to the XAML rendering loop
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        DateTime prev;
        TextBlock fps = new TextBlock();
        void CompositionTarget_Rendering(object sender, object e)
        {

            device.Clear(0, 0, 0, 255);

            foreach (var mesh in meshes)
            {
                mesh.Rotation = new Vector3(mesh.Rotation.X, mesh.Rotation.Y + .01f, mesh.Rotation.Z);
                device.Render(mera, meshes);
            }
            
           
            device.Present();
        }

        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }
    }
}
