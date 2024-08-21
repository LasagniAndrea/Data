using System.ComponentModel;
using System.Runtime.InteropServices;
using EFS.SpheresService;  
using EFS.ACommon;
using System;
using System.Reflection;

namespace EFS.ServiceInstall
{

    // EG 20180423 Analyse du code Correction [CA1405]
    [RunInstaller(true)]
    [ComVisible(false)]
	public class AccountGenInstaller : SpheresServiceInstaller
	{
        public AccountGenInstaller()
            : base(Cst.ServiceEnum.SpheresAccountGen, typeof(AccountGenInstaller))
        {
        }


        public override void Install(System.Collections.IDictionary stateServer)
        {
            // 20100622 MF - Adding additional installation arguments for the "multiple instances" functionality
            this.Context.Parameters.Add(ServiceKeyEnum.ClassType.ToString(), typeof(EFS.SpheresService.AccountGenService).FullName);

            base.Install(stateServer);
        }
	}
}
