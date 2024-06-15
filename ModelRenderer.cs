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
		private Shader _shTexturedShaded;
		private Shader _shFlat;
		private Shader _shWhiteShaded;
		private Texture[] _skins;
		private Model? _currentModel;
		private ModelAsset? _currentAsset;
		private float _width, _height;
		private bool _disposed;
		private enum RenderMode { Textured, TexturedWire, Shaded, ShadedWire, Wire };
		private RenderMode _rmode;

		internal ModelRenderer()
		{
			_disposed = false;
			_shTexturedShaded = new Shader("shaders/default_v.shader", "shaders/default_f.shader");
			_shFlat = new Shader("shaders/default_v.shader", "shaders/flat_f.shader");
			_shWhiteShaded = new Shader("shaders/default_v.shader", "shaders/shaded_f.shader");

			_skins = new Texture[0];

			_rmode = RenderMode.Textured;
			GL.DepthFunc(DepthFunction.Lequal);
		}
		internal void DisplayModel(ModelAsset mdl)
		{
			_currentAsset = mdl;
			_currentModel?.Dispose();
			_currentModel = ModelConvertor.Convert(mdl);
			_skins = new Texture[mdl.SkinCount];
			for (int i = 0; i < mdl.SkinCount; i++)
			{
				_skins[i] = new Texture(mdl.SkinWidth, mdl.SkinHeight, mdl.skins[i].pixels);
			}
		}
		internal void Resize(int w, int h)
		{
			_width = w;
			_height = h;
		}

		public void SetMode(int mode)
		{
			_rmode = (RenderMode)mode;
		}

		internal void Render(ModelState ms)
		{
			if (_currentModel == null)
				return;

			float asp = _width / _height;
			float vfov = MathHelper.DegreesToRadians(60f);

			Matrix4 matpersp = Matrix4.CreatePerspectiveFieldOfView(vfov, asp, 1f, 2000.0f);
			Matrix4 matview = Matrix4.LookAt(Camera.Origin, Camera.Focus, new Vector3(0, 0, 1));
			Matrix4 matmodel = Matrix4.CreateRotationZ(Camera.Yaw);

			_currentModel.Bind(ms.Frame);

			if (_rmode == RenderMode.Textured)
			{
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
				_skins[ms.Skin].Bind();
				_shTexturedShaded.Use();
				_shTexturedShaded.SetUniform("model", matmodel);
				_shTexturedShaded.SetUniform("view", matview);
				_shTexturedShaded.SetUniform("projection", matpersp);

				GL.DrawElements(PrimitiveType.Triangles, _currentModel.Elements, DrawElementsType.UnsignedInt, 0);
			}
			else if (_rmode == RenderMode.TexturedWire)
			{
				GL.Enable(EnableCap.PolygonOffsetFill);
				GL.PolygonOffset(1f, 1);
				_skins[ms.Skin].Bind();
				_shTexturedShaded.Use();
				_shTexturedShaded.SetUniform("model", matmodel);
				_shTexturedShaded.SetUniform("view", matview);
				_shTexturedShaded.SetUniform("projection", matpersp);

				GL.DrawElements(PrimitiveType.Triangles, _currentModel.Elements, DrawElementsType.UnsignedInt, 0);
				GL.PolygonOffset(0, 0);
				GL.Disable(EnableCap.PolygonOffsetFill);

				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

				_shFlat.Use();
				_shFlat.SetUniform("model", matmodel);
				_shFlat.SetUniform("view", matview);
				_shFlat.SetUniform("projection", matpersp);
				_shFlat.SetUniform("color", new Vector3(0.9f, 0.9f, 0.9f));
				GL.DrawElements(PrimitiveType.Triangles, _currentModel.Elements, DrawElementsType.UnsignedInt, 0);

				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			}
			else if (_rmode == RenderMode.Shaded)
			{
				_shWhiteShaded.Use();
				_shWhiteShaded.SetUniform("model", matmodel);
				_shWhiteShaded.SetUniform("view", matview);
				_shWhiteShaded.SetUniform("projection", matpersp);

				GL.DrawElements(PrimitiveType.Triangles, _currentModel.Elements, DrawElementsType.UnsignedInt, 0);
			}
			else if (_rmode == RenderMode.Wire)
			{
				_shFlat.Use();
				_shFlat.SetUniform("model", matmodel);
				_shFlat.SetUniform("view", matview);
				_shFlat.SetUniform("projection", matpersp);

				GL.CullFace(CullFaceMode.Back);
				_shFlat.SetUniform("color", new Vector3(0.6f,0.6f,0.6f));
				GL.DrawElements(PrimitiveType.Triangles, _currentModel.Elements, DrawElementsType.UnsignedInt, 0);
				GL.CullFace(CullFaceMode.Front);
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

				_shFlat.SetUniform("color", new Vector3(0.9f, 0.9f, 0.9f));
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
				GL.DrawElements(PrimitiveType.Triangles, _currentModel.Elements, DrawElementsType.UnsignedInt, 0);
			}
			else if (_rmode == RenderMode.ShadedWire)
			{
				GL.Enable(EnableCap.PolygonOffsetFill);
				GL.PolygonOffset(1f, 1);
				_shWhiteShaded.Use();
				_shWhiteShaded.SetUniform("model", matmodel);
				_shWhiteShaded.SetUniform("view", matview);
				_shWhiteShaded.SetUniform("projection", matpersp);

				GL.DrawElements(PrimitiveType.Triangles, _currentModel.Elements, DrawElementsType.UnsignedInt, 0);
				GL.PolygonOffset(0, 0);
				GL.Disable(EnableCap.PolygonOffsetFill);

				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

				_shFlat.Use();
				_shFlat.SetUniform("model", matmodel);
				_shFlat.SetUniform("view", matview);
				_shFlat.SetUniform("projection", matpersp);
				_shFlat.SetUniform("color", new Vector3(0f,0f,0f));
				GL.DrawElements(PrimitiveType.Triangles, _currentModel.Elements, DrawElementsType.UnsignedInt, 0);

				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			}
			else
			{
				_shFlat.Use();
				_shFlat.SetUniform("color", new Vector3(1f, 0f, 1f));
				_shFlat.SetUniform("model", matmodel);
				_shFlat.SetUniform("view", matview);
				_shFlat.SetUniform("projection", matpersp);

				GL.DrawElements(PrimitiveType.Triangles, _currentModel.Elements, DrawElementsType.UnsignedInt, 0);
			}
		}
		public void Dispose()
		{
			if (_disposed) return;
			_shTexturedShaded.Dispose();
			_shFlat.Dispose();
			_shWhiteShaded.Dispose();
			_currentModel?.Dispose();
			_disposed = true;
		}
	}
}
