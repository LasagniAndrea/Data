using System.ComponentModel;
using System.Runtime.InteropServices;
//
using EFS.SpheresService;
using EFS.ACommon;

namespace EFS.ServiceInstall
{
    [RunInstaller(true)]
    [ComVisible(false)]
    public partial class LoggerInstaller : SpheresServiceInstaller
    {
        public LoggerInstaller()
            : base(Cst.ServiceEnum.SpheresLogger, typeof(LoggerInstaller)) { }

        public override void Install(System.Collections.IDictionary stateServer)
        {
            this.Context.Parameters.Add(ServiceKeyEnum.ClassType.ToString(), typeof(EFS.SpheresService.LoggerService).FullName);

            base.Install(stateServer);
        }
    }
}
