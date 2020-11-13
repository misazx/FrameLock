using System.Collections.Generic;

namespace UnityFrame{
	public interface IClip
	{
		string name{ get; set;}

		List<ClipEvent> clipEvents {get;}

		IClip New();

		IClip UF_Clone();
	}
}
