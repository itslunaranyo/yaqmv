using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using System.Xml.Linq;
using System.Diagnostics.Eventing.Reader;

namespace yaqmv
{
	internal class Model : IDisposable
	{
		private int _vertexBufferObject;
		private int _vertexArrayObject;
		private int _elementBufferObject;
		private int _count;
		public int Elements;

		private readonly int _attribPos = 0;
		private readonly int _attribUV = 1;
		private readonly int _attribNorm = 2;

		private bool _hasUVs = false;
		private bool _hasNormals = false;

		private bool _disposed;

		public Model(int framecount, int[] indices, List<Vector2> uvs, List<Vector3> positions, List<Vector3> normals)
		{
			_count = uvs.Count;
			int len = uvs.Count * 2 + positions.Count * 3 + normals.Count * 3;
			float[] vbof = new float[len];
			int i = 0;

			// vao: uvs, then position and normal interleaved w/ multiple poses end to end
			foreach (var uv in uvs)
			{
				vbof[i++] = uv.X;
				vbof[i++] = uv.Y;
			}
			for (int j = 0; j < positions.Count; j++)
			{
				vbof[i++] = positions[j].X;
				vbof[i++] = positions[j].Y;
				vbof[i++] = positions[j].Z;
				vbof[i++] = normals[j].X;
				vbof[i++] = normals[j].Y;
				vbof[i++] = normals[j].Z;
			}

			_hasUVs = true;
			_hasNormals = true;

			BuildModelBuffers(indices, vbof);
		}

		public Model(int[] indices, List<Vector2> uvs, List<Vector3> positions)
		{
			_count = uvs.Count;
			int len = uvs.Count * 2 + positions.Count * 3;
			float[] vbof = new float[len];
			int i = 0;

			// vao: uvs, then position and normal interleaved w/ multiple poses end to end
			foreach (var uv in uvs)
			{
				vbof[i++] = uv.X;
				vbof[i++] = uv.Y;
			}
			for (int j = 0; j < positions.Count; j++)
			{
				vbof[i++] = positions[j].X;
				vbof[i++] = positions[j].Y;
				vbof[i++] = positions[j].Z;
			}

			_hasUVs = true;

			BuildModelBuffers(indices, vbof);
		}

		public Model(int[] indices, List<Vector3> positions)
		{
			int len = positions.Count * 3;
			float[] vbof = new float[len];
			int i = 0;

			// vao: uvs, then position and normal interleaved w/ multiple poses end to end
			for (int j = 0; j < positions.Count; j++)
			{
				vbof[i++] = positions[j].X;
				vbof[i++] = positions[j].Y;
				vbof[i++] = positions[j].Z;
			}

			BuildModelBuffers(indices, vbof);
		}

		public void BuildModelBuffers(int[] indices, float[] vbof)
		{
			Elements = indices.Length;
			_vertexBufferObject = GL.GenBuffer();
			_elementBufferObject = GL.GenBuffer();
			_vertexArrayObject = GL.GenVertexArray();

			GL.BindVertexArray(_vertexArrayObject);
			GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
			GL.BufferData(BufferTarget.ArrayBuffer, vbof.Length * sizeof(float), vbof, BufferUsageHint.StaticDraw);
			GL.BufferData(BufferTarget.ElementArrayBuffer, Elements * sizeof(uint), indices, BufferUsageHint.StaticDraw);
		}

		public void Bind(int pose = 0)
		{
			int offset = 0;
			int stride = 3 * sizeof(float);

			GL.BindVertexArray(_vertexArrayObject);
			if (_hasUVs)
			{
				GL.VertexAttribPointer(_attribUV, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
				GL.EnableVertexAttribArray(_attribUV);
				offset += 2 * sizeof(float) * _count;
			}
			if (_hasNormals)
			{
				stride = 6 * sizeof(float);
				GL.VertexAttribPointer(_attribNorm, 3, VertexAttribPointerType.Float, false, stride, offset + (_count * pose * 6 + 3) * sizeof(float));
				GL.EnableVertexAttribArray(_attribNorm);
			}
			GL.VertexAttribPointer(_attribPos, 3, VertexAttribPointerType.Float, false, stride, offset + (_count * pose * 6) * sizeof(float));
			GL.EnableVertexAttribArray(_attribPos);
		}
		~Model()
		{
			if (!_disposed)
			{
				Debug.WriteLine("GPU resource leak - undisposed buffers");
			}
		}
		public void Dispose()
		{
			if (_disposed) return;
			GL.DeleteBuffers(1, ref _vertexBufferObject);
			GL.DeleteBuffers(1, ref _elementBufferObject);
			GL.DeleteVertexArrays(1, ref _vertexArrayObject);
			_disposed = true;
		}
	}
}
