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

namespace yaqmv
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
        private Shader shader;
        private Texture CurrentSkin;
        private ModelAsset? CurrentModelAsset;
        private Model? CurrentModel;
        private Stopwatch _time;

        public MainWindow()
		{
            _time = new Stopwatch();
			_time.Start();

			InitializeComponent();

            var settings = new GLWpfControlSettings
            {
                MajorVersion = 3,
                MinorVersion = 3,
				GraphicsProfile = ContextProfile.Core
			};
            OpenTkControl.Start(settings);

			shader = new Shader("shaders/default_v.shader", "shaders/default_f.shader");
            
			CurrentModelAsset = new ModelAsset("c:/games/quake/id1/progs/hknight.mdl");
			//CurrentModelAsset = new ModelAsset("c:/games/quake/copper_dev/progs/m_rock1.mdl");
			CurrentModel = ModelConvertor.Convert(CurrentModelAsset, shader);
			CurrentSkin = new Texture(CurrentModelAsset.SkinWidth, CurrentModelAsset.SkinHeight, CurrentModelAsset.skins[0].pixels);
		}

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
			GL.Viewport(0, 0, (int)OpenTkControl.ActualWidth, (int)OpenTkControl.ActualHeight);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		}
		private void OnReady()
        {
            GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
            GL.Viewport(0, 0, (int)OpenTkControl.ActualWidth, (int)OpenTkControl.ActualHeight);
            GL.Enable(EnableCap.DepthTest);
        }

        private void OnRender(TimeSpan delta)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (CurrentModel == null)
                return;
            //GL.CullFace(CullFaceMode.FrontAndBack);
            float asp = (float)(OpenTkControl.ActualWidth / OpenTkControl.ActualHeight);
            float vfov = MathHelper.DegreesToRadians(60f);

            Matrix4 matpersp = Matrix4.CreatePerspectiveFieldOfView(vfov, asp, 1f, 2000.0f);
            Matrix4 matview = Matrix4.LookAt(new Vector3(64,64,24), new Vector3(0,0,16), new Vector3(0,0,1));
            Matrix4 matmodel = Matrix4.Identity;
            //Matrix4 matmodel = Matrix4.CreateRotationZ((float)_time.Elapsed.TotalSeconds);

            shader.Use();
            CurrentModel.Bind((int)Math.Floor(_time.Elapsed.TotalSeconds*10) % CurrentModelAsset.FrameCount);
			CurrentSkin.Bind();
            shader.SetUniform("model", matmodel);
            shader.SetUniform("view", matview);
            shader.SetUniform("projection", matpersp);

            GL.DrawElements(PrimitiveType.Triangles, CurrentModel.Elements, DrawElementsType.UnsignedInt, 0);
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
            shader.Dispose();
        }
	}
}