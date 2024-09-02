using OpenTK.Graphics.OpenGL;
using OpenTK.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace yaqmv
{
	public partial class SkinWindow : GLWpfControl
	{
		private SkinRenderer? _skinRenderer;
		public SkinWindow()
		{
			Focusable = false;

			MouseMove += new MouseEventHandler(OnMouseMove);
			MouseDown += new MouseButtonEventHandler(OnMouseDown);
		}
		public void Init(GLWpfControlSettings settings)
		{
			Start(settings);

			_skinRenderer = new SkinRenderer();
		}
		internal void LoadModel(ModelAsset mdl)
		{
			Context.MakeCurrent();
			_skinRenderer.DisplayModel(mdl);
			_skinRenderer.MakeTheQuadModelNow();
			Camera2D.Reset();
		}

		public void OnRender(SkinState _ss)
		{
			_skinRenderer.Render(_ss, (float)ActualWidth, (float)ActualHeight);
		}
		public void OnUnload(object sender, RoutedEventArgs e)
		{
			_skinRenderer.Dispose();
		}

		public void SetMode(bool skin, bool wire)
		{
			_skinRenderer.SetMode(skin, wire);
		}

		private System.Windows.Point _mousePos;
		private bool _buttonDownLeft;
		private bool _buttonDownRight;
		private bool _buttonDownMiddle;

		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ButtonState == MouseButtonState.Pressed)
			{
				if (e.ChangedButton == MouseButton.Left) _buttonDownLeft = true;
				if (e.ChangedButton == MouseButton.Right) _buttonDownRight = true;
				if (e.ChangedButton == MouseButton.Middle) _buttonDownMiddle = true;
			}
		}
		private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			var newPos = e.GetPosition(this);
			var delta = newPos - _mousePos;

			if (_buttonDownLeft)
				Camera2D.Pan((float)delta.X, (float)delta.Y);
			else if (_buttonDownRight)
				Camera2D.Pan((float)delta.X, (float)delta.Y);
			else if (_buttonDownMiddle)
				Camera2D.Pan((float)delta.X, (float)delta.Y);

			if (Mouse.LeftButton != MouseButtonState.Pressed)
				_buttonDownLeft = false;
			if (Mouse.RightButton != MouseButtonState.Pressed)
				_buttonDownRight = false;
			if (Mouse.MiddleButton != MouseButtonState.Pressed)
				_buttonDownMiddle = false;

			_mousePos = newPos;
			e.Handled = true;
		}

		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			if (e.Delta < 0)
				Camera2D.ZoomOut();
			else
				Camera2D.ZoomIn();
			e.Handled = true;
		}
	}
}
