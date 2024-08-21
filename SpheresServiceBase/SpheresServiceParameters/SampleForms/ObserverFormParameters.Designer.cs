namespace EFS.SpheresServiceParameters.SampleForms
{
    partial class ObserverFormParameters
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ObserverFormParameters));
            ((System.ComponentModel.ISupportInitialize)(this.containerBannerControls)).BeginInit();
            this.containerBannerControls.Panel2.SuspendLayout();
            this.containerBannerControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.containerParametersButtons)).BeginInit();
            this.containerParametersButtons.Panel1.SuspendLayout();
            this.containerParametersButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkCollection1
            // 
            this.checkCollection1.BodyText = "Check to activate the service observer";
            this.checkCollection1.Checkbox1Label = "Activate";
            this.checkCollection1.Checkbox1Property = "ACTIVATEOBSERVER";
            this.checkCollection1.Checkbox1Visible = true;
            // 
            // containerBannerControls
            // 
            // 
            // containerBannerControls.Panel1
            // 
            this.containerBannerControls.Panel1.BackgroundImage = null;
            // 
            // containerParametersButtons
            // 
            // 
            // ObserverFormParameters
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BannerBitmap = null;
            this.BannerText = "Service observer (activity control)";
            this.ClientSize = new System.Drawing.Size(499, 383);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ObserverFormParameters";
            this.Text = "ObserverFormParameters";
            this.containerBannerControls.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.containerBannerControls)).EndInit();
            this.containerBannerControls.ResumeLayout(false);
            this.containerParametersButtons.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.containerParametersButtons)).EndInit();
            this.containerParametersButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
    }
}