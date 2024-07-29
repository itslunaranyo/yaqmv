using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using OpenTK.Mathematics;

namespace yaqmv
{
	internal static class Camera2D
	{
		private static float _panH = 0;
		private static float _panV = 0;
		private static float _zoom = 1f;

		private static float _sensitivity = 1f;

		internal static void Reset()
		{
			_panH = 0;
			_panV = 0;
			_zoom = 1f;
		}
		internal static void Recenter()
		{
			_panH = 0;
			_panV = 0;
			_zoom = 1f;
		}
		internal static void Pan(float slideh, float slidev)
		{
			_panH += slideh * _sensitivity / _zoom;
			_panV += slidev * _sensitivity / _zoom;
		}

		internal static void ZoomIn() { _zoom += 0.5f; }
		internal static void ZoomOut() { _zoom = MathF.Max(_zoom - 0.5f, 0.5f); }
		internal static float Zoom { get { return _zoom; } }

		internal static Vector3 Origin { get { return new Vector3(_panH, _panV, -2); } }
		internal static Vector3 Focus { get { return new Vector3(_panH, _panV, 2); } }
	}
}
