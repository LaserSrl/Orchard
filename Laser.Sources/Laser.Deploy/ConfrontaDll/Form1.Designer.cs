namespace ConfrontaDll {
    partial class ConfrontaDllfrm {
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
            this.goBtn = new System.Windows.Forms.Button();
            this.pathTxt = new System.Windows.Forms.TextBox();
            this.resultTxt = new System.Windows.Forms.TextBox();
            this.pathLbl = new System.Windows.Forms.Label();
            this.browseFolder = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.lblFilter = new System.Windows.Forms.Label();
            this.objChk = new System.Windows.Forms.CheckBox();
            this.debugChk = new System.Windows.Forms.CheckBox();
            this.resourcesChk = new System.Windows.Forms.CheckBox();
            this.removeBinBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // goBtn
            // 
            this.goBtn.BackColor = System.Drawing.Color.Black;
            this.goBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.goBtn.ForeColor = System.Drawing.Color.Yellow;
            this.goBtn.Location = new System.Drawing.Point(34, 388);
            this.goBtn.Name = "goBtn";
            this.goBtn.Size = new System.Drawing.Size(663, 45);
            this.goBtn.TabIndex = 0;
            this.goBtn.Text = "GO";
            this.goBtn.UseVisualStyleBackColor = false;
            this.goBtn.Click += new System.EventHandler(this.goBtn_Click);
            // 
            // pathTxt
            // 
            this.pathTxt.Location = new System.Drawing.Point(34, 33);
            this.pathTxt.Name = "pathTxt";
            this.pathTxt.Size = new System.Drawing.Size(663, 20);
            this.pathTxt.TabIndex = 1;
            // 
            // resultTxt
            // 
            this.resultTxt.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.resultTxt.Location = new System.Drawing.Point(34, 111);
            this.resultTxt.Multiline = true;
            this.resultTxt.Name = "resultTxt";
            this.resultTxt.ReadOnly = true;
            this.resultTxt.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.resultTxt.Size = new System.Drawing.Size(663, 271);
            this.resultTxt.TabIndex = 2;
            // 
            // pathLbl
            // 
            this.pathLbl.AutoSize = true;
            this.pathLbl.Location = new System.Drawing.Point(31, 17);
            this.pathLbl.Name = "pathLbl";
            this.pathLbl.Size = new System.Drawing.Size(127, 13);
            this.pathLbl.TabIndex = 3;
            this.pathLbl.Text = "Inserisci path cartella root";
            // 
            // browseFolder
            // 
            this.browseFolder.BackColor = System.Drawing.Color.Black;
            this.browseFolder.Cursor = System.Windows.Forms.Cursors.Hand;
            this.browseFolder.ForeColor = System.Drawing.Color.Yellow;
            this.browseFolder.Location = new System.Drawing.Point(619, 33);
            this.browseFolder.Name = "browseFolder";
            this.browseFolder.Size = new System.Drawing.Size(78, 20);
            this.browseFolder.TabIndex = 4;
            this.browseFolder.Text = "Browse";
            this.browseFolder.UseVisualStyleBackColor = false;
            this.browseFolder.Click += new System.EventHandler(this.browseFolder_Click);
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.HelpRequest += new System.EventHandler(this.folderBrowserDialog1_HelpRequest);
            // 
            // lblFilter
            // 
            this.lblFilter.AutoSize = true;
            this.lblFilter.Location = new System.Drawing.Point(31, 73);
            this.lblFilter.Name = "lblFilter";
            this.lblFilter.Size = new System.Drawing.Size(139, 13);
            this.lblFilter.TabIndex = 5;
            this.lblFilter.Text = "Cartelle da non considerare:";
            // 
            // objChk
            // 
            this.objChk.AutoSize = true;
            this.objChk.Location = new System.Drawing.Point(186, 73);
            this.objChk.Name = "objChk";
            this.objChk.Size = new System.Drawing.Size(45, 17);
            this.objChk.TabIndex = 6;
            this.objChk.Text = "/obj";
            this.objChk.UseVisualStyleBackColor = true;
            // 
            // debugChk
            // 
            this.debugChk.AutoSize = true;
            this.debugChk.Location = new System.Drawing.Point(237, 73);
            this.debugChk.Name = "debugChk";
            this.debugChk.Size = new System.Drawing.Size(63, 17);
            this.debugChk.TabIndex = 7;
            this.debugChk.Text = "/Debug";
            this.debugChk.UseVisualStyleBackColor = true;
            // 
            // resourcesChk
            // 
            this.resourcesChk.AutoSize = true;
            this.resourcesChk.Location = new System.Drawing.Point(470, 73);
            this.resourcesChk.Name = "resourcesChk";
            this.resourcesChk.Size = new System.Drawing.Size(136, 17);
            this.resourcesChk.TabIndex = 8;
            this.resourcesChk.Text = "escludere resources dlll";
            this.resourcesChk.UseVisualStyleBackColor = true;
            // 
            // removeBinBtn
            // 
            this.removeBinBtn.BackColor = System.Drawing.Color.Black;
            this.removeBinBtn.ForeColor = System.Drawing.Color.Yellow;
            this.removeBinBtn.Location = new System.Drawing.Point(34, 443);
            this.removeBinBtn.Name = "removeBinBtn";
            this.removeBinBtn.Size = new System.Drawing.Size(174, 23);
            this.removeBinBtn.TabIndex = 9;
            this.removeBinBtn.Text = "elimina bin folder sotto la root";
            this.removeBinBtn.UseVisualStyleBackColor = false;
            this.removeBinBtn.Click += new System.EventHandler(this.button1_Click);
            // 
            // ConfrontaDllfrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Yellow;
            this.ClientSize = new System.Drawing.Size(741, 478);
            this.Controls.Add(this.removeBinBtn);
            this.Controls.Add(this.resourcesChk);
            this.Controls.Add(this.debugChk);
            this.Controls.Add(this.objChk);
            this.Controls.Add(this.lblFilter);
            this.Controls.Add(this.browseFolder);
            this.Controls.Add(this.pathLbl);
            this.Controls.Add(this.resultTxt);
            this.Controls.Add(this.pathTxt);
            this.Controls.Add(this.goBtn);
            this.Name = "ConfrontaDllfrm";
            this.Text = "ConfrontaDll";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button goBtn;
        private System.Windows.Forms.TextBox pathTxt;
        private System.Windows.Forms.TextBox resultTxt;
        private System.Windows.Forms.Label pathLbl;
        private System.Windows.Forms.Button browseFolder;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Label lblFilter;
        private System.Windows.Forms.CheckBox objChk;
        private System.Windows.Forms.CheckBox debugChk;
        private System.Windows.Forms.CheckBox resourcesChk;
        private System.Windows.Forms.Button removeBinBtn;
    }
}

