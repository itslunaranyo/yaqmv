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
using System.Collections.ObjectModel;
using System.ComponentModel;
using static yaqmv.ModelAsset;
using System.Runtime.CompilerServices;

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
			NotifyPropertyChanged("StatsText");
			NotifyPropertyChanged("AnimStatsText");
			NotifyPropertyChanged("CurrentFrameText");
		}


		internal static MainWindow Get { get { return (MainWindow)Application.Current.MainWindow; } }

		private void MWOnSizeChanged(object sender, SizeChangedEventArgs e) { _mw.OnSizeChanged(sender, e); }
		private void MWOnRender(TimeSpan delta) { _mw.OnRender(delta); }
		private void MWOnUnload(object sender, RoutedEventArgs e) { _mw.OnUnload(sender, e); }

		internal void Display(ModelAsset mdl)
		{
			_modelstate = new ModelState();
			_mw.LoadModel(mdl);
			SelectAnim(0);
			AnimSelect.ItemsSource = _loadedAsset.AnimNames;
			AnimSelect.SelectedIndex = 0;
			SkinSelect.ItemsSource = _loadedAsset.SkinNames;
			SkinSelect.SelectedIndex = 0;
			NotifyPropertyChanged("StatsText");
		}

		internal ModelState GetModelState()
		{
			return _modelstate;
		}

		private void SelectAnim(int a)
		{
			if (a != _modelstate.Anim)
				_time.Restart();
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



		// =====================
		// PROPERTIES
		// =====================

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

		public string StatsText { get { 
			return "Vertices: " + _loadedAsset.VertexCount.ToString() +
				"\nTriangles: " + _loadedAsset.TriangleCount.ToString() +
				"\nFrames: " + _loadedAsset.FrameCount.ToString() +
				"\nSkins: " + _loadedAsset.SkinCount.ToString();
			}
		}
		public string AnimStatsText { get {
			return "Sequence #: " + _modelstate.Anim.ToString() +
				"\nFrames: " + (_loadedAsset.anims[_modelstate.Anim].last - _loadedAsset.anims[_modelstate.Anim].first + 1).ToString() +
				" (" + _loadedAsset.anims[_modelstate.Anim].first.ToString() +
				"-" + _loadedAsset.anims[_modelstate.Anim].last.ToString() + ")";
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
		private void MWOnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
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
		private void MWOnMouseWheel(object sender, MouseWheelEventArgs e)
		{
			int a = _modelstate.Frame;
			a = (a + ((e.Delta > 0) ? -1 : 1));
			a = Math.Min(_loadedAsset.anims[_modelstate.Anim].last, Math.Max(a, _loadedAsset.anims[_modelstate.Anim].first));
			TimelineValue = a;
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
			SelectedSkin = SkinSelect.SelectedIndex == -1 ? 0 : SkinSelect.SelectedIndex;
		}

		private void Timeline_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			//NotifyPropertyChanged("CurrentFrameText");
		}
	}
}