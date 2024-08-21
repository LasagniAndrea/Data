using System.ComponentModel;
using System.Runtime.InteropServices;
using EFS.SpheresService;   
using EFS.ACommon;

namespace EFS.ServiceInstall
{
    // EG 20180423 Analyse du code Correction [CA1405]
    [RunInstaller(true)]
    [ComVisible(false)]
	public class ConfMsgGenInstaller : SpheresServiceInstaller
	{
		public ConfMsgGenInstaller() : base(
			Cst.ServiceEnum.SpheresConfirmationMsgGen,typeof(ConfMsgGenInstaller)){}

        public override void Install(System.Collections.IDictionary stateServer)
        {
            // 20100622 MF - Adding additional installation arguments for the "multiple instances" functionality
            this.Context.Parameters.Add(ServiceKeyEnum.ClassType.ToString(), typeof(EFS.SpheresService.SpheresConfirmationMsgGenService).FullName);

            base.Install(stateServer);
        }
	}
}
