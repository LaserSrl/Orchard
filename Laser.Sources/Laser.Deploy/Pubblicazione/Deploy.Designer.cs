﻿namespace Pubblicazione {
    partial class Deploy {
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
            this.button1 = new System.Windows.Forms.Button();
            this.btnAll = new System.Windows.Forms.Button();
            this.chkDeleteFolder = new System.Windows.Forms.CheckBox();
            this.btnoprnfolder = new System.Windows.Forms.Button();
            this.btnzip = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.chkThemesDev = new System.Windows.Forms.CheckBox();
            this.chkModulesDev = new System.Windows.Forms.CheckBox();
            this.chkAllLibrariesDev = new System.Windows.Forms.CheckBox();
            this.clbThemes = new System.Windows.Forms.CheckedListBox();
            this.clbModules = new System.Windows.Forms.CheckedListBox();
            this.clbLibrary = new System.Windows.Forms.CheckedListBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.clbLibraryOrchard = new System.Windows.Forms.CheckedListBox();
            this.clbThemesOrchard = new System.Windows.Forms.CheckedListBox();
            this.clbModulesOrchard = new System.Windows.Forms.CheckedListBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.Mylog = new System.Windows.Forms.TextBox();
            this.Autoseleziona = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.btnReset = new System.Windows.Forms.Button();
            this.SaveSetting = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.tbOrchardDev = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.TheprogressBar = new System.Windows.Forms.ProgressBar();
            this.OperazioneTerminata = new System.Windows.Forms.Label();
            this.CheckAll = new System.Windows.Forms.CheckBox();
            this.btnFullDeploy = new System.Windows.Forms.Button();
            this.chkAllCoreModules = new System.Windows.Forms.CheckBox();
            this.chkAllCoreThemes = new System.Windows.Forms.CheckBox();
            this.chkAllCoreLibraries = new System.Windows.Forms.CheckBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(23, 527);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Only dll";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnOnlyDll);
            // 
            // btnAll
            // 
            this.btnAll.Location = new System.Drawing.Point(113, 527);
            this.btnAll.Name = "btnAll";
            this.btnAll.Size = new System.Drawing.Size(75, 23);
            this.btnAll.TabIndex = 7;
            this.btnAll.Text = "All";
            this.btnAll.UseVisualStyleBackColor = true;
            this.btnAll.Click += new System.EventHandler(this.btnAll_Click);
            // 
            // chkDeleteFolder
            // 
            this.chkDeleteFolder.AutoSize = true;
            this.chkDeleteFolder.Checked = true;
            this.chkDeleteFolder.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDeleteFolder.Location = new System.Drawing.Point(26, 552);
            this.chkDeleteFolder.Name = "chkDeleteFolder";
            this.chkDeleteFolder.Size = new System.Drawing.Size(120, 17);
            this.chkDeleteFolder.TabIndex = 8;
            this.chkDeleteFolder.Text = "Delete folder deploy";
            this.chkDeleteFolder.UseVisualStyleBackColor = true;
            // 
            // btnoprnfolder
            // 
            this.btnoprnfolder.Location = new System.Drawing.Point(780, 527);
            this.btnoprnfolder.Name = "btnoprnfolder";
            this.btnoprnfolder.Size = new System.Drawing.Size(75, 23);
            this.btnoprnfolder.TabIndex = 9;
            this.btnoprnfolder.Text = "Open folder";
            this.btnoprnfolder.UseVisualStyleBackColor = true;
            this.btnoprnfolder.Click += new System.EventHandler(this.btnoprnfolder_Click);
            // 
            // btnzip
            // 
            this.btnzip.Location = new System.Drawing.Point(692, 527);
            this.btnzip.Name = "btnzip";
            this.btnzip.Size = new System.Drawing.Size(75, 23);
            this.btnzip.TabIndex = 10;
            this.btnzip.Text = "zip";
            this.btnzip.UseVisualStyleBackColor = true;
            this.btnzip.Click += new System.EventHandler(this.btnzip_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Location = new System.Drawing.Point(5, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(872, 519);
            this.tabControl1.TabIndex = 11;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.chkThemesDev);
            this.tabPage1.Controls.Add(this.chkModulesDev);
            this.tabPage1.Controls.Add(this.chkAllLibrariesDev);
            this.tabPage1.Controls.Add(this.clbThemes);
            this.tabPage1.Controls.Add(this.clbModules);
            this.tabPage1.Controls.Add(this.clbLibrary);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(864, 493);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "OrchardDev";
            this.tabPage1.UseVisualStyleBackColor = true;
            this.tabPage1.Click += new System.EventHandler(this.tabPage1_Click);
            // 
            // chkThemesDev
            // 
            this.chkThemesDev.AutoSize = true;
            this.chkThemesDev.Location = new System.Drawing.Point(6, 370);
            this.chkThemesDev.Name = "chkThemesDev";
            this.chkThemesDev.Size = new System.Drawing.Size(64, 17);
            this.chkThemesDev.TabIndex = 14;
            this.chkThemesDev.Text = "Themes";
            this.chkThemesDev.UseVisualStyleBackColor = true;
            this.chkThemesDev.CheckedChanged += new System.EventHandler(this.chkThemesDev_CheckedChanged);
            // 
            // chkModulesDev
            // 
            this.chkModulesDev.AutoSize = true;
            this.chkModulesDev.Location = new System.Drawing.Point(6, 97);
            this.chkModulesDev.Name = "chkModulesDev";
            this.chkModulesDev.Size = new System.Drawing.Size(66, 17);
            this.chkModulesDev.TabIndex = 13;
            this.chkModulesDev.Text = "Modules";
            this.chkModulesDev.UseVisualStyleBackColor = true;
            this.chkModulesDev.CheckedChanged += new System.EventHandler(this.chkModulesDev_CheckedChanged);
            // 
            // chkAllLibrariesDev
            // 
            this.chkAllLibrariesDev.AutoSize = true;
            this.chkAllLibrariesDev.Location = new System.Drawing.Point(8, 4);
            this.chkAllLibrariesDev.Name = "chkAllLibrariesDev";
            this.chkAllLibrariesDev.Size = new System.Drawing.Size(65, 17);
            this.chkAllLibrariesDev.TabIndex = 12;
            this.chkAllLibrariesDev.Text = "Libraries";
            this.chkAllLibrariesDev.UseVisualStyleBackColor = true;
            this.chkAllLibrariesDev.CheckedChanged += new System.EventHandler(this.chkAllLibrariesDev_CheckedChanged);
            // 
            // clbThemes
            // 
            this.clbThemes.CheckOnClick = true;
            this.clbThemes.FormattingEnabled = true;
            this.clbThemes.Location = new System.Drawing.Point(6, 393);
            this.clbThemes.Name = "clbThemes";
            this.clbThemes.Size = new System.Drawing.Size(833, 94);
            this.clbThemes.TabIndex = 8;
            // 
            // clbModules
            // 
            this.clbModules.CheckOnClick = true;
            this.clbModules.FormattingEnabled = true;
            this.clbModules.Location = new System.Drawing.Point(6, 120);
            this.clbModules.Name = "clbModules";
            this.clbModules.Size = new System.Drawing.Size(833, 244);
            this.clbModules.TabIndex = 7;
            // 
            // clbLibrary
            // 
            this.clbLibrary.CheckOnClick = true;
            this.clbLibrary.FormattingEnabled = true;
            this.clbLibrary.Location = new System.Drawing.Point(6, 27);
            this.clbLibrary.Name = "clbLibrary";
            this.clbLibrary.Size = new System.Drawing.Size(833, 64);
            this.clbLibrary.TabIndex = 6;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.chkAllCoreLibraries);
            this.tabPage2.Controls.Add(this.chkAllCoreThemes);
            this.tabPage2.Controls.Add(this.chkAllCoreModules);
            this.tabPage2.Controls.Add(this.clbLibraryOrchard);
            this.tabPage2.Controls.Add(this.clbThemesOrchard);
            this.tabPage2.Controls.Add(this.clbModulesOrchard);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(864, 493);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Orchard";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // clbLibraryOrchard
            // 
            this.clbLibraryOrchard.CheckOnClick = true;
            this.clbLibraryOrchard.FormattingEnabled = true;
            this.clbLibraryOrchard.Location = new System.Drawing.Point(8, 35);
            this.clbLibraryOrchard.Name = "clbLibraryOrchard";
            this.clbLibraryOrchard.Size = new System.Drawing.Size(833, 64);
            this.clbLibraryOrchard.TabIndex = 16;
            // 
            // clbThemesOrchard
            // 
            this.clbThemesOrchard.CheckOnClick = true;
            this.clbThemesOrchard.FormattingEnabled = true;
            this.clbThemesOrchard.Location = new System.Drawing.Point(8, 395);
            this.clbThemesOrchard.Name = "clbThemesOrchard";
            this.clbThemesOrchard.Size = new System.Drawing.Size(833, 94);
            this.clbThemesOrchard.TabIndex = 13;
            // 
            // clbModulesOrchard
            // 
            this.clbModulesOrchard.CheckOnClick = true;
            this.clbModulesOrchard.FormattingEnabled = true;
            this.clbModulesOrchard.Location = new System.Drawing.Point(8, 124);
            this.clbModulesOrchard.Name = "clbModulesOrchard";
            this.clbModulesOrchard.Size = new System.Drawing.Size(833, 244);
            this.clbModulesOrchard.TabIndex = 12;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.dateTimePicker1);
            this.tabPage4.Controls.Add(this.button3);
            this.tabPage4.Controls.Add(this.button2);
            this.tabPage4.Controls.Add(this.Mylog);
            this.tabPage4.Controls.Add(this.Autoseleziona);
            this.tabPage4.Controls.Add(this.panel1);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(864, 493);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Tools";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.CustomFormat = "\"dd/MM/yyyy HH mm ss\"";
            this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker1.Location = new System.Drawing.Point(58, 412);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(160, 20);
            this.dateTimePicker1.TabIndex = 18;
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(247, 438);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(139, 23);
            this.button3.TabIndex = 17;
            this.button3.Text = "Select Modules";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(247, 409);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(139, 23);
            this.button2.TabIndex = 16;
            this.button2.Text = "Select dll";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Mylog
            // 
            this.Mylog.Location = new System.Drawing.Point(6, 3);
            this.Mylog.Multiline = true;
            this.Mylog.Name = "Mylog";
            this.Mylog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.Mylog.Size = new System.Drawing.Size(852, 392);
            this.Mylog.TabIndex = 15;
            // 
            // Autoseleziona
            // 
            this.Autoseleziona.Location = new System.Drawing.Point(92, 438);
            this.Autoseleziona.Name = "Autoseleziona";
            this.Autoseleziona.Size = new System.Drawing.Size(126, 23);
            this.Autoseleziona.TabIndex = 12;
            this.Autoseleziona.Text = "Calculate from date";
            this.Autoseleziona.UseVisualStyleBackColor = true;
            this.Autoseleziona.Click += new System.EventHandler(this.Autoseleziona_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panel1.Location = new System.Drawing.Point(6, 401);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(852, 80);
            this.panel1.TabIndex = 15;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.btnReset);
            this.tabPage3.Controls.Add(this.SaveSetting);
            this.tabPage3.Controls.Add(this.label5);
            this.tabPage3.Controls.Add(this.tbOrchardDev);
            this.tabPage3.Controls.Add(this.label4);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(864, 493);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Setting";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(771, 46);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(75, 23);
            this.btnReset.TabIndex = 14;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // SaveSetting
            // 
            this.SaveSetting.Location = new System.Drawing.Point(771, 17);
            this.SaveSetting.Name = "SaveSetting";
            this.SaveSetting.Size = new System.Drawing.Size(75, 23);
            this.SaveSetting.TabIndex = 13;
            this.SaveSetting.Text = "Save";
            this.SaveSetting.UseVisualStyleBackColor = true;
            this.SaveSetting.Click += new System.EventHandler(this.SaveSetting_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(492, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(268, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Default is D:\\Sviluppo\\DotNet\\Laser.Platform.Orchard\\";
            // 
            // tbOrchardDev
            // 
            this.tbOrchardDev.Location = new System.Drawing.Point(126, 18);
            this.tbOrchardDev.Name = "tbOrchardDev";
            this.tbOrchardDev.Size = new System.Drawing.Size(355, 20);
            this.tbOrchardDev.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(104, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Platform Base Folder";
            // 
            // TheprogressBar
            // 
            this.TheprogressBar.Location = new System.Drawing.Point(293, 527);
            this.TheprogressBar.Name = "TheprogressBar";
            this.TheprogressBar.Size = new System.Drawing.Size(393, 23);
            this.TheprogressBar.TabIndex = 12;
            this.TheprogressBar.Visible = false;
            // 
            // OperazioneTerminata
            // 
            this.OperazioneTerminata.AutoSize = true;
            this.OperazioneTerminata.Location = new System.Drawing.Point(389, 533);
            this.OperazioneTerminata.Name = "OperazioneTerminata";
            this.OperazioneTerminata.Size = new System.Drawing.Size(111, 13);
            this.OperazioneTerminata.TabIndex = 13;
            this.OperazioneTerminata.Text = "Operazione Terminata";
            this.OperazioneTerminata.Visible = false;
            // 
            // CheckAll
            // 
            this.CheckAll.AutoSize = true;
            this.CheckAll.Location = new System.Drawing.Point(804, 5);
            this.CheckAll.Name = "CheckAll";
            this.CheckAll.Size = new System.Drawing.Size(71, 17);
            this.CheckAll.TabIndex = 14;
            this.CheckAll.Text = "Check All";
            this.CheckAll.UseVisualStyleBackColor = true;
            this.CheckAll.CheckedChanged += new System.EventHandler(this.CheckAll_CheckedChanged);
            // 
            // btnFullDeploy
            // 
            this.btnFullDeploy.Location = new System.Drawing.Point(203, 527);
            this.btnFullDeploy.Name = "btnFullDeploy";
            this.btnFullDeploy.Size = new System.Drawing.Size(75, 23);
            this.btnFullDeploy.TabIndex = 15;
            this.btnFullDeploy.Text = "Full";
            this.btnFullDeploy.UseVisualStyleBackColor = true;
            this.btnFullDeploy.Click += new System.EventHandler(this.btnFullDeploy_Click);
            // 
            // chkAllCoreModules
            // 
            this.chkAllCoreModules.AutoSize = true;
            this.chkAllCoreModules.Location = new System.Drawing.Point(11, 107);
            this.chkAllCoreModules.Name = "chkAllCoreModules";
            this.chkAllCoreModules.Size = new System.Drawing.Size(82, 17);
            this.chkAllCoreModules.TabIndex = 17;
            this.chkAllCoreModules.Text = "Moduli Core";
            this.chkAllCoreModules.UseVisualStyleBackColor = true;
            this.chkAllCoreModules.CheckedChanged += new System.EventHandler(this.chkAllCoreModules_CheckedChanged);
            // 
            // chkAllCoreThemes
            // 
            this.chkAllCoreThemes.AutoSize = true;
            this.chkAllCoreThemes.Location = new System.Drawing.Point(11, 378);
            this.chkAllCoreThemes.Name = "chkAllCoreThemes";
            this.chkAllCoreThemes.Size = new System.Drawing.Size(74, 17);
            this.chkAllCoreThemes.TabIndex = 18;
            this.chkAllCoreThemes.Text = "Temi Core";
            this.chkAllCoreThemes.UseVisualStyleBackColor = true;
            this.chkAllCoreThemes.CheckedChanged += new System.EventHandler(this.chkAllCoreThemes_CheckedChanged);
            // 
            // chkAllCoreLibraries
            // 
            this.chkAllCoreLibraries.AutoSize = true;
            this.chkAllCoreLibraries.Location = new System.Drawing.Point(10, 16);
            this.chkAllCoreLibraries.Name = "chkAllCoreLibraries";
            this.chkAllCoreLibraries.Size = new System.Drawing.Size(85, 17);
            this.chkAllCoreLibraries.TabIndex = 19;
            this.chkAllCoreLibraries.Text = "Librerie Core";
            this.chkAllCoreLibraries.UseVisualStyleBackColor = true;
            this.chkAllCoreLibraries.CheckedChanged += new System.EventHandler(this.chkAllCoreLibraries_CheckedChanged);
            // 
            // Deploy
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(881, 597);
            this.Controls.Add(this.btnFullDeploy);
            this.Controls.Add(this.CheckAll);
            this.Controls.Add(this.OperazioneTerminata);
            this.Controls.Add(this.TheprogressBar);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnzip);
            this.Controls.Add(this.btnoprnfolder);
            this.Controls.Add(this.chkDeleteFolder);
            this.Controls.Add(this.btnAll);
            this.Controls.Add(this.button1);
            this.Name = "Deploy";
            this.Text = "Deploy";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnAll;
        private System.Windows.Forms.CheckBox chkDeleteFolder;
        private System.Windows.Forms.Button btnoprnfolder;
        private System.Windows.Forms.Button btnzip;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.CheckedListBox clbThemes;
        private System.Windows.Forms.CheckedListBox clbModules;
        private System.Windows.Forms.CheckedListBox clbLibrary;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbOrchardDev;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button SaveSetting;
        private System.Windows.Forms.CheckedListBox clbThemesOrchard;
        private System.Windows.Forms.CheckedListBox clbModulesOrchard;
        private System.Windows.Forms.CheckedListBox clbLibraryOrchard;
        private System.Windows.Forms.ProgressBar TheprogressBar;
        private System.Windows.Forms.Label OperazioneTerminata;
        private System.Windows.Forms.Button Autoseleziona;
        private System.Windows.Forms.CheckBox CheckAll;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.DateTimePicker dateTimePicker1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox Mylog;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnFullDeploy;
        private System.Windows.Forms.CheckBox chkAllLibrariesDev;
        private System.Windows.Forms.CheckBox chkThemesDev;
        private System.Windows.Forms.CheckBox chkModulesDev;
        private System.Windows.Forms.CheckBox chkAllCoreLibraries;
        private System.Windows.Forms.CheckBox chkAllCoreThemes;
        private System.Windows.Forms.CheckBox chkAllCoreModules;
    }
}

