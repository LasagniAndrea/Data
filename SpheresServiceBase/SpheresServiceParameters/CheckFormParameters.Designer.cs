namespace EFS.SpheresServiceParameters
{
    partial class CheckFormParameters
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
            this.checkCollection1 = new EFS.SpheresServiceParameters.CheckCollection();
            this.containerParametersButtons.Panel1.SuspendLayout();
            this.containerParametersButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // containerParametersButtons.Panel1
            // 
            this.containerParametersButtons.Panel1.Controls.Add(this.checkCollection1);
            // 
            // checkCollection1
            // 
            this.checkCollection1.BodyText = "TEST";
            this.checkCollection1.Checkbox1Property = null;
            this.checkCollection1.Checkbox1Visible = true;
            this.checkCollection1.Checkbox2Property = null;
            this.checkCollection1.Checkbox3Property = null;
            this.checkCollection1.Location = new System.Drawing.Point(4, 4);
            this.checkCollection1.MaximumSize = new System.Drawing.Size(488, 265);
            this.checkCollection1.MinimumSize = new System.Drawing.Size(488, 265);
            this.checkCollection1.Name = "checkCollection1";
            this.checkCollection1.Size = new System.Drawing.Size(488, 265);
            this.checkCollection1.TabIndex = 0;
            // 
            // CheckFormParameters
            // 
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.BannerBitmap = ((System.Drawing.Image)(resources.GetObject("$this.BannerImage")));

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(499, 383);
            this.EnableBackButton = true;
            this.Name = "CheckFormParameters";
            this.Text = "CheckFormParameters";
            this.containerParametersButtons.Panel1.ResumeLayout(false);
            this.containerParametersButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        protected CheckCollection checkCollection1;
    }
}