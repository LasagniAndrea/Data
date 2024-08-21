namespace EFS.SpheresServiceParameters
{
    partial class BaseCollection
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

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblTitleSection = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblTitleSection
            // 
            this.lblTitleSection.AutoSize = true;
            this.lblTitleSection.Location = new System.Drawing.Point(13, 12);
            this.lblTitleSection.Name = "lblTitleSection";
            this.lblTitleSection.Size = new System.Drawing.Size(35, 13);
            this.lblTitleSection.TabIndex = 1;
            this.lblTitleSection.Text = "TEST";
            // 
            // BaseCollection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblTitleSection);
            this.MaximumSize = new System.Drawing.Size(488, 265);
            this.MinimumSize = new System.Drawing.Size(488, 265);
            this.Name = "BaseCollection";
            this.Size = new System.Drawing.Size(488, 265);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitleSection;
    }
}
