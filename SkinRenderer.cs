using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace yaqmv
{
	internal class SkinRenderer : IDisposable
	{
		private Model? _quadModel;
		private Model? _currentModel;
		private ModelAsset? _currentAsset;
		private bool _disposed;
		
		internal SkinRenderer()
		{
			_disposed = false;
			MakeTheQuadModelNow();
		}
		internal void DisplayModel(ModelAsset mdl)
		{
			_currentAsset = mdl;
			_currentModel?.Dispose();
			_currentModel = ModelConvertor.ConvertSkinMesh(mdl);
			Utility.GLWhatsWrong("skinrenderer.displaymodel");
		}
		internal void MakeTheQuadModelNow()
		{
			if (_quadModel != null) return;

			int[] quadindices = {
				0, 1, 2,
				0, 2, 3
			};
			List<Vector3> quadverts = new(4) {
				new Vector3(-0.5f, 0.5f, 0),
				new Vector3(0.5f, 0.5f, 0),
				new Vector3(0.5f, -0.5f, 0),
				new Vector3(-0.5f, -0.5f, 0),
			};

			List<Vector2> quaduvs = new(4) {
				new Vector2(1f, 0f),
				new Vector2(0f, 0f),
				new Vector2(0f, 1f),
				new Vector2(1f, 1f),
			};
			_quadModel = new Model(quadindices, quaduvs, quadverts);
	}

	internal void Render(ModelState ms, float w, float h)
		{
			GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			if (_currentAsset == null)
				return;

			GL.Disable(EnableCap.DepthTest);
			GL.Disable(EnableCap.CullFace);

			float asp = w / h;
			float vfov = MathHelper.DegreesToRadians(60f);

			Matrix4 matpersp = Matrix4.CreateOrthographic(w / Camera2D.Zoom, h / Camera2D.Zoom, 0.1f, 100);
			Matrix4 matview = Matrix4.LookAt(Camera2D.Origin, Camera2D.Focus, new Vector3(0, 1, 0));
			Matrix4 matmodel = Matrix4.CreateScale(_currentAsset.SkinWidth, _currentAsset.SkinHeight, 1);

			GL.Viewport(0, 0, (int)w, (int)h);

			_quadModel.Bind();
			GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
			_currentAsset?.skins[ms.Skin].images[ms.Skinframe].Tex.Bind();
			Shader.Textured.Use();
			Shader.Textured.SetUniform("model", matmodel);
			Shader.Textured.SetUniform("view", matview);
			Shader.Textured.SetUniform("projection", matpersp);
			
			GL.DrawElements(PrimitiveType.Triangles, _quadModel.Elements, DrawElementsType.UnsignedInt, 0);
			GL.BindVertexArray(0);

			matmodel = Matrix4.CreateTranslation(-0.5f, -0.5f, 0) * Matrix4.CreateScale(-_currentAsset.SkinWidth, -_currentAsset.SkinHeight, 1);
			_currentModel.Bind();
			Shader.Flat.Use();
			Shader.Flat.SetUniform("model", matmodel);
			Shader.Flat.SetUniform("view", matview);
			Shader.Flat.SetUniform("projection", matpersp);
			Shader.Flat.SetUniform("color", new Vector3(0.9f, 0.9f, 0.9f));

			GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
			GL.DrawElements(PrimitiveType.Triangles, _currentModel.Elements, DrawElementsType.UnsignedInt, 0);
			GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
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
