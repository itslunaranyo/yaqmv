using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System.Windows.Media.Media3D;
using System.Windows.Resources;

namespace yaqmv
{
	public class Shader : IDisposable
	{
        int Handle;
        private bool _disposed = false;

        public Shader(string vertexPath, string fragmentPath)
        {
			string VertexShaderSource = Resource.ReadText(vertexPath);
			var VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);
            GL.CompileShader(VertexShader);
            GL.GetShader(VertexShader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(VertexShader);
                Debug.WriteLine(infoLog);
            }

            string FragmentShaderSource = Resource.ReadText(fragmentPath);
            var FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);
            GL.CompileShader(FragmentShader);
            GL.GetShader(FragmentShader, ShaderParameter.CompileStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(FragmentShader);
                Debug.WriteLine(infoLog);
            }

            Handle = GL.CreateProgram();
            GL.AttachShader(Handle, VertexShader);
            GL.AttachShader(Handle, FragmentShader);

            GL.LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(Handle);
                Debug.WriteLine(infoLog);
            }

            GL.DetachShader(Handle, VertexShader);
            GL.DetachShader(Handle, FragmentShader);
            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(VertexShader);
        }
		public int GetAttribLocation(string attribName)
		{
			return GL.GetAttribLocation(Handle, attribName);
		}
		public void Use()
        {
            GL.UseProgram(Handle);
        }
        public void SetUniform(string name, Matrix4 mat)
        {
            int loc = GL.GetUniformLocation(Handle, name);
			GL.UniformMatrix4(loc, false, ref mat);
		}
		public void SetUniform(string name, Matrix3 mat)
		{
			int loc = GL.GetUniformLocation(Handle, name);
			GL.UniformMatrix3(loc, false, ref mat);
		}
		public void SetUniform(string name, Vector3 vec)
		{
			int loc = GL.GetUniformLocation(Handle, name);
			GL.Uniform3(loc, ref vec);
		}
		public void SetUniform(string name, Vector2 vec)
		{
			int loc = GL.GetUniformLocation(Handle, name);
			GL.Uniform2(loc, ref vec);
		}
		public void SetUniform(string name, float f)
		{
			int loc = GL.GetUniformLocation(Handle, name);
			GL.Uniform1(loc, f);
		}

		protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                GL.DeleteProgram(Handle);

                _disposed = true;
            }
        }
        ~Shader()
        {
            if (!_disposed)
            {
                Debug.WriteLine("GPU resource leak - undisposed shader");
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
