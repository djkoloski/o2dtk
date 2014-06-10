using System.Collections;

namespace o2dtk
{
	namespace Utility
	{
		public class TypeInfo
		{
			// Determines if the given class implements the given interface
			public static bool ClassImplementsInterface(System.Type class_, System.Type interface_)
			{
				System.Type[] infs = class_.GetInterfaces();
				foreach (System.Type inf in infs)
					if (inf == interface_)
						return true;
				return false;
			}
		}
	}
}
