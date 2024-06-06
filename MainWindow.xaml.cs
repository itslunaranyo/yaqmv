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
using System.ComponentModel;
using System.Windows.Threading;

namespace yaqmv
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		internal ModelWindow _mw;
		internal ModelAsset _loadedAsset;
		internal ModelState _modelstate;
		private DispatcherTimer _time;

		public MainWindow()
		{
			InitializeComponent();
			DataContext = this;

			_time = new DispatcherTimer();
			_time.Interval = TimeSpan.FromSeconds(0.1);
			_time.Tick += Anim_Tick;
			_time.Start();

			_mw = (ModelWindow)FindName("ModelWindow");
			_loadedAsset = new ModelAsset();
			Playing = false;
		}
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			NotifyPropertyChanged("StatsText");
			NotifyPropertyChanged("AnimStatsText");
			NotifyPropertyChanged("CurrentFrameText");
		}
		private void Window_Unloaded(object sender, RoutedEventArgs e) { _mw.OnUnload(sender, e); }

		private void OnSizeChanged(object sender, SizeChangedEventArgs e) { _mw.OnSizeChanged(sender, e); }
		private void OnRender(TimeSpan delta) { _mw.OnRender(delta); }

		internal void Display(ModelAsset mdl)
		{
			_modelstate = new ModelState();
			_mw.LoadModel(mdl);
			Playing = false;
			SelectAnim(0);
			AnimSelect.ItemsSource = _loadedAsset.AnimNames;
			AnimSelect.SelectedIndex = 0;
			SkinSelect.ItemsSource = _loadedAsset.SkinNames;
			SkinSelect.SelectedIndex = 0;
			NotifyPropertyChanged("StatsText");
		}

		internal ModelState GetModelState(TimeSpan delta)
		{
			return _modelstate;
		}

		private void SelectAnim(int a)
		{
			if (a != _modelstate.Anim)
			_modelstate.Anim = a;

			Timeline.Value = _loadedAsset.anims[_modelstate.Anim].first;
			NotifyPropertyChanged("TimelineMin");
			NotifyPropertyChanged("TimelineMax");
			NotifyPropertyChanged("AnimStatsText");
		}
		public int SelectedSkin
		{
			get { return _modelstate.Skin; }
			set { _modelstate.Skin = value; }
		}
		private void Anim_Tick(object sender, EventArgs e)
		{
			if (Timeline.Value == _loadedAsset.anims[_modelstate.Anim].last)
				Timeline.Value = _loadedAsset.anims[_modelstate.Anim].first;
			else
				Timeline.Value += 1;

		}

		private void StepForward()
		{
			TimelineValue = ClampToTimeline(_modelstate.Frame + 1);
		}
		private void StepBackward()
		{
			TimelineValue = ClampToTimeline(_modelstate.Frame - 1);
		}

		// =====================
		// PROPERTIES
		// =====================
		internal static MainWindow Get { get { return (MainWindow)Application.Current.MainWindow; } }

		private bool _playing;
		public bool Playing
		{
			get { return _playing; }
			set {
				_playing = value;
				if (_playing)
				{
					BPlay.Content = "⏸";
					_time.Start();
				}
				else
				{
					BPlay.Content = "▶";
					_time.Stop();
				}
			}
		}

		public int TimelineValue { 
			get { return _modelstate.Frame; } 
			set { 
				_modelstate.Frame = value;
				NotifyPropertyChanged("TimelineValue");
				NotifyPropertyChanged("CurrentFrameText");
			} 
		}
		public int TimelineMin { get { return _loadedAsset.anims[_modelstate.Anim].first; } }
		public int TimelineMax { get { return _loadedAsset.anims[_modelstate.Anim].last; } }
		public int ClampToTimeline(int a)
		{
			return Math.Min(TimelineMax, Math.Max(a, TimelineMin));
		}

		public string StatsText { get { 
			return "Vertices: " + _loadedAsset.VertexCount.ToString() +
				"\nTriangles: " + _loadedAsset.TriangleCount.ToString() +
				"\nFrames: " + _loadedAsset.FrameCount.ToString() +
				"\nSkins: " + _loadedAsset.SkinCount.ToString();
			}
		}
		public string AnimStatsText { get {
			return "Sequence #: " + _modelstate.Anim.ToString() +
				"\nFrames: " + (TimelineMax - TimelineMin + 1).ToString() +
				" (" + TimelineMin.ToString() +
				"-" + TimelineMax.ToString() + ")";
			}
		}
		public string CurrentFrameText { get {
			int ftime = ((int)Timeline.Value > _loadedAsset.frames.Length) ? 0 : (int)Timeline.Value;
			return "Current: " + _loadedAsset.frames[ftime].name + 
				" (" + ftime.ToString() + ")";
			}
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		
		// =====================
		// INPUT
		// =====================

		static System.Windows.Point _mousepos;
		private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			var newpos = e.GetPosition(this);
			var delta = newpos - _mousepos;

			if (Mouse.LeftButton == MouseButtonState.Pressed)
				Camera.Orbit((float)delta.X, (float)delta.Y);
			else if (Mouse.RightButton == MouseButtonState.Pressed)
				Camera.Dolly(-(float)delta.Y);
			else if (Mouse.MiddleButton == MouseButtonState.Pressed)
				Camera.Pan((float)delta.X, (float)delta.Y);

			_mousepos = newpos;
		}
		private void OnMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (e.Delta > 0) StepForward();
			else StepBackward();
		}
		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.F)
			{
				Camera.Recenter(_loadedAsset.CenterOfFrame(0), _loadedAsset.RadiusOfFrame(0));
			}
			if (e.Key == Key.K)
			{
				_modelstate.Skin = (_modelstate.Skin + 1) % _loadedAsset.SkinCount;
			}
			if (e.Key == Key.OemPeriod)
			{
				StepForward();
			}
			if (e.Key == Key.OemComma)
			{
				StepBackward();
			}
			if (e.Key == Key.M)
			{
				_mw.ToggleMode();
			}
			e.Handled = true;
		}

		private void MenuFileOpen(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Quake model files (*.mdl)|*.mdl|All files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == true)
			{
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
			SelectedSkin = SkinSelect.SelectedIndex == -1 ? 0 : SkinSelect.SelectedIndex;
		}

		private void Timeline_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			//NotifyPropertyChanged("CurrentFrameText");
		}

		private void BSkipEnd_Click(object sender, RoutedEventArgs e)
		{
			Playing = false;
			Timeline.Value = _loadedAsset.anims[_modelstate.Anim].last;
		}

		private void BPlayPause_Click(object sender, RoutedEventArgs e)
		{
			Playing = !Playing;
		}

		private void BSkipStart_Click(object sender, RoutedEventArgs e)
		{
			Playing = false;
			Timeline.Value = _loadedAsset.anims[_modelstate.Anim].first;
		}
	}
}