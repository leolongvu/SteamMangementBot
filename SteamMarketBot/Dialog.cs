using System;
using System.Windows.Forms;

namespace SteamMarketBot
{

    public partial class Dialog : Form
    {

        public string MailCode
        {
            get { return EmailCoTB.Text; }
            set { DescriptTB.Text = value; }
        }
        public string GuardDesc
        {
            get { return DescriptTB.Text; }
            set { DescriptTB.Text = value; }
        }

        public string capchaText
        {
            get { return CaptchaTB.Text; }
            set { CaptchaTB.Text = value; }
        }

        public bool codgroupEnab
        {
            get { return CodeGroGB.Enabled; }
            set { CodeGroGB.Enabled = value; }
        }

        public bool capchgroupEnab
        {
            get { return CaptchaGB.Enabled; }
            set { CaptchaGB.Enabled = value; }
        }

        public PictureBox capchImg
        {
            get { return CaptchaPB; }
            set { CaptchaPB = value; }
        }

        public Dialog()
        {
            InitializeComponent();
        }

        private void CaptchaBu_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            return;
        }
    }
}
