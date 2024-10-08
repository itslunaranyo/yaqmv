﻿using System.Text;
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
using OpenTK.Wpf;

namespace yaqmv
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		internal ModelWindow _modelWindow;
		internal SkinWindow _skinWindow;
		internal ModelAsset _loadedAsset;
		internal ModelState _modelState;
		internal SkinState _skinState;
		private DispatcherTimer _time;
		public GLWpfControlSettings GlobalGLWPFSettings { get; private set; }

		public bool IsModelLoaded { get; private set; }
		public bool IsSkinWindowVisible { get; private set; }
		public bool IsFlagsWindowVisible { get; private set; }

		public MainWindow()
		{
			InitializeComponent();

			// start the GL windows from here and not in their constructors so we can pass
			// the shared context around
			GlobalGLWPFSettings = new GLWpfControlSettings
			{
				MajorVersion = 3,
				MinorVersion = 3,
				Profile = OpenTK.Windowing.Common.ContextProfile.Core,
				Samples = 8,
			};
			_modelWindow = (ModelWindow)FindName("ModelWindow");
			_modelWindow.Init(GlobalGLWPFSettings);
			
			GlobalGLWPFSettings.SharedContext = (OpenTK.Windowing.Desktop.IGLFWGraphicsContext?)_modelWindow.Context;

			_skinWindow = (SkinWindow)FindName("SkinWindow");
			_skinWindow.Init(GlobalGLWPFSettings);

			Shader.Init();

			DataContext = this;

			_time = new DispatcherTimer();
			_time.Interval = TimeSpan.FromSeconds(0.1);
			_time.Tick += Anim_Tick;
			_time.Start();

			_loadedAsset = new ModelAsset();
			IsModelLoaded = false;
			Playing = false;
		}
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			NotifyPropertyChanged("StatsText");
			NotifyPropertyChanged("AnimStatsText");
			NotifyPropertyChanged("AnimStatsText");
			NotifyPropertyChanged("SkinText");
		}
		private void Window_Unloaded(object sender, RoutedEventArgs e) { _modelWindow.OnUnload(sender, e); }

		private void OnSizeChanged(object sender, SizeChangedEventArgs e)
		{
			// change size of child grids correctly
			double widthDiff = e.NewSize.Width - e.PreviousSize.Width;

		}

		static double skinTime = 0;

		private void ModelWindow_OnRender(TimeSpan delta)
		{
			int i;

			if (IsModelLoaded && _loadedAsset.skins.Length > 0)
			{
				int skinLength;
				skinLength = _loadedAsset.skins[_modelState.Skin].images.Length;
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
			
			_modelWindow.OnRender(_modelState);
		}
		
		private void SkinWindow_OnRender(TimeSpan delta)
		{
			_skinWindow.OnRender(_skinState);
		}
		
		internal void Display(ModelAsset mdl)
		{
			_modelState = new ModelState();

			_modelWindow.LoadModel(mdl);
			_skinWindow.LoadModel(mdl);

			IsModelLoaded = true;
			BSkinImport.IsEnabled = true;
			BSkinExport.IsEnabled = true;
			BUVExport.IsEnabled = true;

			Playing = false;
			SelectAnim(0);
			AnimSelect.ItemsSource = _loadedAsset.AnimNames;
			AnimSelect.SelectedIndex = 0;

			SkinSelect.ItemsSource = _loadedAsset.SkinNames;
			SkinSelect.SelectedIndex = 0;
			SkinFrameSelect.ItemsSource = _loadedAsset.SkinFrameNames[0];
			SkinFrameSelect.SelectedIndex = 0;
			SetSkinFrameSelectStatus();

			NotifyPropertyChanged("StatsText");
			NotifyPropertyChanged("SkinText");
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
			set { _modelState.Skin = value; _skinState.Skin = value; }
		}

		public int SelectedSkinFrame
		{
			get { return _skinState.Skinframe; }
			set { _skinState.Skinframe = value; }
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
				NotifyPropertyChanged("AnimStatsText");
			} 
		}
		public int TimelineMin { get { return _loadedAsset.anims[_modelState.Anim].first; } }
		public int TimelineMax { get { return _loadedAsset.anims[_modelState.Anim].last; } }
		public int ClampToTimeline(int a)
		{
			return Math.Min(TimelineMax, Math.Max(a, TimelineMin));
		}

		public string StatsText { get { 
			//return "Vertices: " + _loadedAsset.VertexCount.ToString() +
			//	"\nTriangles: " + _loadedAsset.TriangleCount.ToString() +
			//	"\nFrames: " + _loadedAsset.FrameCount.ToString() +
			//	"\nSkins: " + _loadedAsset.SkinCount.ToString();
			return _loadedAsset.VertexCount.ToString() +
				"\n" + _loadedAsset.TriangleCount.ToString() +
				"\n" + _loadedAsset.FrameCount.ToString() +
				"\n" + _loadedAsset.SkinCount.ToString();
			}
		}
		public string AnimStatsText { get {
			int ftime = ((int)Timeline.Value > _loadedAsset.frames.Length) ? 0 : (int)Timeline.Value;
			return "Frames: " + (TimelineMax - TimelineMin + 1).ToString() +
				" (" + TimelineMin.ToString() +
				"-" + TimelineMax.ToString() + 
				")\nCurrent: " + ftime.ToString() +
				" (" + _loadedAsset.frames[ftime].name + ")";
			}
		}

		public string SkinText
		{
			get
			{
				if (!IsModelLoaded)
					return "";

				return _loadedAsset.SkinWidth.ToString() +
					" x " +
					_loadedAsset.SkinHeight.ToString();
			}
		}

		private bool _UVShow = false;
		private bool _UVOverlay = false;
		public bool UVShow { 
			get { return _UVShow; } 
			set {
				_UVShow = value;
				_UVOverlay = false;
				NotifyPropertyChanged("UVOverlay");
				SkinWindow.SetMode((!_UVShow || _UVOverlay), (_UVShow || _UVOverlay));
			}
		}
		public bool UVOverlay { 
			get { return _UVOverlay; } 
			set {
				_UVOverlay = value;
				_UVShow = false;
				NotifyPropertyChanged("UVShow");
				SkinWindow.SetMode((!_UVShow || _UVOverlay), (_UVShow || _UVOverlay));
			} }


		public event PropertyChangedEventHandler? PropertyChanged;
		private void NotifyPropertyChanged(String propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		// =====================
		// COMMANDS
		// =====================

		public static RoutedCommand QuitCmd = new RoutedCommand();
		public static RoutedCommand FocusCmd = new RoutedCommand();
		public static RoutedCommand ViewSkinCmd = new RoutedCommand();
		public static RoutedCommand ViewFlagsCmd = new RoutedCommand();
		private double _oldSkinWinWidth;

		private void FocusCamera()
		{
			Camera3D.Recenter(_loadedAsset.CenterOfFrame(0), _loadedAsset.RadiusOfFrame(0));
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
		private void MenuFileQuit(object sender, RoutedEventArgs e) { Close(); }
		private void MenuViewFocus(object sender, RoutedEventArgs e) { FocusCamera(); }
		private void MenuViewSkin(object sender, RoutedEventArgs e) { ToggleSkinWindow(); }
		private void MenuViewFlags(object sender, RoutedEventArgs e) { ToggleFlagsWindow(); }

		private void ToggleSkinWindow()
		{
			double newWidth = Width;
			if (IsSkinWindowVisible)
			{
				_oldSkinWinWidth = SkinColumn.ActualWidth + 6;
				newWidth -= _oldSkinWinWidth;
			}
			IsSkinWindowVisible = !IsSkinWindowVisible;
			NotifyPropertyChanged("IsSkinWindowVisible");
			if (IsSkinWindowVisible)
			{
				if (_oldSkinWinWidth == 0)
				{
					_oldSkinWinWidth = ModelColumn.ActualWidth;
					var glc = new GridLengthConverter();
					SkinColumn.Width = (GridLength)glc.ConvertFrom(_oldSkinWinWidth);
					_oldSkinWinWidth += 6;
				}
				newWidth += _oldSkinWinWidth;
			}
			Width = newWidth;
		}

		private void ToggleFlagsWindow()
		{
			if (IsFlagsWindowVisible)
				Width -= FlagsColumn.Width.Value;
			IsFlagsWindowVisible = !IsFlagsWindowVisible;
			NotifyPropertyChanged("IsFlagsWindowVisible");
			if (IsFlagsWindowVisible)
				Width += FlagsColumn.Width.Value;
		}       // =====================
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
			// if a mousewheel event bubbles past something that could have handled it if it were
			// focused, focus it and try again
			var target = e.Source as UIElement;
			if (target != null && target.Focusable && !target.IsFocused)
			{
				target.Focus();
				target.RaiseEvent(e);
				return;
			}

			// default to scrolling through the animation for mousewheels anywhere else
			if (e.Delta < 0) StepForward();
			else StepBackward();
		}

		private void OnKeyDown(object sender, KeyEventArgs e)
		{
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


		private void AnimSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SelectAnim(AnimSelect.SelectedIndex == -1 ? 0 : AnimSelect.SelectedIndex);
		}

		private void SkinSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SelectedSkin = SkinSelect.SelectedIndex == -1 ? 0 : SkinSelect.SelectedIndex;
			SetSkinFrameSelectStatus();
		}
		private void SetSkinFrameSelectStatus()
		{
			SkinFrameSelect.IsEnabled = (_loadedAsset.skins[SelectedSkin].images.Length > 1);
			if (SkinFrameSelect.IsEnabled)
			{
				SkinFrameSelect.ItemsSource = _loadedAsset.SkinFrameNames[0];
				SkinFrameSelect.SelectedIndex = 0;
			}
		}
		private void SkinFrameSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SelectedSkinFrame = SkinFrameSelect.SelectedIndex == -1 ? 0 : SkinFrameSelect.SelectedIndex;
		}

		private void Timeline_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			//NotifyPropertyChanged("AnimStatsText");
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

		private void BSkinImport_Click(object sender, RoutedEventArgs e)
		{
			if (!IsModelLoaded) return;
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Quake model files (*.mdl)|*.mdl|All files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == true)
			{
				_loadedAsset = new ModelAsset(openFileDialog.FileName);

				Display(_loadedAsset);
			}
		}
		private void BSkinExport_Click(object sender, RoutedEventArgs e)
		{
			if (!IsModelLoaded) return;
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "BMP files (*.bmp)|*.bmp|PNG files (*.png)|*.png|All files (*.*)|*.*";
			if (saveFileDialog.ShowDialog() == true)
			{
				//saveFileDialog.FileName;

			}
		}
		private void BUVExport_Click(object sender, RoutedEventArgs e)
		{
			if (!IsModelLoaded) return;
			SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "BMP files (*.bmp)|*.bmp|PNG files (*.png)|*.png|All files (*.*)|*.*";
			if (saveFileDialog.ShowDialog() == true)
			{
				//saveFileDialog.FileName;

			}
		}

	}
}