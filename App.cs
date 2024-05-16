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
			_modelstate = new ModelState();
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
		internal void CycleAnim(bool back)
		{
			_time.Restart();
			if (LoadedAsset == null) return;
			_modelstate.Anim = (_modelstate.Anim + (back? -1 : 1));
			_modelstate.Anim = Math.Min(LoadedAsset.anims.Count() - 1, Math.Max(_modelstate.Anim, 0));
		}
		internal ModelState GetModelState()
		{
			_modelstate.Frame = LoadedAsset.anims[_modelstate.Anim].first + 
				((int)Math.Floor(_time.Elapsed.TotalSeconds * 10) % (LoadedAsset.anims[_modelstate.Anim].frameCount));
			return _modelstate;
		}
	}

}
