using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Wpf;
//using OpenTK.Windowing.Common;
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
				Profile = OpenTK.Windowing.Common.ContextProfile.Core
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
			MouseDown += new MouseButtonEventHandler(OnMouseDown);
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

		private System.Windows.Point _mousepos;
		private bool ButtonDownLeft;
		private bool ButtonDownRight;
		private bool ButtonDownMiddle;

		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ButtonState == MouseButtonState.Pressed)
			{
				if (e.ChangedButton == MouseButton.Left)   ButtonDownLeft = true;
				if (e.ChangedButton == MouseButton.Right)  ButtonDownRight = true;
				if (e.ChangedButton == MouseButton.Middle) ButtonDownMiddle = true;
			}
		}
		private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			var newpos = e.GetPosition(this);
			var delta = newpos - _mousepos;

			if (ButtonDownLeft)
				Camera.Orbit((float)delta.X, (float)delta.Y);
			else if (ButtonDownRight)
				Camera.Dolly(-(float)delta.Y);
			else if (ButtonDownMiddle)
				Camera.Pan((float)delta.X, (float)delta.Y);

			if (Mouse.LeftButton != MouseButtonState.Pressed)
				ButtonDownLeft = false;
			else if (Mouse.RightButton != MouseButtonState.Pressed)
				ButtonDownRight = false;
			else if (Mouse.MiddleButton != MouseButtonState.Pressed)
				ButtonDownMiddle = false;

			_mousepos = newpos;
			e.Handled = true;
		}
	}
}
