using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConfrontaDll {
    public partial class ConfrontaDllfrm : Form {

        public ConfrontaDllfrm() {
            InitializeComponent();
        }

        private void goBtn_Click(object sender, EventArgs e) {
            resultTxt.Text = "";
            string path = this.pathTxt.Text;
            if (path.Equals("")) {
                this.resultTxt.Text = "nessun path inserito";
                return;
            }

            DirectoryInfo root = new DirectoryInfo(path);
            List<FileInfo> allDllFile;
            try {
                allDllFile = root.GetFiles("*.dll", SearchOption.AllDirectories).ToList<FileInfo>();
            } catch (DirectoryNotFoundException ex) {
                resultTxt.Text = "path non valido";
                return;
            }

            List<FileInfo> fileToRemove = new List<FileInfo>();
            List<FileInfo> fileToCompare = new List<FileInfo>();
            Dictionary<string, List<FileInfo>> differentDll = new Dictionary<string, List<FileInfo>>();

            foreach (FileInfo file in allDllFile) {
                if (objChk.Checked) {
                    if (removeIfContains("\\obj\\", file, fileToRemove))
                        continue;
                }
                if (debugChk.Checked) {
                    if (removeIfContains("\\Debug\\", file, fileToRemove))
                        continue;
                }
                if (resourcesChk.Checked) {
                    if (removeIfContains(".resources.", file, fileToRemove))
                        continue;
                }
            }

            fileToCompare = allDllFile.Except(fileToRemove).ToList<FileInfo>();

            foreach (FileInfo file in fileToCompare) {
                foreach (FileInfo compareFile in fileToCompare) {
                    if (file.Name.Equals(compareFile.Name) && file.Length != compareFile.Length) {
                        if (!differentDll.ContainsKey(file.Name)) {
                            differentDll[file.Name] = new List<FileInfo>();
                        }
                        differentDll[file.Name].Add(file);
                        break;
                    }
                }
            }
            if (differentDll.Count == 0) {
                resultTxt.Text = "nessuna differenza trovata";
            } else {
                resultTxt.Text = "differenze trovate:" + Environment.NewLine;
                foreach (string fileNameDiffernt in differentDll.Keys.OrderBy(s => s)) {
                    
                    this.resultTxt.AppendText(Environment.NewLine + fileNameDiffernt + Environment.NewLine);
                    foreach (FileInfo fileInfo in differentDll[fileNameDiffernt].OrderBy(l => l.Length)) {
                        string toPrint = "->path: " + fileInfo.FullName.Substring(path.Length, fileInfo.FullName.Length - path.Length)
                            + ", length: " + fileInfo.Length + '\n';
                        this.resultTxt.AppendText(toPrint);
                    }
                }
                resultTxt.AppendText(Environment.NewLine + differentDll.Keys.Count + " dll non sempre della stessa dimensione");
            }
        }

        private bool removeIfContains(string v, FileInfo file, List<FileInfo> fileToRemove) {
            if (file.FullName.Contains(v)) {
                fileToRemove.Add(file);
                return true;
            }
            return false;
        }

        private void browseFolder_Click(object sender, EventArgs e) {
            DialogResult result = folderBrowserDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                pathTxt.Text = folderBrowserDialog1.SelectedPath;
            }
        }


        private void folderBrowserDialog1_HelpRequest(object sender, EventArgs e) {

        }

        private void button1_Click(object sender, EventArgs e) {
            string[] binDirectories;
            if (pathTxt.Text != "") {
                var confirmResult = MessageBox.Show("Sei sicuro di cancellare tutte le cartelle bin partendo dalla root indicata?",
                                     "Confirm Delete!!",
                                     MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.Yes) {
                    try {
                        binDirectories = Directory.GetDirectories(pathTxt.Text, "bin", SearchOption.AllDirectories);
                    } catch (DirectoryNotFoundException ex) {
                        resultTxt.Text = "path non valido";
                        return;
                    }
                    if (binDirectories.Count<string>() == 0) {
                        resultTxt.Text = "nessuna cartella bin trovata";
                    } else {
                        resultTxt.Text = "cartelle eliminate: \n";
                        foreach (string dirPath in binDirectories) {
                            if (dirPath.EndsWith("bin")) {
                                Directory.Delete(dirPath, true);
                                this.resultTxt.AppendText(dirPath + " \n");
                            }
                        }
                    }
                }
            } else {
                resultTxt.Text = "inserire un path";
            }

        }
    }
}
