using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Data.SqlServerCe;
namespace Pubblicazione {
    public enum ExclusionFileOptions { None, Deploy, ModulesThemes };
    public partial class Deploy : Form {
        private BackgroundWorker bw = new BackgroundWorker();

        private enum TipoDeploy { dll, modulo };

        private string basepath, premodulo, deploypath;
        private Dictionary<string, string> ProgettiDaSelezionare;
        private Dictionary<string, string> elencoLibrerie;

        private Dictionary<string, string> elencoModuli;

        private Dictionary<string, string> elencoTemi;

        private Dictionary<string, string> elencoLibrerieOrchard;

        private Dictionary<string, string> elencoModuliOrchard;

        private Dictionary<string, string> elencoTemiOrchard;
        private Int32 totaleprogress = 0;
        private Int32 progressstep = 0;

        private string[] additionalFiles;


        public Deploy() {
            InitializeComponent();
            additionalFiles = new string[] {
                "*System.Net.FtpClient.dll"
            };
        }
        private void btnFullDeploy_Click(object sender, EventArgs e) {
            bw.RunWorkerAsync("btnFullDeploy");
        }

        private void btnAll_Click(object sender, EventArgs e) {
            bw.RunWorkerAsync("btnAll");
        }

        private void btnOnlyDll(object sender, EventArgs e) {
            //  this.button1.Enabled = false;
            bw.RunWorkerAsync("OnlyDll");

            //this.TheprogressBar.Value = 0;
            //this.TheprogressBar.Maximum = 1+ (this.clbModules.CheckedItems.Count + this.clbModulesOrchard.CheckedItems.Count + this.clbThemes.CheckedItems.Count + this.clbThemesOrchard.CheckedItems.Count) ;
            //this.TheprogressBar.Step = 1;
            //string deploypath = basepath + @"DeployScripts\Deploy";
            //if (Directory.Exists(deploypath) && this.chkDeleteFolder.Checked)
            //    Directory.Delete(deploypath, true);
            //Directory.CreateDirectory(deploypath);
            //TheprogressBar.PerformStep();
            //CopyDll();
            //this.tabControl1.SelectedIndex = 2;
            //this.button1.Enabled = true;
            //    MessageBox.Show("Operazione terminata");
        }

        private void CopyDll() {

            foreach (var a in this.clbLibrary.CheckedItems) {
                DirectoryInfo parentDir = Directory.GetParent(elencoModuli[this.clbModules.Items[0].ToString()]);
                ProcessXcopy(parentDir.Parent.FullName + @"\*" + a.ToString() + ".dll", deploypath + @"\Modules\");
                ProcessXcopy(parentDir.Parent.Parent.FullName + @"\Themes\*" + a.ToString() + ".dll", deploypath + @"\Themes\");
                progressstep++;
                bw.ReportProgress(100 * progressstep / totaleprogress);
                Thread.Sleep(100);
            }

            foreach (var a in this.clbLibraryOrchard.CheckedItems) {
                DirectoryInfo parentDir = Directory.GetParent(elencoModuliOrchard[this.clbModulesOrchard.Items[0].ToString()]);
                ProcessXcopy(parentDir.Parent.FullName + @"\*" + a.ToString() + ".dll", deploypath + @"\Modules\");
                ProcessXcopy(parentDir.Parent.Parent.FullName + @"\Themes\*" + a.ToString() + ".dll", deploypath + @"\Themes\");
                progressstep++;
                bw.ReportProgress(100 * progressstep / totaleprogress);
                Thread.Sleep(100);
            }
            foreach (var a in this.clbModules.CheckedItems) {
                DirectoryInfo parentDir = Directory.GetParent(elencoModuli[a.ToString()]);
                //  ProcessXcopy(parentDir.FullName + @"\*.*", deploypath + @"\Modules\" + a.ToString());
                ProcessXcopy(parentDir.Parent.FullName + @"\*" + a.ToString() + ".dll", deploypath + @"\Modules\");
                ProcessXcopy(parentDir.Parent.Parent.FullName + @"\Themes\*" + a.ToString() + ".dll", deploypath + @"\Themes\");
                progressstep++;
                bw.ReportProgress(100 * progressstep / totaleprogress);
                Thread.Sleep(100);
            }
            foreach (var a in this.clbModulesOrchard.CheckedItems) {
                DirectoryInfo parentDir = Directory.GetParent(elencoModuliOrchard[a.ToString()]);
                //  ProcessXcopy(parentDir.FullName + @"\*.*", deploypath + @"\Modules\" + a.ToString());
                ProcessXcopy(parentDir.Parent.FullName + @"\*" + a.ToString() + ".dll", deploypath + @"\Modules\");
                ProcessXcopy(parentDir.Parent.Parent.FullName + @"\Themes\*" + a.ToString() + ".dll", deploypath + @"\Themes\");
                progressstep++;
                bw.ReportProgress(100 * progressstep / totaleprogress);
                Thread.Sleep(100);
            }
        }

        private ProjectClass ReadProject(string pathsoluzion, Dictionary<string, string> MyelencoLibrerie, Dictionary<string, string> MyelencoModuli, Dictionary<string, string> MyelencoTemi) {
            //this.tbOrchardDev.Text = Properties.Settings.Default.BasePlatformRootPath;
            string Content;
            try {
                Content = File.ReadAllText(pathsoluzion);
            } catch {
                Content = "";
            }
            Regex projReg = new Regex(
                "Project\\(\"\\{[\\w-]*\\}\"\\) = \"([\\w _]*.*)\", \"(.*\\.(cs|vcx|vb)proj)\""
                , RegexOptions.Compiled);
            var matches = projReg.Matches(Content).Cast<Match>();
            var Projects = matches.Select(x => x.Groups[2].Value).ToList();
            for (int i = 0; i < Projects.Count; ++i) {
                if (!Path.IsPathRooted(Projects[i]))
                    Projects[i] = Path.Combine(Path.GetDirectoryName(pathsoluzion),
                        Projects[i]);
                Projects[i] = Path.GetFullPath(Projects[i]);
                if (!Projects[i].EndsWith(".Tests.csproj")) { //Esclude i progetti di test che per convenzione sono nella subfolder Tests del modulo e hanno nome che termina con .Tests.csproj
                    if (Projects[i].Contains(basepath) && Projects[i].Contains(@"\Themes\")) {
                        if (!Projects[i].EndsWith(@"\Themes\Themes.csproj")) {
                            MyelencoTemi.Add(Projects[i].Split('\\').LastOrDefault().Replace(".csproj", ""), Projects[i].Remove(Projects[i].LastIndexOf('\\') + 1));
                        } else {
                            MyelencoTemi.Add("SafeMode", Path.Combine(basepath, "Orchard.Sources\\src\\Orchard.Web\\Themes\\SafeMode\\"));
                            MyelencoTemi.Add("TheAdmin", Path.Combine(basepath, "Orchard.Sources\\src\\Orchard.Web\\Themes\\TheAdmin\\"));
                            MyelencoTemi.Add("TheThemeMachine", Path.Combine(basepath, "Orchard.Sources\\src\\Orchard.Web\\Themes\\TheThemeMachine\\"));
                        }
                    }
                    else if (Projects[i].Contains(basepath) && Projects[i].Contains(@"\Libraries\"))
                        MyelencoLibrerie.Add(Projects[i].Split('\\').LastOrDefault().Replace(".csproj", ""), Projects[i].Remove(Projects[i].LastIndexOf('\\') + 1));
                    else if (Projects[i].Contains(basepath) && Projects[i].EndsWith(@"SitesFiles\SitesFiles.csproj")) {
                        // do nothing => exclude this project
                    }
                    else //if (Projects[i].Contains(basepath) && Projects[i].Contains(@"\Modules\"))
                        MyelencoModuli.Add(Projects[i].Split('\\').LastOrDefault().Replace(".csproj", ""), Projects[i].Remove(Projects[i].LastIndexOf('\\') + 1));

                }
            }

            var orderelencoLibrerie = from x in MyelencoLibrerie orderby x.Key ascending select x;
            var orderelencoModuli = from x in MyelencoModuli orderby x.Key ascending select x;
            var orderelencoTemi = from x in MyelencoTemi orderby x.Key ascending select x;

            ProjectClass PC = new ProjectClass();
            PC.ListCommon = orderelencoLibrerie;
            PC.ListModule = orderelencoModuli;
            PC.ListTheme = orderelencoTemi;
            return PC;
        }

        private void bw_RunWorkedCompleted(object sender, RunWorkerCompletedEventArgs e) {
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e) {
            TheprogressBar.Value = e.ProgressPercentage;
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e) {
            Action action = () => OperazioneTerminata.Visible = false;
            OperazioneTerminata.Invoke(action);

            action = () => Mylog.Text = "";
            Mylog.Invoke(action);

            action = () => tabControl1.SelectedIndex = 2;
            tabControl1.Invoke(action);

            action = () => TheprogressBar.Visible = true;
            TheprogressBar.Invoke(action);

            //   this.btnAll.Enabled = false;
            progressstep = 0;
            //  ;
            switch (e.Argument.ToString()) {
                case "btnFullDeploy":

                    totaleprogress = 1 + 10 + additionalFiles.Count();

                    action = () => btnFullDeploy.Enabled = false;
                    btnFullDeploy.Invoke(action);
                    break;

                case "btnAll":

                    totaleprogress = 1 + (this.clbModules.CheckedItems.Count + this.clbModulesOrchard.CheckedItems.Count + this.clbThemes.CheckedItems.Count + this.clbThemesOrchard.CheckedItems.Count) * 2;

                    action = () => btnAll.Enabled = false;
                    btnAll.Invoke(action);
                    break;

                case "OnlyDll":
                    totaleprogress = 1 + (this.clbModules.CheckedItems.Count + this.clbModulesOrchard.CheckedItems.Count
                        + this.clbThemes.CheckedItems.Count + this.clbThemesOrchard.CheckedItems.Count
                        + this.clbLibrary.CheckedItems.Count + this.clbLibraryOrchard.CheckedItems.Count
                        );
                    action = () => button1.Enabled = false;
                    button1.Invoke(action);
                    break;
            }

            // this.TheprogressBar.Value = 0;
            // this.TheprogressBar.Maximum = (this.clbModules.CheckedItems.Count + this.clbModulesOrchard.CheckedItems.Count + this.clbThemes.CheckedItems.Count + this.clbThemesOrchard.CheckedItems.Count) * 2;
            // this.TheprogressBar.Step = 1;
            if (Directory.Exists(deploypath) && this.chkDeleteFolder.Checked) {
                Directory.Delete(deploypath, true);
                Thread.Sleep(1);
            }
            Directory.CreateDirectory(deploypath);
            //foreach (var a in this.clbLibrary.CheckedItems) {
            //    DirectoryInfo parentDir = Directory.GetParent(elencoModuli[this.clbModules.Items[0].ToString()]);
            //    ProcessXcopy(parentDir.Parent.FullName + @"\*" + a.ToString() + ".dll", deploypath + @"\Modules\");
            //    ProcessXcopy(parentDir.Parent.Parent.FullName + @"\Themes\*" + a.ToString() + ".dll", deploypath + @"\Themes\");
            //}
            progressstep++;
            bw.ReportProgress(100 * progressstep / totaleprogress);
            Thread.Sleep(100);
            if (e.Argument.ToString() == "btnFullDeploy") {
                ProcessXcopy(Path.Combine(basepath, @"Orchard.Sources\src\Orchard.Web\App_Data\Localization", "*.*"), Path.Combine(deploypath, @"App_Data\Localization"));
                progressstep++;
                bw.ReportProgress(100 * progressstep / totaleprogress);
                Thread.Sleep(100);
                File.Copy(Path.Combine(basepath, @"Orchard.Sources\src\Orchard.Web\App_Data", @"hrestart.txt"), Path.Combine(deploypath, @"App_Data\hrestart.txt"));
                progressstep++;
                bw.ReportProgress(100 * progressstep / totaleprogress);
                Thread.Sleep(100);
                ProcessXcopy(Path.Combine(basepath, @"Orchard.Sources\src\Orchard.Web\bin", "*.*"), Path.Combine(deploypath, @"bin"), ExclusionFileOptions.Deploy);
                progressstep++;
                bw.ReportProgress(100 * progressstep / totaleprogress);
                Thread.Sleep(100);
                ProcessXcopy(Path.Combine(basepath, @"Orchard.Sources\src\Orchard.Web\Config", "*.*"), Path.Combine(deploypath, @"Config"));
                progressstep++;
                bw.ReportProgress(100 * progressstep / totaleprogress);
                Thread.Sleep(100);
                ProcessXcopy(Path.Combine(basepath, @"Orchard.Sources\src\Orchard.Web\Core", "*.*"), Path.Combine(deploypath, @"Core"));
                progressstep++;
                bw.ReportProgress(100 * progressstep / totaleprogress);
                Thread.Sleep(100);
                ProcessXcopy(Path.Combine(basepath, @"Orchard.Sources\src\Orchard.Web\Modules", "*.*"), Path.Combine(deploypath, @"Modules"));
                ProcessXcopy(Path.Combine(basepath, @"Orchard.Sources\src\Orchard.Web\Modules", "*.recipe.xml"), Path.Combine(deploypath, @"Modules"), ExclusionFileOptions.None);
                progressstep++;
                bw.ReportProgress(100 * progressstep / totaleprogress);
                Thread.Sleep(100);
                ProcessXcopy(Path.Combine(basepath, @"Orchard.Sources\src\Orchard.Web\Themes", "*.*"), Path.Combine(deploypath, @"Themes"));
                progressstep++;
                bw.ReportProgress(100 * progressstep / totaleprogress);
                Thread.Sleep(100);
                File.Copy(Path.Combine(basepath, @"Orchard.Sources\src\Orchard.Web", @"global.asax"), Path.Combine(deploypath, "Global.asax"));
                progressstep++;
                bw.ReportProgress(100 * progressstep / totaleprogress);
                Thread.Sleep(100);
                File.Copy(Path.Combine(basepath, @"Orchard.Sources\src\Orchard.Web", @"web.config"), Path.Combine(deploypath, "web.config"));
                progressstep++;
                bw.ReportProgress(100 * progressstep / totaleprogress);
                Thread.Sleep(100);
                File.Copy(Path.Combine(basepath, @"Orchard.Sources\src\Orchard.Web", @"refresh.html"), Path.Combine(deploypath, "refresh.html"));
                progressstep++;
                bw.ReportProgress(100 * progressstep / totaleprogress);
                Thread.Sleep(100);
                foreach (var additionalFile in additionalFiles) {
                    ProcessXcopy(Path.Combine(basepath, @"Orchard.Sources\src\Orchard.Web\Modules", additionalFile), Path.Combine(deploypath, "Modules"), ExclusionFileOptions.None);
                    ProcessXcopy(Path.Combine(basepath, @"Orchard.Sources\src\Orchard.Web\Themes", additionalFile), Path.Combine(deploypath, "Themes"), ExclusionFileOptions.None);
                    progressstep++;
                    bw.ReportProgress(100 * progressstep / totaleprogress);
                    Thread.Sleep(100);
                }
            } else {
                if (e.Argument.ToString() == "btnAll") {
                    foreach (var a in this.clbModules.CheckedItems) {
                        DirectoryInfo parentDir = Directory.GetParent(elencoModuli[a.ToString()]);
                        ProcessXcopy(parentDir.FullName + @"\*.*", deploypath + @"\Modules\" + a.ToString());
                        ProcessXcopy(parentDir.FullName + @"\*.recipe.xml", deploypath + @"\Modules\" + a.ToString(), ExclusionFileOptions.None);
                        foreach (var additionalFile in additionalFiles) {
                            ProcessXcopy(Path.Combine(parentDir.FullName, additionalFile), Path.Combine(deploypath, "Modules", a.ToString()), ExclusionFileOptions.None);
                        }
                        progressstep++;
                        bw.ReportProgress(100 * progressstep / totaleprogress);
                        Thread.Sleep(100);
                    }
                    foreach (var a in this.clbModulesOrchard.CheckedItems) {
                        DirectoryInfo parentDir = Directory.GetParent(elencoModuliOrchard[a.ToString()]);
                        ProcessXcopy(parentDir.FullName + @"\*.*", deploypath + @"\Modules\" + a.ToString());
                        ProcessXcopy(parentDir.FullName + @"\*.recipe.xml", deploypath + @"\Modules\" + a.ToString(), ExclusionFileOptions.None);
                        foreach (var additionalFile in additionalFiles) {
                            ProcessXcopy(Path.Combine(parentDir.FullName, additionalFile), Path.Combine(deploypath, "Modules", a.ToString()), ExclusionFileOptions.None);
                        }
                        progressstep++;
                        bw.ReportProgress(100 * progressstep / totaleprogress);
                        Thread.Sleep(100);
                    }
                    foreach (var a in this.clbThemes.CheckedItems) {
                        ProcessXcopy(Path.Combine(elencoTemi[a.ToString()], "*.*"), Path.Combine(deploypath, @"Themes", a.ToString()));
                        foreach (var additionalFile in additionalFiles) {
                            ProcessXcopy(Path.Combine(elencoTemi[a.ToString()], additionalFile), Path.Combine(deploypath, "Themes", a.ToString()), ExclusionFileOptions.None);
                        }
                        progressstep++;
                        bw.ReportProgress(100 * progressstep / totaleprogress);
                        Thread.Sleep(100);
                    }
                    if (this.clbThemesOrchard.CheckedItems.Count > 0) { // se ho scelto almeno un tema Orchard, allora devo copiare anche \Themes\bin\*.*
                        File.Copy(Path.Combine(basepath, @"Orchard.Sources\src\Orchard.Web\Themes\web.config"), Path.Combine(deploypath, @"Themes\web.config"));
                        ProcessXcopy(Path.Combine(basepath, @"Orchard.Sources\src\Orchard.Web\Themes\bin", "*.*"), Path.Combine(deploypath, @"Themes\bin"));
                    }
                    foreach (var a in this.clbThemesOrchard.CheckedItems) {
                        ProcessXcopy(Path.Combine(elencoTemiOrchard[a.ToString()], "*.*"), Path.Combine(deploypath, @"Themes", a.ToString()));
                        foreach (var additionalFile in additionalFiles) {
                            ProcessXcopy(Path.Combine(elencoTemiOrchard[a.ToString()], additionalFile), Path.Combine(deploypath, "Themes", a.ToString()), ExclusionFileOptions.None);
                        }

                        progressstep++;
                        bw.ReportProgress(100 * progressstep / totaleprogress);
                        Thread.Sleep(100);
                    }
                }
                CopyDll();
            }
            // XCOPY ..\Orchard.Sources\src\Orchard.Web\Modules\*%nomefile% DeploySingleFile\Modules\ /S /Y /EXCLUDE:deploy.excludelist.txt
            //XCOPY ..\Orchard.Sources\src\Orchard.Web\Themes\*%nomefile% DeploySingleFile\Themes\ /S /Y /EXCLUDE:deploy.excludelist.txt
            //XCOPY ..\Orchard.Sources\src\Orchard.Web\Modules\*.* Deploy\Modules\ /S /Y /EXCLUDE:deploy.excludelist.txt
            //XCOPY ..\Orchard.Sources\src\Orchard.Web\Themes\*.* Deploy\Themes\ /S /Y /EXCLUDE:deploy.excludelist.txt
            //     this.tabControl1.SelectedIndex = 2;

            bw.ReportProgress(0);
            Thread.Sleep(100);
            //       this.OperazioneTerminata.Visible = true;

            //          this.btnAll.Enabled = true;

            switch (e.Argument.ToString()) {
                case "btnFullDeploy":
                    action = () => btnFullDeploy.Enabled = true;
                    btnFullDeploy.Invoke(action);
                    break;
                case "btnAll":
                    action = () => btnAll.Enabled = true;
                    btnAll.Invoke(action);
                    break;

                case "OnlyDll":
                    action = () => button1.Enabled = true;
                    button1.Invoke(action);
                    break;
            }
            action = () => TheprogressBar.Visible = false;
            TheprogressBar.Invoke(action);
            action = () => OperazioneTerminata.Visible = true;
            OperazioneTerminata.Invoke(action);
        }

        private void Form1_Load(object sender, EventArgs e) {
            Initialize();
            bw.WorkerReportsProgress = true;
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkedCompleted);

        }

        private void OutputDataReceived(object sender, DataReceivedEventArgs e) {
            Action action = () => Mylog.AppendText(e.Data + "\r\n");
            Mylog.Invoke(action);
        }

        private void ProcessXcopy(string SolutionDirectory, string TargetDirectory, ExclusionFileOptions exclusionFile = ExclusionFileOptions.ModulesThemes) {
            if (!(Directory.Exists(TargetDirectory)))
                Directory.CreateDirectory(TargetDirectory);
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            //     startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "xcopy";
            //      startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            if (exclusionFile == ExclusionFileOptions.Deploy) {
                startInfo.Arguments = "\"" + SolutionDirectory + "\"" + " " + "\"" + TargetDirectory + "\"" + @" /S /Y /EXCLUDE:" + Path.Combine(basepath, @"Laser.Sources\Laser.Deploy\DeployScripts\deploy.excludelist.txt"); //@" /e /y /I";
            } else if (exclusionFile == ExclusionFileOptions.ModulesThemes) {
                startInfo.Arguments = "\"" + SolutionDirectory + "\"" + " " + "\"" + TargetDirectory + "\"" + @" /S /Y /EXCLUDE:" + Path.Combine(basepath, @"Laser.Sources\Laser.Deploy\DeployScripts\modulesthemes.excludelist.txt"); //@" /e /y /I";
            } else if (exclusionFile == ExclusionFileOptions.None) {
                startInfo.Arguments = "\"" + SolutionDirectory + "\"" + " " + "\"" + TargetDirectory + "\"" + " /S /Y"; //@" /e /y /I";
            } else {
                startInfo.Arguments = "\"" + SolutionDirectory + "\"" + " " + "\"" + TargetDirectory + "\"" + " /S /Y"; //@" /e /y /I";
            }
            startInfo.RedirectStandardOutput = true;

            try {
                //using (Process exeProcess = Process.Start(startInfo) {
                //    exeProcess.WaitForExit();
                //}

                var process =
               new Process {
                   StartInfo = startInfo
               };

                process.OutputDataReceived += OutputDataReceived;

                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();
            } catch (Exception exp) {
                throw exp;
            }
        }

        private void btnoprnfolder_Click(object sender, EventArgs e) {
            Process.Start("explorer.exe", deploypath);
        }

        private void btnzip_Click(object sender, EventArgs e) {
            if (File.Exists(Path.Combine(basepath, @"Laser.Sources\Laser.Deploy\DeployScripts\Deploy.zip")))
                File.Delete(Path.Combine(basepath, @"Laser.Sources\Laser.Deploy\DeployScripts\Deploy.zip"));
            System.IO.Compression.ZipFile.CreateFromDirectory(Path.Combine(basepath, @"Laser.Sources\Laser.Deploy\DeployScripts\Deploy"), Path.Combine(basepath, @"Laser.Sources\Laser.Deploy\DeployScripts\Deploy.zip"));
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e) {
        }

        private void Initialize() {
            basepath = Properties.Settings.Default.BasePlatformRootPath;// @"C:\Sviluppo\Laser.Orchard.Community\";
            premodulo = Path.Combine(Properties.Settings.Default.BasePlatformRootPath, @"Orchard.Sources\src\Orchard.Web\Modules\").ToLower();
            deploypath = Path.Combine(basepath, @"Laser.Sources\Laser.Deploy\DeployScripts\Deploy");

            this.clbLibrary.Items.Clear();
            this.clbModules.Items.Clear();
            this.clbThemes.Items.Clear();
            this.clbLibraryOrchard.Items.Clear();
            this.clbModulesOrchard.Items.Clear();
            this.clbThemesOrchard.Items.Clear();
            elencoModuli = new Dictionary<string, string>();
            elencoLibrerie = new Dictionary<string, string>();
            elencoTemi = new Dictionary<string, string>();
            elencoModuliOrchard = new Dictionary<string, string>();
            elencoLibrerieOrchard = new Dictionary<string, string>();
            elencoTemiOrchard = new Dictionary<string, string>();
            this.tbOrchardDev.Text = basepath;//Properties.Settings.Default.BasePlatformRootPath;
            string SlnPath = Path.Combine(basepath, @"Laser.Sources\Laser.Orchard\Laser.Orchard.sln");
            ProjectClass PC = ReadProject(SlnPath, elencoLibrerie, elencoModuli, elencoTemi);

            foreach (var prog in PC.ListCommon)
                this.clbLibrary.Items.Add(prog.Key);
            foreach (var prog in PC.ListModule)
                this.clbModules.Items.Add(prog.Key);
            foreach (var prog in PC.ListTheme)
                this.clbThemes.Items.Add(prog.Key);

            SlnPath = Path.Combine(basepath, @"Orchard.Sources\src\Orchard.sln");
            PC = ReadProject(SlnPath, elencoLibrerieOrchard, elencoModuliOrchard, elencoTemiOrchard);
            foreach (var prog in PC.ListCommon)
                this.clbLibraryOrchard.Items.Add(prog.Key);
            foreach (var prog in PC.ListModule)
                this.clbModulesOrchard.Items.Add(prog.Key);
            foreach (var prog in PC.ListTheme)
                this.clbThemesOrchard.Items.Add(prog.Key);
        }

        private void SaveSetting_Click(object sender, EventArgs e) {
            Properties.Settings.Default.BasePlatformRootPath = this.tbOrchardDev.Text;
            Properties.Settings.Default.Save();
            Initialize();
        }

        private FileInfo[] elencofile(string search, SearchOption tiporicerca, DateTime fromdate) {
            var directory = new DirectoryInfo(basepath);
            if (directory.Exists) {
                var files = directory.GetFiles(search, tiporicerca)
    .Where(file => file.LastWriteTime >= fromdate && file.FullName.IndexOf(@"\obj\") < 0 && file.FullName.IndexOf(@"\bin\") < 0).ToArray<FileInfo>();

                //foreach (FileInfo file in files) {
                //    if (file.FullName.IndexOf(@"\obj\") < 0) {
                //        //Orchard.Sources\src\Orchard.Web\Modules\
                //        this.Mylog.Text += file.FullName + "\r\n";
                //    }
                //}
                return files;
            } else
                return null;
        }

        private void Autoseleziona_Click(object sender, EventArgs e) {
            ProgettiDaSelezionare = new Dictionary<string, string>();
            this.Mylog.Text = "";
            var directory = new DirectoryInfo(basepath);//basepath
            DateTime from_date = dateTimePicker1.Value;
            this.Mylog.Text = "";

            FileInfo[] a = elencofile("*.info", SearchOption.AllDirectories, from_date);
            RicavaElencoProgetti(a, TipoDeploy.modulo);
            a = elencofile("module.txt", SearchOption.AllDirectories, from_date);
            RicavaElencoProgetti(a, TipoDeploy.modulo);
            a = elencofile("*.cshtml", SearchOption.AllDirectories, from_date);
            RicavaElencoProgetti(a, TipoDeploy.modulo);
            a = elencofile("*.cs", SearchOption.AllDirectories, from_date);
            RicavaElencoProgetti(a, TipoDeploy.dll);
            foreach (string key in ProgettiDaSelezionare.Keys)
                Mylog.Text += ProgettiDaSelezionare[key].PadLeft(7) + " " + key + "\r\n";

            Mylog.Text += " --- ALTRI FILES MODIFICATI ---\r\n";
            a = elencofile("*", SearchOption.AllDirectories, from_date);
            var estenzioneTolteperaltrifiles = ".cs,.dll,.pdb,.log".Split(','); ;
            foreach (FileInfo key in a) {
                if (key.FullName.ToLower().IndexOf(premodulo) > -1) {
                    if (!estenzioneTolteperaltrifiles.Contains(key.Extension)) {
                        string nomeprogetto = key.FullName.ToLower().Replace(premodulo, "").Split('\\')[0];
                        if (ProgettiDaSelezionare.ContainsKey(nomeprogetto)) {
                            if (ProgettiDaSelezionare[nomeprogetto] != TipoDeploy.modulo.ToString()) {
                                Mylog.Text += key.FullName + "\r\n";
                            }
                        } else {
                            Mylog.Text += key.FullName + "\r\n";
                        }
                    }
                }
            }
        }

        private void RicavaElencoProgetti(FileInfo[] elencoProgetti, TipoDeploy td) {
            foreach (FileInfo file in elencoProgetti) {
                if (file.FullName.ToLower().IndexOf(premodulo) > -1) {
                    string nomeprogetto = file.FullName.ToLower().Replace(premodulo, "").Split('\\')[0];
                    if (!ProgettiDaSelezionare.ContainsKey(nomeprogetto)) {
                        ProgettiDaSelezionare.Add(nomeprogetto, td.ToString());
                    } else {
                        if (td == TipoDeploy.modulo && ProgettiDaSelezionare[nomeprogetto] != TipoDeploy.modulo.ToString())
                            ProgettiDaSelezionare[nomeprogetto] = TipoDeploy.modulo.ToString();
                    }
                }
            }
        }

        private void tabPage1_Click(object sender, EventArgs e) {
        }

        private void CheckAll_CheckedChanged(object sender, EventArgs e) {
            if (this.CheckAll.Checked) {
                this.CheckAll.Text = "Uncheck All";
                for (int i = 0; i < this.clbLibrary.Items.Count; i++) {
                    this.clbLibrary.SetItemChecked(i, true);
                }
                for (int i = 0; i < this.clbLibraryOrchard.Items.Count; i++) {
                    this.clbLibraryOrchard.SetItemChecked(i, true);
                }
                for (int i = 0; i < this.clbThemesOrchard.Items.Count; i++) {
                    this.clbThemesOrchard.SetItemChecked(i, true);
                }
                for (int i = 0; i < this.clbThemes.Items.Count; i++) {
                    this.clbThemes.SetItemChecked(i, true);
                }
                for (int i = 0; i < this.clbModules.Items.Count; i++) {
                    this.clbModules.SetItemChecked(i, true);
                }
                for (int i = 0; i < this.clbModulesOrchard.Items.Count; i++) {
                    this.clbModulesOrchard.SetItemChecked(i, true);
                }
            } else {
                this.CheckAll.Text = "Check All";
                for (int i = 0; i < this.clbLibrary.Items.Count; i++) {
                    this.clbLibrary.SetItemChecked(i, false);
                }
                for (int i = 0; i < this.clbLibraryOrchard.Items.Count; i++) {
                    this.clbLibraryOrchard.SetItemChecked(i, false);
                }
                for (int i = 0; i < this.clbThemes.Items.Count; i++) {
                    this.clbThemes.SetItemChecked(i, false);
                }
                for (int i = 0; i < this.clbThemesOrchard.Items.Count; i++) {
                    this.clbThemesOrchard.SetItemChecked(i, false);
                }
                for (int i = 0; i < this.clbModules.Items.Count; i++) {
                    this.clbModules.SetItemChecked(i, false);
                }
                for (int i = 0; i < this.clbModulesOrchard.Items.Count; i++) {
                    this.clbModulesOrchard.SetItemChecked(i, false);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e) {
            for (int i = 0; i < this.clbModules.Items.Count; i++) {
                this.clbModules.SetItemChecked(i, false);
            }
            for (int i = 0; i < this.clbModulesOrchard.Items.Count; i++) {
                this.clbModulesOrchard.SetItemChecked(i, false);
            }

            foreach (string key in ProgettiDaSelezionare.Keys) {
                if (ProgettiDaSelezionare[key] == TipoDeploy.dll.ToString()) {
                    for (int i = 0; i < this.clbModules.Items.Count; i++) {
                        if (((string)this.clbModules.Items[i]).ToLower() == key) {
                            this.clbModules.SetItemChecked(i, true);
                        }
                    }
                    for (int i = 0; i < this.clbModulesOrchard.Items.Count; i++) {
                        if (((string)this.clbModulesOrchard.Items[i]).ToLower() == key) {
                            this.clbModulesOrchard.SetItemChecked(i, true);
                        }
                    }
                }
            }
            //  this.chkDeleteFolder.Checked = true;
            //  bw.RunWorkerAsync("OnlyDll");
        }

        private void button3_Click(object sender, EventArgs e) {
            for (int i = 0; i < this.clbModules.Items.Count; i++) {
                this.clbModules.SetItemChecked(i, false);
            }
            for (int i = 0; i < this.clbModulesOrchard.Items.Count; i++) {
                this.clbModulesOrchard.SetItemChecked(i, false);
            }

            // moduli
            //   this.chkDeleteFolder.Checked = false;
            foreach (string key in ProgettiDaSelezionare.Keys) {
                if (ProgettiDaSelezionare[key] == TipoDeploy.modulo.ToString()) {
                    for (int i = 0; i < this.clbModules.Items.Count; i++) {
                        if (((string)this.clbModules.Items[i]).ToLower() == key) {
                            this.clbModules.SetItemChecked(i, true);
                        }
                    }
                    for (int i = 0; i < this.clbModulesOrchard.Items.Count; i++) {
                        if (((string)this.clbModulesOrchard.Items[i]).ToLower() == key) {
                            this.clbModulesOrchard.SetItemChecked(i, true);
                        }
                    }
                }
            }

            // bw.RunWorkerAsync("btnAll");
        }

        private void btnReset_Click(object sender, EventArgs e) {
            tbOrchardDev.Text = Properties.Settings.Default.BasePlatformRootPath;
        }

        private void chkAllLibrariesDev_CheckedChanged(object sender, EventArgs e) {
            for (int i = 0; i < this.clbLibrary.Items.Count; i++) {
                this.clbLibrary.SetItemChecked(i, ((CheckBox)sender).Checked);
            }
        }

        private void chkModulesDev_CheckedChanged(object sender, EventArgs e) {
            for (int i = 0; i < this.clbModules.Items.Count; i++) {
                this.clbModules.SetItemChecked(i, ((CheckBox)sender).Checked);
            }

        }

        private void chkThemesDev_CheckedChanged(object sender, EventArgs e) {
            for (int i = 0; i < this.clbThemes.Items.Count; i++) {
                this.clbThemes.SetItemChecked(i, ((CheckBox)sender).Checked);
            }

        }

        private void chkAllCoreModules_CheckedChanged(object sender, EventArgs e) {
            for (int i = 0; i < this.clbModulesOrchard.Items.Count; i++) {
                this.clbModulesOrchard.SetItemChecked(i, ((CheckBox)sender).Checked);
            }
        }

        private void chkAllCoreLibraries_CheckedChanged(object sender, EventArgs e) {
            for (int i = 0; i < this.clbLibraryOrchard.Items.Count; i++) {
                this.clbLibraryOrchard.SetItemChecked(i, ((CheckBox)sender).Checked);
            }

        }

        private void chkAllCoreThemes_CheckedChanged(object sender, EventArgs e) {
            for (int i = 0; i < this.clbThemesOrchard.Items.Count; i++) {
                this.clbThemesOrchard.SetItemChecked(i, ((CheckBox)sender).Checked);
            }
        }

    }

    public class ProjectClass {

        public IOrderedEnumerable<KeyValuePair<string, string>> ListCommon { get; set; }

        public IOrderedEnumerable<KeyValuePair<string, string>> ListModule { get; set; }

        public IOrderedEnumerable<KeyValuePair<string, string>> ListTheme { get; set; }
    }
}