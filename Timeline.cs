using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace yaqmv
{
	/// <summary>
	/// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
	///
	/// Step 1a) Using this custom control in a XAML file that exists in the current project.
	/// Add this XmlNamespace attribute to the root element of the markup file where it is 
	/// to be used:
	///
	///     xmlns:yaqmv="clr-namespace:yaqmv"
	///
	///
	/// Step 1b) Using this custom control in a XAML file that exists in a different project.
	/// Add this XmlNamespace attribute to the root element of the markup file where it is 
	/// to be used:
	///
	///     xmlns:yaqmv="clr-namespace:yaqmv;assembly=yaqmv"
	///
	/// You will also need to add a project reference from the project where the XAML file lives
	/// to this project and Rebuild to avoid compilation errors:
	///
	///     Right click on the target project in the Solution Explorer and
	///     "Add Reference"->"Projects"->[Browse to and select this project]
	///
	///
	/// Step 2)
	/// Go ahead and use your control in the XAML file.
	///
	///     <yaqmv:Timeline/>
	///
	/// </summary>
	public class Timeline : RangeBase
    {
		static Timeline()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata(typeof(Timeline)));

			FrameworkPropertyMetadata valuePropertyMeta = new FrameworkPropertyMetadata();
			valuePropertyMeta.AffectsRender = true;
			ValueProperty.OverrideMetadata(typeof(Timeline), valuePropertyMeta);

			valuePropertyMeta = new FrameworkPropertyMetadata();
			valuePropertyMeta.AffectsRender = true; 
			MinimumProperty.OverrideMetadata(typeof(Timeline), valuePropertyMeta);

			valuePropertyMeta = new FrameworkPropertyMetadata();
			valuePropertyMeta.AffectsRender = true; 
			MaximumProperty.OverrideMetadata(typeof(Timeline), valuePropertyMeta);
		}

		private void HandleHit(double px)
		{
			int len = (int)(Maximum - Minimum) + 1;
			double step = ActualWidth / len;

			double hit = px / step;
			Value = Math.Min(Maximum, Minimum + (int)hit);
		}

		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			Point p = e.GetPosition(this);
			HandleHit(p.X);
		}
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (Mouse.LeftButton == MouseButtonState.Pressed)
			{
				Point p = e.GetPosition(this);
				HandleHit(p.X);
			}
		}

		protected override void OnRender(DrawingContext dc)
		{
			base.OnRender(dc);

			// background
			SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(96,96,96));
			Pen pen = new Pen(Brushes.Black, 1);
			dc.DrawRectangle(brush, null, new Rect(0, 0, ActualWidth, ActualHeight));

			int len = (int)(Maximum - Minimum) + 1;
			double step = ActualWidth / len;

			// yellow current frame
			if (len > 1)
			{
				brush = new SolidColorBrush(Color.FromRgb(220, 192, 0));
				Rect frame = new Rect((int)(step * (Value - Minimum)), 0, (int)(step), ActualHeight);
				dc.DrawRectangle(brush, null, frame);
			}

			// draw notches
			if (len > 0)
			{
				double hh = ActualHeight * 0.5f - 0.5f;
				int offset = 0;
				for (int i = 1; i < len; i++)
				{
					offset = (int)(step * i);
					dc.DrawLine(pen, new Point(offset - 0.5f, hh), new Point(offset - 0.5f, ActualHeight));
				}
			}

			// final border
			dc.DrawRectangle(null, pen, new Rect(0.5f, 0.5f, ActualWidth-1, ActualHeight-1));
		}
	}
}
