using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;

using EFS.ACommon;
using EFS.Common;

using NetworkManagement;

namespace EFS.SpheresService
{
    public class Manager
    {
        #region OnEvent
        /*
		static void OnEvent( object sender, EntryWrittenEventArgs args )
		{
			Console.WriteLine( "EVENT> Someone has written to the log!" );
		}
        */
        #endregion OnEvent


        #region Members
        // EG 20140425 Gestion Custom.config
        public static string servicesManagerCustomConfigSource = "\\servicesManagerCustom.config";
        private static NotifyIcon notifyIcon;
        private static bool isSpheresServicesOpened;
        private static System.Timers.Timer timer;
        private static ServiceController currentService;
        private static FrmSpheresServices frmManager;
        private static ContextMenuStrip contextMenu;

        private static string currentServer;
        private static int interval;
        private static string currentServiceName;
        private static string currentServiceEnum;
        private static string currentServiceVersion;
        private static bool isControlService;
        private static bool isWaitForStatusPending;

        private static ImageList imgStatusService;
        private static ImageList imlContextMenu;
        public static ServiceController[] services;
        public static Dictionary<string, List<Pair<Pair<string, Cst.ServiceEnum>, ServiceController>>> dicServices;

        #endregion Members


        #region Accessors
        #region CurrentServer
        public static string CurrentServer
        {
            set { currentServer = value; }
            get { return currentServer; }
        }
        #endregion CurrentServer
        #region CurrentServiceVersion
        public static string CurrentServiceVersion
        {
            set { currentServiceVersion = value; }
            get { return currentServiceVersion; }
        }
        #endregion CurrentServiceVersion
        #region CurrentService
        // EG 20120629 TODO
        public static ServiceController CurrentService
        {
            set
            {
                if (currentServiceName != value.ServiceName)
                {
                    currentService = value;
                    currentServiceName = value.ServiceName;
                }
            }
            get
            {
                return currentService;
            }
        }
        #endregion CurrentService
        #region CurrentServiceName
        public static string CurrentServiceName
        {
            get { return currentServiceName; }
        }
        #endregion CurrentServiceName
        #region CurrentServiceEnum
        public static string CurrentServiceEnum
        {
            set { currentServiceEnum = value; }
            get { return currentServiceEnum; }
        }
        #endregion CurrentServiceEnum
        #region Icons
        private static Icon Icons_Manager
        {
            get
            {
                try { return Icon.FromHandle(((Bitmap)imgStatusService.Images[0]).GetHicon()); }
                catch { return null; }
            }
        }
        private static Icon Icons_Start
        {
            get
            {
                try { return Icon.FromHandle(((Bitmap)imgStatusService.Images[1]).GetHicon()); }
                catch { return null; }
            }
        }
        private static Icon Icons_Pause
        {
            get
            {
                try { return Icon.FromHandle(((Bitmap)imgStatusService.Images[2]).GetHicon()); }
                catch { return null; }
            }
        }
        private static Icon Icons_Stop
        {
            get
            {
                try { return Icon.FromHandle(((Bitmap)imgStatusService.Images[3]).GetHicon()); }
                catch { return null; }
            }
        }
        #endregion Icons
        #region Interval
        public static int Interval
        {
            get { return interval; }
            set { interval = value; }
        }
        #endregion Interval
        #region IsControlService
        public static bool IsControlService
        {
            get { return isControlService; }
            set { isControlService = value; }
        }
        #endregion IsControlService
        #region IsWaitForStatusPending
        public static bool IsWaitForStatusPending
        {
            get { return isWaitForStatusPending; }
            set { isWaitForStatusPending = value; }
        }
        #endregion IsWaitForStatusPending
        #region AssemblyVersion
        public static string AssemblyVersion
        {
            get
            {
                Version assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                return "v" + assemblyVersion.Major.ToString() + "." + assemblyVersion.Minor + "." + assemblyVersion.Build;
            }
        }
        #endregion AssemblyVersion
        #endregion Accessors

        #region Main
        static void Main()
        {
            try
            {
                Ressource.ExternalRessource = new ResourceManager("ServicesManager.ServicesManager", Assembly.Load("ServicesManager"));

                GetIconsStatusService();
                GetContextMenuIcons();
                GetConfigValues();

                SetUpTimer();
                
                notifyIcon = new NotifyIcon
                {
                    Visible = true,
                    Icon = Icons_Manager,
                    
                };
                notifyIcon.Click += new EventHandler(OnOpenServices);
                

                dicServices = InitializeServices(currentServer);
                if (null != dicServices)
                    BindControlMenu(dicServices);

                Application.Run();
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] DisplayError d'une exception
                DisplayError(ex);
            }
        }
        #endregion Main

        #region Events
        #region OnAbout
        private static void OnAbout(object sender, System.EventArgs e)
        {
            try
            {
                string title = Ressource.GetString("Title");
                StringBuilder sb = new StringBuilder();
                sb.Append(title + Environment.NewLine);
                sb.Append("_________________________________________________" + Environment.NewLine + Environment.NewLine);
                sb.Append(Software.CopyrightWithYear + Environment.NewLine + Environment.NewLine);
                sb.AppendFormat("{0} ({1}-bit / {2} CPU){3}", Environment.OSVersion.ToString(), (Environment.Is64BitOperatingSystem ? "64" : "32"), Environment.ProcessorCount.ToString(), Environment.NewLine);
                sb.AppendFormat("Common Langage Runtime {0}", Environment.Version.ToString(), Environment.NewLine);
                MessageBox.Show(sb.ToString(), "About " + title, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] DisplayError d'une exception
                DisplayError(ex);
            }
        }

        #endregion OnAbout
        #region OnExit
        private static void OnExit(object sender, System.EventArgs e)
        {
            try
            {
                if (null != timer)
                {
                    timer.Stop();
                    timer.Dispose();
                }
                notifyIcon.Visible = false;
                notifyIcon.Dispose();
                Application.Exit();
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] DisplayError d'une exception
                DisplayError(ex);
            }
        }

        #endregion OnExit
        #region OnEventLog
        private static void OnEventLog(object sender, System.EventArgs e)
        {
            try
            {
                FrmEventLog frmEventLog = new FrmEventLog
                {
                    TopLevel = true
                };
                if (null != frmEventLog)
                    frmEventLog.ShowDialog(frmManager);
                else
                    frmEventLog.ShowDialog();
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] DisplayError d'une exception
                DisplayError(ex);
            }
        }
        #endregion OnEventLog

        #region OnOpenServices
        /// <summary>
        /// Ouverture de la forme du Manager de services
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// EG 20210824 [XXXXX] Déplacement ligne de valorisation isSpheresServicesOpened = true
        private static void OnOpenServices(object sender, System.EventArgs e)
        {
            try
            {
                if (false == isSpheresServicesOpened)
                {
                    Manager.GetConfigValues();
                    isSpheresServicesOpened = true;
                    frmManager = new FrmSpheresServices
                    {
                        TopLevel = true
                    };
                    frmManager.ShowDialog();
                    isSpheresServicesOpened = false;

                }
                else if (null != frmManager)
                {
                    frmManager.WindowState = FormWindowState.Normal;
                    frmManager.Visible = true;
                }
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] DisplayError d'une exception
                DisplayError(ex);
            }
        }
        #endregion OnOpenServices
        #region OnParam
        private static void OnParam(object sender, System.EventArgs e)
        {
            try
            {
                FrmManagerOptions frmManagerOptions = new FrmManagerOptions
                {
                    TopLevel = true
                };
                if (null != frmManager)
                    frmManagerOptions.ShowDialog(frmManager);
                else
                    frmManagerOptions.ShowDialog();
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] DisplayError d'une exception
                DisplayError(ex);
            }
        }
        #endregion OnParam
        #region OnPause
        public static void OnPause(object sender, System.EventArgs e)
        {
            try
            {
                if ((ServiceControllerStatus.Paused != currentService.Status) && (currentService.CanPauseAndContinue))
                {
                    DialogResult response = DialogResult.Yes;
                    if (isControlService)
                    {
                        string message = Ressource.GetString2("ControlPauseService", currentServiceName, currentServer);
                        response = MessageBox.Show(message, "Pause service", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    }
                    if (DialogResult.Yes == response)
                    {
                        DisplayMessage("Wait...");
                        currentService.Pause();
                        currentService.Refresh();
                        if (isWaitForStatusPending)
                        {
                            try { currentService.WaitForStatus(ServiceControllerStatus.PausePending, TimeSpan.FromSeconds(1)); }
                            catch (System.ServiceProcess.TimeoutException) { currentService.WaitForStatus(ServiceControllerStatus.Paused); }
                            catch (Exception ex)
                            {
                                // FI 20200910 [XXXXX] DisplayError d'une exception
                                DisplayError(ex);
                            }
                        }
                        else
                        {
                            currentService.WaitForStatus(ServiceControllerStatus.Paused);
                        }
                        RefreshFrmManager();
                    }
                }
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] DisplayError d'une exception
                DisplayError(ex);
            
            }
        }
        #endregion OnPause
        #region OnPauseAll
        public static void OnPauseAll(object sender, System.EventArgs e)
        {
            DialogResult response = DialogResult.Yes;
            if (isControlService)
            {
                string message = Ressource.GetString2("ControlPauseAllService", currentServer);
                response = MessageBox.Show(message, "Pause all services", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }
            if (DialogResult.Yes == response)
            {
                DisplayMessage("Wait...");
                if ((null != dicServices) && dicServices.ContainsKey(currentServiceVersion))
                {
                    dicServices[currentServiceVersion].ForEach(item =>
                    {
                        try
                        {
                            if ((ServiceControllerStatus.Paused != item.Second.Status) && (item.Second.CanPauseAndContinue))
                            {
                                item.Second.Pause();
                                item.Second.Refresh();
                                if (isWaitForStatusPending)
                                {
                                    try { item.Second.WaitForStatus(ServiceControllerStatus.PausePending, TimeSpan.FromSeconds(1)); }
                                    catch (System.ServiceProcess.TimeoutException) { item.Second.WaitForStatus(ServiceControllerStatus.Paused); }
                                    catch (Exception ex)
                                    {
                                        // FI 20200910 [XXXXX] DisplayError d'une exception
                                        DisplayError(ex);
                                    }
                                }
                                else
                                    item.Second.WaitForStatus(ServiceControllerStatus.Paused);
                            }
                        }
                        catch (Exception ex)
                        {
                            // FI 20200910 [XXXXX] DisplayError d'une exception
                            DisplayError(ex);
                        }
                    }
                    );
                    RefreshFrmManager();
                }
            }
        }
        #endregion OnPauseAll
        #region OnSetService
        // EG 20140425 CustomServiceEnum setting change
        public static void OnSetService(object sender, System.EventArgs e)
        {
            try
            {
                if (sender is ToolStripMenuItem mnuService)
                {
                    if (mnuService.OwnerItem is ToolStripMenuItem mnuVersion)
                    {
                        if ((null != dicServices) && dicServices.ContainsKey(mnuVersion.Name))
                        {
                            Pair<Pair<string, Cst.ServiceEnum>, ServiceController> service =
                                dicServices[mnuVersion.Name].Find(match => match.First.First.ToString() == mnuService.Text);
                            if (null != service)
                            {
                                CurrentServiceVersion = mnuVersion.Text;
                                CurrentService = service.Second;
                                CurrentServiceEnum = service.First.First.ToString();
                                if (isSpheresServicesOpened && (null != frmManager))
                                {
                                    frmManager.SetVersion(mnuVersion.Text);
                                    frmManager.SetService(mnuService.Text);
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] DisplayError d'une exception
                DisplayError(ex);
            }
        }
        #endregion OnSetService

        #region OnStartContinue
        public static void OnStartContinue(object sender, System.EventArgs e)
        {
            try
            {
                DialogResult response = DialogResult.Yes;
                if (isControlService)
                {
                    string message = Ressource.GetString2("ControlStartService", currentServiceName, currentServer);
                    response = MessageBox.Show(message, "Start service", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                }
                if (DialogResult.Yes == response)
                {
                    DisplayMessage("Wait...");
                    if (ServiceControllerStatus.Stopped == Manager.CurrentService.Status)
                    {
                        currentService.Start();
                        currentService.Refresh();
                        if (isWaitForStatusPending)
                        {
                            try { currentService.WaitForStatus(ServiceControllerStatus.StartPending, TimeSpan.FromSeconds(1)); }
                            catch (System.ServiceProcess.TimeoutException) { currentService.WaitForStatus(ServiceControllerStatus.Running); }
                            catch (Exception ex)
                            {
                                // FI 20200910 [XXXXX] DisplayError d'une exception
                                DisplayError(ex);
                            }
                        }
                        else
                            currentService.WaitForStatus(ServiceControllerStatus.Running);
                    }
                    else if (ServiceControllerStatus.Paused == Manager.CurrentService.Status)
                    {
                        currentService.Continue();
                        currentService.Refresh();
                        if (isWaitForStatusPending)
                        {
                            try { currentService.WaitForStatus(ServiceControllerStatus.ContinuePending, TimeSpan.FromSeconds(1)); }
                            catch (System.ServiceProcess.TimeoutException) { currentService.WaitForStatus(ServiceControllerStatus.Running); }
                            catch (Exception ex)
                            {
                                // FI 20200910 [XXXXX] DisplayError d'une exception
                                DisplayError(ex);
                            }
                        }
                        else
                        {
                            currentService.WaitForStatus(ServiceControllerStatus.Running);
                        }
                    }
                    RefreshFrmManager();
                }
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] DisplayError d'une exception            
                DisplayError(ex);
            }
        }
        #endregion OnStartContinue
        #region OnStartAllContinue
        // EG 20210824 [XXXXX] Utilisation du ContinuePending ou StartPending pour WaitForStatus en fonction du statut en cours du service
        public static void OnStartAllContinue(object sender, System.EventArgs e)
        {
            DialogResult response = DialogResult.Yes;
            if (isControlService)
            {
                string message = Ressource.GetString2("ControlStartAllService", currentServer);
                response = MessageBox.Show(message, "Start all services", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }
            if (DialogResult.Yes == response)
            {
                DisplayMessage("Wait...");
                if ((null != dicServices) && dicServices.ContainsKey(currentServiceVersion))
                {
                    // PM 2020117 [XXXXX] Démmarrer le logger en premier
                    List<Pair<Pair<string, Cst.ServiceEnum>, ServiceController>> servicesOfVersion = dicServices[currentServiceVersion].Where(i => i.First.Second != Cst.ServiceEnum.SpheresLogger).ToList();
                    List<Pair<Pair<string, Cst.ServiceEnum>, ServiceController>> servicesLoggerOfVersion = dicServices[currentServiceVersion].Where(i => i.First.Second == Cst.ServiceEnum.SpheresLogger).ToList();
                    if (servicesLoggerOfVersion.Count() > 0)
                    {
                        servicesLoggerOfVersion.AddRange(servicesOfVersion);
                        servicesOfVersion = servicesLoggerOfVersion;
                    }
                    ServiceControllerStatus status = ServiceControllerStatus.ContinuePending;
                    servicesOfVersion.ForEach(item =>
                    //dicServices[currentServiceVersion].ForEach(item =>
                    {
                        try
                        {
                            bool isWait = false;
                            if (ServiceControllerStatus.Stopped == item.Second.Status)
                            {
                                status = ServiceControllerStatus.StartPending;
                                item.Second.Start();
                                item.Second.Refresh();
                                isWait = true;
                            }
                            else if (ServiceControllerStatus.Paused == item.Second.Status)
                            {
                                status = ServiceControllerStatus.ContinuePending;
                                item.Second.Continue();
                                item.Second.Refresh();
                                isWait = true;
                            }
                            if (isWait)
                            {
                                if (isWaitForStatusPending)
                                {
                                    try { item.Second.WaitForStatus(status, TimeSpan.FromSeconds(1)); }
                                    catch (System.ServiceProcess.TimeoutException) { item.Second.WaitForStatus(ServiceControllerStatus.Running); }
                                    catch (Exception ex)
                                    {
                                        // FI 20200910 [XXXXX] DisplayError d'une exception
                                        DisplayError(ex);
                                    }
                                }
                                else
                                    item.Second.WaitForStatus(ServiceControllerStatus.Running);
                            }
                        }
                        catch (Exception ex)
                        {
                            // FI 20200910 [XXXXX] DisplayError d'une exception                         
                            DisplayError(ex);
                        }
                    }
                    );
                    RefreshFrmManager();
                }
            }
        }
        #endregion OnStartAllContinue
        #region OnStop
        public static void OnStop(object sender, System.EventArgs e)
        {
            try
            {
                if ((ServiceControllerStatus.Stopped != currentService.Status) && (currentService.CanStop))
                {
                    DialogResult response = DialogResult.Yes;
                    if (isControlService)
                    {
                        string message = Ressource.GetString2("ControlStopService", currentServiceName, currentServer);
                        response = MessageBox.Show(message, "Stop service", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    }
                    if (DialogResult.Yes == response)
                    {
                        DisplayMessage("Wait...");
                        currentService.Stop();
                        currentService.Refresh();
                        if (isWaitForStatusPending)
                        {
                            try { currentService.WaitForStatus(ServiceControllerStatus.StopPending, TimeSpan.FromSeconds(1)); }
                            catch (System.ServiceProcess.TimeoutException) { currentService.WaitForStatus(ServiceControllerStatus.Stopped); }
                            catch (Exception ex)
                            {
                                // FI 20200910 [XXXXX] DisplayError d'une exception
                                DisplayError(ex);
                            }
                        }
                        else
                        {
                            currentService.WaitForStatus(ServiceControllerStatus.Stopped);
                        }
                        RefreshFrmManager();
                    }
                }
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] DisplayError d'une exception             
                DisplayError(ex);
            }
        }
        #endregion OnStop
        #region OnStopAll
        // EG 20210824 [XXXXX] L'ensemble des services applicatifs de Spheres dépendent du service Logger
        // un arrêt du service Logger entraine nécessairement un arrêt de ses dépendances.
        // voir documentation :  https://docs.microsoft.com/fr-fr/dotnet/api/system.serviceprocess.servicecontroller.stop?view=dotnet-plat-ext-5.0
        // ceci n'est pas vrai pour START et PAUSE
        public static void OnStopAll(object sender, System.EventArgs e)
        {
            DialogResult response = DialogResult.Yes;
            if (isControlService)
            {
                string message = Ressource.GetString2("ControlStopAllService", currentServer);
                response = MessageBox.Show(message, "Stop all services", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }
            if (DialogResult.Yes == response)
            {
                DisplayMessage("Wait...");
                if ((null != dicServices) && dicServices.ContainsKey(currentServiceVersion))
                {
                    // PM 2020117 [XXXXX] Stopper le logger en dernier
                    List<Pair<Pair<string, Cst.ServiceEnum>, ServiceController>> servicesOfVersion = dicServices[currentServiceVersion].Where(i => i.First.Second != Cst.ServiceEnum.SpheresLogger).ToList();
                    List<Pair<Pair<string, Cst.ServiceEnum>, ServiceController>> servicesLoggerOfVersion = dicServices[currentServiceVersion].Where(i => i.First.Second == Cst.ServiceEnum.SpheresLogger).ToList();
                    if (servicesLoggerOfVersion.Count() > 0)
                    {
                        // EG 20210824 [XXXXX] Upd
                        //servicesOfVersion.AddRange(servicesLoggerOfVersion);
                        servicesOfVersion = servicesLoggerOfVersion;
                    }
                    servicesOfVersion.ForEach(item =>
                    {
                        try
                        {
                            if ((ServiceControllerStatus.Stopped != item.Second.Status) && (item.Second.CanStop))
                            {
                                item.Second.Stop();
                                item.Second.Refresh();
                                if (isWaitForStatusPending)
                                {
                                    try { item.Second.WaitForStatus(ServiceControllerStatus.StopPending, TimeSpan.FromSeconds(1)); }
                                    catch (System.ServiceProcess.TimeoutException) { item.Second.WaitForStatus(ServiceControllerStatus.Stopped); }
                                    catch (Exception ex)
                                    {
                                        // FI 20200910 [XXXXX] DisplayError d'une exception
                                        DisplayError(ex);
                                    }
                                }
                                else
                                    item.Second.WaitForStatus(ServiceControllerStatus.Stopped);
                            }
                        }
                        catch (Exception ex)
                        {
                            // FI 20200910 [XXXXX] DisplayError d'une exception
                            DisplayError(ex);
                        }
                    }
                    );
                    RefreshFrmManager();
                }
            }
        }
        #endregion OnStopAll
        #region OnTimer
        private static void OnTimer(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                GetServiceStatus();
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] DisplayError d'une exception
                DisplayError(ex);
            }
        }
        #endregion OnTimer
        #endregion Events

        #region Methods
        #region CreateMenu
        private static ContextMenuStrip CreateMenu()
        {
            ToolStripMenuItem mnu;
            contextMenu = new ContextMenuStrip();
            try
            {
                // Gestionnaire des services
                mnu = new ToolStripMenuItem(Ressource.GetString("OpenManager"), imlContextMenu.Images[0], new EventHandler(OnOpenServices));
                contextMenu.Items.Add(new ToolStripMenuItem(Ressource.GetString("OpenManager"), imlContextMenu.Images[0], new EventHandler(OnOpenServices)));
                contextMenu.Items.Add("-");
                // Listes des services actuels
                contextMenu.Items.Add(new ToolStripMenuItem(Ressource.GetString2("CurrentService", currentServer, string.Empty, string.Empty)) {Name = currentServer});
                contextMenu.Items.Add("-");
                // Actions sur service en cours
                contextMenu.Items.Add(new ToolStripMenuItem("Stop", imlContextMenu.Images[4], new EventHandler(OnStop)));
                contextMenu.Items.Add(new ToolStripMenuItem("Suspend", imlContextMenu.Images[3], new EventHandler(OnPause)));
                contextMenu.Items.Add(new ToolStripMenuItem("Start", imlContextMenu.Images[2], new EventHandler(OnStartContinue)));
                contextMenu.Items.Add("-");
                contextMenu.Items.Add(new ToolStripMenuItem("Options...", imlContextMenu.Images[5], new EventHandler(OnParam)));
                contextMenu.Items.Add(new ToolStripMenuItem("Logs...", imlContextMenu.Images[6], new EventHandler(OnEventLog)));
                contextMenu.Items.Add(new ToolStripMenuItem("About...", imlContextMenu.Images[7], new EventHandler(OnAbout)));
                contextMenu.Items.Add("-");
                contextMenu.Items.Add(new ToolStripMenuItem("Exit", null, new EventHandler(OnExit)));
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] DisplayError d'une exception
                DisplayError(ex);
            }
            return contextMenu;
        }

        #endregion CreateMenu
        #region CheckCurrentService
        public static void CheckCurrentService()
        {
            if (contextMenu.Items[2] is ToolStripMenuItem mnuServer)
            {
                foreach (ToolStripMenuItem mnuVersion in mnuServer.DropDownItems)
                {
                    if (mnuVersion.Name == currentServiceVersion)
                        mnuVersion.Image = imlContextMenu.Images[1];
                    else
                        mnuVersion.Image = null;

                    foreach (ToolStripMenuItem mnuService in mnuVersion.DropDownItems)
                    {
                        if ((mnuService.Name == CurrentServiceName) && (mnuVersion.Name == currentServiceVersion))
                        {
                            mnuService.Image = imlContextMenu.Images[1];
                            mnuService.Checked = true;
                        }
                        else
                        {
                            mnuService.Checked = false;
                            mnuService.Image = null;
                        }
                    }
                }
            }
        }
        #endregion CheckCurrentService

        #region DisplayError
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        /// FI 20200910 [XXXXX] Add Method
        private static void DisplayError(Exception exception)
        {
            if (isSpheresServicesOpened && (null != frmManager))
                frmManager.DisplayError(exception);
        }
        #endregion DisplayError
        #region DisplayMessage
        private static void DisplayMessage(string pMessage)
        {
            if (isSpheresServicesOpened && (null != frmManager))
                frmManager.SetMessage = pMessage;
        }
        #endregion DisplayError
        #region GetConfigValues
        public static void GetConfigValues()
        {
            try
            {
                StringDictionary settings = ReadConfigValues();
                if (null != settings)
                {
                    interval = Convert.ToInt32(settings["Interval"]);
                    currentServer = settings["DefaultServer"];
                    if (("." == currentServer) || ("local" == currentServer) || StrFunc.IsEmpty(currentServer))
                        currentServer = Environment.MachineName;
                    currentServiceVersion = settings["DefaultVersion"];
                    if (StrFunc.IsEmpty(currentServiceVersion))
                        currentServiceVersion = AssemblyVersion;
                    //currentServiceName = settings["DefaultService"];
                    currentServiceEnum = settings["DefaultService"];
                    isControlService = Convert.ToBoolean(settings["ControlService"]);
                    isWaitForStatusPending = Convert.ToBoolean(settings["WaitForStatusPending"]);
                }
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] DisplayError d'une exception
                DisplayError(ex);
            }
        }
        #endregion GetConfigValues
        #region GetIconsStatusService
        private static void GetIconsStatusService()
        {
            ResourceManager resources = new ResourceManager(typeof(FrmSpheresServices));
            imgStatusService = new ImageList
            {
                ImageSize = new Size(16, 16),
                ImageStream = ((ImageListStreamer)(resources.GetObject("imgStatusService.ImageStream"))),
                TransparentColor = Color.Transparent
            };
        }
        #endregion GetIconsStatusService
        #region GetContextMenuIcons
        private static void GetContextMenuIcons()
        {
            ResourceManager resources = new ResourceManager(typeof(FrmSpheresServices));
            imlContextMenu = new ImageList
            {
                ImageSize = new Size(16, 16),
                ImageStream = ((ImageListStreamer)(resources.GetObject("imlContextMenu.ImageStream"))),
                TransparentColor = Color.Transparent
            };
        }
        #endregion GetIconsStatusService

        #region GetServerNames
        /// <summary>
        /// Recherche et chargement des servers disponibles
        /// </summary>
        /// <param name="pCmbServer"></param>
        public static void GetServerNames(ComboBox pCmbServer)
        {
            GetServerNames(pCmbServer, currentServer);
        }
        public static void GetServerNames(ComboBox pCmbServer, string pServerName)
        {
            pCmbServer.Items.Clear();
            try
            {
                NetServers netServers = new NetServers(ServerTypeEnum.SERVER);
                pCmbServer.Items.Add(Environment.MachineName);
                string[] servers = NetServers.GetServers();
                if (0 < servers.Length)
                {
                    foreach (string server in servers)
                    {
                        if (false == server.Equals(Environment.MachineName))
                            pCmbServer.Items.Add(server);
                    }
                }

                //string[] servers = NetServers.GetServers();
                //if (0 < servers.Length)
                //{
                //    foreach (string server in servers)
                //    {
                //        if (server.Equals(Environment.MachineName))
                //            pCmbServer.Items.Insert(0, server);
                //        else
                //            pCmbServer.Items.Add(server);
                //    }
                //}
                //else
                //    pCmbServer.Items.Add(Environment.MachineName);

                if (0 < pCmbServer.Items.Count)
                {
                    int i = pCmbServer.FindStringExact(pServerName);
                    pCmbServer.SelectedIndex = (-1 < i ? i : 0);
                }
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] DisplayError d'une exception
                DisplayError(ex);
                pCmbServer.SelectedIndex = -1;
            }
        }
        #endregion GetServerNames
        #region BindControlVersion
        /// <summary>
        /// Chargement des versions disponible pour les services Spheres
        /// </summary>
        /// <param name="pDicServices"></param>
        /// <param name="pCmbVersion"></param>
        // EG 20140425 pCmbVersion SelectedIndex change
        public static void BindControlVersion(Dictionary<string, List<Pair<Pair<string, Cst.ServiceEnum>, ServiceController>>> pDicServices, ComboBox pCmbVersion)
        {
            pCmbVersion.Items.Clear();
            if (null != pDicServices)
            {
                foreach (string key in pDicServices.Keys)
                    pCmbVersion.Items.Add(key);
                //int i = pCmbVersion.FindStringExact(AssemblyVersion);
                int i = pCmbVersion.FindStringExact(CurrentServiceVersion);
                if (-1 == i)
                    i = pCmbVersion.FindStringExact(AssemblyVersion);
                pCmbVersion.SelectedIndex = (-1 < i ? i : 0);
            }
        }
        #endregion BindControlVersion
        #region BindControlServices
        /// <summary>
        /// Chargment des Services pour la version demandée
        /// </summary>
        /// <param name="pDicServices"></param>
        /// <param name="pVersion"></param>
        /// <param name="pCmbService"></param>
        // EG 20140425 Upd (pServiceEnum == item.First.First.ToString())
        public static void BindControlServices(Dictionary<string, List<Pair<Pair<string, Cst.ServiceEnum>, ServiceController>>> pDicServices,
            string pVersion, ComboBox pCmbService, string pServiceEnum)
        {
            string currentServiceText = string.Empty;
            ToolStripMenuItem mnuServer = contextMenu.Items[2] as ToolStripMenuItem;
            currentServiceVersion = pVersion;
            pCmbService.Items.Clear();
            if ((null != pDicServices) && pDicServices.ContainsKey(pVersion))
            {
                pDicServices[pVersion].ForEach(item =>
                {
                    pCmbService.Items.Add(item.First.First.ToString());
                    if (StrFunc.IsEmpty(currentServiceName) || (pServiceEnum == item.First.First.ToString()))
                    {
                        currentService = item.Second;
                        currentServiceName = item.Second.ServiceName;
                        currentServiceEnum = item.First.First.ToString();
                        ToolStripMenuItem mnuVersion = mnuServer.DropDownItems[currentServiceVersion] as ToolStripMenuItem;
                        ToolStripMenuItem mnuService = mnuVersion.DropDownItems[currentServiceName] as ToolStripMenuItem;
                        mnuService.Checked = true;
                        mnuService.Image = imlContextMenu.Images[1];
                        mnuServer.Text = Ressource.GetString2("CurrentService", currentServer, currentServiceName);
                    }
                });
                if (0 < pCmbService.Items.Count)
                {
                    int i = pCmbService.FindStringExact(currentServiceEnum);
                    pCmbService.SelectedIndex = (-1 < i ? i : 0);
                }
            }
            RefreshFrmManager();
        }
        #endregion BindControlServices
        #region BindControlMenu
        /// <summary>
        /// Chargement des menus Services pour toutes les versions
        /// </summary>
        /// <param name="pDicServices"></param>
        /// <param name="pVersion"></param>
        /// <param name="pCmbService"></param>
        public static void BindControlMenu(Dictionary<string, List<Pair<Pair<string, Cst.ServiceEnum>, ServiceController>>> pDicServices)
        {
            notifyIcon.ContextMenuStrip = CreateMenu();
            ToolStripMenuItem mnuServer = contextMenu.Items[2] as ToolStripMenuItem;
            mnuServer.DropDown = null;
            if ((null != mnuServer) && (null != pDicServices))
            {
                // Ajout des versions
                ToolStripMenuItem mnuVersion = null;
                ToolStripMenuItem mnuService = null;
                foreach (string key in pDicServices.Keys)
                {
                    mnuVersion = new ToolStripMenuItem(key)
                    {
                        Name = key
                    };
                    mnuServer.DropDownItems.Add(mnuVersion);
                    pDicServices[key].ForEach(item =>
                    {
                        mnuService = new ToolStripMenuItem(item.First.First.ToString(), null, new EventHandler(OnSetService))
                        {
                            Name = item.Second.ServiceName
                        };
                        mnuVersion.DropDownItems.Add(mnuService);
                    });
                    if ((null != mnuVersion) && mnuVersion.HasDropDownItems)
                        ((ToolStripDropDownItem)mnuVersion).DropDownDirection = ToolStripDropDownDirection.Left;
                }
                if (mnuServer.HasDropDownItems)
                    ((ToolStripDropDownItem)mnuServer).DropDownDirection = ToolStripDropDownDirection.Left;
            }
        }
        #endregion BindControlMenu

        #region InitializeServices
        public static Dictionary<string, List<Pair<Pair<string, Cst.ServiceEnum>, ServiceController>>> InitializeServices(string pServerName)
        {
            Dictionary<string, List<Pair<Pair<string, Cst.ServiceEnum>, ServiceController>>> ret = null;
            try
            {
                ServiceController[] _services = ServiceController.GetServices(pServerName);
                if ((null != _services) && (0 < _services.Length))
                {
                    string version = null;
                    string instance = null;
                    string serviceIdentifier = null;
                    foreach (ServiceController _service in _services)
                    {
                        if (SpheresServiceTools.IsSpheresService(_service.ServiceName))
                        {
                            foreach (Cst.ServiceEnum serviceEnum in Enum.GetValues(typeof(Cst.ServiceEnum)))
                            {
                                if (_service.ServiceName.Contains(serviceEnum.ToString()))
                                {
                                    version = _service.ServiceName.Replace(serviceEnum.ToString(), string.Empty);
                                    ServiceInfo serviceInfo = new ServiceInfo(_service.ServiceName);
                                    serviceIdentifier = serviceEnum.ToString();
                                    if (null != serviceInfo)
                                    {
                                        instance = serviceInfo.GetInformationsKey(ServiceKeyEnum.Instance);
                                        if (StrFunc.IsFilled(instance))
                                        {
                                            //serviceIdentifier += " (" + instance + ")";
                                            serviceIdentifier += " - Inst:" + instance;
                                            //version = version.Remove(version.LastIndexOf(instance));
                                            version = version.Remove(version.LastIndexOf("-Inst:" + instance));
                                        }
                                    }

                                    Pair<string, Cst.ServiceEnum> _p1 =
                                        new Pair<string, Cst.ServiceEnum> { First = serviceIdentifier, Second = serviceEnum };
                                    Pair<Pair<string, Cst.ServiceEnum>, ServiceController> service =
                                        new Pair<Pair<string, Cst.ServiceEnum>, ServiceController> { First = _p1, Second = _service };

                                    if (null == ret)
                                        ret = new Dictionary<string, List<Pair<Pair<string, Cst.ServiceEnum>, ServiceController>>>();
                                    if (false == ret.ContainsKey(version))
                                    {
                                        List<Pair<Pair<string, Cst.ServiceEnum>, ServiceController>> value
                                            = new List<Pair<Pair<string, Cst.ServiceEnum>, ServiceController>>
                                            {
                                                service
                                            };
                                        ret.Add(version, value);
                                    }
                                    else if (false == ret[version].Exists(match => (match == service)))
                                    {
                                        ret[version].Add(service);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    // PM 20201118 [XXXXX] Ajout d'un tri sur les noms de services
                    if ((ret != default(Dictionary<string, List<Pair<Pair<string, Cst.ServiceEnum>, ServiceController>>>)) && (ret.Count() > 0))
                    {
                        ret = (
                            from servicesOfVersion in ret
                            select new { servicesOfVersion.Key, Value = servicesOfVersion.Value.OrderBy(s => s.First.First).ToList() }
                            ).ToDictionary(s => s.Key, s => s.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] DisplayError d'une exception
                DisplayError(ex);
            }
            dicServices = ret;
            return ret;
        }
        #endregion InitializeServices
        #region GetServiceStatus
        public static void GetServiceStatus()
        {
            string serviceTips = string.Empty;
            try
            {
                if (null != currentService)
                {
                    currentService.Refresh();
                    switch (currentService.Status)
                    {
                        case ServiceControllerStatus.Paused:
                            notifyIcon.Icon = Icons_Pause;
                            serviceTips = "Suspended";
                            break;
                        case ServiceControllerStatus.Running:
                            notifyIcon.Icon = Icons_Start;
                            serviceTips = "Started";
                            break;
                        case ServiceControllerStatus.Stopped:
                            notifyIcon.Icon = Icons_Stop;
                            serviceTips = "Stopped";
                            break;
                        case ServiceControllerStatus.ContinuePending:
                            notifyIcon.Icon = Icons_Pause;
                            serviceTips = "Recovery in progress";
                            break;
                        case ServiceControllerStatus.PausePending:
                            notifyIcon.Icon = Icons_Pause;
                            serviceTips = "Suspension in progress";
                            break;
                        case ServiceControllerStatus.StartPending:
                            notifyIcon.Icon = Icons_Pause;
                            serviceTips = "Starting in progress";
                            break;
                        case ServiceControllerStatus.StopPending:
                            notifyIcon.Icon = Icons_Pause;
                            serviceTips = "Stop in progress";
                            break;
                    }
                    serviceTips = serviceTips + @"- \\" + currentServer + " - " + currentService.ServiceName;
                    if (64 < serviceTips.Length)
                        serviceTips = serviceTips.Substring(0, 60) + "...";
                    notifyIcon.Text = serviceTips;

                    contextMenu.Items[6].Text = currentService.ServiceName + "-" + "Started";
                    contextMenu.Items[6].Enabled = (ServiceControllerStatus.Stopped == currentService.Status ||
                        ServiceControllerStatus.Paused == currentService.Status);

                    contextMenu.Items[5].Text = currentService.ServiceName + "-" + "Suspended";
                    contextMenu.Items[5].Enabled = (ServiceControllerStatus.Running == currentService.Status) &&
                        currentService.CanPauseAndContinue;

                    contextMenu.Items[4].Text = currentService.ServiceName + "-" + "Stopped";
                    contextMenu.Items[4].Enabled = ((ServiceControllerStatus.Running == currentService.Status) ||
                        (ServiceControllerStatus.Paused == currentService.Status)) &&
                        currentService.CanStop;
                }
                else
                {
                    notifyIcon.Icon = Icons_Manager;
                    contextMenu.Items[6].Text = "Started";
                    contextMenu.Items[6].Enabled = false;
                    contextMenu.Items[5].Text = "Suspended";
                    contextMenu.Items[5].Enabled = false;
                    contextMenu.Items[4].Text = "Stopped";
                    contextMenu.Items[4].Enabled = false;
                }
                RefreshFrmManager();
            }
            catch (Exception ex)
            {
                if (false == ex.GetType().Equals(typeof(System.InvalidOperationException)))
                    DisplayError(ex);
            }
        }
        #endregion GetServiceStatus
        #region ReadConfigValues
        // EG 20140425 Gestion Custom.config
        public static StringDictionary ReadConfigValues()
        {
            try
            {
                Config config = new Config(Application.ExecutablePath + ".config");
                Config customConfig = new Config(Path.GetDirectoryName(Application.ExecutablePath) + servicesManagerCustomConfigSource);
                if (null != customConfig)
                    return customConfig.GetConfig();
                else
                    return config.GetConfig();
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] DisplayError d'une exception             
                DisplayError(ex);
            }
            return null;
        }
        #endregion GetConfigValues
        #region RefreshFrmManager
        // EG 20140824 [XXXXX] la méthode devient publique
        public static void RefreshFrmManager()
        {
            if (isSpheresServicesOpened && (null != frmManager))
            {
                frmManager.DisplayServiceInformation();
                bool isEnabledBtnStartAll = false;
                bool isEnabledBtnPauseAll = false;
                bool isEnabledBtnStopAll = false;
                if ((null != dicServices) && dicServices.ContainsKey(frmManager.CurrentVersion))
                {
                    dicServices[frmManager.CurrentVersion].ForEach(item =>
                    {
                        item.Second.Refresh();
                        isEnabledBtnStartAll = isEnabledBtnStartAll ||
                                               (ServiceControllerStatus.Stopped == item.Second.Status) ||
                                               (ServiceControllerStatus.Paused == item.Second.Status);

                        isEnabledBtnPauseAll = isEnabledBtnPauseAll || (ServiceControllerStatus.Running == item.Second.Status);
                        isEnabledBtnStopAll = isEnabledBtnStopAll ||
                            (ServiceControllerStatus.Running == item.Second.Status) ||
                            (ServiceControllerStatus.Paused == item.Second.Status);
                    });
                    frmManager.UpdateButtonsAllServices(isEnabledBtnStartAll, isEnabledBtnPauseAll, isEnabledBtnStopAll);
                }
            }
        }
        #endregion RefreshFrmManager
        #region SetUpTimer
        private static void SetUpTimer()
        {
            try
            {
                if (0 < interval)
                {
                    timer = new System.Timers.Timer
                    {
                        AutoReset = true,
                        Interval = interval * 1000
                    };
                    timer.Start();
                    timer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimer);
                }
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] DisplayError d'une exception             
                DisplayError(ex);
            }
        }
        #endregion SetUpTimer
        #endregion Methods
    }

    #region SpheresEventLogEntry
    public class SpheresEventLogEntry
    {
        #region Members
        private readonly DateTime m_Date;
        private readonly string m_Source;
        private readonly string m_Category;
        private readonly long m_Event;
        private readonly string m_User;
        private readonly string m_MachineName;
        private string m_MessageTitle;
        private readonly string m_Message;
        private readonly byte[] m_Data;

        //private EventLogEntryType m_EntryType;
        private readonly int m_ImageIndex;
        private readonly Color m_ForeColor;
        #endregion Members
        #region Accessors
        /*public byte[] Data         {get {return m_Data;}}*/
        public DateTime Date { get { return m_Date; } }
        public string Source { get { return m_Source; } }
        public string Category { get { return m_Category; } }
        public long Event { get { return m_Event; } }
        public string User { get { return m_User; } }
        public string MachineName { get { return m_MachineName; } }
        public string Message { get { return m_Message; } }

        public string MessageTitle
        {
            get
            {
                if (StrFunc.IsEmpty(m_MessageTitle))
                    SetMessageTitle();
                return m_MessageTitle;
            }
        }


        public int ImageIndex { get { return m_ImageIndex; } }
        public Color ForeColor { get { return m_ForeColor; } }
        #endregion Accessors
        #region Constructors
        public SpheresEventLogEntry(EventLogEntry pEntry)
        {
            m_Date = pEntry.TimeGenerated;
            m_Source = pEntry.Source;
            m_Category = pEntry.Category == "(0)" ? "None" : pEntry.Category;
            m_Event = pEntry.InstanceId;
            m_User = StrFunc.IsFilled(pEntry.UserName) ? pEntry.UserName : ProcessStateTools.StatusUnknown;
            m_MachineName = pEntry.MachineName;
            m_Message = pEntry.Message;
            m_Data = pEntry.Data;

            //m_EntryType   = pEntry.EntryType;
            switch (pEntry.EntryType)
            {
                case EventLogEntryType.Error:
                case EventLogEntryType.FailureAudit:
                    m_ImageIndex = 0;
                    m_ForeColor = Color.Red;
                    break;
                case EventLogEntryType.Information:
                    m_ImageIndex = 1;
                    m_ForeColor = Color.MidnightBlue;
                    break;
                case EventLogEntryType.SuccessAudit:
                    m_ImageIndex = 2;
                    m_ForeColor = Color.Green;
                    break;
                case EventLogEntryType.Warning:
                    m_ImageIndex = 3;
                    m_ForeColor = Color.DarkOrange;
                    break;
            }
        }
        #endregion Constructors

        #region Methods
        #region GetData
        public byte[] GetData() { return m_Data; }
        #endregion GetData
        #region SetMessageTitle
        private void SetMessageTitle()
        {
            m_MessageTitle = m_Message.Substring(0, m_Message.IndexOf(Environment.NewLine));
        }
        #endregion SetMessageTitle
        #endregion Methods


    }

    #endregion SpheresEventLogEntry
}
