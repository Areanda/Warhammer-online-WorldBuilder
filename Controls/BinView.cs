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
    public partial class BinView : UserControl
    {
        WorldBuilder.MYPHash.Asset _asset;
        public BinView()
        {
            InitializeComponent();
        }

        public void LoadBin(WorldBuilder.MYPHash.Asset asset)
        {
            _asset = asset;
        }

        private async void BinView_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            await MYPHash.SaveAsset(frmBuilder.WarFolder, _asset, File.ReadAllBytes(files[0]),0);
        }

        private void BinView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }
    }
}
