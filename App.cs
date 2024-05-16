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
			if (LoadedAsset == null) return;

			int a = _modelstate.Anim;
			a = (a + (back? -1 : 1));
			a = Math.Min(LoadedAsset.anims.Count() - 1, Math.Max(a, 0));
			SelectAnim(a);
		}
		internal void SelectAnim(int a)
		{
			if (LoadedAsset == null) return;

			if (a != _modelstate.Anim)
				_time.Restart();
			_modelstate.Anim = a;
		}

		internal ModelState GetModelState()
		{
			_modelstate.Frame = LoadedAsset.anims[_modelstate.Anim].first + 
				((int)Math.Floor(_time.Elapsed.TotalSeconds * 10) % (LoadedAsset.anims[_modelstate.Anim].frameCount));
			return _modelstate;
		}
	}

}
