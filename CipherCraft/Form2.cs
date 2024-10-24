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

namespace CipherCraft
{

    public partial class Form2 : Form
    {
        EJMA256 ejma = new EJMA256();

        private byte[] HEADER = new byte[64];

        private string TITLE = "EJMA256_V1";
        private string EXT = ".ejma";

        private const int DEF_ROUNDS = 8;
        private int numFiles = 0;
        private string[] files;
        private string pwd;
        private int rounds;
        private byte[] KHASH = new byte[64];

        public Form2()
        {
            InitializeComponent();
            inst();
        }
        void inst()
        {
            Rounds.Value = DEF_ROUNDS;
            rounds = (int)Rounds.Value;
            password_Enter();
        }


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

        private void BuildHeader()
        {
            string TIME = DateTime.Now.ToString("MM/dd/yyyy hh:mm tt");
            headWrite(TITLE, 0x0);
            headWrite(TIME, 0xA);
            HEADER[0x1D] = (byte)rounds;
            headWrite(getChecksum(), 0x1E);
        }

        private void Encrypt_Files_Click(object sender, EventArgs e)
        {
            if (numFiles == 0)
            {
                log("No Files To Encrypt");
            }
            else
            {
                log("Encrypt " + numFiles + " files");
                BuildHeader();
                for (int i = 0; i < files.Length; i++)
                {
                    byte[] buffer = File.ReadAllBytes(files[i]);
                    if (i == 0) //if first round put all necessary information
                    {
                        buffer = ejma.Encrypt(buffer, rounds, pwd); //will symmetricate
                    }
                    else
                    {
                        buffer = ejma.Encrypt(buffer);
                    }
                    byte[] WRITE_BUFFER = new byte[HEADER.Length + buffer.Length];
                    HEADER.CopyTo(WRITE_BUFFER, 0);
                    buffer.CopyTo(WRITE_BUFFER, 0x40);
                    File.WriteAllBytes(files[i] + EXT, WRITE_BUFFER);
                    log(Path.GetFileName(files[i]) + " done");
                }
                listBox1.Items.Clear();
            }
        }
        private void Decrypt_Files_Click(object sender, EventArgs e)
        {
            if (numFiles == 0)
            {
                log("No Files To Decrypt");
            }
            else
            {
                log("Decrypting " + numFiles + " files");
                for (int i = 0; i < files.Length; i++)
                {
                    byte[] buffer = File.ReadAllBytes(files[i]);
                    int checksum = readHeaderChecksum(ref buffer);
                    int rounds = readHeaderRounds(ref buffer);

                    if (checksum == getChecksum())
                    {
                        byte[] ENCRYPTION_BUFFER = new byte[buffer.Length - HEADER.Length];
                        for (int j = 0; j < ENCRYPTION_BUFFER.Length; j++) ENCRYPTION_BUFFER[j] = buffer[64 + j];
                        if (i > 0)
                        {
                            ENCRYPTION_BUFFER = ejma.Decrypt(ENCRYPTION_BUFFER);
                        }
                        else //only build tables once
                        {
                            ENCRYPTION_BUFFER = ejma.Decrypt(ENCRYPTION_BUFFER, rounds, pwd); //will desymmetricate
                        }
                        File.WriteAllBytes(files[i].Substring(0, files[i].Length - EXT.Length), ENCRYPTION_BUFFER);
                        log(Path.GetFileName(files[i]) + " done");
                    }
                    else
                    {
                        log(Path.GetFileName(files[i]) + " incorrect key");
                    }                    
                }
            }
        }
        int readHeaderChecksum(ref byte[] head)
        {
            int ret = 0;
            for (int i = 0; i < 4; i++)
            {
                ret |= ((head[0x1E + i] << ((3 - i) << 3)));
            }
            return ret;
        }
        int readHeaderRounds(ref byte[] head)
        {
            return head[0x1D];
        }

        private void log(string a)
        {
            Log.Text += a + "\n";
        }

        private void Password_TextChanged(object sender, EventArgs e)
        {
            password_Enter();
        }
        void password_Enter()
        {
            pwd = Password.Text;
            ejma.rda.kHashingAlgorithm(pwd).CopyTo(KHASH, 0);
            label91.Text = "KHASH: " + Print.hashToString(KHASH);
        }

        private void Rounds_ValueChanged(object sender, EventArgs e)
        {
            rounds = (int)Rounds.Value;
        }
        int getChecksum()
        {
            int checksum = 0;
            for (int i = 0; i < KHASH.Length; i++)
            {
                checksum += KHASH[i];
            }
            return checksum;
        }
        void headWrite(string a, int j)
        {
            for (int i = 0; i < a.Length; i++)
            {
                HEADER[j + i] = (byte)a[i];
            }
        }
        void headWrite(int a, int j)
        {
            for (int i = 0; i < 4; i++)
            {
                HEADER[j + i] = (byte)(a >> ((3 - i) << 3));
            }
        }
    }
}
