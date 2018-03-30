using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorldBuilder
{
    public class CSV
    {
        public List<List<string>> Lines = new List<List<string>>();
        public int RowIndex { get; set; }

        public List<string> Row
        {
            get
            {
                return ReadRow();
            }
        }
        public bool EOF
        {
            get
            {
                return RowIndex == Lines.Count-1;
            }
        }
        public CSV()
        {
            RowIndex = 0;
        }
        public CSV(string text)
        {
            foreach (string row in text.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList())
            {
                List<string> cols = new List<string>();
                foreach (string col in row.Split(new char[] { ',' }))
                {
                    cols.Add(col);
                }
                Lines.Add(cols);
            }
            RowIndex = 0;
        }

        public void MoveToEnd()
        {
            RowIndex = Lines.Count - 1;
        }

        public void NewRow()
        {
            Lines.Insert(RowIndex+1, new List<string>());
        }

        public void RemoveRow()
        {
            Lines.RemoveAt(RowIndex);
        }
        public void Remove(List<int> rows)
        {
            List<List<string>> lines = new List<List<string>>();
            for (int i = 0; i < Lines.Count; i++)
            {
                if (!rows.Contains(i))
                    lines.Add(Lines[i]);
            }
            Lines = lines;
            if (RowIndex > Lines.Count - 1)
                RowIndex = lines.Count - 1;
        }

        public void WriteCol(int col, string value)
        {
            List<string> line = Lines[RowIndex];
            if ( line.Count-1 < col)
            {
                int count = col - line.Count;
                for (int i = 0; i <= count ; i++)
                    line.Add("");
            }
            line[col] = value;
        }
      

        public List<string> ReadRow()
        {
            return Lines[RowIndex];
        }

        public void NextRow()
        {
            RowIndex++;
        }

        public string ToText()
        {
            StringBuilder builder = new StringBuilder();
            foreach (List<string> row in Lines)
            {
                for (int i = 0; i < row.Count; i++)
                {
                    if(i<row.Count -1)
                        builder.Append(row[i] + ",");
                    else
                        builder.Append(row[i]);
                }
                builder.Append("\n");
            }
            return builder.ToString();
        }

    }
}
