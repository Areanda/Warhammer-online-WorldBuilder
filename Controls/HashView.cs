using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WorldBuilder.Controls
{
    public partial class HashView : UserControl
    {

        public delegate void AssetDelegate(WorldBuilder.MYPHash.Asset asset);
        public event AssetDelegate OnAssetSelected;

        public HashView()
        {
            InitializeComponent();
        }

        private async void AssetsView_Load(object sender, EventArgs e)
        {

        }


        public async Task LoadAssetTree()
        {
            foreach (string file in Directory.GetFiles(frmBuilder.WarFolder, "*.MYP"))
            {
                MythicPackage p;
                if (Enum.TryParse<MythicPackage>(Path.GetFileNameWithoutExtension(file).ToUpper(), out p))
                {
                    TreeNode node = new TreeNode(p.ToString());
                    node.Tag = p;
                   node.Nodes.Add("");
                    treeView1.Nodes.Add(node);
                }
            }
        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {

        }


        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            e.Node.Nodes.Clear();

            MythicPackage parent = (MythicPackage)e.Node.Tag;
            var myp = new MYP();
            using (var stream = new FileStream(Path.Combine(frmBuilder.WarFolder,parent.ToString() + ".MYP"), FileMode.Open, FileAccess.Read))
            {
                myp.Load(stream);
                List<TreeNode> nodes = new List<TreeNode>();
                foreach (var key in myp.Files.Keys)
                {
                    string name = key.ToString();
                    if (MYPHash.DeHash.ContainsKey(key))
                        name = MYPHash.DeHash[key];
                    TreeNode node = new TreeNode(name);
                    node.Tag = myp.Files[key];
                    nodes.Add(node);
                }

                e.Node.Nodes.AddRange(nodes.OrderBy(x => x.Text).ToArray());
            }
        }
        private string GetExtension(byte[] buffer)
        {
            byte[] outbuffer = buffer;

            string file = "";

            file = System.Text.Encoding.ASCII.GetString(outbuffer, 0, outbuffer.Length);
            char[] separators = { ',' };
            int commaNum = file.Split(separators, 10).Length;
            string header = System.Text.Encoding.ASCII.GetString(outbuffer, 0, 4);
            string ext = "txt";

            if (outbuffer[0] == 0 && outbuffer[1] == 1 && outbuffer[2] == 0)
            {
                ext = "ttf";
            }
            else if (outbuffer[0] == 0x0a && outbuffer[1] == 0x05 && outbuffer[2] == 0x01 && outbuffer[3] == 0x08)
            {
                ext = "pcx";
            }
            else if (header.IndexOf("PK") >= 0)
            {
                ext = "zip";
            }
            else if (header.IndexOf("SCPT") >= 0)
            {
                ext = "scpt";
            }
            else if (header.IndexOf("<") >= 0)
            {
                ext = "xml";
            }
            else if (file.IndexOf("lua") >= 0 && file.IndexOf("lua") < 50)
            {
                ext = "lua";
            }
            else if (header.IndexOf("DDS") >= 0)
            {
                ext = "dds";
            }
            else if (header.IndexOf("XSM") >= 0)
            {
                ext = "xsm";
            }
            else if (header.IndexOf("XAC") >= 0)
            {
                ext = "xac";
            }
            else if (header.IndexOf("8BPS") >= 0)
            {
                ext = "8bps";
            }
            else if (header.IndexOf("bdLF") >= 0)
            {
                ext = "db";
            }
            else if (header.IndexOf("gsLF") >= 0)
            {
                ext = "geom";
            }
            else if (header.IndexOf("idLF") >= 0)
            {
                ext = "diffuse";
            }
            else if (header.IndexOf("psLF") >= 0)
            {
                ext = "specular";
            }
            else if (header.IndexOf("amLF") >= 0)
            {
                ext = "mask";
            }
            else if (header.IndexOf("ntLF") >= 0)
            {
                ext = "tint";
            }
            else if (header.IndexOf("lgLF") >= 0)
            {
                ext = "glow";
            }
            else if (file.IndexOf("Gamebry") >= 0)
            {
                ext = "nif";
            }
            else if (file.IndexOf("WMPHOTO") >= 0)
            {
                ext = "lmp";
            }
            else if (header.IndexOf("RIFF") >= 0)
            {
                string data = System.Text.Encoding.ASCII.GetString(outbuffer, 8, 4);
                if (data.IndexOf("WAVE") >= 0)
                {
                    ext = "wav";
                }
                else
                {
                    ext = "riff";
                }
            }
            else if (header.IndexOf("; Zo") >= 0)
            {
                ext = "zone.txt";
            }
            else if (header.IndexOf("\0\0\0\0") >= 0)
            {
                ext = "zero.txt";
            }
            else if (header.IndexOf("PNG") >= 0)
            {
                ext = "png";
            }
            else if (header.IndexOf("AMX") >= 0)
            {
                ext = "amx";
            }
            else if (header.IndexOf("SIDS") >= 0)
            {
                ext = "sids";
            } //SIDS
            else if (commaNum >= 10)
            {
                ext = "csv";
            }

            return ext;
        }
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is FileInArchive)
            {
                FileInArchive file = (FileInArchive)e.Node.Tag;
                WorldBuilder.MYPHash.Asset asset = new MYPHash.Asset();
                asset.File = file;
                if (OnAssetSelected != null)
                {
                    long key = ((long)file.Descriptor.ph << 32) + file.Descriptor.sh;
                    byte[] data = MYPHash.GetAssetData(frmBuilder.WarFolder, key, 0);
                    asset.Hash = key;
                     asset.Filename = key.ToString() + "." + GetExtension(data);
                    asset.Name = asset.Filename;
                    OnAssetSelected(asset);
                }
            }
        }
    }
}
