#region Using Directives
using System;
using System.Drawing;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Configuration;
using System.ComponentModel;
using System.IO;
using System.ServiceProcess;
using System.Windows.Forms;

using EFS.ACommon;
using EFS.Common;
using NetworkManagement;
#endregion Using Directives

namespace EFS.SpheresService
{
	/// <summary>
	/// Description résumée de FrmManagerOptions.
	/// </summary>
	public class FrmManagerOptions : Form
	{
		private Label lblDefaultServer;
		private ComboBox cmbDefaultServer;
		private Label lblDefaultService;
        private ComboBox cmbDefaultVersion;
		private ComboBox cmbDefaultService;
		private Label lblInterval;
		private NumericUpDown updInterval;
		private CheckBox chkControlService;
		private Button btnOk;
		private Button btnExit;
		private ImageList imlButton;
		private CheckBox chkWaitForStatusPending;
        private Dictionary<string, List<Pair<Pair<string, Cst.ServiceEnum>, ServiceController>>> m_DicServices;
        private Panel pnlButton;

        string defaultServer;
        string defaultVersion;
        string defaultService;
        
		private System.ComponentModel.IContainer components;

		public FrmManagerOptions()
		{
			InitializeComponent();
			this.chkControlService.Text       = Ressource.GetString("ControlService");
			this.chkWaitForStatusPending.Text = Ressource.GetString("WaitForStatusPending");
			this.Text                         = Ressource.GetString("OptionTitle");
			this.lblInterval.Text             = Ressource.GetString("Interval");
			GetConfigValues();
		}

		protected override void Dispose( bool disposing )
		{
			if (disposing && (null != components))
				components.Dispose();
			base.Dispose(disposing);
		}

		#region Code généré par le Concepteur Windows Form
		/// <summary>
		/// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
		/// le contenu de cette méthode avec l'éditeur de code.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmManagerOptions));
            this.lblDefaultServer = new System.Windows.Forms.Label();
            this.cmbDefaultServer = new System.Windows.Forms.ComboBox();
            this.lblDefaultService = new System.Windows.Forms.Label();
            this.cmbDefaultService = new System.Windows.Forms.ComboBox();
            this.lblInterval = new System.Windows.Forms.Label();
            this.updInterval = new System.Windows.Forms.NumericUpDown();
            this.chkControlService = new System.Windows.Forms.CheckBox();
            this.btnOk = new System.Windows.Forms.Button();
            this.imlButton = new System.Windows.Forms.ImageList(this.components);
            this.btnExit = new System.Windows.Forms.Button();
            this.chkWaitForStatusPending = new System.Windows.Forms.CheckBox();
            this.cmbDefaultVersion = new System.Windows.Forms.ComboBox();
            this.pnlButton = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.updInterval)).BeginInit();
            this.pnlButton.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblDefaultServer
            // 
            this.lblDefaultServer.Location = new System.Drawing.Point(8, 8);
            this.lblDefaultServer.Name = "lblDefaultServer";
            this.lblDefaultServer.Size = new System.Drawing.Size(136, 23);
            this.lblDefaultServer.TabIndex = 0;
            this.lblDefaultServer.Text = "Default server";
            this.lblDefaultServer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmbDefaultServer
            // 
            this.cmbDefaultServer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDefaultServer.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(86)))), ((int)(((byte)(159)))));
            this.cmbDefaultServer.Location = new System.Drawing.Point(100, 8);
            this.cmbDefaultServer.Name = "cmbDefaultServer";
            this.cmbDefaultServer.Size = new System.Drawing.Size(354, 21);
            this.cmbDefaultServer.TabIndex = 1;
            this.cmbDefaultServer.SelectedIndexChanged += new System.EventHandler(this.OnServerChanged);
            // 
            // lblDefaultService
            // 
            this.lblDefaultService.Location = new System.Drawing.Point(8, 40);
            this.lblDefaultService.Name = "lblDefaultService";
            this.lblDefaultService.Size = new System.Drawing.Size(136, 23);
            this.lblDefaultService.TabIndex = 2;
            this.lblDefaultService.Text = "Default service";
            this.lblDefaultService.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cmbDefaultService
            // 
            this.cmbDefaultService.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDefaultService.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(86)))), ((int)(((byte)(159)))));
            this.cmbDefaultService.Location = new System.Drawing.Point(178, 40);
            this.cmbDefaultService.Name = "cmbDefaultService";
            this.cmbDefaultService.Size = new System.Drawing.Size(276, 21);
            this.cmbDefaultService.TabIndex = 3;
            // 
            // lblInterval
            // 
            this.lblInterval.Location = new System.Drawing.Point(8, 72);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Size = new System.Drawing.Size(264, 23);
            this.lblInterval.TabIndex = 4;
            this.lblInterval.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // updInterval
            // 
            this.updInterval.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.updInterval.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(86)))), ((int)(((byte)(159)))));
            this.updInterval.Location = new System.Drawing.Point(390, 72);
            this.updInterval.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this.updInterval.Name = "updInterval";
            this.updInterval.Size = new System.Drawing.Size(64, 20);
            this.updInterval.TabIndex = 5;
            this.updInterval.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // chkControlService
            // 
            this.chkControlService.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkControlService.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkControlService.Location = new System.Drawing.Point(9, 96);
            this.chkControlService.Name = "chkControlService";
            this.chkControlService.Size = new System.Drawing.Size(327, 20);
            this.chkControlService.TabIndex = 6;
            this.chkControlService.Text = "Check the action on the services";
            // 
            // btnOk
            // 
            this.btnOk.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnOk.BackgroundImage")));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOk.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOk.ForeColor = System.Drawing.Color.White;
            this.btnOk.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOk.Location = new System.Drawing.Point(357, 9);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(50, 23);
            this.btnOk.TabIndex = 7;
            this.btnOk.Text = "Ok";
            this.btnOk.Click += new System.EventHandler(this.OnValid);
            // 
            // imlButton
            // 
            this.imlButton.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imlButton.ImageStream")));
            this.imlButton.TransparentColor = System.Drawing.Color.Transparent;
            this.imlButton.Images.SetKeyName(0, "");
            this.imlButton.Images.SetKeyName(1, "");
            // 
            // btnExit
            // 
            this.btnExit.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnExit.BackgroundImage")));
            this.btnExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnExit.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnExit.ForeColor = System.Drawing.Color.White;
            this.btnExit.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnExit.ImageKey = "(aucun)";
            this.btnExit.Location = new System.Drawing.Point(413, 9);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(50, 23);
            this.btnExit.TabIndex = 0;
            this.btnExit.Text = "Exit";
            // 
            // chkWaitForStatusPending
            // 
            this.chkWaitForStatusPending.Checked = true;
            this.chkWaitForStatusPending.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkWaitForStatusPending.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(86)))), ((int)(((byte)(159)))));
            this.chkWaitForStatusPending.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(181)))), ((int)(((byte)(211)))));
            this.chkWaitForStatusPending.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkWaitForStatusPending.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkWaitForStatusPending.Location = new System.Drawing.Point(9, 123);
            this.chkWaitForStatusPending.Name = "chkWaitForStatusPending";
            this.chkWaitForStatusPending.Size = new System.Drawing.Size(364, 28);
            this.chkWaitForStatusPending.TabIndex = 8;
            this.chkWaitForStatusPending.Text = "Don\'t wait the total execution of the action asked for the service";
            // 
            // cmbDefaultVersion
            // 
            this.cmbDefaultVersion.BackColor = System.Drawing.SystemColors.Window;
            this.cmbDefaultVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDefaultVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(86)))), ((int)(((byte)(159)))));
            this.cmbDefaultVersion.Location = new System.Drawing.Point(100, 40);
            this.cmbDefaultVersion.Name = "cmbDefaultVersion";
            this.cmbDefaultVersion.Size = new System.Drawing.Size(72, 21);
            this.cmbDefaultVersion.TabIndex = 9;
            this.cmbDefaultVersion.SelectedIndexChanged += new System.EventHandler(this.OnVersionChanged);
            // 
            // pnlButton
            // 
            this.pnlButton.Controls.Add(this.btnOk);
            this.pnlButton.Controls.Add(this.btnExit);
            this.pnlButton.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButton.Location = new System.Drawing.Point(0, 157);
            this.pnlButton.Name = "pnlButton";
            this.pnlButton.Size = new System.Drawing.Size(466, 35);
            this.pnlButton.TabIndex = 0;
            // 
            // FrmManagerOptions
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this.btnExit;
            this.ClientSize = new System.Drawing.Size(466, 192);
            this.Controls.Add(this.pnlButton);
            this.Controls.Add(this.cmbDefaultVersion);
            this.Controls.Add(this.chkWaitForStatusPending);
            this.Controls.Add(this.chkControlService);
            this.Controls.Add(this.updInterval);
            this.Controls.Add(this.lblInterval);
            this.Controls.Add(this.cmbDefaultService);
            this.Controls.Add(this.lblDefaultService);
            this.Controls.Add(this.cmbDefaultServer);
            this.Controls.Add(this.lblDefaultServer);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmManagerOptions";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.updInterval)).EndInit();
            this.pnlButton.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		#region Methods
		#region GetConfigValues
		private void GetConfigValues()
		{
			try
			{
				StringDictionary settings = Manager.ReadConfigValues();
				if (null != settings)
				{
					updInterval.Value    = Convert.ToInt32(settings["Interval"]);
                    chkControlService.Checked = Convert.ToBoolean(settings["ControlService"]);
                    chkWaitForStatusPending.Checked = Convert.ToBoolean(settings["WaitForStatusPending"]);
                    string defaultServer = settings["DefaultServer"];
                    if (("." == defaultServer) || ("local" == defaultServer))
                        defaultServer = Environment.MachineName;
                    defaultVersion = settings["DefaultVersion"];
                    if (StrFunc.IsEmpty(defaultVersion))
                        defaultVersion = Manager.AssemblyVersion;
                    defaultService = settings["DefaultService"];
                    GetServerNames();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message.ToString(),"GetConfigValues" ,MessageBoxButtons.OK,MessageBoxIcon.Error);
			}
		}
		#endregion GetConfigValues

		// EG 20140425 Gestion Custom.config
        private void OnValid(object sender, System.EventArgs e)
		{
			try
			{
                Config config = new Config(Path.GetDirectoryName(Application.ExecutablePath) + Manager.servicesManagerCustomConfigSource);
                if (null == config)
                    config = new Config(Application.ExecutablePath + ".config");
				if (null != config)
				{
					if (null != cmbDefaultServer.SelectedItem)
						config.SaveSetting("DefaultServer",cmbDefaultServer.SelectedItem.ToString());
                    if (null != cmbDefaultVersion.SelectedItem)
                        config.SaveSetting("DefaultVersion", cmbDefaultVersion.SelectedItem.ToString());
					if (null != cmbDefaultService.SelectedItem) 
						config.SaveSetting("DefaultService", cmbDefaultService.SelectedItem.ToString());
					config.SaveSetting("Interval",updInterval.Value.ToString());
					config.SaveSetting("ControlService",chkControlService.Checked.ToString());
					config.SaveSetting("WaitForStatusPending",chkWaitForStatusPending.Checked.ToString());
					config.Close();
					Manager.Interval               = Convert.ToInt32(updInterval.Value);
					Manager.IsControlService       = chkControlService.Checked;
					Manager.IsWaitForStatusPending = chkWaitForStatusPending.Checked;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message.ToString(),"OnValid" ,MessageBoxButtons.OK,MessageBoxIcon.Error);
			}
		}
		#endregion Methods

        #region OnServerChanged
        /// <summary>
        /// Rechargement du dictionnaire des services par version après changement du serveur
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnServerChanged(object sender, System.EventArgs e)
        {
            m_DicServices = Manager.InitializeServices(cmbDefaultServer.SelectedItem.ToString());
            defaultServer = cmbDefaultServer.SelectedItem.ToString();
            BindControlVersion();
            BindControlServices();
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
            BindControlServices();
            defaultVersion = cmbDefaultVersion.SelectedItem.ToString();
        }
        #endregion OnVersionChanged

        #region OnServiceChanged
        private void OnServiceChanged(object sender, System.EventArgs e)
        {
            defaultService = cmbDefaultService.SelectedItem.ToString();
        }
        #endregion OnServiceChanged

        public void GetServerNames()
        {
            cmbDefaultServer.Items.Clear();
            try
            {
                NetServers netServers = new NetServers(ServerTypeEnum.SERVER);
                string[] servers = NetServers.GetServers();
                if (0 < servers.Length)
                {
                    foreach (string server in servers)
                    {
                        if (server.Equals(Environment.MachineName))
                            cmbDefaultServer.Items.Insert(0, server);
                        else
                            cmbDefaultServer.Items.Add(server);
                    }
                }
                else
                    cmbDefaultServer.Items.Add(Environment.MachineName);

                if (0 < cmbDefaultServer.Items.Count)
                {
                    int i = cmbDefaultServer.FindStringExact(defaultServer);
                    cmbDefaultServer.SelectedIndex = (-1 < i ? i : 0);
                }
            }
            // EG 20160404 Migration vs2013
            catch (Exception)
            {
                cmbDefaultServer.SelectedIndex = -1;
            }
        }


        #region BindControlVersion
        /// <summary>
        /// Chargement des versions disponible pour les services Spheres
        /// </summary>
        /// <param name="pDicServices"></param>
        /// <param name="pCmbVersion"></param>
        private void BindControlVersion()
        {
            cmbDefaultVersion.Items.Clear();
            if (null != m_DicServices)
            {
                foreach (string key in m_DicServices.Keys)
                    cmbDefaultVersion.Items.Add(key);
                int i = cmbDefaultVersion.FindStringExact(defaultVersion);
                cmbDefaultVersion.SelectedIndex = (-1 < i ? i : 0);
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
        private void BindControlServices()
        {

            string version = cmbDefaultVersion.SelectedItem.ToString();
            cmbDefaultService.Items.Clear();
            if ((null != m_DicServices) && m_DicServices.ContainsKey(version))
                m_DicServices[version].ForEach(item => {cmbDefaultService.Items.Add(item.First.First.ToString());});
            int i = cmbDefaultService.FindStringExact(defaultService);
            cmbDefaultService.SelectedIndex = (-1 < i ? i : 0);
        }
        #endregion BindControlServices
	}
}
