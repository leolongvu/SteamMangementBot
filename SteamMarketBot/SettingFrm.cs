using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace SteamMarketBot
{
    public partial class SettingFrm : Form
    {
        Properties.Settings settings = Properties.Settings.Default;
        static Main main = new Main();

        public SettingFrm()
        {
            InitializeComponent();          
        }

        private void SettingFrm_Load(object sender, EventArgs e)
        {
            PassTB.Text = "";
            PassTB.PasswordChar = '*';

            UserNamTB.Enabled = false;
            PassTB.Enabled = false;

            comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text.Equals("New Account"))
            {
                UserNamTB.Text = "";
                PassTB.Text = "";
                UserNamTB.Enabled = true;
                PassTB.Enabled = true;
            }
            else
            {
                main.LoadingCookie();
                main.LoadConfigs();
                UserNamTB.Enabled = false;
                PassTB.Enabled = false;
            }
        }

        private void Saveacclist()
        {
            TextWriter config_writer = new StreamWriter(Application.StartupPath + @"\" + "acclists.ini");
            for (int i = 0; i < comboBox1.Items.Count; i++)
            {
                if (!comboBox1.Items[i].ToString().Equals("New Account") && !comboBox1.Items[i].ToString().Equals(""))
                {
                    config_writer.WriteLine(comboBox1.Items[i].ToString());
                }
            }
            config_writer.WriteLine("//");
            config_writer.Close();
        }

        public void Loadacclist()
        {
            if (File.Exists(Application.StartupPath + @"\" + "acclists.ini"))
            {
                TextReader config_reader = new StreamReader(Application.StartupPath + @"\" + "acclists.ini");
                string input;
                while ((input = config_reader.ReadLine()) != "//")
                {
                    comboBox1.Items.Add(input);                 
                }
                config_reader.Close();
                comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
            }
            else
            {
                MessageBox.Show("No account list founded", "Error");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int count = 0;
            for (int i = 0; i < comboBox1.Items.Count; i++)
            {
                if (comboBox1.Items[i].ToString().Equals(UserNamTB.Text))
                {
                    count++;
                }
            }
            if (count == 0 && !UserNamTB.Text.Equals(""))
            {
                comboBox1.Items.Add(UserNamTB.Text);
                comboBox1.SelectedIndex = comboBox1.Items.Count - 1;
            }
            Saveacclist();
            main.SaveConfigs();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!comboBox1.SelectedItem.ToString().Equals("New Account"))
            {
                UserNamTB.Enabled = true;
                PassTB.Enabled = true;
            }
        }      
    }
}
