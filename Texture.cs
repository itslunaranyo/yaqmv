using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using System.Reflection.Metadata;
using Accessibility;
using System.Diagnostics;
using System.Windows.Media.TextFormatting;

namespace yaqmv
{
	internal class Texture
	{

		private int _handle { get; }

		public Texture(int w, int h, byte[] pixeldata)
		{
			_handle = GL.GenTexture();

			GL.BindTexture(TextureTarget.Texture2D, _handle);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest); 
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder); 
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder); 
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, w, h, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixeldata);
			//GLWhatsWrong();
		}
		public void Bind(TextureUnit unit = TextureUnit.Texture0)
		{
			GL.ActiveTexture(unit);
			GL.BindTexture(TextureTarget.Texture2D, _handle);
			//GLWhatsWrong();
		}

		private bool _disposed;
		~Texture()
		{
			if (!_disposed)
			{
				Debug.WriteLine("GPU resource leak - undisposed texture");
			}
		}
		public void Dispose()
		{
			if (_disposed) return;
			GL.DeleteTexture(_handle);
			_disposed = true;
		}
	}
}