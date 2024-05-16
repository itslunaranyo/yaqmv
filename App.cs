using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;

namespace yaqmv
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class YAQMVApp : Application
	{
		internal static YAQMVApp App { get { return (YAQMVApp)Application.Current; } }

		public YAQMVApp()
		{
			Debug.Print("app started");
		}

	}

}
