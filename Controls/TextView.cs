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
    public partial class TextView : UserControl
    {
        private WorldBuilder.MYPHash.Asset _asset;
        private byte[] _data;
        public TextView()
        {
            InitializeComponent();
        }
        public void LoadText(WorldBuilder.MYPHash.Asset asset)
        {
            _asset = asset;
            byte[] data = null;
            if (asset.File == null)
                _data = MYPHash.GetAssetData(frmBuilder.WarFolder, asset, 0);
            else
                _data = MYPHash.GetAssetData(frmBuilder.WarFolder, asset.Hash, 0);

            data = _data;
            if (_data != null)
            {
                if (asset.Name.ToUpper().EndsWith(".XML") || asset.Name.ToUpper().EndsWith(".MOD"))
                {
                  //  txtText.ConfigurationManager.Language = "xml";
                    txtText.Text = System.Text.ASCIIEncoding.ASCII.GetString(data);
                }
                else if (asset.Name.ToUpper().EndsWith(".LUA"))
                {
                   // txtText.ConfigurationManager.Language = "LUA";
                    txtText.Text = System.Text.ASCIIEncoding.ASCII.GetString(data);
                }
                else if (asset.Name.ToUpper().EndsWith(".PSH") || asset.Name.ToUpper().EndsWith(".VSH") || asset.Name.ToUpper().EndsWith("H"))
                {
                //    txtText.ConfigurationManager.Language = "cpp";
                    txtText.Text = System.Text.ASCIIEncoding.ASCII.GetString(data);
                }
                else if (asset.Name.ToUpper().EndsWith(".DAT"))
                {
                    txtText.Text = System.Text.ASCIIEncoding.ASCII.GetString(data);
                }
                else if (asset.Name.ToUpper().EndsWith(".TXT"))
                {
                    txtText.Text = System.Text.UnicodeEncoding.Unicode.GetString(data);
                }


            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (_asset.Name.ToUpper().EndsWith(".TXT"))
            {
                var a = System.Text.UnicodeEncoding.Unicode.GetString(_data);
                var b = System.Text.UnicodeEncoding.Unicode.GetBytes(txtText.Text);
                await MYPHash.SaveAsset(frmBuilder.WarFolder, _asset, b,0);

            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (_asset.Name.ToUpper().EndsWith(".TXT"))
            {
                var a = System.Text.UnicodeEncoding.Unicode.GetBytes(txtText.Text);
                await MYPHash.SaveAsset(frmBuilder.WarFolder, _asset, _data, 1);
            }
        }
    }
}
