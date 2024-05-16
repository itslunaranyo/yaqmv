using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Drawing;
using Microsoft.Win32;
using System.Collections.ObjectModel;

namespace yaqmv
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		internal ModelWindow _mw;
		internal ModelAsset LoadedAsset;
		internal ModelState _modelstate;
		private Stopwatch _time;

		public MainWindow()
		{
			InitializeComponent();

			_mw = (ModelWindow)FindName("ModelWindow");
			LoadedAsset = new ModelAsset();
			_time = new Stopwatch();
		}
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
		}


		internal static MainWindow Get { get { return (MainWindow)Application.Current.MainWindow; } }

		private void MWOnSizeChanged(object sender, SizeChangedEventArgs e) { _mw.OnSizeChanged(sender, e); }
		private void MWOnRender(TimeSpan delta) { _mw.OnRender(delta); }
		private void MWOnUnload(object sender, RoutedEventArgs e) { _mw.OnUnload(sender, e); }

		internal void Display(ModelAsset mdl)
		{
			_modelstate = new ModelState();
			AnimSelect.ItemsSource = LoadedAsset.AnimNames;
			AnimSelect.SelectedIndex = 0;
			_mw.LoadModel(mdl);
		}

		internal ModelState GetModelState()
		{
			_modelstate.Frame = LoadedAsset.anims[_modelstate.Anim].first +
				((int)Math.Floor(_time.Elapsed.TotalSeconds * 10) % (LoadedAsset.anims[_modelstate.Anim].frameCount));
			return _modelstate;
		}

		private void SelectAnim(int a)
		{
			if (a != _modelstate.Anim)
				_time.Restart();
			_modelstate.Anim = a;
		}

		// =====================
		// INPUT
		// =====================

		static System.Windows.Point _mousepos;
		private void MWOnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			var newpos = e.GetPosition(this);
			var delta = newpos - _mousepos;

			if (Mouse.LeftButton == MouseButtonState.Pressed)
				Camera.Orbit((float)delta.X, (float)delta.Y);
			else if (Mouse.RightButton == MouseButtonState.Pressed)
				Camera.Dolly((float)delta.X + (float)delta.Y);
			else if (Mouse.MiddleButton == MouseButtonState.Pressed)
				Camera.Pan((float)delta.X, (float)delta.Y);

			_mousepos = newpos;
		}
		private void MWOnMouseWheel(object sender, MouseWheelEventArgs e)
		{
			int a = _modelstate.Anim;
			a = (a + ((e.Delta > 0) ? -1 : 1));
			a = Math.Min(LoadedAsset.anims.Count() - 1, Math.Max(a, 0));
			SelectAnim(a);
		}
		private void MWOnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.F)
			{
				Camera.Recenter(LoadedAsset.CenterOfFrame(0), LoadedAsset.RadiusOfFrame(0));
			}
			if (e.Key == Key.K)
			{
				_modelstate.Skin = (_modelstate.Skin + 1) % LoadedAsset.SkinCount;
			}
		}

		private void MenuFileOpen(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Quake model files (*.mdl)|*.mdl|All files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == true)
			{
				_time.Restart();
				LoadedAsset = new ModelAsset(openFileDialog.FileName);

				Display(LoadedAsset);
			}
		}
		private void MenuFileQuit(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void AnimSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SelectAnim(AnimSelect.SelectedIndex);
		}
	}
}