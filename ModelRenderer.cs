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
		private Model? _currentModel;
		private ModelAsset? _currentAsset;
		private bool _disposed;
		private enum RenderMode { Textured, TexturedWire, Shaded, ShadedWire, Wire };
		private RenderMode _rmode;

		internal ModelRenderer()
		{
			_disposed = false;

			_rmode = RenderMode.Textured;
			GL.DepthFunc(DepthFunction.Lequal);
		}
		internal void DisplayModel(ModelAsset mdl)
		{
			_currentAsset = mdl;
			_currentModel?.Dispose();
			_currentModel = ModelConvertor.ConvertAnimatedMesh(mdl);
		}

		public void SetMode(int mode)
		{
			_rmode = (RenderMode)mode;
		}

		internal void Render(ModelState ms, float w, float h)
		{
			GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			if (_currentModel == null)
				return;

			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Front);

			float asp = w / h;
			float vfov = MathHelper.DegreesToRadians(60f);

			Matrix4 matpersp = Matrix4.CreatePerspectiveFieldOfView(vfov, asp, 1f, 2000.0f);
			Matrix4 matview = Matrix4.LookAt(Camera3D.Origin, Camera3D.Focus, new Vector3(0, 0, 1));
			Matrix4 matmodel = Matrix4.CreateRotationZ(Camera3D.Yaw);

			GL.Viewport(0, 0, (int)w, (int)h);
			_currentModel.Bind(ms.Frame);

			if (_rmode == RenderMode.Textured)
			{
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
				_currentAsset?.skins[ms.Skin].images[ms.Skinframe].Tex.Bind();
				Shader.TexturedShaded.Use();
				Shader.TexturedShaded.SetUniform("model", matmodel);
				Shader.TexturedShaded.SetUniform("view", matview);
				Shader.TexturedShaded.SetUniform("projection", matpersp);

				GL.DrawElements(PrimitiveType.Triangles, _currentModel.Elements, DrawElementsType.UnsignedInt, 0);
			}
			else if (_rmode == RenderMode.TexturedWire)
			{
				GL.Enable(EnableCap.PolygonOffsetFill);
				GL.PolygonOffset(1f, 1);
				_currentAsset?.skins[ms.Skin].images[ms.Skinframe].Tex.Bind();
				Shader.TexturedShaded.Use();
				Shader.TexturedShaded.SetUniform("model", matmodel);
				Shader.TexturedShaded.SetUniform("view", matview);
				Shader.TexturedShaded.SetUniform("projection", matpersp);

				GL.DrawElements(PrimitiveType.Triangles, _currentModel.Elements, DrawElementsType.UnsignedInt, 0);
				GL.PolygonOffset(0, 0);
				GL.Disable(EnableCap.PolygonOffsetFill);

				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

				Shader.Flat.Use();
				Shader.Flat.SetUniform("model", matmodel);
				Shader.Flat.SetUniform("view", matview);
				Shader.Flat.SetUniform("projection", matpersp);
				Shader.Flat.SetUniform("color", new Vector3(0.9f, 0.9f, 0.9f));
				GL.DrawElements(PrimitiveType.Triangles, _currentModel.Elements, DrawElementsType.UnsignedInt, 0);

				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			}
			else if (_rmode == RenderMode.Shaded)
			{
				Shader.WhiteShaded.Use();
				Shader.WhiteShaded.SetUniform("model", matmodel);
				Shader.WhiteShaded.SetUniform("view", matview);
				Shader.WhiteShaded.SetUniform("projection", matpersp);

				GL.DrawElements(PrimitiveType.Triangles, _currentModel.Elements, DrawElementsType.UnsignedInt, 0);
			}
			else if (_rmode == RenderMode.Wire)
			{
				Shader.Flat.Use();
				Shader.Flat.SetUniform("model", matmodel);
				Shader.Flat.SetUniform("view", matview);
				Shader.Flat.SetUniform("projection", matpersp);

				GL.CullFace(CullFaceMode.Back);
				Shader.Flat.SetUniform("color", new Vector3(0.6f,0.6f,0.6f));
				GL.DrawElements(PrimitiveType.Triangles, _currentModel.Elements, DrawElementsType.UnsignedInt, 0);
				GL.CullFace(CullFaceMode.Front);
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

				Shader.Flat.SetUniform("color", new Vector3(0.9f, 0.9f, 0.9f));
				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
				GL.DrawElements(PrimitiveType.Triangles, _currentModel.Elements, DrawElementsType.UnsignedInt, 0);
			}
			else if (_rmode == RenderMode.ShadedWire)
			{
				GL.Enable(EnableCap.PolygonOffsetFill);
				GL.PolygonOffset(1f, 1);
				Shader.WhiteShaded.Use();
				Shader.WhiteShaded.SetUniform("model", matmodel);
				Shader.WhiteShaded.SetUniform("view", matview);
				Shader.WhiteShaded.SetUniform("projection", matpersp);

				GL.DrawElements(PrimitiveType.Triangles, _currentModel.Elements, DrawElementsType.UnsignedInt, 0);
				GL.PolygonOffset(0, 0);
				GL.Disable(EnableCap.PolygonOffsetFill);

				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

				Shader.Flat.Use();
				Shader.Flat.SetUniform("model", matmodel);
				Shader.Flat.SetUniform("view", matview);
				Shader.Flat.SetUniform("projection", matpersp);
				Shader.Flat.SetUniform("color", new Vector3(0f,0f,0f));
				GL.DrawElements(PrimitiveType.Triangles, _currentModel.Elements, DrawElementsType.UnsignedInt, 0);

				GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			}
			else
			{
				Shader.Flat.Use();
				Shader.Flat.SetUniform("color", new Vector3(1f, 0f, 1f));
				Shader.Flat.SetUniform("model", matmodel);
				Shader.Flat.SetUniform("view", matview);
				Shader.Flat.SetUniform("projection", matpersp);

				GL.DrawElements(PrimitiveType.Triangles, _currentModel.Elements, DrawElementsType.UnsignedInt, 0);
			}
			GL.BindVertexArray(0);
		}
		public void Dispose()
		{
			if (_disposed) return;
			_currentModel?.Dispose();
			_disposed = true;
		}
	}
}
