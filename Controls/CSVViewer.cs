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
    public partial class CSVViewer : UserControl
    {
        private CSV _csv;
        private int _skip = 1;
        private Dictionary<int, ListViewItem> _lviCache = new Dictionary<int, ListViewItem>();
        private bool _disableUpdates;
        private int _currentRow = 0;
        private int _currentCol = 0;
        private bool _changed = false;
        WorldBuilder.MYPHash.Asset _asset;

        public delegate void AssetDelegate(WorldBuilder.MYPHash.Asset asset, byte[] data);
        public event AssetDelegate OnSave;

        public CSVViewer()
        {
            InitializeComponent();
   
            listView1.Controls.Add(txt);
            listView1.FullRowSelect = true;
            txt.Leave += txt_Leave;
        }

        private void txt_Leave(object sender, EventArgs e)
        {
            
            txt.Visible = false;
            _csv.RowIndex = _currentRow;
            _csv.WriteCol(_currentCol, txt.Text);
            _disableUpdates = true;
            txtRaw.Text = _csv.ToText();
            listView1.Items[_currentRow].SubItems[_currentCol].Text = txt.Text;
            _disableUpdates = false;
        }
        private readonly TextBox txt = new TextBox { BorderStyle = BorderStyle.FixedSingle, Visible = false };

        public void LoadCSV(WorldBuilder.MYPHash.Asset asset)
        {
            _asset =  asset;
            listView1.VirtualMode = true;
            byte[] data = MYPHash.GetAssetData(frmBuilder.WarFolder, asset, 0 );
            if (data != null)
            {
                _csv = new CSV(System.Text.ASCIIEncoding.ASCII.GetString(data));
                LoadCSV(_csv);
            }

        }
        public void LoadCSV(string text)
        {
            _csv = new CSV(text);
            LoadCSV(_csv);
        }
        public void LoadCSV(CSV csv)
        {
            listView1.Items.Clear();
            listView1.Columns.Clear();
            _lviCache = new Dictionary<int, ListViewItem>();

            _csv = csv;
            _disableUpdates = true;
            if (csv.Lines.Count == 0)
            {
                listView1.VirtualListSize = 0;
                return;
            }

            Dictionary<string, int> counts = new Dictionary<string, int>();

            foreach (var cCol in _csv.Row)
            {
                string csvCol = cCol;
                ColumnHeader col = new ColumnHeader();


                col.Text = csvCol;
                if (!counts.ContainsKey(csvCol))
                {
                    counts[csvCol] = 1;
                }
                else
                {
                    counts[csvCol]++;
                    csvCol = csvCol + counts[csvCol];
                }

                listView1.Columns.Add(col);

            }
            _csv.NextRow();
            listView1.VirtualMode = true;
            listView1.VirtualListSize = _csv.Lines.Count;

            txtRaw.Text = _csv.ToText();
            _disableUpdates = false;
            _changed = false;
        }
        private void listView1_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            if (_csv != null)
            {
                if (!_lviCache.ContainsKey(e.ItemIndex + _skip))
                {
                    listView1_CacheVirtualItems(null, new CacheVirtualItemsEventArgs(e.ItemIndex, e.ItemIndex));
                }
                e.Item = _lviCache[e.ItemIndex];
            }
       
        }

        private void listView1_CacheVirtualItems(object sender, CacheVirtualItemsEventArgs e)
        {
            if (_csv != null)
            {
                for (int c = e.StartIndex; c <= e.EndIndex; c++)
                {

                    _csv.RowIndex = c;
                    if (!_lviCache.ContainsKey(_csv.RowIndex))
                    {
                        var rowData = _csv.Row;
          
              
                        var row = new ListViewItem();
                        row.Tag = c;

                        if (rowData.Count > 0)
                            row.Text = rowData[0];

                        for (int i = 1; i < listView1.Columns.Count; i++)
                        {
                            if (i < rowData.Count)
                                row.SubItems.Add(rowData[i]);
                            else
                                row.SubItems.Add("");
                        }
                        if (c % 2 == 0)
                        {
                            row.BackColor = Color.FromArgb(255, 240, 240, 240);
                        }
                        _lviCache[c] = row;
                    }
                }
            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListViewHitTestInfo hit = listView1.HitTest(e.Location);

            Rectangle rowBounds = hit.SubItem.Bounds;
            Rectangle labelBounds = hit.Item.GetBounds(ItemBoundsPortion.Label);

            _currentRow = (int)hit.Item.Tag;
            _currentCol = hit.Item.SubItems.IndexOf(hit.SubItem);
            int leftMargin = labelBounds.Left - 1;
            txt.Bounds = new Rectangle(rowBounds.Left + leftMargin, rowBounds.Top, rowBounds.Width - leftMargin - 1, rowBounds.Height);
            txt.Text = hit.SubItem.Text;
            txt.SelectAll();
            txt.Visible = true;
            txt.Focus();
        }

        private void txtRaw_TextChanged(object sender, EventArgs e)
        {
            if (_disableUpdates)
                return;

            _changed = true;
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _disableUpdates = true;
            List<int> toRemove = new List<int>();
            foreach (int s in listView1.SelectedIndices)
            {
                toRemove.Add(s);
            }

            _currentRow = 0;
            _csv.Remove(toRemove);
            LoadCSV(_csv.ToText());
            _disableUpdates = false;
        }

        private void txtRaw_Leave(object sender, EventArgs e)
        {
            if (_changed)
            {
                _csv = new CSV(txtRaw.Text);
                LoadCSV(txtRaw.Text);
            }
        }

        private void addRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count > 0)
                _currentRow = listView1.Items[listView1.SelectedIndices[0]].Index;
            else
                _currentRow = _csv.Lines.Count - 1;

            _csv.RowIndex = _currentRow;

            _csv.NewRow();
            LoadCSV(_csv);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(listView1.SelectedIndices.Count > 0)
                _currentRow = listView1.Items[listView1.SelectedIndices[0]].Index;
            _csv.RowIndex = _currentRow;
        }

        private async void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (OnSave != null)
                OnSave(_asset, System.Text.ASCIIEncoding.ASCII.GetBytes(_csv.ToText()));

            if (_asset != null)
            {
                await MYPHash.SaveAsset(frmBuilder.WarFolder, _asset, System.Text.ASCIIEncoding.ASCII.GetBytes(_csv.ToText()),0);
            }
        }

     
    }
}
