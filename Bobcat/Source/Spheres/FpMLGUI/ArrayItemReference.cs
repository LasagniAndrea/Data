using System;
using System.Collections;

namespace EFS.GUI
{
	#region public class FpMLItemReference
	public class FpMLItemReference
	{
		#region member
		public string code;
		public string Value;
		public string ExtValue;
		#endregion member

		#region constructor
		public FpMLItemReference(string pCode,string pValue)
		{
			code = pCode;
			Value = pValue;
		}

		public FpMLItemReference(string pCode,string pValue,string pExtValue)
		{
			code     = pCode;
			Value    = pValue;
			ExtValue = pExtValue;
		}
		#endregion constructor
	}
	#endregion FpMLItemReference
	
	#region public class CompareFpMLItemReference
	public class CompareFpMLItemReference : IComparer
	{
		public int Compare (object pItemReference1, object pItemReference2)
		{
			FpMLItemReference itemReference1 = (FpMLItemReference) pItemReference1;
			FpMLItemReference itemReference2 = (FpMLItemReference) pItemReference2;
			if (pItemReference1==null || pItemReference2==null)	
				return 0;
			else
			{
				string s1 = itemReference1.code + "_" + itemReference1.Value;
				string s2 = itemReference2.code + "_" + itemReference2.Value;
				return (s1.CompareTo(s2));
			}
		}
	}
	#endregion FpMLItemReference
}
