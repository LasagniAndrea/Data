namespace EFS.SpheresServiceParameters
{
    partial class CheckCollection
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
            this.checkButton1 = new System.Windows.Forms.CheckBox();
            this.checkButton2 = new System.Windows.Forms.CheckBox();
            this.checkButton3 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // checkButton1
            // 
            this.checkButton1.AutoSize = true;
            this.checkButton1.Location = new System.Drawing.Point(23, 93);
            this.checkButton1.Name = "checkButton1";
            this.checkButton1.Size = new System.Drawing.Size(14, 13);
            this.checkButton1.TabIndex = 2;
            this.checkButton1.TabStop = true;
            this.checkButton1.UseVisualStyleBackColor = true;
            // 
            // checkButton2
            // 
            this.checkButton2.AutoSize = true;
            this.checkButton2.Location = new System.Drawing.Point(23, 134);
            this.checkButton2.Name = "checkButton2";
            this.checkButton2.Size = new System.Drawing.Size(14, 13);
            this.checkButton2.TabIndex = 3;
            this.checkButton2.UseVisualStyleBackColor = true;
            this.checkButton2.Visible = false;
            // 
            // checkButton3
            // 
            this.checkButton3.AutoSize = true;
            this.checkButton3.Location = new System.Drawing.Point(23, 175);
            this.checkButton3.Name = "checkButton3";
            this.checkButton3.Size = new System.Drawing.Size(14, 13);
            this.checkButton3.TabIndex = 4;
            this.checkButton3.UseVisualStyleBackColor = true;
            this.checkButton3.Visible = false;
            // 
            // RadioCollection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.checkButton3);
            this.Controls.Add(this.checkButton2);
            this.Controls.Add(this.checkButton1);
            this.Name = "CheckCollection";
            this.Controls.SetChildIndex(this.checkButton1, 0);
            this.Controls.SetChildIndex(this.checkButton2, 0);
            this.Controls.SetChildIndex(this.checkButton3, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.CheckBox checkButton1;
        private System.Windows.Forms.CheckBox checkButton2;
        private System.Windows.Forms.CheckBox checkButton3;

        #endregion
    }
}
