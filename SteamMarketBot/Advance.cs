using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamMarketBot
{
    public partial class Advance : Form
    {
        public Advance()
        {
            InitializeComponent();
        }

        public bool IsGame
        {
            get { return checkBox1.Checked; }
            set { checkBox1.Checked = value; }
        }

        public string Game
        {
            get { return SteamLibrary.GetAppNameFromIndex(comboBox1.SelectedIndex); }
        }

        public string SearchName
        {
            get { return textBox3.Text; }
            set { textBox3.Text = value; }
        }

        public bool IsMin
        {
            get { return checkBox2.Checked; }
            set { checkBox2.Checked = value; }
        }

        public string Minimum
        {
            get { return textBox1.Text; }
            set { textBox1.Text = value; }
        }

        public bool IsMax
        {
            get { return checkBox3.Checked; }
            set { checkBox3.Checked = value; }
        }

        public string Maximum
        {
            get { return textBox2.Text; }
            set { textBox2.Text = value; }
        }

        public bool IsQuan
        {
            get { return checkBox4.Checked; }
            set { checkBox4.Checked = value; }
        }

        public string Quantity
        {
            get { return textBox4.Text; }
            set { textBox4.Text = value; }
        }

        public int searchCount
        {
            get { return Convert.ToInt32(textBox5.Text); }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            return;
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Convert.ToInt32(textBox5.Text);
            }
            catch
            {
                textBox5.Text = "50";
            }
        }
    }
}
