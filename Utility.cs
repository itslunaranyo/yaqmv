using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Resources;
using OpenTK.Graphics.OpenGL;

namespace yaqmv
{
	public static class Utility
	{
		public static void GLWhatsWrong(string pfix = "")
		{
			var e = GL.GetError();
			if (e != ErrorCode.NoError)
				Debug.WriteLine(pfix + ": " + e);
			else if (pfix.Length != 0)
				Debug.WriteLine(pfix + " ok");
		}
		public static string ReadResourceText(string path)
		{
			StreamResourceInfo srInfo = YAQMVApp.GetResourceStream(new Uri(path, UriKind.Relative));
			StreamReader sr = new StreamReader(srInfo.Stream);
			string read = sr.ReadToEnd();
			return read;
		}
	}

}
