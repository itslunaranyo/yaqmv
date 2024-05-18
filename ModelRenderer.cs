using System;
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
		private Texture[] Skins;
		private Model CurrentModel;
		private ModelAsset CurrentAsset;
		private float Width, Height;
		private bool _disposed;

		internal ModelRenderer(ModelAsset mdl)
		{
			_disposed = false;
			shader = new Shader("shaders/default_v.shader", "shaders/default_f.shader");

			CurrentAsset = mdl;
			CurrentModel?.Dispose();
			CurrentModel = ModelConvertor.Convert(mdl, shader);
			Skins = new Texture[mdl.SkinCount];
			for(int i = 0; i < mdl.SkinCount; i++)
			{
				Skins[i] = new Texture(mdl.SkinWidth, mdl.SkinHeight, mdl.skins[i].pixels);
			}
		}
		internal void Resize(int w, int h)
		{
			Width = w;
			Height = h;
		}

		internal void Render(ModelState ms)
		{
			float asp = Width / Height;
			float vfov = MathHelper.DegreesToRadians(60f);

			Matrix4 matpersp = Matrix4.CreatePerspectiveFieldOfView(vfov, asp, 1f, 2000.0f);
			Matrix4 matview = Matrix4.LookAt(Camera.Origin, Camera.Focus, new Vector3(0, 0, 1));
			Matrix4 matmodel = Matrix4.CreateRotationZ(Camera.Yaw);

			shader.Use();
			CurrentModel.Bind(ms.Frame);
			Skins[ms.Skin].Bind();
			shader.SetUniform("model", matmodel);
			shader.SetUniform("view", matview);
			shader.SetUniform("projection", matpersp);

			GL.DrawElements(PrimitiveType.Triangles, CurrentModel.Elements, DrawElementsType.UnsignedInt, 0);
		}
		public void Dispose()
		{
			if (_disposed) return;
			shader.Dispose();
			CurrentModel?.Dispose();
			_disposed = true;
		}
	}
}
