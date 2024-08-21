#region Using Directives
using System;

using EFS.ACommon;
using EFS.EFSTools;
using EFS.EFSTools.MQueue;
using EFS.Process;

#endregion Using Directives

namespace EFS.SpheresService
{
	public class SpheresAccrualsGenService : SpheresServiceBase
	{
		#region protected serviceEnum
		protected override Cst.ServiceEnum serviceEnum
		{
			get{return  Cst.ServiceEnum.SpheresAccrualsGen;} 
		}
		#endregion

		#region Constructor
		public SpheresAccrualsGenService() : base(){}
		public SpheresAccrualsGenService(string pServiceName) : base(pServiceName){}
		#endregion Constructor
		
		#region Main [principal process entry] 
		static void Main(string[] args)
		{
			#if (DEBUG)
				SpheresAccrualsGenService test = new SpheresAccrualsGenService(SpheresServiceBase.GetServiceKeyName(Cst.ServiceEnum.SpheresAccrualsGen));
				test.ActivateService(); 
			#else
				System.ServiceProcess.ServiceBase[] ServicesToRun;
				if ((null != args)&& (0 < args.Length))
					ServicesToRun = new System.ServiceProcess.ServiceBase[] { new SpheresAccrualsGenService(args[0].Substring(2)) };
				else
					ServicesToRun = new System.ServiceProcess.ServiceBase[] { new SpheresAccrualsGenService(SpheresServiceBase.GetServiceKeyName(Cst.ServiceEnum.SpheresAccrualsGen)) };
				System.ServiceProcess.ServiceBase.Run(ServicesToRun);
			#endif
		}
		#endregion Main [principal process entry] 
		
		#region Process
		protected override LevelStatus ExecuteProcess()
		{
			try
			{
				//EventsValProcess eventsValProcess = new EventsValProcess(this.MQueue,this.AppInstance);
				//return eventsValProcess.LevelStatus;
				AccrualsGenProcess accrualsGenProcess = new AccrualsGenProcess(this.MQueue,this.AppInstance);
				return accrualsGenProcess.LevelStatus;
			}
			catch(OTCmlException ex ){throw ex;}
			catch(Exception ex) {throw new OTCmlException("ExecuteProcess",ex);}
		}
		#endregion Process
	}
}
