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

namespace WorldBuilder
{
    public partial class frmHashTest : Form
    {
        public frmHashTest()
        {
            InitializeComponent();
        }

        private void frmHashTest_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                btnSearch_Click(null, null);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
          
        }

        private void txtFileName_TextChanged(object sender, EventArgs e)
        {
            lblHash.Text = Convert.ToString(MYP.HashWAR(txtFileName.Text), 16);
            txtResults.Text = "";
            if (MYPHash.Hashes.ContainsKey(MYP.HashWAR(txtFileName.Text)))
                txtResults.Text = "Found in " + MYPHash.Hashes[MYP.HashWAR(txtFileName.Text)].ToString();
            else
                txtResults.Text = "Not Found";
        }
    }
}
