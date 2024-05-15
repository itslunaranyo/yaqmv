using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace yaqmv
{
    internal struct ModelState
    {
		internal int Skin;
		internal int Frame;
		internal float FrameLerp;
		public ModelState()
		{
			Skin = 0;
			Frame = 0;
			FrameLerp = 0f;
		}
	}
}
