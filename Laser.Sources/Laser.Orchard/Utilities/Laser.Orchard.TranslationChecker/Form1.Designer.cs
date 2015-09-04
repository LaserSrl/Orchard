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
            this.btnCopyToClipboard = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.lblTranslatorWsUrl = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
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
            this.chkTranslations.Location = new System.Drawing.Point(0, 45);
            this.chkTranslations.Name = "chkTranslations";
            this.chkTranslations.Size = new System.Drawing.Size(449, 439);
            this.chkTranslations.TabIndex = 4;
            // 
            // btnCheckUncheckAll
            // 
            this.btnCheckUncheckAll.Location = new System.Drawing.Point(0, 505);
            this.btnCheckUncheckAll.Name = "btnCheckUncheckAll";
            this.btnCheckUncheckAll.Size = new System.Drawing.Size(130, 23);
            this.btnCheckUncheckAll.TabIndex = 5;
            this.btnCheckUncheckAll.Text = "Check/Uncheck All";
            this.btnCheckUncheckAll.UseVisualStyleBackColor = true;
            this.btnCheckUncheckAll.Click += new System.EventHandler(this.btnCheckUncheckAll_Click);
            // 
            // btnAskForTranslations
            // 
            this.btnAskForTranslations.Location = new System.Drawing.Point(137, 505);
            this.btnAskForTranslations.Name = "btnAskForTranslations";
            this.btnAskForTranslations.Size = new System.Drawing.Size(136, 23);
            this.btnAskForTranslations.TabIndex = 6;
            this.btnAskForTranslations.Text = "Ask for translations";
            this.btnAskForTranslations.UseVisualStyleBackColor = true;
            this.btnAskForTranslations.Click += new System.EventHandler(this.btnAskForTranslations_Click);
            // 
            // btnCopyToClipboard
            // 
            this.btnCopyToClipboard.Location = new System.Drawing.Point(280, 504);
            this.btnCopyToClipboard.Name = "btnCopyToClipboard";
            this.btnCopyToClipboard.Size = new System.Drawing.Size(136, 23);
            this.btnCopyToClipboard.TabIndex = 7;
            this.btnCopyToClipboard.Text = "Copy To clipboard";
            this.btnCopyToClipboard.UseVisualStyleBackColor = true;
            this.btnCopyToClipboard.Click += new System.EventHandler(this.btnCopyToClipboard_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lblTranslatorWsUrl);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.btnCopyToClipboard);
            this.panel1.Controls.Add(this.chkTranslations);
            this.panel1.Controls.Add(this.btnAskForTranslations);
            this.panel1.Controls.Add(this.btnCheckUncheckAll);
            this.panel1.Location = new System.Drawing.Point(487, 56);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(449, 554);
            this.panel1.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Translator WS Url:";
            // 
            // lblTranslatorWsUrl
            // 
            this.lblTranslatorWsUrl.AutoSize = true;
            this.lblTranslatorWsUrl.Location = new System.Drawing.Point(113, 13);
            this.lblTranslatorWsUrl.Name = "lblTranslatorWsUrl";
            this.lblTranslatorWsUrl.Size = new System.Drawing.Size(16, 13);
            this.lblTranslatorWsUrl.TabIndex = 9;
            this.lblTranslatorWsUrl.Text = "...";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(948, 622);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtLogOperations);
            this.Controls.Add(this.cmbLingue);
            this.Controls.Add(this.btnCheckTraduzioni);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
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
        private System.Windows.Forms.Button btnCopyToClipboard;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblTranslatorWsUrl;
        private System.Windows.Forms.Label label2;
    }
}

