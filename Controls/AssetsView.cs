using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WorldBuilder.Controls
{
    public partial class AssetsView : UserControl
    {

        public delegate void AssetDelegate(WorldBuilder.MYPHash.Asset asset);
        public event AssetDelegate OnAssetSelected;

        public AssetsView()
        {
            InitializeComponent();
        }

        private async void AssetsView_Load(object sender, EventArgs e)
        {
           
        }


        public async Task LoadAssetTree()
        {
            foreach (var asset in MYPHash.Assets.Values)
            {
                TreeNode node = new TreeNode(asset.Name);
                node.Tag = asset;
                if(asset.Assets.Count > 0)
                    node.Nodes.Add("");

                treeView1.Nodes.Add(node);
            }
        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {

        }


        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            e.Node.Nodes.Clear();

            WorldBuilder.MYPHash.Asset parent = (WorldBuilder.MYPHash.Asset)e.Node.Tag;
            foreach (var asset in parent.Assets.Values.OrderBy(x=>x.Name).ToList())
            {
                TreeNode node = new TreeNode(asset.Name);
                node.Tag = asset;
                if (asset.Assets.Count > 0)
                    node.Nodes.Add("");

                e.Node.Nodes.Add(node);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            WorldBuilder.MYPHash.Asset asset = (WorldBuilder.MYPHash.Asset)e.Node.Tag;
            if (asset.Assets.Count == 0 && OnAssetSelected  != null)
            {
                OnAssetSelected(asset);
            }
        }
    }
}
