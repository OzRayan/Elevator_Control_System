using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace _1926120_AssignmentOne
{
    public partial class Message : Form
    {
        public Message()
        {
            InitializeComponent();
        }

        private void Message_Load(object sender, EventArgs e)
        {
            Hide();
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Hide();
        }

        public void showData(DataTable dt) 
        {
            listBox1.Items.Clear();
            listBox1.Items.Add("Log ID \t From level \tTo level \t Date & time");
            listBox1.Items.Add(new string('_', listBox1.Width));
            foreach (DataRow row in dt.Rows)
            {
                listBox1.Items.Add("" + row[0] + " \t " + row[1] + " \t\t" + row[2] + " \t" + row[3] + "");
            }
        }
    }
}
