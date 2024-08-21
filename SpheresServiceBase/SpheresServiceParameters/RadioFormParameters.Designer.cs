namespace EFS.SpheresServiceParameters
{
    partial class RadioFormParameters
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
            this.radioCollection1 = new EFS.SpheresServiceParameters.RadioCollection();
            this.containerParametersButtons.Panel1.SuspendLayout();
            this.containerParametersButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // containerParametersButtons
            // 
            // 
            // containerParametersButtons.Panel1
            // 
            this.containerParametersButtons.Panel1.Controls.Add(this.radioCollection1);
            // 
            // radioCollection1
            // 
            this.radioCollection1.BodyText = "TEST";
            this.radioCollection1.Button1Value = null;
            this.radioCollection1.Button2Value = null;
            this.radioCollection1.Button3Value = null;
            this.radioCollection1.Location = new System.Drawing.Point(4, 4);
            this.radioCollection1.MaximumSize = new System.Drawing.Size(488, 265);
            this.radioCollection1.MinimumSize = new System.Drawing.Size(488, 265);
            this.radioCollection1.Name = "radioCollection1";
            this.radioCollection1.Size = new System.Drawing.Size(488, 265);
            this.radioCollection1.TabIndex = 0;
            // 
            // RadioFormParameters
            // 
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.BannerBitmap = ((System.Drawing.Image)(resources.GetObject("$this.BannerImage")));

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(499, 383);
            this.EnableBackButton = true;
            this.Name = "RadioFormParameters";
            this.Text = "Form1";
            this.containerParametersButtons.Panel1.ResumeLayout(false);
            this.containerParametersButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        protected RadioCollection radioCollection1;
    }
}