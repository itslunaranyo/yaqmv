using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Mathematics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace yaqmv
{
    internal static class Camera
    {
		private static float _pitch = 0.32f;
		private static float _yaw = -0.64f;
		private static float _panh = 0;
		private static float _panv = 8;
		private static float _range = 96;

		private static float _sens_orbit = 0.025f;
		private static float _sens_pan = 0.4f;
		private static float _sens_dolly = 2f;
		private static float _sensitivity = 1f;

		public static void Reset()
		{
			_pitch = 0.3f;
			_yaw = -0.6f;
			_panh = 0;
			_panv = 8;
			_range = 100;
		}

		public static void Orbit(float turnh, float turnv)
		{
			_pitch = MathF.Min(MathF.Max(_pitch + turnv * _sensitivity * _sens_orbit, -1.57f), 1.57f);
			_yaw = (_yaw + turnh * _sensitivity * _sens_orbit) % (2 * MathF.PI);
		}
		public static void Pan(float slideh, float slidev)
		{
			_panh -= slideh * _sensitivity * _sens_pan;
			_panv += slidev * _sensitivity * _sens_pan;
		}
		public static void Dolly(float amt)
		{
			_range = MathF.Min(MathF.Max(_range + amt * _sensitivity * _sens_dolly, 8f), 512f);
		}

		public static Vector3 Origin {
			get {
				return Focus + new Vector3(_range * MathF.Cos(_pitch), 0, _range * MathF.Sin(_pitch));
			}
		}
		public static Vector3 Focus {
			get {
				return new Vector3(0, _panh, _panv);
			}
		}
		public static float Yaw { get { return _yaw; } }
	}
}
