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
using System.ComponentModel;
using static yaqmv.ModelAsset;

namespace yaqmv
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		internal ModelWindow _mw;
		internal ModelAsset _loadedAsset;
		internal ModelState _modelstate;
		private Stopwatch _time;

		public MainWindow()
		{
			InitializeComponent();
			DataContext = this;

			_mw = (ModelWindow)FindName("ModelWindow");
			_loadedAsset = new ModelAsset();
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
			AnimSelect.ItemsSource = _loadedAsset.AnimNames;
			AnimSelect.SelectedIndex = 0;
			SkinSelect.ItemsSource = _loadedAsset.SkinNames;
			SkinSelect.SelectedIndex = 0;
			_mw.LoadModel(mdl);

			TStats.Text = "Vertices: " + _loadedAsset.VertexCount.ToString() +
						"\nTriangles: " + _loadedAsset.TriangleCount.ToString() +
						"\nFrames: " + _loadedAsset.FrameCount.ToString() +
						"\nSkins: " + _loadedAsset.SkinCount.ToString();
		}

		internal ModelState GetModelState()
		{
			_modelstate.Frame = _loadedAsset.anims[_modelstate.Anim].first +
				((int)Math.Floor(_time.Elapsed.TotalSeconds * 10) % (_loadedAsset.anims[_modelstate.Anim].frameCount));
			return _modelstate;
		}

		private void SelectAnim(int a)
		{
			if (a != _modelstate.Anim)
				_time.Restart();
			_modelstate.Anim = a;

			TAnimStats.Text = "Sequence #: " + a.ToString() +
				"\nFrames: " + (_loadedAsset.anims[_modelstate.Anim].last - _loadedAsset.anims[_modelstate.Anim].first + 1).ToString() +
				" (" + _loadedAsset.anims[_modelstate.Anim].first.ToString() +
				"-" + _loadedAsset.anims[_modelstate.Anim].last.ToString() + ")";
		}
		private void SelectSkin(int s)
		{
			_modelstate.Skin = s;
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
			a = Math.Min(_loadedAsset.anims.Count() - 1, Math.Max(a, 0));
			SelectAnim(a);
		}
		private void MWOnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.F)
			{
				Camera.Recenter(_loadedAsset.CenterOfFrame(0), _loadedAsset.RadiusOfFrame(0));
			}
			if (e.Key == Key.K)
			{
				_modelstate.Skin = (_modelstate.Skin + 1) % _loadedAsset.SkinCount;
			}
		}

		private void MenuFileOpen(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Quake model files (*.mdl)|*.mdl|All files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == true)
			{
				_time.Restart();
				_loadedAsset = new ModelAsset(openFileDialog.FileName);

				Display(_loadedAsset);
			}
		}
		private void MenuFileQuit(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void AnimSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SelectAnim(AnimSelect.SelectedIndex == -1 ? 0 : AnimSelect.SelectedIndex);
		}

		private void SkinSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SelectSkin(SkinSelect.SelectedIndex == -1 ? 0 : SkinSelect.SelectedIndex);
		}
	}
}