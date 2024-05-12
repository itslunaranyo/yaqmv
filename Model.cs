using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using System.Xml.Linq;

namespace yaqmv
{
	internal class Model
	{
		private int VertexBufferObject;
		private int VertexArrayObject;
		private int ElementBufferObject;
		private int Count;
		public int Elements;

		private readonly int attrib_pos = 0;
		private readonly int attrib_norm = 1;
		private readonly int attrib_uv = 2;

		public Model(int framecount, int[] indices, List<Vector2> uvs, List<Vector3> positions, List<Vector3> normals, Shader shader)
		{
			Count = uvs.Count;
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

			Elements = indices.Length;
			VertexBufferObject = GL.GenBuffer();
			ElementBufferObject = GL.GenBuffer();
			VertexArrayObject = GL.GenVertexArray();

			GL.BindVertexArray(VertexArrayObject);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
			GL.BufferData(BufferTarget.ArrayBuffer, vbof.Length * sizeof(float), vbof, BufferUsageHint.StaticDraw);
			GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

		}
		public void Bind(int pose)
		{
			GL.BindVertexArray(VertexArrayObject);
			GL.VertexAttribPointer(attrib_uv, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
			GL.EnableVertexAttribArray(attrib_uv);
			GL.VertexAttribPointer(attrib_pos, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), (Count * 2 + Count * pose * 6) * sizeof(float));
			GL.EnableVertexAttribArray(attrib_pos);
			GL.VertexAttribPointer(attrib_norm, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), (Count * 2 + Count * pose * 6 + 3) * sizeof(float));
			GL.EnableVertexAttribArray(attrib_norm);
		}
	}
}
