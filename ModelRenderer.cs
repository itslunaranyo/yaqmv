﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Wpf;
using System.Windows;

namespace yaqmv
{
	internal class ModelRenderer : IDisposable
	{
		private Shader shader;
		private Texture CurrentSkin;
		private Model CurrentModel;
		private ModelAsset CurrentAsset;
		private Stopwatch _time;
		private float Width, Height;
		private bool _disposed;

		internal ModelRenderer(ModelAsset mdl)
		{
			_time = new Stopwatch();
			_time.Start();
			_disposed = false;
			shader = new Shader("shaders/default_v.shader", "shaders/default_f.shader");

			CurrentAsset = mdl;
			CurrentModel = ModelConvertor.Convert(mdl, shader);
			CurrentSkin = new Texture(mdl.SkinWidth, mdl.SkinHeight, mdl.skins[0].pixels);
		}
		internal void Resize(int w, int h)
		{
			Width = w;
			Height = h;
		}

		internal void Render()
		{
			float asp = Width / Height;
			float vfov = MathHelper.DegreesToRadians(60f);

			Matrix4 matpersp = Matrix4.CreatePerspectiveFieldOfView(vfov, asp, 1f, 2000.0f);
			Matrix4 matview = Matrix4.LookAt(Camera.Origin, Camera.Focus, new Vector3(0, 0, 1));
			Matrix4 matmodel = Matrix4.CreateRotationZ(Camera.Yaw);

			shader.Use();
			CurrentModel.Bind((int)Math.Floor(_time.Elapsed.TotalSeconds * 10) % CurrentAsset.FrameCount);
			CurrentSkin.Bind();
			shader.SetUniform("model", matmodel);
			shader.SetUniform("view", matview);
			shader.SetUniform("projection", matpersp);

			GL.DrawElements(PrimitiveType.Triangles, CurrentModel.Elements, DrawElementsType.UnsignedInt, 0);
		}
		public void Dispose()
		{
			if (_disposed) return;
			shader.Dispose();
			_disposed = true;
		}
	}
}
