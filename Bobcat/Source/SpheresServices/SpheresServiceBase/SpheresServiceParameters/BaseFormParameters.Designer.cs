namespace EFS.SpheresServiceParameters
{
    partial class BaseFormParameters
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BaseFormParameters));
            this.containerBannerControls = new System.Windows.Forms.SplitContainer();
            this.lblTitle = new System.Windows.Forms.Label();
            this.containerParametersButtons = new System.Windows.Forms.SplitContainer();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.containerBannerControls.Panel1.SuspendLayout();
            this.containerBannerControls.Panel2.SuspendLayout();
            this.containerBannerControls.SuspendLayout();
            this.containerParametersButtons.Panel2.SuspendLayout();
            this.containerParametersButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // containerBannerControls
            // 
            this.containerBannerControls.Dock = System.Windows.Forms.DockStyle.Fill;
            this.containerBannerControls.IsSplitterFixed = true;
            this.containerBannerControls.Location = new System.Drawing.Point(0, 0);
            this.containerBannerControls.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.containerBannerControls.Name = "containerBannerControls";
            this.containerBannerControls.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // containerBannerControls.Panel1
            // 
            this.containerBannerControls.Panel1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BannerImage")));
            this.containerBannerControls.Panel1.Controls.Add(this.lblTitle);

            // 
            // containerBannerControls.Panel2
            // 
            this.containerBannerControls.Panel2.Controls.Add(this.containerParametersButtons);
            this.containerBannerControls.Size = new System.Drawing.Size(499, 383);
            this.containerBannerControls.SplitterDistance = 72;
            this.containerBannerControls.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.25F);
            this.lblTitle.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblTitle.Location = new System.Drawing.Point(12, 9);
            this.lblTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(209, 20);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Database information (1/2)";
            // 
            // containerParametersButtons
            // 
            this.containerParametersButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.containerParametersButtons.IsSplitterFixed = true;
            this.containerParametersButtons.Location = new System.Drawing.Point(0, 0);
            this.containerParametersButtons.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.containerParametersButtons.Name = "containerParametersButtons";
            this.containerParametersButtons.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // containerParametersButtons.Panel2
            // 
            this.containerParametersButtons.Panel2.Controls.Add(this.btnNext);
            this.containerParametersButtons.Panel2.Controls.Add(this.btnBack);
            this.containerParametersButtons.Panel2.Controls.Add(this.btnCancel);
            this.containerParametersButtons.Size = new System.Drawing.Size(499, 307);
            this.containerParametersButtons.SplitterDistance = 255;
            this.containerParametersButtons.TabIndex = 0;
            // 
            // btnNext
            // 
            this.btnNext.Location = new System.Drawing.Point(398, 13);
            this.btnNext.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(88, 23);
            this.btnNext.TabIndex = 2;
            this.btnNext.Text = "&Next >";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.BtnNext_Click);
            // 
            // btnBack
            // 
            this.btnBack.Enabled = false;
            this.btnBack.Location = new System.Drawing.Point(294, 13);
            this.btnBack.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(88, 23);
            this.btnBack.TabIndex = 1;
            this.btnBack.Text = "< &Back";
            this.btnBack.UseVisualStyleBackColor = true;
            this.btnBack.Click += new System.EventHandler(this.BtnBack_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(190, 13);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 23);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // BaseFormParameters
            // 
            this.AcceptButton = this.btnNext;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(499, 383);
            this.Controls.Add(this.containerBannerControls);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.MaximizeBox = false;
            this.Name = "BaseFormParameters";
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Load += new System.EventHandler(this.BaseFormParameters_Load);
            this.containerBannerControls.Panel1.ResumeLayout(false);
            this.containerBannerControls.Panel1.PerformLayout();
            this.containerBannerControls.Panel2.ResumeLayout(false);
            this.containerBannerControls.ResumeLayout(false);
            this.containerParametersButtons.Panel2.ResumeLayout(false);
            this.containerParametersButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.SplitContainer containerBannerControls;
        private System.Windows.Forms.Label lblTitle;
        protected System.Windows.Forms.SplitContainer containerParametersButtons;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Button btnCancel;
    }
}