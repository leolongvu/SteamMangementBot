namespace SteamMarketBot
{
    partial class Dialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.CaptchaGB = new System.Windows.Forms.GroupBox();
            this.CaptchaBu = new System.Windows.Forms.Button();
            this.CaptchaTB = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.CaptchaPB = new System.Windows.Forms.PictureBox();
            this.CodeGroGB = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.EmailCoTB = new System.Windows.Forms.TextBox();
            this.DescriptTB = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.CaptchaGB.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CaptchaPB)).BeginInit();
            this.CodeGroGB.SuspendLayout();
            this.SuspendLayout();
            // 
            // CaptchaGB
            // 
            this.CaptchaGB.Controls.Add(this.CaptchaTB);
            this.CaptchaGB.Controls.Add(this.label1);
            this.CaptchaGB.Controls.Add(this.CaptchaPB);
            this.CaptchaGB.Font = new System.Drawing.Font("Arial Narrow", 10.2F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CaptchaGB.Location = new System.Drawing.Point(3, 3);
            this.CaptchaGB.Name = "CaptchaGB";
            this.CaptchaGB.Size = new System.Drawing.Size(329, 123);
            this.CaptchaGB.TabIndex = 0;
            this.CaptchaGB.TabStop = false;
            this.CaptchaGB.Text = "Captcha ";
            // 
            // CaptchaBu
            // 
            this.CaptchaBu.Font = new System.Drawing.Font("Arial Narrow", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CaptchaBu.Location = new System.Drawing.Point(243, 277);
            this.CaptchaBu.Name = "CaptchaBu";
            this.CaptchaBu.Size = new System.Drawing.Size(85, 32);
            this.CaptchaBu.TabIndex = 3;
            this.CaptchaBu.Text = "OK";
            this.CaptchaBu.UseVisualStyleBackColor = true;
            this.CaptchaBu.Click += new System.EventHandler(this.CaptchaBu_Click);
            // 
            // CaptchaTB
            // 
            this.CaptchaTB.Font = new System.Drawing.Font("Arial Narrow", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CaptchaTB.Location = new System.Drawing.Point(144, 88);
            this.CaptchaTB.Name = "CaptchaTB";
            this.CaptchaTB.Size = new System.Drawing.Size(179, 27);
            this.CaptchaTB.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial Narrow", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(16, 92);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(122, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Enter Captcha text:";
            // 
            // CaptchaPB
            // 
            this.CaptchaPB.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.CaptchaPB.Location = new System.Drawing.Point(68, 26);
            this.CaptchaPB.Name = "CaptchaPB";
            this.CaptchaPB.Size = new System.Drawing.Size(255, 56);
            this.CaptchaPB.TabIndex = 0;
            this.CaptchaPB.TabStop = false;
            // 
            // CodeGroGB
            // 
            this.CodeGroGB.Controls.Add(this.label4);
            this.CodeGroGB.Controls.Add(this.label3);
            this.CodeGroGB.Controls.Add(this.DescriptTB);
            this.CodeGroGB.Controls.Add(this.EmailCoTB);
            this.CodeGroGB.Controls.Add(this.label2);
            this.CodeGroGB.Font = new System.Drawing.Font("Arial Narrow", 10.2F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CodeGroGB.Location = new System.Drawing.Point(4, 132);
            this.CodeGroGB.Name = "CodeGroGB";
            this.CodeGroGB.Size = new System.Drawing.Size(328, 139);
            this.CodeGroGB.TabIndex = 1;
            this.CodeGroGB.TabStop = false;
            this.CodeGroGB.Text = "Steam Guard Check";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial Narrow", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(8, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(123, 20);
            this.label2.TabIndex = 4;
            this.label2.Text = "Enter access code:";
            // 
            // EmailCoTB
            // 
            this.EmailCoTB.Font = new System.Drawing.Font("Arial Narrow", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EmailCoTB.Location = new System.Drawing.Point(143, 70);
            this.EmailCoTB.Name = "EmailCoTB";
            this.EmailCoTB.Size = new System.Drawing.Size(179, 27);
            this.EmailCoTB.TabIndex = 4;
            // 
            // DescriptTB
            // 
            this.DescriptTB.Font = new System.Drawing.Font("Arial Narrow", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DescriptTB.Location = new System.Drawing.Point(143, 103);
            this.DescriptTB.Name = "DescriptTB";
            this.DescriptTB.Size = new System.Drawing.Size(179, 27);
            this.DescriptTB.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial Narrow", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(15, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 20);
            this.label3.TabIndex = 6;
            this.label3.Text = "Email code:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial Narrow", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(15, 107);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(84, 20);
            this.label4.TabIndex = 7;
            this.label4.Text = "Description:";
            // 
            // Dialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 313);
            this.Controls.Add(this.CaptchaBu);
            this.Controls.Add(this.CodeGroGB);
            this.Controls.Add(this.CaptchaGB);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "Dialog";
            this.CaptchaGB.ResumeLayout(false);
            this.CaptchaGB.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CaptchaPB)).EndInit();
            this.CodeGroGB.ResumeLayout(false);
            this.CodeGroGB.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox CaptchaGB;
        private System.Windows.Forms.TextBox CaptchaTB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox CaptchaPB;
        private System.Windows.Forms.Button CaptchaBu;
        private System.Windows.Forms.GroupBox CodeGroGB;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox DescriptTB;
        private System.Windows.Forms.TextBox EmailCoTB;

    }
}