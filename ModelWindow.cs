﻿using OpenTK.Mathematics;
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
		private ModelRenderer? _modelRenderer;
		public ModelWindow()
		{
			Focusable = false;

			MouseMove += new MouseEventHandler(OnMouseMove);
			MouseDown += new MouseButtonEventHandler(OnMouseDown);
		}

		public void Init(GLWpfControlSettings settings)
		{
			Start(settings);
			_modelRenderer = new ModelRenderer();
		}

		public void SetMode(int mode)
		{
			_modelRenderer?.SetMode(mode);
		}

		internal void LoadModel(ModelAsset mdl)
		{
			Context.MakeCurrent();
			_modelRenderer?.DisplayModel(mdl);
			Camera3D.Reset(mdl.CenterOfFrame(0), mdl.RadiusOfFrame(0));
		}

		public void OnRender(ModelState _ms)
		{
			_modelRenderer?.Render(_ms, (float)ActualWidth, (float)ActualHeight);
		}
		public void OnUnload(object sender, RoutedEventArgs e)
		{
			_modelRenderer?.Dispose();
		}

		private System.Windows.Point _mousePos;
		private bool _buttonDownLeft;
		private bool _buttonDownRight;
		private bool _buttonDownMiddle;

		private void OnMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (e.ButtonState == MouseButtonState.Pressed)
			{
				if (e.ChangedButton == MouseButton.Left)   _buttonDownLeft = true;
				if (e.ChangedButton == MouseButton.Right)  _buttonDownRight = true;
				if (e.ChangedButton == MouseButton.Middle) _buttonDownMiddle = true;
			}
		}
		private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			var newPos = e.GetPosition(this);
			var delta = newPos - _mousePos;

			if (_buttonDownLeft)
				Camera3D.Orbit((float)delta.X, (float)delta.Y);
			else if (_buttonDownRight)
				Camera3D.Dolly(-(float)delta.Y);
			else if (_buttonDownMiddle)
				Camera3D.Pan((float)delta.X, (float)delta.Y);

			if (Mouse.LeftButton != MouseButtonState.Pressed)
				_buttonDownLeft = false;
			if (Mouse.RightButton != MouseButtonState.Pressed)
				_buttonDownRight = false;
			if (Mouse.MiddleButton != MouseButtonState.Pressed)
				_buttonDownMiddle = false;

			_mousePos = newPos;
			e.Handled = true;
		}
	}
}
