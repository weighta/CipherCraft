using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CipherCraft
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        EJMA256 ejma = new EJMA256();

        private byte[] header = new byte[64];
        private int numFiles = 0;
        private string[] files;

        private void panel6_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void panel6_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                files = (string[])e.Data.GetData(DataFormats.FileDrop);
                LoadFiles();
            }
            catch
            {
                log("Error Loading Files");
            }
        }

        void LoadFiles()
        {
            numFiles = files.Length;
            listBox1.Items.Clear();
            for (int i = 0; i < files.Length; i++) listBox1.Items.Add(files[i]);
        }


        private void Encrypt_Files_Click(object sender, EventArgs e)
        {
            if (numFiles == 0)
            {
                log("No Files To Encrypt");
            }
            else
            {
                byte[] buffer = new byte[64];
                buffer[0] = 47;
                log("Encrypt " + numFiles + " files");
                buffer = ejma.Encrypt(buffer, 1, "hi");
                buffer = ejma.Decrypt(buffer, 1, "hi");
            }
        }

        private void log(string a)
        {
            Log.Text += a + "\n";
        }
    }
}
