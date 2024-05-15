using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Resources;

namespace yaqmv
{
	public static class Utility
	{
		public static string ReadResourceText(string path)
		{
			StreamResourceInfo srinfo = YAQMVApp.GetResourceStream(new Uri(path, UriKind.Relative));
			StreamReader sr = new StreamReader(srinfo.Stream);
			string read = sr.ReadToEnd();
			return read;
		}
	}

}
