using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace yaqmv
{
    internal static class Camera3D
    {
		private static float _pitch = 0.32f;
		private static float _yaw = -0.64f;
		private static float _panH = 0;
		private static float _panV = 8;
		private static float _range = 96;

		private static float _sensOrbit = 0.025f;
		private static float _sensPan = 0.0033f;
		private static float _sensDolly = 2f;
		private static float _sensitivity = 1f;

		internal static void Reset(Vector3 center, float radius)
		{
			_pitch = 0.3f;
			_yaw = -0.6f;
			_panH = 0;
			_panV = center.Z;
			_range = radius * 1.2f;
		}
		internal static void Recenter(Vector3 center, float radius)
		{
			_panH = 0;
			_panV = center.Z;
			_range = radius * 1.2f;
		}

		internal static void Orbit(float turnh, float turnv)
		{
			_pitch = MathF.Min(MathF.Max(_pitch + turnv * _sensitivity * _sensOrbit, -1.57f), 1.57f);
			_yaw = (_yaw + turnh * _sensitivity * _sensOrbit) % (2 * MathF.PI);
		}
		internal static void Pan(float slideh, float slidev)
		{
			_panH -= slideh * _sensitivity * _sensPan * _range;
			_panV += slidev * _sensitivity * _sensPan * _range;
		}
		internal static void Dolly(float amt)
		{
			_range = MathF.Min(MathF.Max(_range + amt * _sensitivity * _sensDolly, 8f), 2048f);
		}

		internal static Vector3 Origin {
			get {
				return Focus + new Vector3(_range * MathF.Cos(_pitch), 0, _range * MathF.Sin(_pitch));
			}
		}
		internal static Vector3 Focus {
			get {
				return new Vector3(0, _panH, _panV);
			}
		}
		internal static float Yaw { get { return _yaw; } }
	}
}
