using System;
using System.Drawing;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Management;
using System.Reflection;
using System.Resources;
using System.DirectoryServices;
using NetworkManagement;

using EFS.ACommon;
using EFS.Common;
using EFS.SpheresService;
using EFS.SpheresServiceParameters;
using System.Collections.Generic;

namespace EFS
{
    public class FrmSpheresServices : Form
    {
        #region Members
        private Label lblServer;
        private Label lblService;
        private ComboBox cmbServer;
        private ComboBox cmbService;
        private ComboBox cmbServiceVersion;
        private Label lblStartContinue;
        private Label lblPause;
        private Label lblStop;
        private StatusBar stb;
        private PictureBox picTitle;
        private int errorCount;
        private StatusBarPanel stbPanel;
        private StatusBarPanel stbPanel_Error;
        private ImageList imgStatusService;
        private Label lblStartAllContinue;
        private Label lblStopAll;
        private Label lblPauseAll;
        private ContextMenuStrip contextMenuStripServices;
        private ToolStripMenuItem viewSettingsToolStripMenuItem;
        private ToolStripMenuItem updateSettingsToolStripMenuItem;

        private System.ComponentModel.IContainer components;
        private ImageList imlContextMenu;
        private PictureBox pictureBox1;
        private PictureBox picStartContinue;
        private PictureBox picPause;
        private PictureBox picStop;
        private PictureBox picStartAll;
        private PictureBox picPauseAll;
        private PictureBox picStopAll;
        private TextBox txtDescription;
        private PictureBox picActualise;
        private PictureBox picLog;
        private PictureBox picAddInstance;
        private Label lblAddInstance;
        private PictureBox picRemoveInstance;
        private Label lblRemoveInstance;
        private Dictionary<string, List<Pair<Pair<string, Cst.ServiceEnum>, ServiceController>>> m_DicServices;

        private readonly Color colorStart = Color.FromArgb(((int)(((byte)(81)))), ((int)(((byte)(173)))), ((int)(((byte)(38)))));
        private readonly Color colorPause = Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(54)))), ((int)(((byte)(54)))));
        private readonly Color colorStop = Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(3)))), ((int)(((byte)(3)))));
        private readonly Color colorDisable = Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));

        private readonly Font fntDisabled = null;
        private PictureBox picInfo;
        private Label label1;
        private readonly Font fntEnabled = null;

        #endregion Members
        #region Accessors
        #region CurrentVersion
        /// <summary>
        /// Version courante des services chargées dans la combo
        /// Si la version est non sélectionnée alors = à la version de l'assembly du serviceManager
        /// </summary>
        public string CurrentVersion
        {
            get
            {
                string version = Manager.AssemblyVersion;
                if (ArrFunc.IsFilled(cmbServiceVersion.Items))
                    version = cmbServiceVersion.SelectedItem.ToString();
                return version;
            }
        }

        #endregion CurrentVersion
        #region Status
        #endregion CurrentVersion
        #region Status
        /// <summary>
        /// Getter/Setter de la zone status/message de la forme
        /// </summary>
        private void SetStatus(string value)
        { stb.Panels[0].Text = value; }
        #endregion Status
        #region SetMessage
        /// <summary>
        /// Setter public de la zone status/message de la forme
        /// </summary>
        public string SetMessage
        {
            set { stb.Panels[0].Text = value; }
        }
        #endregion SetMessage
        #endregion Accessors
        #region Constructors
        public FrmSpheresServices()
        {
            InitializeComponent();
            fntDisabled = new Font(lblAddInstance.Font.FontFamily, lblAddInstance.Font.SizeInPoints, FontStyle.Italic);
            fntEnabled = new Font(lblAddInstance.Font.FontFamily, lblAddInstance.Font.SizeInPoints, FontStyle.Regular);
            this.lblAddInstance.Text = Ressource.GetString("AddInstance");
            this.lblRemoveInstance.Text = Ressource.GetString("RemoveInstance");
            this.Text = Ressource.GetString("Title");
            Manager.GetServerNames(cmbServer);
        }
        #endregion Constructors

        #region Code généré par le Concepteur Windows Form
        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmSpheresServices));
            this.picTitle = new System.Windows.Forms.PictureBox();
            this.lblServer = new System.Windows.Forms.Label();
            this.cmbServer = new System.Windows.Forms.ComboBox();
            this.lblService = new System.Windows.Forms.Label();
            this.cmbService = new System.Windows.Forms.ComboBox();
            this.contextMenuStripServices = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.viewSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.updateSettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblStartContinue = new System.Windows.Forms.Label();
            this.lblPause = new System.Windows.Forms.Label();
            this.lblStop = new System.Windows.Forms.Label();
            this.stb = new System.Windows.Forms.StatusBar();
            this.stbPanel = new System.Windows.Forms.StatusBarPanel();
            this.stbPanel_Error = new System.Windows.Forms.StatusBarPanel();
            this.imgStatusService = new System.Windows.Forms.ImageList(this.components);
            this.lblStartAllContinue = new System.Windows.Forms.Label();
            this.lblStopAll = new System.Windows.Forms.Label();
            this.lblPauseAll = new System.Windows.Forms.Label();
            this.cmbServiceVersion = new System.Windows.Forms.ComboBox();
            this.imlContextMenu = new System.Windows.Forms.ImageList(this.components);
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.picStartContinue = new System.Windows.Forms.PictureBox();
            this.picPause = new System.Windows.Forms.PictureBox();
            this.picStop = new System.Windows.Forms.PictureBox();
            this.picStartAll = new System.Windows.Forms.PictureBox();
            this.picPauseAll = new System.Windows.Forms.PictureBox();
            this.picStopAll = new System.Windows.Forms.PictureBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.picActualise = new System.Windows.Forms.PictureBox();
            this.picLog = new System.Windows.Forms.PictureBox();
            this.picAddInstance = new System.Windows.Forms.PictureBox();
            this.lblAddInstance = new System.Windows.Forms.Label();
            this.picRemoveInstance = new System.Windows.Forms.PictureBox();
            this.lblRemoveInstance = new System.Windows.Forms.Label();
            this.picInfo = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picTitle)).BeginInit();
            this.contextMenuStripServices.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.stbPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.stbPanel_Error)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picStartContinue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPause)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picStop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picStartAll)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPauseAll)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picStopAll)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picActualise)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picAddInstance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picRemoveInstance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picInfo)).BeginInit();
            this.SuspendLayout();
            // 
            // picTitle
            // 
            this.picTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.picTitle.Image = ((System.Drawing.Image)(resources.GetObject("picTitle.Image")));
            this.picTitle.InitialImage = ((System.Drawing.Image)(resources.GetObject("picTitle.InitialImage")));
            this.picTitle.Location = new System.Drawing.Point(0, 0);
            this.picTitle.Name = "picTitle";
            this.picTitle.Size = new System.Drawing.Size(498, 47);
            this.picTitle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picTitle.TabIndex = 0;
            this.picTitle.TabStop = false;
            // 
            // lblServer
            // 
            this.lblServer.AutoSize = true;
            this.lblServer.Font = new System.Drawing.Font("Arial", 8.25F);
            this.lblServer.Location = new System.Drawing.Point(54, 56);
            this.lblServer.Name = "lblServer";
            this.lblServer.Size = new System.Drawing.Size(40, 14);
            this.lblServer.TabIndex = 0;
            this.lblServer.Text = "Server";
            // 
            // cmbServer
            // 
            this.cmbServer.BackColor = System.Drawing.SystemColors.Window;
            this.cmbServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbServer.Font = new System.Drawing.Font("Arial", 9F);
            this.cmbServer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(18)))), ((int)(((byte)(56)))));
            this.cmbServer.Location = new System.Drawing.Point(98, 53);
            this.cmbServer.Name = "cmbServer";
            this.cmbServer.Size = new System.Drawing.Size(336, 23);
            this.cmbServer.TabIndex = 99;
            this.cmbServer.TabStop = false;
            this.cmbServer.SelectedIndexChanged += new System.EventHandler(this.OnServerChanged);
            // 
            // lblService
            // 
            this.lblService.AutoSize = true;
            this.lblService.Font = new System.Drawing.Font("Arial", 8.25F);
            this.lblService.Location = new System.Drawing.Point(54, 80);
            this.lblService.Name = "lblService";
            this.lblService.Size = new System.Drawing.Size(44, 14);
            this.lblService.TabIndex = 99;
            this.lblService.Text = "Service";
            // 
            // cmbService
            // 
            this.cmbService.BackColor = System.Drawing.SystemColors.Window;
            this.cmbService.ContextMenuStrip = this.contextMenuStripServices;
            this.cmbService.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbService.Font = new System.Drawing.Font("Arial", 9F);
            this.cmbService.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(18)))), ((int)(((byte)(56)))));
            this.cmbService.Location = new System.Drawing.Point(206, 77);
            this.cmbService.Name = "cmbService";
            this.cmbService.Size = new System.Drawing.Size(278, 23);
            this.cmbService.Sorted = true;
            this.cmbService.TabIndex = 1;
            this.cmbService.SelectedIndexChanged += new System.EventHandler(this.OnServiceChanged);
            // 
            // contextMenuStripServices
            // 
            this.contextMenuStripServices.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewSettingsToolStripMenuItem,
            this.updateSettingsToolStripMenuItem});
            this.contextMenuStripServices.Name = "contextMenuStripServices";
            this.contextMenuStripServices.Size = new System.Drawing.Size(157, 48);
            this.contextMenuStripServices.Text = "Settings";
            this.contextMenuStripServices.Opening += new System.ComponentModel.CancelEventHandler(this.PreRenderPanelSettings);
            // 
            // viewSettingsToolStripMenuItem
            // 
            this.viewSettingsToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("viewSettingsToolStripMenuItem.Image")));
            this.viewSettingsToolStripMenuItem.Name = "viewSettingsToolStripMenuItem";
            this.viewSettingsToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.viewSettingsToolStripMenuItem.Text = "&View settings";
            this.viewSettingsToolStripMenuItem.Click += new System.EventHandler(this.OnClickSettings);
            // 
            // updateSettingsToolStripMenuItem
            // 
            this.updateSettingsToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("updateSettingsToolStripMenuItem.Image")));
            this.updateSettingsToolStripMenuItem.Name = "updateSettingsToolStripMenuItem";
            this.updateSettingsToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.updateSettingsToolStripMenuItem.Text = "&Update settings";
            this.updateSettingsToolStripMenuItem.Click += new System.EventHandler(this.OnClickWriteSettings);
            // 
            // lblStartContinue
            // 
            this.lblStartContinue.BackColor = System.Drawing.Color.Transparent;
            this.lblStartContinue.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblStartContinue.Font = new System.Drawing.Font("Arial", 8.25F);
            this.lblStartContinue.ForeColor = System.Drawing.Color.Black;
            this.lblStartContinue.Location = new System.Drawing.Point(437, 101);
            this.lblStartContinue.Name = "lblStartContinue";
            this.lblStartContinue.Size = new System.Drawing.Size(60, 22);
            this.lblStartContinue.TabIndex = 99;
            this.lblStartContinue.Text = "Start";
            this.lblStartContinue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStartContinue.Click += new System.EventHandler(this.OnStartContinue);
            // 
            // lblPause
            // 
            this.lblPause.BackColor = System.Drawing.Color.Transparent;
            this.lblPause.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblPause.Font = new System.Drawing.Font("Arial", 8.25F);
            this.lblPause.ForeColor = System.Drawing.Color.Black;
            this.lblPause.Location = new System.Drawing.Point(437, 123);
            this.lblPause.Name = "lblPause";
            this.lblPause.Size = new System.Drawing.Size(60, 22);
            this.lblPause.TabIndex = 99;
            this.lblPause.Text = "Pause";
            this.lblPause.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblPause.Click += new System.EventHandler(this.OnPause);
            // 
            // lblStop
            // 
            this.lblStop.BackColor = System.Drawing.Color.Transparent;
            this.lblStop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblStop.Font = new System.Drawing.Font("Arial", 8F);
            this.lblStop.ForeColor = System.Drawing.Color.Black;
            this.lblStop.Location = new System.Drawing.Point(437, 145);
            this.lblStop.Name = "lblStop";
            this.lblStop.Size = new System.Drawing.Size(60, 22);
            this.lblStop.TabIndex = 99;
            this.lblStop.Text = "Stop";
            this.lblStop.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStop.Click += new System.EventHandler(this.OnStop);
            // 
            // stb
            // 
            this.stb.Location = new System.Drawing.Point(0, 247);
            this.stb.Name = "stb";
            this.stb.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.stbPanel,
            this.stbPanel_Error});
            this.stb.ShowPanels = true;
            this.stb.Size = new System.Drawing.Size(498, 28);
            this.stb.SizingGrip = false;
            this.stb.TabIndex = 99;
            // 
            // stbPanel
            // 
            this.stbPanel.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
            this.stbPanel.Name = "stbPanel";
            this.stbPanel.Width = 398;
            // 
            // stbPanel_Error
            // 
            this.stbPanel_Error.Name = "stbPanel_Error";
            // 
            // imgStatusService
            // 
            this.imgStatusService.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgStatusService.ImageStream")));
            this.imgStatusService.TransparentColor = System.Drawing.Color.Transparent;
            this.imgStatusService.Images.SetKeyName(0, "SpheresServiceManager-16-2.png");
            this.imgStatusService.Images.SetKeyName(1, "Play.png");
            this.imgStatusService.Images.SetKeyName(2, "Pause.png");
            this.imgStatusService.Images.SetKeyName(3, "Stop.png");
            // 
            // lblStartAllContinue
            // 
            this.lblStartAllContinue.BackColor = System.Drawing.Color.Transparent;
            this.lblStartAllContinue.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblStartAllContinue.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStartAllContinue.ForeColor = System.Drawing.Color.Black;
            this.lblStartAllContinue.Location = new System.Drawing.Point(437, 174);
            this.lblStartAllContinue.Name = "lblStartAllContinue";
            this.lblStartAllContinue.Size = new System.Drawing.Size(60, 22);
            this.lblStartAllContinue.TabIndex = 101;
            this.lblStartAllContinue.Text = "Start all";
            this.lblStartAllContinue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStartAllContinue.Click += new System.EventHandler(this.OnStartAllContinue);
            // 
            // lblStopAll
            // 
            this.lblStopAll.BackColor = System.Drawing.Color.Transparent;
            this.lblStopAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblStopAll.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStopAll.ForeColor = System.Drawing.Color.Black;
            this.lblStopAll.Location = new System.Drawing.Point(437, 218);
            this.lblStopAll.Name = "lblStopAll";
            this.lblStopAll.Size = new System.Drawing.Size(60, 22);
            this.lblStopAll.TabIndex = 103;
            this.lblStopAll.Text = "Stop all";
            this.lblStopAll.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblStopAll.Click += new System.EventHandler(this.OnStopAll);
            // 
            // lblPauseAll
            // 
            this.lblPauseAll.BackColor = System.Drawing.Color.Transparent;
            this.lblPauseAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblPauseAll.Font = new System.Drawing.Font("Arial", 8.25F);
            this.lblPauseAll.ForeColor = System.Drawing.Color.Black;
            this.lblPauseAll.Location = new System.Drawing.Point(437, 196);
            this.lblPauseAll.Name = "lblPauseAll";
            this.lblPauseAll.Size = new System.Drawing.Size(60, 22);
            this.lblPauseAll.TabIndex = 105;
            this.lblPauseAll.Text = "Pause all";
            this.lblPauseAll.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblPauseAll.Click += new System.EventHandler(this.OnPauseAll);
            // 
            // cmbServiceVersion
            // 
            this.cmbServiceVersion.BackColor = System.Drawing.SystemColors.Window;
            this.cmbServiceVersion.ContextMenuStrip = this.contextMenuStripServices;
            this.cmbServiceVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbServiceVersion.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbServiceVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(18)))), ((int)(((byte)(56)))));
            this.cmbServiceVersion.Location = new System.Drawing.Point(97, 77);
            this.cmbServiceVersion.Name = "cmbServiceVersion";
            this.cmbServiceVersion.Size = new System.Drawing.Size(103, 23);
            this.cmbServiceVersion.Sorted = true;
            this.cmbServiceVersion.TabIndex = 0;
            this.cmbServiceVersion.SelectedIndexChanged += new System.EventHandler(this.OnVersionChanged);
            // 
            // imlContextMenu
            // 
            this.imlContextMenu.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imlContextMenu.ImageStream")));
            this.imlContextMenu.TransparentColor = System.Drawing.Color.Transparent;
            this.imlContextMenu.Images.SetKeyName(0, "WinServiceManager.png");
            this.imlContextMenu.Images.SetKeyName(1, "CheckList.png");
            this.imlContextMenu.Images.SetKeyName(2, "Play.png");
            this.imlContextMenu.Images.SetKeyName(3, "Pause.png");
            this.imlContextMenu.Images.SetKeyName(4, "Stop.png");
            this.imlContextMenu.Images.SetKeyName(5, "Settings.png");
            this.imlContextMenu.Images.SetKeyName(6, "loupe.png");
            this.imlContextMenu.Images.SetKeyName(7, "BannerHelp-Blue.png");
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Left;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.InitialImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.InitialImage")));
            this.pictureBox1.Location = new System.Drawing.Point(0, 47);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(48, 200);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 107;
            this.pictureBox1.TabStop = false;
            // 
            // picStartContinue
            // 
            this.picStartContinue.BackColor = System.Drawing.Color.Transparent;
            this.picStartContinue.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.picStartContinue.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picStartContinue.Enabled = false;
            this.picStartContinue.Image = ((System.Drawing.Image)(resources.GetObject("picStartContinue.Image")));
            this.picStartContinue.Location = new System.Drawing.Point(413, 102);
            this.picStartContinue.Name = "picStartContinue";
            this.picStartContinue.Size = new System.Drawing.Size(20, 20);
            this.picStartContinue.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picStartContinue.TabIndex = 108;
            this.picStartContinue.TabStop = false;
            this.picStartContinue.Click += new System.EventHandler(this.OnStartContinue);
            // 
            // picPause
            // 
            this.picPause.BackColor = System.Drawing.Color.Transparent;
            this.picPause.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.picPause.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picPause.Enabled = false;
            this.picPause.Image = ((System.Drawing.Image)(resources.GetObject("picPause.Image")));
            this.picPause.Location = new System.Drawing.Point(413, 124);
            this.picPause.Name = "picPause";
            this.picPause.Size = new System.Drawing.Size(20, 20);
            this.picPause.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picPause.TabIndex = 109;
            this.picPause.TabStop = false;
            this.picPause.Click += new System.EventHandler(this.OnPause);
            // 
            // picStop
            // 
            this.picStop.BackColor = System.Drawing.Color.Transparent;
            this.picStop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.picStop.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picStop.Enabled = false;
            this.picStop.Image = ((System.Drawing.Image)(resources.GetObject("picStop.Image")));
            this.picStop.Location = new System.Drawing.Point(413, 146);
            this.picStop.Name = "picStop";
            this.picStop.Size = new System.Drawing.Size(20, 20);
            this.picStop.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picStop.TabIndex = 110;
            this.picStop.TabStop = false;
            this.picStop.Click += new System.EventHandler(this.OnStop);
            // 
            // picStartAll
            // 
            this.picStartAll.BackColor = System.Drawing.Color.Transparent;
            this.picStartAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.picStartAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picStartAll.Enabled = false;
            this.picStartAll.Image = ((System.Drawing.Image)(resources.GetObject("picStartAll.Image")));
            this.picStartAll.Location = new System.Drawing.Point(413, 175);
            this.picStartAll.Name = "picStartAll";
            this.picStartAll.Size = new System.Drawing.Size(20, 20);
            this.picStartAll.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picStartAll.TabIndex = 111;
            this.picStartAll.TabStop = false;
            this.picStartAll.Click += new System.EventHandler(this.OnStartAllContinue);
            // 
            // picPauseAll
            // 
            this.picPauseAll.BackColor = System.Drawing.Color.Transparent;
            this.picPauseAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.picPauseAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picPauseAll.Enabled = false;
            this.picPauseAll.Image = ((System.Drawing.Image)(resources.GetObject("picPauseAll.Image")));
            this.picPauseAll.Location = new System.Drawing.Point(413, 196);
            this.picPauseAll.Name = "picPauseAll";
            this.picPauseAll.Size = new System.Drawing.Size(20, 20);
            this.picPauseAll.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picPauseAll.TabIndex = 112;
            this.picPauseAll.TabStop = false;
            this.picPauseAll.Click += new System.EventHandler(this.OnPauseAll);
            // 
            // picStopAll
            // 
            this.picStopAll.BackColor = System.Drawing.Color.Transparent;
            this.picStopAll.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.picStopAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picStopAll.Enabled = false;
            this.picStopAll.Image = ((System.Drawing.Image)(resources.GetObject("picStopAll.Image")));
            this.picStopAll.Location = new System.Drawing.Point(413, 219);
            this.picStopAll.Name = "picStopAll";
            this.picStopAll.Size = new System.Drawing.Size(20, 20);
            this.picStopAll.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picStopAll.TabIndex = 113;
            this.picStopAll.TabStop = false;
            this.picStopAll.Click += new System.EventHandler(this.OnStopAll);
            // 
            // txtDescription
            // 
            this.txtDescription.BackColor = System.Drawing.Color.White;
            this.txtDescription.Cursor = System.Windows.Forms.Cursors.Hand;
            this.txtDescription.Font = new System.Drawing.Font("Arial", 8F);
            this.txtDescription.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(124)))), ((int)(((byte)(18)))), ((int)(((byte)(56)))));
            this.txtDescription.Location = new System.Drawing.Point(98, 104);
            this.txtDescription.Multiline = true;
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.ReadOnly = true;
            this.txtDescription.Size = new System.Drawing.Size(300, 109);
            this.txtDescription.TabIndex = 116;
            this.txtDescription.TabStop = false;
            // 
            // picActualise
            // 
            this.picActualise.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.picActualise.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picActualise.Image = ((System.Drawing.Image)(resources.GetObject("picActualise.Image")));
            this.picActualise.Location = new System.Drawing.Point(440, 54);
            this.picActualise.Name = "picActualise";
            this.picActualise.Size = new System.Drawing.Size(20, 20);
            this.picActualise.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picActualise.TabIndex = 117;
            this.picActualise.TabStop = false;
            this.picActualise.Click += new System.EventHandler(this.OnServerChanged);
            // 
            // picLog
            // 
            this.picLog.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.picLog.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picLog.Image = ((System.Drawing.Image)(resources.GetObject("picLog.Image")));
            this.picLog.Location = new System.Drawing.Point(466, 54);
            this.picLog.Name = "picLog";
            this.picLog.Size = new System.Drawing.Size(20, 20);
            this.picLog.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picLog.TabIndex = 118;
            this.picLog.TabStop = false;
            this.picLog.Click += new System.EventHandler(this.OnEventLog);
            // 
            // picAddInstance
            // 
            this.picAddInstance.BackColor = System.Drawing.Color.Transparent;
            this.picAddInstance.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.picAddInstance.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picAddInstance.Enabled = false;
            this.picAddInstance.Image = ((System.Drawing.Image)(resources.GetObject("picAddInstance.Image")));
            this.picAddInstance.Location = new System.Drawing.Point(98, 220);
            this.picAddInstance.Name = "picAddInstance";
            this.picAddInstance.Size = new System.Drawing.Size(20, 20);
            this.picAddInstance.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picAddInstance.TabIndex = 120;
            this.picAddInstance.TabStop = false;
            this.picAddInstance.Click += new System.EventHandler(this.OnAddInstance);
            // 
            // lblAddInstance
            // 
            this.lblAddInstance.BackColor = System.Drawing.Color.Transparent;
            this.lblAddInstance.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblAddInstance.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAddInstance.ForeColor = System.Drawing.Color.Black;
            this.lblAddInstance.Location = new System.Drawing.Point(121, 219);
            this.lblAddInstance.Name = "lblAddInstance";
            this.lblAddInstance.Size = new System.Drawing.Size(103, 22);
            this.lblAddInstance.TabIndex = 119;
            this.lblAddInstance.Text = "Add an instance";
            this.lblAddInstance.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblAddInstance.Click += new System.EventHandler(this.OnAddInstance);
            // 
            // picRemoveInstance
            // 
            this.picRemoveInstance.BackColor = System.Drawing.Color.Transparent;
            this.picRemoveInstance.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.picRemoveInstance.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picRemoveInstance.Enabled = false;
            this.picRemoveInstance.Image = ((System.Drawing.Image)(resources.GetObject("picRemoveInstance.Image")));
            this.picRemoveInstance.Location = new System.Drawing.Point(230, 220);
            this.picRemoveInstance.Name = "picRemoveInstance";
            this.picRemoveInstance.Size = new System.Drawing.Size(20, 20);
            this.picRemoveInstance.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picRemoveInstance.TabIndex = 122;
            this.picRemoveInstance.TabStop = false;
            this.picRemoveInstance.Click += new System.EventHandler(this.OnRemoveInstance);
            // 
            // lblRemoveInstance
            // 
            this.lblRemoveInstance.BackColor = System.Drawing.Color.Transparent;
            this.lblRemoveInstance.Cursor = System.Windows.Forms.Cursors.Hand;
            this.lblRemoveInstance.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRemoveInstance.ForeColor = System.Drawing.Color.Black;
            this.lblRemoveInstance.Location = new System.Drawing.Point(253, 219);
            this.lblRemoveInstance.Name = "lblRemoveInstance";
            this.lblRemoveInstance.Size = new System.Drawing.Size(133, 22);
            this.lblRemoveInstance.TabIndex = 121;
            this.lblRemoveInstance.Text = "Remove this instance";
            this.lblRemoveInstance.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblRemoveInstance.Click += new System.EventHandler(this.OnRemoveInstance);
            // 
            // picInfo
            // 
            this.picInfo.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.picInfo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picInfo.Enabled = false;
            this.picInfo.Image = ((System.Drawing.Image)(resources.GetObject("picInfo.Image")));
            this.picInfo.Location = new System.Drawing.Point(380, 195);
            this.picInfo.Name = "picInfo";
            this.picInfo.Size = new System.Drawing.Size(16, 16);
            this.picInfo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.picInfo.TabIndex = 123;
            this.picInfo.TabStop = false;
            this.picInfo.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(145, 165);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(105, 14);
            this.label1.TabIndex = 124;
            this.label1.Text = "Application services";
            this.label1.Click += new System.EventHandler(this.Label1_Click);
            // 
            // FrmSpheresServices
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.ClientSize = new System.Drawing.Size(498, 275);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.cmbServiceVersion);
            this.Controls.Add(this.cmbServer);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.picRemoveInstance);
            this.Controls.Add(this.lblRemoveInstance);
            this.Controls.Add(this.picInfo);
            this.Controls.Add(this.picLog);
            this.Controls.Add(this.picActualise);
            this.Controls.Add(this.picAddInstance);
            this.Controls.Add(this.picStopAll);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lblAddInstance);
            this.Controls.Add(this.picStop);
            this.Controls.Add(this.picPauseAll);
            this.Controls.Add(this.picStartAll);
            this.Controls.Add(this.picPause);
            this.Controls.Add(this.picStartContinue);
            this.Controls.Add(this.picTitle);
            this.Controls.Add(this.lblPauseAll);
            this.Controls.Add(this.lblStopAll);
            this.Controls.Add(this.cmbService);
            this.Controls.Add(this.lblStartAllContinue);
            this.Controls.Add(this.lblServer);
            this.Controls.Add(this.lblService);
            this.Controls.Add(this.lblStartContinue);
            this.Controls.Add(this.stb);
            this.Controls.Add(this.lblStop);
            this.Controls.Add(this.lblPause);
            this.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FrmSpheresServices";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Application services manager";
            this.Load += new System.EventHandler(this.OnLoad);
            this.Shown += new System.EventHandler(this.OnShown);
            ((System.ComponentModel.ISupportInitialize)(this.picTitle)).EndInit();
            this.contextMenuStripServices.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.stbPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.stbPanel_Error)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picStartContinue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPause)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picStop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picStartAll)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPauseAll)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picStopAll)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picActualise)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picLog)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picAddInstance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picRemoveInstance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picInfo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        #region Events
        #region Dispose
        protected override void Dispose(bool disposing)
        {
            if (disposing && (null != components))
                components.Dispose();
            base.Dispose(disposing);
        }
        #endregion Dispose
        #region OnAddInstance
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// EG 20120629 TODO
        /// FI 20131008 [19041] call methode SpheresServiceParametersHelper.CreateServiceFromService
        /// FI 20131107 [XXXXX] Add test sur (ArrFunc.IsFilled(parameters))
        /// FI 20161020 [XXXXX] Modify
        private void OnAddInstance(object sender, System.EventArgs e)
        {
            DialogResult response = DialogResult.Yes;

            if (Manager.IsControlService)
            {
                string message = Ressource.GetString2("ControlAddService", Manager.CurrentServiceName, Manager.CurrentServer);
                response = MessageBox.Show(message, "Add instance", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }

            if (DialogResult.Yes == response)
            {

                Dictionary<string, object> parameters =
                    SpheresServiceParametersHelper.CallModalConfiguration(
                    Manager.CurrentService,
                    SpheresServiceParametersHelper.ConfigurationMode.Write);

                // FI 20131107 [] lorsque l'utilisateur annule alors parameters est vide
                if (ArrFunc.IsFilled(parameters))
                {
                    try
                    {
                        int ret = SpheresServiceParametersHelper.CreateServiceFromService(Manager.CurrentServiceName, parameters, out string standardOutput);

                        switch (ret)
                        {
                            case -1:
                                string newServiceName = (string)parameters[ServiceKeyEnum.Instance.ToString()];
                                DisplayError(true, String.Format("Service {0} already exists", newServiceName));
                                break;
                            case 0:
                                // FI 20161020 [XXXXX] StartStop Update
                                ServiceInfo serviceInfo = new ServiceInfo(Manager.CurrentServiceName);
                                string version = serviceInfo.GetVersion();
                                string path = serviceInfo.GetInformationsKey(ServiceKeyEnum.PathInstall);
                                SpheresServiceParametersHelper.StartStopToolsDel(path);
                                // PM 20201119 [XXXXX] Démarrage de SpheresLogger en premier et arret en dernier
                                //SpheresServiceParametersHelper.StartStopToolsAdd(path, version, string.Empty);
                                SpheresServiceParametersHelper.StartStopToolsAdd(path, version);

                                OnServerChanged(cmbServer, null);
                                break;
                            default:
                                DisplayError(true, standardOutput);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        // FI 20200910 [XXXXX] DisplayError d'une exception
                        DisplayError(ex);
                    }
                }
            }
        }

        #endregion OnAddInstance
        #region OnEventLog
        private void OnEventLog(object sender, System.EventArgs e)
        {
            try
            {
                FrmEventLog frmEventLog = new FrmEventLog
                {
                    TopLevel = true,
                    Service = cmbService.SelectedItem.ToString()
                };
                frmEventLog.ShowDialog(this);
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] DisplayError d'une exception
                DisplayError(ex);
            }
        }
        #endregion OnEventLog

        #region OnLoad
        private void OnLoad(object sender, System.EventArgs e)
        {
        }
        #endregion OnLoad
        #region OnShown
        /// <summary>
        /// Se déclenche à chaque fois que le formulaire est affiché en premier
        /// Mise à jour des contrôles
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnShown(object sender, System.EventArgs e)
        {
            Manager.RefreshFrmManager();
        }
        #endregion OnShown

        #region OnPause
        private void OnPause(object sender, System.EventArgs e) { Manager.OnPause(sender, e); }
        #endregion OnPause
        #region OnPauseAll
        private void OnPauseAll(object sender, System.EventArgs e)
        {
            Manager.OnPauseAll(sender, e);
        }
        #endregion OnPauseAll
        #region OnRemoveInstance
        // EG 20120629 TODO
        private void OnRemoveInstance(object sender, System.EventArgs e)
        {
            DialogResult response = DialogResult.Yes;

            if (Manager.IsControlService)
            {
                string message = Ressource.GetString2("ControlRemoveService", Manager.CurrentServiceName, Manager.CurrentServer);
                response = MessageBox.Show(message, "Remove instance", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            }

            if (DialogResult.Yes == response)
            {
                switch (SpheresServiceParametersHelper.CanRemove(Manager.CurrentService.Status))
                {
                    case SpheresServiceParametersHelper.ServiceStatePreRemove.Stop:
                        Manager.OnStop(this, new EventArgs());
                        break;
                    case SpheresServiceParametersHelper.ServiceStatePreRemove.Pending:
                        break;

                    case SpheresServiceParametersHelper.ServiceStatePreRemove.Remove:
                        RemoveInstanceService();
                        break;
                }
            }
        }
        #endregion OnRemoveInstance
        #region OnServerChanged
        /// <summary>
        /// Rechargement du dictionnaire des services par version après changement du serveur
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnServerChanged(object sender, System.EventArgs e)
        {
            m_DicServices = Manager.InitializeServices(cmbServer.SelectedItem.ToString());
            if (null != m_DicServices)
            {
                Manager.BindControlMenu(m_DicServices);
                Manager.BindControlVersion(m_DicServices, cmbServiceVersion);
                Manager.BindControlServices(m_DicServices, CurrentVersion, cmbService, Manager.CurrentServiceEnum);
            }
        }
        #endregion OnServerChanged
        #region OnVersionChanged
        /// <summary>
        /// Alimentation de la combo des services au changement de la version
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnVersionChanged(object sender, System.EventArgs e)
        {
            Manager.BindControlServices(m_DicServices, CurrentVersion, cmbService, Manager.CurrentServiceEnum);
        }
        #endregion OnVersionChanged
        #region OnServiceChanged
        /// <summary>
        /// Affichage des informations de service 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnServiceChanged(object sender, System.EventArgs e)
        {
            errorCount = 0;
            DisplayServiceInformation();
            PurgeError();
        }
        #endregion OnServiceChanged
        #region OnStartContinue
        private void OnStartContinue(object sender, System.EventArgs e) { Manager.OnStartContinue(sender, e); }
        #endregion OnStartContinue
        #region OnStop
        private void OnStop(object sender, System.EventArgs e) { Manager.OnStop(sender, e); }
        #endregion OnStop
        #region OnStartAllContinue
        private void OnStartAllContinue(object sender, System.EventArgs e)
        {
            Manager.OnStartAllContinue(sender, e);
        }
        #endregion OnStartAllContinue
        #region OnStopAll
        private void OnStopAll(object sender, System.EventArgs e)
        {
            Manager.OnStopAll(sender, e);
        }
        #endregion OnStopAll

        // 20100621 MF - parameters visualisation
        #region ServicePanelSettings

        private void OnClickWriteSettings(object sender, EventArgs e)
        {
            ClickSettings(SpheresServiceParametersHelper.ConfigurationMode.Update);
        }

        private void OnClickSettings(object sender, EventArgs e)
        {
            ClickSettings(SpheresServiceParametersHelper.ConfigurationMode.Read);
        }

        private void ClickSettings(SpheresServiceParametersHelper.ConfigurationMode mode)
        {
            ServiceInfo serviceInfo = new ServiceInfo(Manager.CurrentServiceName);

            string serviceClassType = serviceInfo.GetInformationsKey(ServiceKeyEnum.ClassType);
            string instanceNamePrefix = serviceInfo.GetInformationsKey(ServiceKeyEnum.Prefix);


            Dictionary<string, object> parameters =
                SpheresServiceParametersHelper.CallModalConfiguration(Manager.CurrentService, mode);

            parameters.Add(ServiceKeyEnum.ClassType.ToString(), serviceClassType);
            parameters.Add(ServiceKeyEnum.Prefix.ToString(), instanceNamePrefix);


            if (mode == SpheresServiceParametersHelper.ConfigurationMode.Update)
            {
                SpheresServiceParametersHelper.CallWriteInstallInformation(Manager.CurrentServiceName, serviceInfo, parameters);
                SpheresServiceParametersHelper.CallUpdateSectionConfig(Manager.CurrentServiceName, parameters);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreRenderPanelSettings(object sender, CancelEventArgs e)
        {
            bool cantShowParameters = Manager.CurrentService == null ||
                !SpheresServiceParametersHelper.ImplementsISpheresServiceParameters(Manager.CurrentService.ServiceName);

            bool isRunning = !cantShowParameters &&
                SpheresServiceParametersHelper.CanRemove(Manager.CurrentService.Status) !=
                    SpheresServiceParametersHelper.ServiceStatePreRemove.Remove;

            bool disableFlag = e.Cancel || cmbService.SelectedItem == null || cantShowParameters;

            viewSettingsToolStripMenuItem.Enabled = !disableFlag;
            updateSettingsToolStripMenuItem.Enabled = !disableFlag && !isRunning;

        }
        // 20100621 MF - parameters visualisation END

        #endregion

        #endregion Events

        #region Methods
        #region DisplayError
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        /// FI 20200910 [XXXXX] Add 
        public void DisplayError(Exception exception)
        {
            DisplayError(true, exception.Message, ExceptionTools.GetMessageExtended(exception));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsError"></param>
        /// <param name="pMessage"></param>
        public void DisplayError(bool pIsError, string pMessage)
        {
            DisplayError(pIsError, pMessage, pMessage);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsError"></param>
        /// <param name="pMessage"></param>
        /// <param name="pTooltip"></param>
        /// FI 20200910 [XXXXX] Add 
        public void DisplayError(bool pIsError, string pMessage, string pTooltip)
        {
            if (pIsError)
            {
                errorCount++;
                if (10 > errorCount)
                {
                    stb.Panels[1].Text = pMessage;
                    stb.Panels[1].ToolTipText = pTooltip;
                    stb.Panels[1].AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
                }
            }
        }
        #endregion DisplayError
        #region DisplayServiceInformation
        /// <summary>
        /// Affichage des informations du service sélectionné
        /// </summary>
        /// EG 20210824 [XXXXX] Appel d'une nouvelle fonction DisplayStatusOfCurrentService() pour Mise à jour du message relatif au service actif sur la forme (bas d'écran) 
        public void DisplayServiceInformation()
        {
            try
            {
                bool canAddInstances = false;
                bool canRemoveInstance = false;
                bool isStartEnabled = false;
                bool isStopEnabled = false;
                bool isPauseEnabled = false;
                txtDescription.Text = string.Empty;
                SetStatus(string.Empty);
                int i = cmbService.SelectedIndex;
                if (-1 < i)
                {
                    if ((null != m_DicServices) && m_DicServices.ContainsKey(CurrentVersion))
                    {
                        Pair<Pair<string, Cst.ServiceEnum>, ServiceController> service =
                            m_DicServices[CurrentVersion].Find(match => match.First.First.ToString() == cmbService.SelectedItem.ToString());
                        if (null != service)
                        {
                            service.Second.Refresh();
                            Manager.CurrentService = (ServiceController)service.Second;
                            ServiceInfo CurrentServiceInfo = new ServiceInfo(Manager.CurrentServiceName);

                            txtDescription.Text = string.Empty;

                            if (0 == cmbServer.SelectedIndex)
                                canAddInstances = SpheresServiceParametersHelper.ImplementsISpheresServiceParameters(Manager.CurrentService.ServiceName);

                            if (canAddInstances)
                                canRemoveInstance = StrFunc.IsFilled(CurrentServiceInfo.GetInformationsKey(ServiceKeyEnum.Instance));

                            // PM 20160816 [22136] La première instance de la Gateway BCS possède déjà un nom d'instance
                            if (CurrentServiceInfo.GetInformationsKey(ServiceKeyEnum.ServiceEnum) != Cst.ServiceEnum.SpheresGateBCS.ToString())
                            {
                                canAddInstances = canAddInstances && (!canRemoveInstance);
                            }
                            //
                            isStartEnabled = (ServiceControllerStatus.Stopped == Manager.CurrentService.Status) ||
                                             (ServiceControllerStatus.Paused == Manager.CurrentService.Status);
                            isPauseEnabled = (ServiceControllerStatus.Running == Manager.CurrentService.Status);
                            isStopEnabled = (ServiceControllerStatus.Running == Manager.CurrentService.Status) ||
                                            (ServiceControllerStatus.Paused == Manager.CurrentService.Status);


                            try
                            {
                                Manager.CheckCurrentService();
                                txtDescription.Text = CurrentServiceInfo.GetInformationsKey(ServiceKeyEnum.Description);
                            }
                            catch { }
                            finally
                            {
                                DisplayStatusOfCurrentService();
                            }
                        }
                    }
                }
                // Set Styles
                picAddInstance.Enabled = canAddInstances;
                lblAddInstance.Font = canAddInstances ? fntEnabled : fntDisabled;
                lblAddInstance.ForeColor = canAddInstances ? colorStart : colorDisable;

                picRemoveInstance.Enabled = canRemoveInstance;
                lblRemoveInstance.Font = canRemoveInstance ? fntEnabled : fntDisabled;
                lblRemoveInstance.ForeColor = canRemoveInstance ? colorStop : colorDisable;
                //
                picStartContinue.Enabled = isStartEnabled;
                lblStartContinue.Font = isStartEnabled ? fntEnabled : fntDisabled;
                lblStartContinue.ForeColor = isStartEnabled ? colorStart : colorDisable;
                //
                picPause.Enabled = isPauseEnabled;
                lblPause.Font = isPauseEnabled ? fntEnabled : fntDisabled;
                lblPause.ForeColor = isPauseEnabled ? colorPause : colorDisable;
                //
                picStop.Enabled = isStopEnabled;
                lblStop.Font = isStopEnabled ? fntEnabled : fntDisabled;
                lblStop.ForeColor = isStopEnabled ? colorStop : colorDisable;
                //
                picInfo.Visible = canRemoveInstance;
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] DisplayError d'une exception
                DisplayError(ex);
            }
        }
        #endregion DisplayServiceInformation
        #region DisplayStatusOfCurrentService
        /// <summary>
        /// Mise à jour du message relatif au service actif sur la forme (bas d'écran) 
        /// </summary>
        /// EG 20210824 [XXXXX] New
        private void DisplayStatusOfCurrentService()
        {
            string message = String.Empty;
            if (ServiceControllerStatus.Paused == Manager.CurrentService.Status)
                message = "Suspended";
            else if (ServiceControllerStatus.Running == Manager.CurrentService.Status)
                message = "Started";
            else if (ServiceControllerStatus.Stopped == Manager.CurrentService.Status)
                message = "Stopped";
            else if (ServiceControllerStatus.ContinuePending == Manager.CurrentService.Status)
                message = "Recovery in progress";
            else if (ServiceControllerStatus.PausePending == Manager.CurrentService.Status)
                message = "Suspension in progress";
            else if (ServiceControllerStatus.StartPending == Manager.CurrentService.Status)
                message = "Starting in progress";
            else if (ServiceControllerStatus.StopPending == Manager.CurrentService.Status)
                message = "Stop in progress";

            SetStatus(message + @" - \\" + Manager.CurrentServer + " - " + Manager.CurrentServiceVersion + " : " + Manager.CurrentServiceName);
        }
        #endregion DisplayStatusOfCurrentService
        #region PurgeError
        private void PurgeError()
        {
            try { stb.Panels[1].Text = string.Empty; }
            catch (Exception) { }
        }
        #endregion PurgeError
        #region UpdateButtonsAllServices
        public void UpdateButtonsAllServices()
        {
            UpdateButtonsAllServices(false, false, false);
        }
        public void UpdateButtonsAllServices(bool pIsEnabledBtnStartAll, bool pIsEnabledBtnPauseAll, bool pIsEnabledBtnStopAll)
        {
            try
            {
                picStartAll.Enabled = pIsEnabledBtnStartAll;
                lblStartAllContinue.Font = pIsEnabledBtnStartAll ? fntEnabled : fntDisabled;
                lblStartAllContinue.ForeColor = pIsEnabledBtnStartAll ? colorStart : colorDisable;

                picPauseAll.Enabled = pIsEnabledBtnPauseAll;
                lblPauseAll.Font = pIsEnabledBtnPauseAll ? fntEnabled : fntDisabled;
                lblPauseAll.ForeColor = pIsEnabledBtnPauseAll ? colorPause : colorDisable;

                picStopAll.Enabled = pIsEnabledBtnStopAll;
                lblStopAll.Font = pIsEnabledBtnStopAll ? fntEnabled : fntDisabled;
                lblStopAll.ForeColor = pIsEnabledBtnStopAll ? colorStop : colorDisable;
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] DisplayError d'une exception
                DisplayError(ex);
            }
        }
        #endregion UpdateButtonsAllServices
        #region SetVersion
        public void SetVersion(string pVersionName)
        {
            int i = -1;
            if (0 < cmbServiceVersion.Items.Count)
                i = cmbServiceVersion.FindStringExact(pVersionName);
            cmbServiceVersion.SelectedIndex = (-1 < i ? i : 0);
        }
        #endregion SetVersion

        #region SetService
        public void SetService(string pServiceName)
        {
            int i = -1;
            if (0 < cmbService.Items.Count)
                i = cmbService.FindStringExact(pServiceName);
            cmbService.SelectedIndex = (-1 < i ? i : 0);
        }
        #endregion SetService


        /// <summary>
        /// Suppression du service {pServiceName}
        /// </summary>
        /// FI 20161020 [XXXXX] Modify
        private void RemoveInstanceService()
        {
            // FI 20161020 [XXXXX] sauvegarde de version et path
            ServiceInfo serviceInfo = new ServiceInfo(Manager.CurrentServiceName);
            string version = serviceInfo.GetVersion();
            string path = serviceInfo.GetInformationsKey(ServiceKeyEnum.PathInstall);

            try
            {
                SpheresServiceParametersHelper.CallDeleteInstallInformation(Manager.CurrentServiceName);
                SpheresServiceParametersHelper.DeleteService(Manager.CurrentServiceName, out int exitCode, out string standardOutput);

                if (0 == exitCode)
                {
                    // FI 20161020 [XXXXX] Call UpdateStartStopTools 
                    SpheresServiceParametersHelper.StartStopToolsDel(path);
                    // PM 20201119 [XXXXX] Démarrage de SpheresLogger en premier et arret en dernier
                    //SpheresServiceParametersHelper.StartStopToolsAdd(path, version, string.Empty);
                    SpheresServiceParametersHelper.StartStopToolsAdd(path, version);

                    OnServerChanged(cmbServer, null);
                }
                else
                {
                    DisplayError(true, standardOutput);
                }
            }
            catch (Exception ex)
            {
                // FI 20200910 [XXXXX] DisplayError d'une exception
                DisplayError(ex);
            }
        }

        #endregion Methods

        private void Label1_Click(object sender, EventArgs e)
        {

        }
    }
}
