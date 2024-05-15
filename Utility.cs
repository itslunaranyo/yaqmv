using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Resources;

namespace yaqmv
{
	public partial class Resource
	{
		public static string ReadText(string path)
		{
			StreamResourceInfo srinfo = YAQMVApp.GetResourceStream(new Uri(path, UriKind.Relative));
			StreamReader sr = new StreamReader(srinfo.Stream);
			string read = sr.ReadToEnd();
			return read;
		}
	}

}
