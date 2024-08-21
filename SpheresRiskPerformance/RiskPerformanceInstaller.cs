using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;

using EFS.ServiceInstall;
using EFS.ACommon;
using EFS.SpheresService;

namespace EFS.SpheresRiskPerformance
{
    /// <summary>
    /// Installer for the risk performance service
    /// </summary>
    /// <remarks>
    /// The deploy project linked to the installer DOES NOT provide the forms that include input parameters to customize the message queue and the 
    /// other installation custom values. It will use instead the ISpheresServiceParameters services as defined in the RiskPerformanceService class.
    /// </remarks>
    [RunInstaller(true)]
    public class RiskPerformanceInstaller : SpheresServiceInstaller
    {

        /// <summary>
        /// Class constructor used by the MSI installation process
        /// </summary>
        public RiskPerformanceInstaller()
            : base(Cst.ServiceEnum.SpheresRiskPerformance, typeof(RiskPerformanceInstaller))
        {

        }

        /// <summary>
        /// Add the class type of the service in the context parameters collection, to make the service multi-instance compatible.
        /// </summary>
        /// <param name="stateServer"></param>
        public override void Install(System.Collections.IDictionary stateServer)
        {
            this.Context.Parameters.Add(ServiceKeyEnum.ClassType.ToString(), typeof(RiskPerformanceService).FullName);

            base.Install(stateServer);
        }
    }
}
