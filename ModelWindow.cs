using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Wpf;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Security.Cryptography;
using System.Windows.Input;

namespace yaqmv
{
	public partial class ModelWindow : GLWpfControl
	{
		private ModelRenderer mr;
		public ModelWindow()
		{
			var settings = new GLWpfControlSettings
			{
				MajorVersion = 3,
				MinorVersion = 3,
				Profile = ContextProfile.Core
			};
			Focusable = false;
			Start(settings);
			mr = new ModelRenderer();

			GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
			GL.Viewport(0, 0, (int)ActualWidth, (int)ActualHeight);
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Front);

			MouseMove += new MouseEventHandler(OnMouseMove);
		}

		public void SetMode(int mode)
		{
			mr.SetMode(mode);
		}

		internal void LoadModel(ModelAsset mdl)
		{
			mr.DisplayModel(mdl);
			mr.Resize((int)ActualWidth, (int)ActualHeight);
			Camera.Reset(mdl.CenterOfFrame(0), mdl.RadiusOfFrame(0));
		}

		public void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			mr.Resize((int)ActualWidth, (int)ActualHeight);
			GL.Viewport(0, 0, (int)ActualWidth, (int)ActualHeight);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		}

		public void OnRender(TimeSpan delta)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			mr.Render(MainWindow.Get.GetModelState(delta));
		}
		public void OnUnload(object sender, RoutedEventArgs e)
		{
			mr.Dispose();
		}


		static System.Windows.Point _mousepos;
		private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			var newpos = e.GetPosition(this);
			var delta = newpos - _mousepos;

			if (Mouse.LeftButton == MouseButtonState.Pressed)
				Camera.Orbit((float)delta.X, (float)delta.Y);
			else if (Mouse.RightButton == MouseButtonState.Pressed)
				Camera.Dolly(-(float)delta.Y);
			else if (Mouse.MiddleButton == MouseButtonState.Pressed)
				Camera.Pan((float)delta.X, (float)delta.Y);

			_mousepos = newpos;
			e.Handled = true;
		}
	}
}
