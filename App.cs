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
		internal ModelState _modelstate;
		private Stopwatch _time;

		internal static YAQMVApp App { get { return (YAQMVApp)Application.Current; } }

		public YAQMVApp()
		{
			_time = new Stopwatch();
			Debug.Print("app started");
			LoadedAsset = null;
		}
		internal bool LoadAsset(string filename)
		{
			_time.Restart();
			LoadedAsset = new ModelAsset(filename);
			((MainWindow)MainWindow).Display(LoadedAsset);
			return true;
		}
		internal void Focus()
		{
			if (LoadedAsset == null) return;
			Camera.Recenter(LoadedAsset.CenterOfFrame(0), LoadedAsset.RadiusOfFrame(0));
		}
		internal void CycleSkin()
		{
			if (LoadedAsset == null) return;
			_modelstate.Skin = (_modelstate.Skin + 1) % LoadedAsset.SkinCount;
		}

		internal ModelState GetModelState()
		{
			_modelstate.Frame = (int)Math.Floor(_time.Elapsed.TotalSeconds * 10) % LoadedAsset.FrameCount;
			return _modelstate;
		}
	}

}
