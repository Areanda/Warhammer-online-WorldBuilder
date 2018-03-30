using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WorldBuilder
{
    public partial class frmName : Form
    {
        public delegate void ValidateDelegate(string text, ref bool valid, ref string errorMsg);
        public event ValidateDelegate OnValidateName;
        public string NameText
        {
            get
            {
                return textBox1.Text;
            }
            set
            {
                textBox1.Text = value;
            }
        }

        public int IntValue
        {
            get
            {
                int value = 0;
                int.TryParse(textBox1.Text, out value);
                return value;
            }
            set
            {
                textBox1.Text = value.ToString();
            }
        }

        public frmName()
        {
            InitializeComponent();
        }

        public frmName(string NameText)
        {
            InitializeComponent();
            textBox1.Text = NameText;
        }

        public frmName(string title, string label)
        {
            InitializeComponent();
            groupBox1.Text = label;
            Text = title;

        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string errorMsg = "Invalid " + groupBox1.Text;
            bool isValid = true;
            if (OnValidateName != null)
            {
                OnValidateName(textBox1.Text, ref isValid, ref errorMsg);
                if (!isValid)
                {
                    MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void frmName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                btnOK_Click(null, null);
            if (e.KeyCode == Keys.Escape)
                btnCancel_Click(null, null);
        }
    }
}
