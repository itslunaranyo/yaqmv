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
		internal ModelAsset? LoadedAsset;

		internal static YAQMVApp App { get { return (YAQMVApp)Application.Current; } }

		public YAQMVApp()
		{
			Debug.Print("app started");
			LoadedAsset = null;
		}
		public bool LoadAsset(string filename)
		{
			LoadedAsset = new ModelAsset(filename);
			((MainWindow)MainWindow).Display(LoadedAsset);
			return true;
		}
		public void Focus()
		{
			if (LoadedAsset == null) return;
			Camera.Recenter(LoadedAsset.CenterOfFrame(0), LoadedAsset.RadiusOfFrame(0));
		}
	}

}
