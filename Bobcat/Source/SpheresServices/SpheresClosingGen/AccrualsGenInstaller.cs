using System.ComponentModel;
using EFS.SpheresService;   
using EFS.ACommon;   

namespace EFS.ServiceInstall
{
	/// <summary>
	/// Description r�sum�e de AccrualGenInstaller.
	/// </summary>
	
	[RunInstaller(true)]
	public class AccrualGenInstaller : SpheresServiceInstaller
	{
		public AccrualGenInstaller() : base(
			Cst.ServiceEnum.SpheresAccrualsGen,
			SpheresServiceBase.GetServiceKeyName(Cst.ServiceEnum.SpheresAccrualsGen)){}
	}
}
