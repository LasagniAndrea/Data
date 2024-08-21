namespace EFS.SpheresServiceParameters.SampleForms
{
    partial class MOMFormParameters
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
            this.containerParametersButtons.Panel1.SuspendLayout();
            this.containerParametersButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // radioCollection1
            // 
            this.radioCollection1.BodyText = "Select the MOM (Message Oriented Middleware) used for your environment";
            this.radioCollection1.Button1Label = "File Watcher (Windows® file system)";
            this.radioCollection1.Button1Value = "FileWatcher";
            this.radioCollection1.Button2Label = "MSMQ® (Microsoft® Message Queuing)";
            this.radioCollection1.Button2Value = "MSMQ";
            this.radioCollection1.Button2Visible = true;
            this.radioCollection1.Button3Label = "IBM MQSeries® (IBM® WebSphere MQ)";
            this.radioCollection1.Button3Value = "IBMMQSeries";
            this.radioCollection1.Button3Visible = true;
            this.radioCollection1.ButtonProperty = "MOMTYPE";
            // 
            // containerParametersButtons
            // 
            // 
            // MOMFormParameters
            // 
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.BannerBitmap = ((System.Drawing.Image)(resources.GetObject("$this.BannerImage")));

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BannerText = "MOM information (1/2)";
            this.ClientSize = new System.Drawing.Size(499, 383);
            this.Name = "MOMFormParameters";
            this.containerParametersButtons.Panel1.ResumeLayout(false);
            this.containerParametersButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
    }
}