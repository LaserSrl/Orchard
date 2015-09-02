namespace Laser.Orchard.TranslationChecker {
    partial class Form1 {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.btnCheckTraduzioni = new System.Windows.Forms.Button();
            this.cmbLingue = new System.Windows.Forms.ComboBox();
            this.txtLogOperations = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.chkTranslations = new System.Windows.Forms.CheckedListBox();
            this.btnCheckUncheckAll = new System.Windows.Forms.Button();
            this.btnAskForTranslations = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnCheckTraduzioni
            // 
            this.btnCheckTraduzioni.Location = new System.Drawing.Point(802, 12);
            this.btnCheckTraduzioni.Name = "btnCheckTraduzioni";
            this.btnCheckTraduzioni.Size = new System.Drawing.Size(134, 23);
            this.btnCheckTraduzioni.TabIndex = 0;
            this.btnCheckTraduzioni.Text = "Missing translations";
            this.btnCheckTraduzioni.UseVisualStyleBackColor = true;
            this.btnCheckTraduzioni.Click += new System.EventHandler(this.btnCheckTraduzioni_Click);
            // 
            // cmbLingue
            // 
            this.cmbLingue.FormattingEnabled = true;
            this.cmbLingue.Location = new System.Drawing.Point(57, 14);
            this.cmbLingue.Name = "cmbLingue";
            this.cmbLingue.Size = new System.Drawing.Size(121, 21);
            this.cmbLingue.TabIndex = 1;
            // 
            // txtLogOperations
            // 
            this.txtLogOperations.Location = new System.Drawing.Point(13, 56);
            this.txtLogOperations.Multiline = true;
            this.txtLogOperations.Name = "txtLogOperations";
            this.txtLogOperations.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLogOperations.Size = new System.Drawing.Size(448, 529);
            this.txtLogOperations.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(39, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Lingua";
            // 
            // chkTranslations
            // 
            this.chkTranslations.FormattingEnabled = true;
            this.chkTranslations.Location = new System.Drawing.Point(487, 56);
            this.chkTranslations.Name = "chkTranslations";
            this.chkTranslations.Size = new System.Drawing.Size(449, 484);
            this.chkTranslations.TabIndex = 4;
            // 
            // btnCheckUncheckAll
            // 
            this.btnCheckUncheckAll.Location = new System.Drawing.Point(487, 561);
            this.btnCheckUncheckAll.Name = "btnCheckUncheckAll";
            this.btnCheckUncheckAll.Size = new System.Drawing.Size(130, 23);
            this.btnCheckUncheckAll.TabIndex = 5;
            this.btnCheckUncheckAll.Text = "Check/Uncheck All";
            this.btnCheckUncheckAll.UseVisualStyleBackColor = true;
            this.btnCheckUncheckAll.Click += new System.EventHandler(this.btnCheckUncheckAll_Click);
            // 
            // btnAskForTranslations
            // 
            this.btnAskForTranslations.Location = new System.Drawing.Point(624, 561);
            this.btnAskForTranslations.Name = "btnAskForTranslations";
            this.btnAskForTranslations.Size = new System.Drawing.Size(136, 23);
            this.btnAskForTranslations.TabIndex = 6;
            this.btnAskForTranslations.Text = "Ask for translations";
            this.btnAskForTranslations.UseVisualStyleBackColor = true;
            this.btnAskForTranslations.Click += new System.EventHandler(this.btnAskForTranslations_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(948, 622);
            this.Controls.Add(this.btnAskForTranslations);
            this.Controls.Add(this.btnCheckUncheckAll);
            this.Controls.Add(this.chkTranslations);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtLogOperations);
            this.Controls.Add(this.cmbLingue);
            this.Controls.Add(this.btnCheckTraduzioni);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCheckTraduzioni;
        private System.Windows.Forms.ComboBox cmbLingue;
        private System.Windows.Forms.TextBox txtLogOperations;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckedListBox chkTranslations;
        internal System.Windows.Forms.Button btnCheckUncheckAll;
        private System.Windows.Forms.Button btnAskForTranslations;
    }
}

