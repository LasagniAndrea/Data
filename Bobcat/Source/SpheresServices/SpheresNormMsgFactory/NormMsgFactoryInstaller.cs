using System.ComponentModel;
using System.Runtime.InteropServices;
using EFS.SpheresService;  
using EFS.ACommon;

namespace EFS.ServiceInstall
{
    [RunInstaller(true)]
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class NormMsgFactoryInstaller : SpheresServiceInstaller
    {
        public NormMsgFactoryInstaller()
            : base(
            Cst.ServiceEnum.SpheresNormMsgFactory, typeof(NormMsgFactoryInstaller)) { }

        public override void Install(System.Collections.IDictionary stateServer)
        {
            this.Context.Parameters.Add(ServiceKeyEnum.ClassType.ToString(), typeof(EFS.SpheresService.SpheresNormMsgFactoryService).FullName);
            base.Install(stateServer);
        }
    }
}
