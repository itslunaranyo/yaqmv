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
//using OpenTK.Windowing.Common;
using OpenTK.Wpf;

namespace yaqmv
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		internal ModelWindow _modelWindow;
		internal ModelAsset _loadedAsset;
		internal ModelState _modelState;
		private DispatcherTimer _time;

		public static GLWpfControlSettings GlobalGLWPFSettings { get; private set; }

		public MainWindow()
		{
			InitializeComponent();

			// start the GL windows from here and not in their constructors so we can pass
			// the shared context around
			GlobalGLWPFSettings = new GLWpfControlSettings
			{
				MajorVersion = 3,
				MinorVersion = 3,
				Profile = OpenTK.Windowing.Common.ContextProfile.Core
			};
			_modelWindow = (ModelWindow)FindName("ModelWindow");
			_modelWindow.Init(GlobalGLWPFSettings);

			GlobalGLWPFSettings.SharedContext = (OpenTK.Windowing.Desktop.IGLFWGraphicsContext?)_modelWindow.Context;



			DataContext = this;

			_time = new DispatcherTimer();
			_time.Interval = TimeSpan.FromSeconds(0.1);
			_time.Tick += Anim_Tick;
			_time.Start();

			_loadedAsset = new ModelAsset();
			Playing = false;
		}
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			NotifyPropertyChanged("StatsText");
			NotifyPropertyChanged("AnimStatsText");
			NotifyPropertyChanged("CurrentFrameText");
		}
		private void Window_Unloaded(object sender, RoutedEventArgs e) { _modelWindow.OnUnload(sender, e); }

		private void OnSizeChanged(object sender, SizeChangedEventArgs e) { _modelWindow.OnSizeChanged(sender, e); }

		static double skinTime = 0;
		private void OnRender(TimeSpan delta)
		{
			// TODO: this won't work on skingroups past the first one
			if (_loadedAsset.skins.Length > 0)
			{
				int i;
				int skinLength = _loadedAsset.skins[_modelState.Skin].images.Length;
				if (skinLength > 1)
				{
					_modelState.Skinframe = 0;
					for (i = 0; i < skinLength; i++)
					{
						if (_loadedAsset.skins[_modelState.Skin].durations[i] > skinTime)
							break;
					}
					if (i == skinLength)
					{
						skinTime -= _loadedAsset.skins[_modelState.Skin].durations[i-1];
						i = 0;
					}

					skinTime += delta.TotalSeconds;
					_modelState.Skinframe = i;
				}
			}
			_modelWindow.OnRender(delta, _modelState);
		}

		internal void Display(ModelAsset mdl)
		{
			_modelState = new ModelState();
			_modelWindow.LoadModel(mdl);
			Playing = false;
			SelectAnim(0);
			AnimSelect.ItemsSource = _loadedAsset.AnimNames;
			AnimSelect.SelectedIndex = 0;
			SkinSelect.ItemsSource = _loadedAsset.SkinNames;
			SkinSelect.SelectedIndex = 0;
			NotifyPropertyChanged("StatsText");
		}

		private void SelectAnim(int a)
		{
			if (a != _modelState.Anim)
			_modelState.Anim = a;

			Timeline.Value = _loadedAsset.anims[_modelState.Anim].first;
			NotifyPropertyChanged("TimelineMin");
			NotifyPropertyChanged("TimelineMax");
			NotifyPropertyChanged("AnimStatsText");
		}
		public int SelectedSkin
		{
			get { return _modelState.Skin; }
			set { _modelState.Skin = value; }
		}
		private void Anim_Tick(object sender, EventArgs e)
		{
			if (Timeline.Value == _loadedAsset.anims[_modelState.Anim].last)
				Timeline.Value = _loadedAsset.anims[_modelState.Anim].first;
			else
				Timeline.Value += 1;

		}

		private void StepForward()
		{
			TimelineValue = ClampToTimeline(_modelState.Frame + 1);
		}
		private void StepBackward()
		{
			TimelineValue = ClampToTimeline(_modelState.Frame - 1);
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
			get { return _modelState.Frame; } 
			set { 
				_modelState.Frame = value;
				NotifyPropertyChanged("TimelineValue");
				NotifyPropertyChanged("CurrentFrameText");
			} 
		}
		public int TimelineMin { get { return _loadedAsset.anims[_modelState.Anim].first; } }
		public int TimelineMax { get { return _loadedAsset.anims[_modelState.Anim].last; } }
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
			return "Sequence #: " + _modelState.Anim.ToString() +
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

		private void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			// fixme: why doesn't the XAML parser want to just bind the handler on the modelwindow?
			//if (e.Source == _mw)
			//{
			//	_mw.OnMouseMove(sender, e);
			//}
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
				_modelState.Skin = (_modelState.Skin + 1) % _loadedAsset.SkinCount;
			}
			if (e.Key == Key.OemPeriod)
			{
				StepForward();
			}
			if (e.Key == Key.OemComma)
			{
				StepBackward();
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
			Timeline.Value = _loadedAsset.anims[_modelState.Anim].last;
		}

		private void BPlayPause_Click(object sender, RoutedEventArgs e)
		{
			Playing = !Playing;
		}

		private void BSkipStart_Click(object sender, RoutedEventArgs e)
		{
			Playing = false;
			Timeline.Value = _loadedAsset.anims[_modelState.Anim].first;
		}

		private void ModeSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			_modelWindow?.SetMode(ModeSelect.SelectedIndex);
		}
	}
}