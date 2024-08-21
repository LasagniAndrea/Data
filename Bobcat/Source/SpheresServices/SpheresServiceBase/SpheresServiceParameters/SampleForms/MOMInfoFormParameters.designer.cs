namespace EFS.SpheresServiceParameters.SampleForms
{
    /// <summary>
    /// Formulaire avec les zones MOMPATH et INSTANCE
    /// </summary>
    partial class MOMInfoFormParameters
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
            // textCollection1
            // 
            this.textCollection1.BodyText = "Enter the folder or the queue used by the MOM (Message Oriented Middleware)      " +
                "                            and Error Email parameters";
            this.textCollection1.Edit1Label = "Folder or Queue :";
            this.textCollection1.Edit1Property = "MOMPATH";
            this.textCollection1.Edit1Visible = true;
            this.textCollection1.Edit2Label = "Instance Name:";
            this.textCollection1.Edit2Property = "INSTANCE";
            this.textCollection1.Edit2Visible = true;
            this.textCollection1.Edit3Label = "";
            this.textCollection1.Edit3Property = "";
            this.textCollection1.Edit4Label = "";
            this.textCollection1.Edit4Property = "";
            // 
            // containerParametersButtons
            // 
            // 
            // MOMInfoFormParameters
            // 
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.BannerBitmap = ((System.Drawing.Image)(resources.GetObject("$this.BannerImage")));

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BannerText = "MOM information (2/2)";
            this.ClientSize = new System.Drawing.Size(499, 383);
            this.Name = "MOMInfoFormParameters";
            this.containerParametersButtons.Panel1.ResumeLayout(false);
            this.containerParametersButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
    }
}