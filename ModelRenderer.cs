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
		private Shader ShTexturedShaded;
		private Shader ShFlat;
		private Shader ShWhiteShaded;
		private Texture[] Skins;
		private Model CurrentModel;
		private ModelAsset CurrentAsset;
		private float Width, Height;
		private bool _disposed;
		private enum RenderMode { Wire, Shaded, Textured, ShadedWire, TexturedWire };
		private RenderMode _rmode;

		internal ModelRenderer(ModelAsset mdl)
		{
			_disposed = false;
			ShTexturedShaded = new Shader("shaders/default_v.shader", "shaders/default_f.shader");
			ShFlat = new Shader("shaders/default_v.shader", "shaders/flat_f.shader");
			ShWhiteShaded = new Shader("shaders/default_v.shader", "shaders/shaded_f.shader");

			CurrentAsset = mdl;
			CurrentModel?.Dispose();
			CurrentModel = ModelConvertor.Convert(mdl);
			Skins = new Texture[mdl.SkinCount];
			for(int i = 0; i < mdl.SkinCount; i++)
			{
				Skins[i] = new Texture(mdl.SkinWidth, mdl.SkinHeight, mdl.skins[i].pixels);
			}

			_rmode = RenderMode.Wire;
			GL.DepthFunc(DepthFunction.Lequal);
		}
		internal void Resize(int w, int h)
		{
			Width = w;
			Height = h;
		}

		public void ToggleMode()
		{
			if (_rmode == RenderMode.Textured)
				_rmode = RenderMode.TexturedWire;
			else if (_rmode == RenderMode.TexturedWire)
				_rmode = RenderMode.Shaded;
			else if (_rmode == RenderMode.Shaded)
				_rmode = RenderMode.ShadedWire;
			else if (_rmode == RenderMode.ShadedWire)
				_rmode = RenderMode.Wire;
			else if (_rmode == RenderMode.Wire)
				_rmode = RenderMode.Textured;
		}

		internal void Render(ModelState ms)
		{
			float asp = Width / Height;
			float vfov = MathHelper.DegreesToRadians(60f);

			Matrix4 matpersp = Matrix4.CreatePerspectiveFieldOfView(vfov, asp, 1f, 2000.0f);
			Matrix4 matview = Matrix4.LookAt(Camera.Origin, Camera.Focus, new Vector3(0, 0, 1));
			Matrix4 matmodel = Matrix4.CreateRotationZ(Camera.Yaw);

			CurrentModel.Bind(ms.Frame);

			if (_rmode == RenderMode.Textured)
			{
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
				Skins[ms.Skin].Bind();
				ShTexturedShaded.Use();
				ShTexturedShaded.SetUniform("model", matmodel);
				ShTexturedShaded.SetUniform("view", matview);
				ShTexturedShaded.SetUniform("projection", matpersp);

				GL.DrawElements(PrimitiveType.Triangles, CurrentModel.Elements, DrawElementsType.UnsignedInt, 0);
			}
			else if (_rmode == RenderMode.TexturedWire)
			{
				GL.Enable(EnableCap.PolygonOffsetFill);
				GL.PolygonOffset(1f, 1);
				Skins[ms.Skin].Bind();
				ShTexturedShaded.Use();
				ShTexturedShaded.SetUniform("model", matmodel);
				ShTexturedShaded.SetUniform("view", matview);
				ShTexturedShaded.SetUniform("projection", matpersp);

				GL.DrawElements(PrimitiveType.Triangles, CurrentModel.Elements, DrawElementsType.UnsignedInt, 0);
				GL.PolygonOffset(0, 0);
				GL.Disable(EnableCap.PolygonOffsetFill);

				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

				ShFlat.Use();
				ShFlat.SetUniform("model", matmodel);
				ShFlat.SetUniform("view", matview);
				ShFlat.SetUniform("projection", matpersp);
				ShFlat.SetUniform("color", new Vector3(0.9f, 0.9f, 0.9f));
				GL.DrawElements(PrimitiveType.Triangles, CurrentModel.Elements, DrawElementsType.UnsignedInt, 0);

				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			}
			else if (_rmode == RenderMode.Shaded)
			{
				ShWhiteShaded.Use();
				ShWhiteShaded.SetUniform("model", matmodel);
				ShWhiteShaded.SetUniform("view", matview);
				ShWhiteShaded.SetUniform("projection", matpersp);

				GL.DrawElements(PrimitiveType.Triangles, CurrentModel.Elements, DrawElementsType.UnsignedInt, 0);
			}
			else if (_rmode == RenderMode.Wire)
			{
				ShFlat.Use();
				ShFlat.SetUniform("model", matmodel);
				ShFlat.SetUniform("view", matview);
				ShFlat.SetUniform("projection", matpersp);

				GL.CullFace(CullFaceMode.Back);
				ShFlat.SetUniform("color", new Vector3(0.6f,0.6f,0.6f));
				GL.DrawElements(PrimitiveType.Triangles, CurrentModel.Elements, DrawElementsType.UnsignedInt, 0);
				GL.CullFace(CullFaceMode.Front);
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

				ShFlat.SetUniform("color", new Vector3(0.9f, 0.9f, 0.9f));
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
				GL.DrawElements(PrimitiveType.Triangles, CurrentModel.Elements, DrawElementsType.UnsignedInt, 0);
			}
			else if (_rmode == RenderMode.ShadedWire)
			{
				GL.Enable(EnableCap.PolygonOffsetFill);
				GL.PolygonOffset(1f, 1);
				ShWhiteShaded.Use();
				ShWhiteShaded.SetUniform("model", matmodel);
				ShWhiteShaded.SetUniform("view", matview);
				ShWhiteShaded.SetUniform("projection", matpersp);

				GL.DrawElements(PrimitiveType.Triangles, CurrentModel.Elements, DrawElementsType.UnsignedInt, 0);
				GL.PolygonOffset(0, 0);
				GL.Disable(EnableCap.PolygonOffsetFill);

				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

				ShFlat.Use();
				ShFlat.SetUniform("model", matmodel);
				ShFlat.SetUniform("view", matview);
				ShFlat.SetUniform("projection", matpersp);
				ShFlat.SetUniform("color", new Vector3(0f,0f,0f));
				GL.DrawElements(PrimitiveType.Triangles, CurrentModel.Elements, DrawElementsType.UnsignedInt, 0);

				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			}
			else
			{
				ShFlat.Use();
				ShFlat.SetUniform("color", new Vector3(1f, 0f, 1f));
				ShFlat.SetUniform("model", matmodel);
				ShFlat.SetUniform("view", matview);
				ShFlat.SetUniform("projection", matpersp);

				GL.DrawElements(PrimitiveType.Triangles, CurrentModel.Elements, DrawElementsType.UnsignedInt, 0);
			}
		}
		public void Dispose()
		{
			if (_disposed) return;
			ShTexturedShaded.Dispose();
			ShFlat.Dispose();
			ShWhiteShaded.Dispose();
			CurrentModel?.Dispose();
			_disposed = true;
		}
	}
}
