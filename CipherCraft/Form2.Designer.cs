
namespace CipherCraft
{
    partial class Form2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form2));
            this.label91 = new System.Windows.Forms.Label();
            this.Log = new System.Windows.Forms.RichTextBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.Rounds = new System.Windows.Forms.NumericUpDown();
            this.progressBar2 = new System.Windows.Forms.ProgressBar();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.panel6 = new System.Windows.Forms.Panel();
            this.label89 = new System.Windows.Forms.Label();
            this.label90 = new System.Windows.Forms.Label();
            this.label88 = new System.Windows.Forms.Label();
            this.Password = new System.Windows.Forms.TextBox();
            this.button6 = new System.Windows.Forms.Button();
            this.Decrypt_Files = new System.Windows.Forms.Button();
            this.Encrypt_Files = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.richTextBox29 = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.Rounds)).BeginInit();
            this.panel6.SuspendLayout();
            this.SuspendLayout();
            // 
            // label91
            // 
            this.label91.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label91.AutoSize = true;
            this.label91.BackColor = System.Drawing.Color.Transparent;
            this.label91.ForeColor = System.Drawing.Color.Aqua;
            this.label91.Location = new System.Drawing.Point(9, 394);
            this.label91.Name = "label91";
            this.label91.Size = new System.Drawing.Size(50, 13);
            this.label91.TabIndex = 27;
            this.label91.Text = "KHASH: ";
            // 
            // Log
            // 
            this.Log.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.Log.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.Log.ForeColor = System.Drawing.Color.Gray;
            this.Log.Location = new System.Drawing.Point(362, 241);
            this.Log.Name = "Log";
            this.Log.Size = new System.Drawing.Size(382, 141);
            this.Log.TabIndex = 26;
            this.Log.Text = "";
            // 
            // checkBox4
            // 
            this.checkBox4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox4.AutoSize = true;
            this.checkBox4.ForeColor = System.Drawing.Color.Aqua;
            this.checkBox4.Location = new System.Drawing.Point(183, 294);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(92, 17);
            this.checkBox4.TabIndex = 25;
            this.checkBox4.Text = "Show Hashes";
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // Rounds
            // 
            this.Rounds.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Rounds.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.Rounds.ForeColor = System.Drawing.Color.Aqua;
            this.Rounds.Increment = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.Rounds.Location = new System.Drawing.Point(70, 293);
            this.Rounds.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.Rounds.Name = "Rounds";
            this.Rounds.Size = new System.Drawing.Size(107, 20);
            this.Rounds.TabIndex = 24;
            this.Rounds.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            // 
            // progressBar2
            // 
            this.progressBar2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.progressBar2.Location = new System.Drawing.Point(12, 368);
            this.progressBar2.Name = "progressBar2";
            this.progressBar2.Size = new System.Drawing.Size(271, 14);
            this.progressBar2.TabIndex = 22;
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.progressBar1.Location = new System.Drawing.Point(12, 348);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(271, 14);
            this.progressBar1.TabIndex = 23;
            // 
            // listBox1
            // 
            this.listBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.listBox1.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.listBox1.ForeColor = System.Drawing.Color.Aqua;
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(12, 176);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(302, 82);
            this.listBox1.TabIndex = 21;
            // 
            // panel6
            // 
            this.panel6.AllowDrop = true;
            this.panel6.BackColor = System.Drawing.Color.Black;
            this.panel6.Controls.Add(this.label89);
            this.panel6.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.panel6.Location = new System.Drawing.Point(12, 12);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(302, 158);
            this.panel6.TabIndex = 20;
            this.panel6.DragDrop += new System.Windows.Forms.DragEventHandler(this.panel6_DragDrop);
            this.panel6.DragEnter += new System.Windows.Forms.DragEventHandler(this.panel6_DragEnter);
            // 
            // label89
            // 
            this.label89.AutoSize = true;
            this.label89.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label89.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label89.Location = new System.Drawing.Point(91, 60);
            this.label89.Name = "label89";
            this.label89.Size = new System.Drawing.Size(110, 30);
            this.label89.TabIndex = 4;
            this.label89.Text = "Drag Files Here To\r\nEncrypt/Decrypt";
            this.label89.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label90
            // 
            this.label90.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label90.AutoSize = true;
            this.label90.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label90.Location = new System.Drawing.Point(9, 295);
            this.label90.Name = "label90";
            this.label90.Size = new System.Drawing.Size(60, 13);
            this.label90.TabIndex = 18;
            this.label90.Text = "# Rounds: ";
            // 
            // label88
            // 
            this.label88.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label88.AutoSize = true;
            this.label88.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label88.Location = new System.Drawing.Point(9, 271);
            this.label88.Name = "label88";
            this.label88.Size = new System.Drawing.Size(33, 13);
            this.label88.TabIndex = 19;
            this.label88.Text = "Pass:";
            // 
            // Password
            // 
            this.Password.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Password.BackColor = System.Drawing.SystemColors.Desktop;
            this.Password.ForeColor = System.Drawing.Color.Aqua;
            this.Password.Location = new System.Drawing.Point(48, 268);
            this.Password.Name = "Password";
            this.Password.Size = new System.Drawing.Size(235, 20);
            this.Password.TabIndex = 17;
            this.Password.Text = "football123";
            // 
            // button6
            // 
            this.button6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button6.BackColor = System.Drawing.Color.Black;
            this.button6.FlatAppearance.BorderColor = System.Drawing.Color.Aqua;
            this.button6.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button6.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.button6.Location = new System.Drawing.Point(669, 210);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(75, 23);
            this.button6.TabIndex = 16;
            this.button6.Text = "Decrypt";
            this.button6.UseVisualStyleBackColor = false;
            // 
            // Decrypt_Files
            // 
            this.Decrypt_Files.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Decrypt_Files.BackColor = System.Drawing.Color.Black;
            this.Decrypt_Files.FlatAppearance.BorderColor = System.Drawing.Color.Aqua;
            this.Decrypt_Files.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Decrypt_Files.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.Decrypt_Files.Location = new System.Drawing.Point(151, 319);
            this.Decrypt_Files.Name = "Decrypt_Files";
            this.Decrypt_Files.Size = new System.Drawing.Size(132, 23);
            this.Decrypt_Files.TabIndex = 13;
            this.Decrypt_Files.Text = "Decrypt";
            this.Decrypt_Files.UseVisualStyleBackColor = false;
            // 
            // Encrypt_Files
            // 
            this.Encrypt_Files.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.Encrypt_Files.BackColor = System.Drawing.Color.Black;
            this.Encrypt_Files.FlatAppearance.BorderColor = System.Drawing.Color.Aqua;
            this.Encrypt_Files.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Encrypt_Files.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.Encrypt_Files.Location = new System.Drawing.Point(12, 319);
            this.Encrypt_Files.Name = "Encrypt_Files";
            this.Encrypt_Files.Size = new System.Drawing.Size(133, 23);
            this.Encrypt_Files.TabIndex = 14;
            this.Encrypt_Files.Text = "Encrypt";
            this.Encrypt_Files.UseVisualStyleBackColor = false;
            this.Encrypt_Files.Click += new System.EventHandler(this.Encrypt_Files_Click);
            // 
            // button5
            // 
            this.button5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button5.BackColor = System.Drawing.Color.Black;
            this.button5.FlatAppearance.BorderColor = System.Drawing.Color.Aqua;
            this.button5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button5.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.button5.Location = new System.Drawing.Point(362, 210);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 15;
            this.button5.Text = "Encrypt";
            this.button5.UseVisualStyleBackColor = false;
            // 
            // richTextBox29
            // 
            this.richTextBox29.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.richTextBox29.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.richTextBox29.ForeColor = System.Drawing.Color.Aqua;
            this.richTextBox29.Location = new System.Drawing.Point(362, 12);
            this.richTextBox29.Name = "richTextBox29";
            this.richTextBox29.Size = new System.Drawing.Size(382, 192);
            this.richTextBox29.TabIndex = 12;
            this.richTextBox29.Text = "I have a secret crush at school.";
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.ClientSize = new System.Drawing.Size(756, 425);
            this.Controls.Add(this.label91);
            this.Controls.Add(this.Log);
            this.Controls.Add(this.checkBox4);
            this.Controls.Add(this.Rounds);
            this.Controls.Add(this.progressBar2);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.panel6);
            this.Controls.Add(this.label90);
            this.Controls.Add(this.label88);
            this.Controls.Add(this.Password);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.Decrypt_Files);
            this.Controls.Add(this.Encrypt_Files);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.richTextBox29);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form2";
            this.Text = "EJMA 256";
            ((System.ComponentModel.ISupportInitialize)(this.Rounds)).EndInit();
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label91;
        private System.Windows.Forms.RichTextBox Log;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.NumericUpDown Rounds;
        private System.Windows.Forms.ProgressBar progressBar2;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Panel panel6;
        private System.Windows.Forms.Label label89;
        private System.Windows.Forms.Label label90;
        private System.Windows.Forms.Label label88;
        private System.Windows.Forms.TextBox Password;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button Decrypt_Files;
        private System.Windows.Forms.Button Encrypt_Files;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.RichTextBox richTextBox29;
    }
}