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
		public void GLWhatsWrong()
		{
			var e = GL.GetError();
			if (e != ErrorCode.NoError)
				Debug.WriteLine(e);
		}

		int Handle { get; }

		public Texture(int w, int h, byte[] pixeldata)
		{
			Handle = GL.GenTexture();

			byte[] fakedata = {
				128, 0, 255, 255, 128, 128, 0, 0, 128, 0, 0, 0, 128, 128, 0, 0,
				128, 128, 0, 0, 128, 0, 0, 0, 128, 128, 0, 0, 128, 0, 0, 0,
				128, 0, 0, 0, 128, 128, 0, 0, 128, 0, 0, 0, 128, 128, 0, 0,
				128, 128, 0, 0, 128, 0, 0, 0, 128, 128, 0, 0, 128, 0, 0, 0,
			};

			GL.BindTexture(TextureTarget.Texture2D, Handle);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest); 
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, w, h, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixeldata);
			//GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 4, 4, 0, PixelFormat.Rgba, PixelType.UnsignedByte, fakedata);
			GLWhatsWrong();
		}
		public void Bind(TextureUnit unit = TextureUnit.Texture0)
		{
			GL.ActiveTexture(unit);
			GL.BindTexture(TextureTarget.Texture2D, Handle);
			GLWhatsWrong();
		}
	}
}