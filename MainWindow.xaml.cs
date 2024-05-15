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

namespace yaqmv
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		ModelWindow _mw;
		public MainWindow()
		{
			InitializeComponent();

			_mw = (ModelWindow)FindName("ModelWindow");
		}
		internal static MainWindow Win { get { return (MainWindow)Application.Current.MainWindow; } }

		private void MWOnSizeChanged(object sender, SizeChangedEventArgs e) { _mw.OnSizeChanged(sender, e); }
		private void MWOnRender(TimeSpan delta) { _mw.OnRender(delta); }
		private void MWOnUnload(object sender, RoutedEventArgs e) { _mw.OnUnload(sender, e); }

		internal void Display(ModelAsset mdl)
		{
			_mw.LoadModel(mdl);
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
		private void MWOnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.F)
			{
				YAQMVApp.App.Focus();
			}
		}

		private void MenuFileQuit(object sender, RoutedEventArgs e)
		{
			Close();
		}
		private void MenuFileOpen(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "Quake model files (*.mdl)|*.mdl|All files (*.*)|*.*";
			if (openFileDialog.ShowDialog() == true)
			{
				YAQMVApp.App.LoadAsset(openFileDialog.FileName);
			}
		}
	}
}