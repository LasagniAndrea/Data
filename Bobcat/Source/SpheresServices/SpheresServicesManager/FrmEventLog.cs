#region Using Directives
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Text;
using System.Windows.Forms;

using EFS.ACommon;
#endregion Using Directives

namespace EFS.SpheresService
{
	/// <summary>
	/// Description résumée de FrmEventLog.
	/// </summary>
	public class FrmEventLog : Form
	{
		#region Members
		private string m_Service       = string.Empty;
		private ListViewItem lastItem  = null;
		private readonly ListViewColumnSorter lvwColumnSorter;

		private ListView lvwEventLog;
		private TextBox txtMessage;
		private ColumnHeader columnHeader_Date;
		private ColumnHeader columnHeader_MachineName;
		private ImageList imlEventLog;
		private ColumnHeader columnHeader_Source;
		private ColumnHeader columnHeader_Category;
		private ColumnHeader columnHeader_Event;
		private ColumnHeader columnHeader_User;
		private ToolTip toolTip1;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Panel pnlEventLog;
		private System.Windows.Forms.Button btnRefresh;
		private System.Windows.Forms.Button btnClear;
		private System.Windows.Forms.RadioButton rbBytes;
		private System.Windows.Forms.RadioButton rbWords;
		private System.Windows.Forms.CheckBox chkData;
		private System.Windows.Forms.TextBox txtData;
		private System.Windows.Forms.Timer timer2;
		private System.Windows.Forms.NumericUpDown updInterval;
		private System.Windows.Forms.Label lblInterval;
		private IContainer components;
		#endregion Members

		#region Accessors
		public string Service
		{
			set {m_Service = value;}
			get {return m_Service;}
		}
		#endregion Accessors
		#region Constructors
		public FrmEventLog()
		{
			InitializeComponent();
			// 
			this.columnHeader_Date.Text         = "Date";
			this.columnHeader_Date.Width        = 150;
			this.columnHeader_Source.Text       = "Source";
			this.columnHeader_Source.Width      = 200;
			this.columnHeader_Category.Text     = "Category";
			this.columnHeader_Category.Width    = 150;
			this.columnHeader_Event.Text        = "Event";
			this.columnHeader_Event.Width       = 60;
			this.columnHeader_User.Text         = "User";
			this.columnHeader_User.Width        = 150;
			this.columnHeader_MachineName.Text  = "MachineName";
			this.columnHeader_MachineName.Width = 120;

            lvwColumnSorter = new ListViewColumnSorter
            {
                Order = SortOrder.Descending
            };
            lvwEventLog.ListViewItemSorter = lvwColumnSorter;
		}
		#endregion Constructors

		#region Events
		#region Dispose
		protected override void Dispose( bool disposing )
		{
			if (disposing & (null != components))
				components.Dispose();
			base.Dispose( disposing );
		}
		#endregion Dispose
		#region OnColumnClick
		private void OnColumnClick(object o, ColumnClickEventArgs e)
		{
			if (e.Column == lvwColumnSorter.SortColumn)
			{
				if (SortOrder.Ascending == lvwColumnSorter.Order)
					lvwColumnSorter.Order = SortOrder.Descending;
				else
					lvwColumnSorter.Order = SortOrder.Ascending;
		    }
			else
			{
				lvwColumnSorter.SortColumn = e.Column;
				lvwColumnSorter.Order      = SortOrder.Ascending;
			}
			lvwEventLog.Sort();
		}
		#endregion OnColumnClick
		#region OnLoad
		private void OnLoad(object sender, System.EventArgs e)
		{
			if (StrFunc.IsFilled(m_Service))
				this.Text += " (" + m_Service + ")";
			RefreshEntries();
			DisplayData();
			SetUpTimer();
		}
		#endregion OnLoad
		#region btnClear_Click
		private void BtnClear_Click(object sender, System.EventArgs e)
		{
			DialogResult result = MessageBox.Show("Confirm to clearing the log entries ?", "Spheres Events log",
				MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
			if (DialogResult.OK == result)
			{
				if (EventLog.Exists(Cst.SpheresEventLog))
				{
					txtMessage.Text    = string.Empty;
					txtData.Text       = string.Empty;
					EventLog  eventLog = new EventLog(Cst.SpheresEventLog);
					eventLog.Clear();
					RefreshEntries();
				}
			}
		}
		#endregion btnClear_Click
		#region btnRefresh_Click
		private void BtnRefresh_Click(object sender, System.EventArgs e)
		{
			RefreshEntries();
		}
		#endregion btnRefresh_Click
		#region OnMouseMove
		private void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			ListViewItem item = lvwEventLog.GetItemAt(e.X,e.Y);
			if (null == item)
			{
				toolTip1.Active = false;
				timer1.Enabled  = false;
			}
			else if (lastItem != item)
			{
				toolTip1.Active = false;
				timer1.Tick    += new EventHandler(Timer1_Tick);
				timer1.Enabled  = true;
			}
			lastItem = item;
		}
		#endregion OnMouseMove
		#region OnSelectedIndexChanged
		private void OnSelectedIndexChanged(object sender, System.EventArgs e)
		{
			DisplayData();
		}
		#endregion OnSelectedIndexChanged
		#region OnTimer
		private void OnTimer(object sender, System.EventArgs e)
		{
			RefreshEntries();
			DisplayData();
		}
		#endregion OnTimer

		#region Timer1_Tick
		private void Timer1_Tick(object sender, System.EventArgs e)
		{
			toolTip1.Active = true;
			SpheresEventLogEntry entry = (SpheresEventLogEntry) lastItem.Tag;
			toolTip1.SetToolTip(lvwEventLog,string.Empty);
			timer1.Enabled  = false;
			timer1.Tick    -= new EventHandler(Timer1_Tick);
		}
		#endregion Timer1_Tick
		#endregion Events

		#region Methods
		#region DisplayData
		private void DisplayData()
		{
			bool isData = false;
			bool isItem = (0 < lvwEventLog.SelectedItems.Count);
			txtData.Text = string.Empty;
			txtMessage.Text = string.Empty;
            byte[] data = null;
			if (isItem)
			{
                SpheresEventLogEntry item = (SpheresEventLogEntry)lvwEventLog.SelectedItems[0].Tag;
                txtMessage.Text = item.Message;
                data = item.GetData();
                isData = (0 < data.Length);
			}
			bool isChecked   = chkData.Checked;
			txtData.Visible  = isChecked;
			rbBytes.Enabled  = isChecked & isData;
			rbWords.Enabled  = isChecked & isData;
			txtMessage.Width = isChecked?400:746;
			if (isData)
			{
				if (rbBytes.Checked)
                    txtData.Text = HexBytesEncodingToString(data);
				else if (rbWords.Checked)
                    txtData.Text = HexWordsEncodingToString(data);
			}
		}
		#endregion DisplayData
		#region GetEntries
		private SpheresEventLogEntry[] GetEntries()
		{
			EventLog  eventLog = new EventLog(Cst.SpheresEventLog);
			ArrayList aEntries = new ArrayList();

			foreach (EventLogEntry entry in eventLog.Entries)
			{
				if (StrFunc.IsEmpty(m_Service) || entry.Source.StartsWith(m_Service))
					aEntries.Add(new SpheresEventLogEntry(entry));
			}
			return (SpheresEventLogEntry[])aEntries.ToArray(typeof(SpheresEventLogEntry));
		}
		#endregion GetEntries
		#region HexBytesEncodingToString
        protected static string HexBytesEncodingToString(byte[] pBytes)
		{
			StrBuilder hex   = new StrBuilder();
			ArrayList aBytes = new ArrayList();
			int line         = 0;
			int lineOffset   = 0;
			for (int i=0;i<pBytes.Length;i++)
			{
				aBytes.Add(pBytes[i]);

				if ((8 == aBytes.Count) || (i==pBytes.Length-1))
				{
					lineOffset++;
					hex          += line.ToString("X4")+": ";
					int remainder = 8-aBytes.Count;
					byte[] result = (byte[])aBytes.ToArray(typeof(System.Byte));
					hex          += HexEncodingToString(result,true);
					if (0 < remainder)
						hex      += new String(' ',(remainder*2)+remainder);
					hex          += Encoding.ASCII.GetString(result) + Cst.CrLf;
					aBytes.Clear();
					line+=(0 == Math.IEEERemainder(lineOffset,2))?2:8;
				}
			}
			return hex.ToString();
		}
		#endregion HexBytesEncodingToString
		#region HexEncodingToString
        protected static string HexEncodingToString(byte[] pBytes)
		{
			return HexEncodingToString(pBytes,false);
		}

		protected static string HexEncodingToString(byte[] pBytes,bool pIsSeparator)
		{
			string hex = string.Empty;
			for (int i=0;i<pBytes.Length;i++)
			{
				hex += pBytes[i].ToString("X2");
				if (pIsSeparator)
					hex += " ";
			}
			return hex;
		}
		#endregion HexEncodingToString
		#region HexWordsEncodingToString
		protected static string HexWordsEncodingToString(byte[] pBytes)
		{
			StrBuilder hex   = new StrBuilder();
			ArrayList aBytes = new ArrayList();
			int line         = 0;
			int lineOffset   = 0;
			for (int i=0;i<pBytes.Length;i++)
			{
				aBytes.Insert(0,pBytes[i]);
				if ((4 == aBytes.Count) || (i==pBytes.Length-1))
				{
					if (0 == Math.IEEERemainder(lineOffset,4))
						hex += line.ToString().PadLeft(4,'0') + ": ";
					hex += HexEncodingToString((byte[])aBytes.ToArray(typeof(System.Byte))) + " ";
					aBytes.Clear();
					line+=10;
					lineOffset++;
				}
			}
			return hex.ToString();
		}
		#endregion HexWordsEncodingToString
		#region InitializeComponent
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmEventLog));
            this.lvwEventLog = new System.Windows.Forms.ListView();
            this.columnHeader_Date = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_Source = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_Category = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_Event = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_User = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_MachineName = new System.Windows.Forms.ColumnHeader();
            this.imlEventLog = new System.Windows.Forms.ImageList(this.components);
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.pnlEventLog = new System.Windows.Forms.Panel();
            this.lblInterval = new System.Windows.Forms.Label();
            this.updInterval = new System.Windows.Forms.NumericUpDown();
            this.rbWords = new System.Windows.Forms.RadioButton();
            this.rbBytes = new System.Windows.Forms.RadioButton();
            this.chkData = new System.Windows.Forms.CheckBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.txtData = new System.Windows.Forms.TextBox();
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.pnlEventLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updInterval)).BeginInit();
            this.SuspendLayout();
            // 
            // lvwEventLog
            // 
            this.lvwEventLog.AllowColumnReorder = true;
            this.lvwEventLog.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_Date,
            this.columnHeader_Source,
            this.columnHeader_Category,
            this.columnHeader_Event,
            this.columnHeader_User,
            this.columnHeader_MachineName});
            this.lvwEventLog.Dock = System.Windows.Forms.DockStyle.Top;
            this.lvwEventLog.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvwEventLog.FullRowSelect = true;
            this.lvwEventLog.GridLines = true;
            this.lvwEventLog.HideSelection = false;
            this.lvwEventLog.HoverSelection = true;
            this.lvwEventLog.Location = new System.Drawing.Point(0, 0);
            this.lvwEventLog.MultiSelect = false;
            this.lvwEventLog.Name = "lvwEventLog";
            this.lvwEventLog.Size = new System.Drawing.Size(874, 256);
            this.lvwEventLog.SmallImageList = this.imlEventLog;
            this.lvwEventLog.TabIndex = 1;
            this.lvwEventLog.UseCompatibleStateImageBehavior = false;
            this.lvwEventLog.View = System.Windows.Forms.View.Details;
            this.lvwEventLog.SelectedIndexChanged += new System.EventHandler(this.OnSelectedIndexChanged);
            this.lvwEventLog.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.OnColumnClick);
            this.lvwEventLog.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnMouseMove);
            // 
            // columnHeader_Event
            // 
            this.columnHeader_Event.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // imlEventLog
            // 
            this.imlEventLog.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imlEventLog.ImageStream")));
            this.imlEventLog.TransparentColor = System.Drawing.Color.Transparent;
            this.imlEventLog.Images.SetKeyName(0, "SubMenu-Red.png");
            this.imlEventLog.Images.SetKeyName(1, "SubMenu-Blue.png");
            this.imlEventLog.Images.SetKeyName(2, "SubMenu-Green.png");
            this.imlEventLog.Images.SetKeyName(3, "SubMenu-Orange.png");
            // 
            // txtMessage
            // 
            this.txtMessage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(216)))), ((int)(((byte)(232)))));
            this.txtMessage.Dock = System.Windows.Forms.DockStyle.Left;
            this.txtMessage.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtMessage.Location = new System.Drawing.Point(0, 256);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ReadOnly = true;
            this.txtMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMessage.Size = new System.Drawing.Size(400, 167);
            this.txtMessage.TabIndex = 3;
            // 
            // timer1
            // 
            this.timer1.Interval = 500;
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // pnlEventLog
            // 
            this.pnlEventLog.Controls.Add(this.lblInterval);
            this.pnlEventLog.Controls.Add(this.updInterval);
            this.pnlEventLog.Controls.Add(this.rbWords);
            this.pnlEventLog.Controls.Add(this.rbBytes);
            this.pnlEventLog.Controls.Add(this.chkData);
            this.pnlEventLog.Controls.Add(this.btnClear);
            this.pnlEventLog.Controls.Add(this.btnRefresh);
            this.pnlEventLog.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlEventLog.Location = new System.Drawing.Point(746, 256);
            this.pnlEventLog.Name = "pnlEventLog";
            this.pnlEventLog.Size = new System.Drawing.Size(128, 167);
            this.pnlEventLog.TabIndex = 5;
            // 
            // lblInterval
            // 
            this.lblInterval.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblInterval.Location = new System.Drawing.Point(8, 67);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Size = new System.Drawing.Size(72, 23);
            this.lblInterval.TabIndex = 12;
            this.lblInterval.Text = "Refresh (sec)";
            // 
            // updInterval
            // 
            this.updInterval.BackColor = System.Drawing.SystemColors.Window;
            this.updInterval.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.updInterval.Location = new System.Drawing.Point(81, 64);
            this.updInterval.Maximum = new decimal(new int[] {
            600,
            0,
            0,
            0});
            this.updInterval.Name = "updInterval";
            this.updInterval.ReadOnly = true;
            this.updInterval.Size = new System.Drawing.Size(40, 20);
            this.updInterval.TabIndex = 11;
            this.updInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.updInterval.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.updInterval.ValueChanged += new System.EventHandler(this.UpdInterval_ValueChanged);
            // 
            // rbWords
            // 
            this.rbWords.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rbWords.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbWords.Location = new System.Drawing.Point(67, 32);
            this.rbWords.Name = "rbWords";
            this.rbWords.Size = new System.Drawing.Size(57, 24);
            this.rbWords.TabIndex = 10;
            this.rbWords.Text = "words";
            this.rbWords.CheckedChanged += new System.EventHandler(this.OnSelectedIndexChanged);
            // 
            // rbBytes
            // 
            this.rbBytes.Checked = true;
            this.rbBytes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.rbBytes.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rbBytes.Location = new System.Drawing.Point(8, 32);
            this.rbBytes.Name = "rbBytes";
            this.rbBytes.Size = new System.Drawing.Size(57, 24);
            this.rbBytes.TabIndex = 9;
            this.rbBytes.TabStop = true;
            this.rbBytes.Text = "bytes";
            this.rbBytes.CheckedChanged += new System.EventHandler(this.OnSelectedIndexChanged);
            // 
            // chkData
            // 
            this.chkData.BackColor = System.Drawing.SystemColors.Control;
            this.chkData.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(86)))), ((int)(((byte)(159)))));
            this.chkData.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkData.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkData.Location = new System.Drawing.Point(8, 8);
            this.chkData.Name = "chkData";
            this.chkData.Size = new System.Drawing.Size(80, 24);
            this.chkData.TabIndex = 8;
            this.chkData.Text = "See Data";
            this.chkData.UseVisualStyleBackColor = false;
            this.chkData.CheckedChanged += new System.EventHandler(this.OnSelectedIndexChanged);
            // 
            // btnClear
            // 
            this.btnClear.BackColor = System.Drawing.Color.Transparent;
            this.btnClear.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnClear.BackgroundImage")));
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnClear.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClear.ForeColor = System.Drawing.Color.White;
            this.btnClear.Location = new System.Drawing.Point(21, 139);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(70, 23);
            this.btnClear.TabIndex = 7;
            this.btnClear.Text = "Purge";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new System.EventHandler(this.BtnClear_Click);
            // 
            // btnRefresh
            // 
            this.btnRefresh.BackColor = System.Drawing.Color.Transparent;
            this.btnRefresh.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnRefresh.BackgroundImage")));
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.ForeColor = System.Drawing.Color.White;
            this.btnRefresh.Location = new System.Drawing.Point(21, 107);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(70, 23);
            this.btnRefresh.TabIndex = 6;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = false;
            this.btnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
            // 
            // txtData
            // 
            this.txtData.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(216)))), ((int)(((byte)(232)))));
            this.txtData.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtData.Dock = System.Windows.Forms.DockStyle.Left;
            this.txtData.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtData.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(139)))), ((int)(((byte)(86)))), ((int)(((byte)(159)))));
            this.txtData.Location = new System.Drawing.Point(400, 256);
            this.txtData.Multiline = true;
            this.txtData.Name = "txtData";
            this.txtData.ReadOnly = true;
            this.txtData.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtData.Size = new System.Drawing.Size(346, 167);
            this.txtData.TabIndex = 6;
            // 
            // timer2
            // 
            this.timer2.Tick += new System.EventHandler(this.OnTimer);
            // 
            // FrmEventLog
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(874, 423);
            this.Controls.Add(this.txtData);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.pnlEventLog);
            this.Controls.Add(this.lvwEventLog);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmEventLog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Spheres Event Log";
            this.Load += new System.EventHandler(this.OnLoad);
            this.pnlEventLog.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.updInterval)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion InitializeComponent
		#region RefreshEntries
		private void RefreshEntries()
		{
			lvwEventLog.BeginUpdate();
			SpheresEventLogEntry[] entries = GetEntries();
			lvwEventLog.Items.Clear();
			txtMessage.Text = string.Empty;
			txtData.Text    = string.Empty;
			ListViewItem item;
			foreach (SpheresEventLogEntry entry in entries)
			{
                item = new ListViewItem(DtFunc.DateTimeToString(entry.Date, DtFunc.FmtDateLongTime))
                {
                    Tag = entry,
                    ImageIndex = entry.ImageIndex,
                    ForeColor = entry.ForeColor,
                };
                item.SubItems.Add(entry.Source);
				item.SubItems.Add(entry.Category);
				item.SubItems.Add(entry.Event.ToString());
				item.SubItems.Add(entry.User);
				item.SubItems.Add(entry.MachineName);
				lvwEventLog.Items.Add(item);
			}
			lvwEventLog.EndUpdate();
		}
		#endregion RefreshEntries
		#region SetUpTimer
		private void SetUpTimer()
		{
			timer2.Start();
			timer2.Enabled = (0 <updInterval.Value);
			if (timer2.Enabled)
				timer2.Interval  = (int)updInterval.Value * 1000;
		}
		#endregion SetUpTimer

		private void UpdInterval_ValueChanged(object sender, System.EventArgs e)
		{
			SetUpTimer();
		}

		#endregion Methods
	}
}
