namespace EFS.SpheresServiceParameters
{
    partial class TextFormParameters
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
        /// Méthode requise pour la prise en charge du concepteur - ne" modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BaseFormParameters));
            this.textCollection1 = new EFS.SpheresServiceParameters.TextCollection();
            this.containerParametersButtons.Panel1.SuspendLayout();
            this.containerParametersButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // containerParametersButtons
            // 
            // 
            // containerParametersButtons.Panel1
            // 
            this.containerParametersButtons.Panel1.Controls.Add(this.textCollection1);
            // 
            // textCollection1
            // 
            this.textCollection1.BodyText = "TEST";
            this.textCollection1.Edit1Label = "label2";
            this.textCollection1.Edit1Property = null;
            this.textCollection1.Edit2Label = "label3";
            this.textCollection1.Edit2Property = null;
            this.textCollection1.Edit3Label = "label5";
            this.textCollection1.Edit3Property = null;
            this.textCollection1.Edit4Label = "label4";
            this.textCollection1.Edit4Property = null;
            this.textCollection1.Location = new System.Drawing.Point(4, 4);
            this.textCollection1.MaximumSize = new System.Drawing.Size(488, 265);
            this.textCollection1.MinimumSize = new System.Drawing.Size(488, 265);
            this.textCollection1.Name = "textCollection1";
            this.textCollection1.Size = new System.Drawing.Size(488, 265);
            this.textCollection1.TabIndex = 0;
            // 
            // TextFormParameters
            // 
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.BannerBitmap = ((System.Drawing.Image)(resources.GetObject("$this.BannerImage")));

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(499, 383);
            this.EnableBackButton = true;
            this.Name = "TextFormParameters";
            this.Text = "Form1";
            this.containerParametersButtons.Panel1.ResumeLayout(false);
            this.containerParametersButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        protected TextCollection textCollection1;
    }
}