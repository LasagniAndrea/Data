using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

using EFS.SpheresService;   

namespace EFS.ServiceInstall
{
	/// <summary>
	/// Description résumée de EventInstaller.
	/// </summary>
	[RunInstaller(true)]
	public class EventsGenInstaller : Installer
	{
		private ServiceInstaller        serviceInstaller;
		private ServiceProcessInstaller processInstaller;
		private Container               components = null;

		public EventsGenInstaller()
		{
			InitializeComponent();
		}

		protected override void Dispose( bool disposing )
		{
			if (disposing && (null !=components) )
				components.Dispose();
			base.Dispose( disposing );
		}


		#region Code généré par le Concepteur de composants
		/// <summary>
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		private void InitializeComponent()
		{
			this.processInstaller = new ServiceProcessInstaller();
			this.serviceInstaller = new ServiceInstaller();
			// 
			// processInstaller
			// 
			this.processInstaller.Account  = ServiceAccount.LocalSystem;
			this.processInstaller.Password = null;
			this.processInstaller.Username = null;
			// 
			// serviceInstaller
			// 
			this.serviceInstaller.ServiceName = "SpheresEventsGen";
			this.serviceInstaller.DisplayName = this.serviceInstaller.ServiceName;
			this.serviceInstaller.StartType   = ServiceStartMode.Automatic;
			this.Installers.Add(serviceInstaller);
			this.Installers.Add(processInstaller);
		}
		#endregion

		protected override void OnBeforeInstall(IDictionary savedState)
		{
			base.OnBeforeInstall(savedState);
			string name = Context.Parameters["Name"];
			if (null != name)
			{
				serviceInstaller.ServiceName = name;
				serviceInstaller.DisplayName = name;
			}
		}
		protected override void OnBeforeUninstall(IDictionary savedState)
		{
			base.OnBeforeUninstall(savedState);
			string name = Context.Parameters["Name"];
			if (null != name)
			{
				serviceInstaller.ServiceName = name;
				serviceInstaller.DisplayName = name;
			}
		}

		public override void Install(IDictionary stateServer)
		{
			base.Install(stateServer);
			ServiceTools.SetRegistryServiceInformations(this.serviceInstaller,this.Context.Parameters);
			ServiceTools.SetEventLogService(this.serviceInstaller.ServiceName,"AppOTCml",this.serviceInstaller.ServiceName + "_L");
		}

		public override void Uninstall(IDictionary stateServer)
		{
			base.Uninstall(stateServer);
			ServiceTools.DeleteEventLogService("AppOTCml",this.serviceInstaller.ServiceName + "_L");
		}
	}
}
