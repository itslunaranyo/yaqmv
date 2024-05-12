using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Wpf;
using OpenTK.Windowing.Common;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Drawing;

namespace yaqmv
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private ModelRenderer mr;
		public int RendererWidth { get { return (int)OpenTkControl.ActualWidth; } }
		public int RendererHeight { get { return (int)OpenTkControl.ActualHeight; } }
		public MainWindow()
		{
			InitializeComponent();

			var settings = new GLWpfControlSettings
			{
				MajorVersion = 3,
				MinorVersion = 3,
				GraphicsProfile = ContextProfile.Core
			};
			OpenTkControl.Start(settings);

			mr = new ModelRenderer("c:/games/quake/id1/progs/hknight.mdl");
			//mr = new ModelRenderer("c:/games/quake/copper_dev/progs/m_rock1.mdl");
			mr.Resize(RendererWidth, RendererHeight);
		}

		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
			mr.Resize(RendererWidth, RendererHeight);
			GL.Viewport(0, 0, RendererWidth, RendererHeight);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		}
		private void OnReady()
        {
            GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
            GL.Viewport(0, 0, RendererWidth, RendererHeight);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Front);
        }

        private void OnRender(TimeSpan delta)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			if (mr == null) return;

			mr.Render();

		}
		private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }

        private void OnUnload(object sender, RoutedEventArgs e)
        {
            mr.Dispose();
        }
	}
}