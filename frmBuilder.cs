using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WorldBuilder.Controls;

namespace WorldBuilder
{
    public partial class frmBuilder : Form, ILog
    {
        public static string WarFolder = @"";
        public frmBuilder()
        {
          
            
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
         
            Append("Intializing Hashes");
            await MYPHash.Intialize(WarFolder, Assembly.GetExecutingAssembly().GetManifestResourceStream("WorldBuilder.hashes_filename.zip"), this);
            MYPHash.PrintStats(this);
            await assetsView1.LoadAssetTree();
            await hashView1.LoadAssetTree();
            Append("Finished");
        }

        public void Append(string msg)
        {
            BeginInvoke(new AppendDelegate(AppendInternval), msg);
        }
        private delegate void AppendDelegate(string line);

        private void AppendInternval(string line)
        {
            txtLog.Text += DateTime.Now.ToString() + "> " + line + "\r\n";
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        private void assetsView1_OnAssetSelected(MYPHash.Asset asset)
        {
            viewPanel.Controls.Clear();
            propertyGrid1.SelectedObject = asset;
            if (asset.Name.EndsWith(".csv"))
            {
                CSVViewer viewer = new CSVViewer();
                viewPanel.Controls.Add(viewer);
                viewer.Dock = DockStyle.Fill;
                viewer.LoadCSV(asset);
            }
            else if (asset.Name.EndsWith(".xml") || asset.Name.EndsWith(".cfg") || asset.Name.EndsWith(".lua") || asset.Name.EndsWith(".txt") || asset.Name.EndsWith(".mod") || asset.Name.EndsWith(".dat")
                || asset.Name.EndsWith(".psh") || asset.Name.EndsWith(".vsh") || asset.Name.EndsWith(".h"))
            {
                TextView viewer = new TextView();
                viewPanel.Controls.Add(viewer);
                viewer.Dock = DockStyle.Fill;
                viewer.LoadText(asset);
            }
            else if (asset.Name.EndsWith(".bin"))
            {
                BinView viewer = new BinView();
                viewPanel.Controls.Add(viewer);
                viewer.Dock = DockStyle.Fill;
                viewer.LoadBin(asset);
            }
        }

        public static bool IsWarRunning()
        {
            foreach (Process p in Process.GetProcesses())
            {

                if (p.ProcessName == "WAR")
                    return true;
            }
            return false;
        }

        public static void UpdateWarData()
        {

      
        }

        private static void StartGame(string auth, string user, bool startIfRunning = false)
        {


            Process Pro = new Process();
            Pro.StartInfo.FileName = File.ReadAllLines("settings.txt")[0];
            Pro.StartInfo.Arguments = " --acctname=" + System.Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(user)) + " --sesstoken=" + auth + " --loadlog=true";
            Pro.Start();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {

        }

        private void hashTestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmHashTest test = new frmHashTest();
            test.Show();
        }

        private void hashView1_Load(object sender, EventArgs e)
        {

        }

    
    }
}
