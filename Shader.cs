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
        private int _handle;
        private bool _disposed = false;

		public static Shader? TexturedShaded;
		public static Shader? Textured;
		public static Shader? Flat;
		public static Shader? WhiteShaded;
		
		public static void Init()
		{
			TexturedShaded = new Shader("shaders/default_v.shader", "shaders/default_f.shader");
			Textured = new Shader("shaders/default_v.shader", "shaders/unlit_f.shader");
			Flat = new Shader("shaders/default_v.shader", "shaders/flat_f.shader");
			WhiteShaded = new Shader("shaders/default_v.shader", "shaders/shaded_f.shader");
		}

		public Shader(string vertexPath, string fragmentPath)
        {
			string vtxShaderSource = Utility.ReadResourceText(vertexPath);
			var vtxShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vtxShader, vtxShaderSource);
            GL.CompileShader(vtxShader);
            GL.GetShader(vtxShader, ShaderParameter.CompileStatus, out int success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(vtxShader);
                Debug.WriteLine(infoLog);
            }

            string frgShaderSource = Utility.ReadResourceText(fragmentPath);
            var frgShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(frgShader, frgShaderSource);
            GL.CompileShader(frgShader);
            GL.GetShader(frgShader, ShaderParameter.CompileStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetShaderInfoLog(frgShader);
                Debug.WriteLine(infoLog);
            }

            _handle = GL.CreateProgram();
            GL.AttachShader(_handle, vtxShader);
            GL.AttachShader(_handle, frgShader);

            GL.LinkProgram(_handle);

            GL.GetProgram(_handle, GetProgramParameterName.LinkStatus, out success);
            if (success == 0)
            {
                string infoLog = GL.GetProgramInfoLog(_handle);
                Debug.WriteLine(infoLog);
            }

            GL.DetachShader(_handle, vtxShader);
            GL.DetachShader(_handle, frgShader);
            GL.DeleteShader(frgShader);
            GL.DeleteShader(vtxShader);
        }
		public int GetAttribLocation(string attribName)
		{
			return GL.GetAttribLocation(_handle, attribName);
		}
		public void Use()
        {
            GL.UseProgram(_handle);
        }
        public void SetUniform(string name, Matrix4 mat)
        {
            int loc = GL.GetUniformLocation(_handle, name);
			GL.UniformMatrix4(loc, false, ref mat);
		}
		public void SetUniform(string name, Matrix3 mat)
		{
			int loc = GL.GetUniformLocation(_handle, name);
			GL.UniformMatrix3(loc, false, ref mat);
		}
		public void SetUniform(string name, Vector3 vec)
		{
			int loc = GL.GetUniformLocation(_handle, name);
			GL.Uniform3(loc, ref vec);
		}
		public void SetUniform(string name, Vector2 vec)
		{
			int loc = GL.GetUniformLocation(_handle, name);
			GL.Uniform2(loc, ref vec);
		}
		public void SetUniform(string name, float f)
		{
			int loc = GL.GetUniformLocation(_handle, name);
			GL.Uniform1(loc, f);
		}

		protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                GL.DeleteProgram(_handle);

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
