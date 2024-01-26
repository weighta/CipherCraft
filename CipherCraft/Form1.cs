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
using System.Threading;

namespace CipherCraft
{
    public partial class Form1 : Form
    {
        int ALGO_INDEX = 0;
        RDA_cipher RDA = new RDA_cipher();
        RDA_ui RDA_UI = new RDA_ui();
        NBase NBASE = new NBase();
        PolyDiv PolyD = new PolyDiv();
        PolyMul PolyM = new PolyMul();
        JessShifter JS = new JessShifter();
        ArkPay ARK = new ArkPay();
        Debug DB = new Debug();
        GF_2_8 gf2_8 = new GF_2_8();
        GF_2_N gf2_n = new GF_2_N();
        GF_P_N gfp_n = new GF_P_N();
        IRR irr = new IRR();
        Enigma eg = new Enigma("04/02/2022");
        Caesar caes = new Caesar();
        Plugboard plug = new Plugboard();
        Paragraph_Analysis para = new Paragraph_Analysis();
        FieldHopper fh = new FieldHopper();
        Integer integer = new Integer();
        NumberSetGFDecode numsetgfdecode = new NumberSetGFDecode();
        EJMA256 ejma = new EJMA256();

        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            tabControl2.SelectTab(5);
            Form2 form2 = new Form2();
            form2.Show();
        }

        void RDA_TAB()
        {
            pictureBox1.Image = RDA_UI.ALGO;
        }
        void MAIN()
        {
            RDA_TAB();
        }
        private void tabPage1_Click(object sender, EventArgs e)
        {

        }
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox2.Text = RDA.textEncrypter(richTextBox1.Text, textBox1.Text, (int)numericUpDown1.Value);
        }
        void VISUAL_DISPLAY()
        {
            if (checkBox1.Checked)
            {
                FRAME_TIMER.Enabled = true;
            }
        }
        private void tabPage2_Click(object sender, EventArgs e)
        {

        }
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
        private void FRAME_TIMER_TICK(object sender, EventArgs e)
        {
            switch (ALGO_INDEX)
            {
                case 0:
                    {
                        if (!RDA_UI.IDLE)
                        {
                            RDA_UI.refresh();
                            pictureBox1.Image = RDA_UI.FRAME.BACKGROUND;
                        }
                        else
                        {
                            FRAME_TIMER.Enabled = false;
                        }
                        break;
                    }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            ALGO_INDEX = 0;
            VISUAL_DISPLAY();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            REP();
        }
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            REP();
        }
        void REP()
        {
            try
            {
                textBox4.Text = NBASE.strRep(int.Parse(textBox2.Text), int.Parse(textBox3.Text));

            }
            catch
            {
                //nothing here go away
            }
            
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            REM();
        }
        void REM()
        {
            try
            {
                string[] a_ = textBox5.Text.Split(' ');
                int[] a = new int[a_.Length];
                for (int i = 0; i < a_.Length; i++) a[i] = int.Parse(a_[i]);
                string[] b_ = textBox6.Text.Split(' ');
                int[] b = new int[b_.Length];
                for (int i = 0; i < b_.Length; i++) b[i] = int.Parse(b_[i]);
                int[] res = PolyD.DIV_REM(a, b);
                string c = ""; for (int i = 0; i < res.Length; i++) c += res[i] + " ";
                textBox7.Text = c;
            }
            catch { }
        }
        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            REM();
        }

        private void textBox8_TextChanged(object sender, EventArgs e) //JESS SHIFTER
        {
            try
            {
                textBox9.Text = JS.j(textBox8.Text, !checkBox2.Checked);
            }
            catch
            {

            }
        }
        private void textBox10_TextChanged(object sender, EventArgs e) //ARKPAY
        {
            try
            {
                textBox11.Text = ARK.Ark(textBox10.Text, !checkBox3.Checked);
            }
            catch
            {

            }
        }

        private void textBox14_TextChanged(object sender, EventArgs e)
        {
            DIV_QUO();
        }
        void DIV_QUO()
        {

            try
            {
                textBox12.Text = DB.intArrToString(PolyD.DIV_QUO(textBox14.Text, textBox15.Text));
            }
            catch
            {

            }
        }
        private void textBox15_TextChanged(object sender, EventArgs e)
        {
            DIV_QUO();
        }

        private void textBox17_TextChanged(object sender, EventArgs e)
        {
            MUL();
        }
        void MUL()
        {
            try
            {
                textBox16.Text = Print.ARR_TO_STR(PolyM.MUL(textBox17.Text, textBox13.Text));
            }
            catch
            {

            }
        }
        private void textBox13_TextChanged(object sender, EventArgs e)
        {
            MUL();
        }

        private void textBox18_TextChanged(object sender, EventArgs e)
        {
            MUL2_8();
        }
        void MUL2_8()
        {
            try
            {
                textBox20.Text = gf2_8.mul(Convert.ToInt32(textBox18.Text), Convert.ToInt32(textBox21.Text), Convert.ToInt32(textBox19.Text)) + "";
            }
            catch
            {

            }
        }
        private void textBox21_TextChanged(object sender, EventArgs e)
        {
            MUL2_8();
        }

        private void textBox24_TextChanged(object sender, EventArgs e)
        {
            MUL_P_N();
        }
        void MUL_P_N()
        {
            try
            {
                int p = num(textBox24.Text);
                int k = num(textBox22.Text);
                int a = num(textBox25.Text);
                int b = num(textBox26.Text);
                int irr_index = num(textBox27.Text);
                gfp_n.IRRSet(gf2_n.pow(p, k));
                textBox23.Text = gfp_n.mul(p, k, a, b, gfp_n.getIRRbyIndex(p, k, irr_index)) + "";
                label85.Text = Print.ARR_TO_STR(NBASE.rep(gfp_n.getIRRbyIndex(p, k, irr_index), p));
            }
            catch
            {

            }
        }
        private void textBox22_TextChanged(object sender, EventArgs e)
        {
            MUL_P_N();
        }
        private void textBox25_TextChanged(object sender, EventArgs e)
        {
            MUL_P_N();
        }
        private void textBox26_TextChanged(object sender, EventArgs e)
        {
            MUL_P_N();
        }
        private void textBox27_TextChanged(object sender, EventArgs e)
        {
            MUL_P_N();
        }

        private void textBox28_TextChanged(object sender, EventArgs e)
        {
            SUM();
        }
        void SUM()
        {
            try
            {
                textBox30.Text = NBASE.sum(Print.strToIntArr(textBox28.Text), Convert.ToInt32(textBox29.Text)) + "";
            }
            catch
            {

            }
        }
        private void textBox29_TextChanged(object sender, EventArgs e)
        {
            SUM();
        }

        private void textBox32_TextChanged(object sender, EventArgs e)
        {
            IRR();
        }
        void IRR()
        {
            try
            {
                textBox33.Text = gfp_n.IRR(NBASE.rep(num(textBox32.Text), num(textBox31.Text)), num(textBox31.Text)) + "";
            }
            catch
            {

            }
        }
        private void textBox31_TextChanged(object sender, EventArgs e)
        {
            IRR();
        }

        int num(string a)
        {
            try
            {
                return int.Parse(a);
            }
            catch
            {
                //Print.say(a);
            }
            return 0;
        }
        long longnum(string a)
        {
            try
            {
                return long.Parse(a);
            }
            catch
            {
                Print.say(a);
            }
            return 0;
        }

        void MATMUL4x4()
        {
            try
            {
                int[] ans = gf2_8.matMul(gf2_8.makeMat(Print.strToIntArr(textBox34.Text)), Print.strToIntArr(textBox35.Text), num(textBox36.Text));
                textBox37.Text = Print.ARR_TO_STR(ans);
                richTextBox3.Text = Print.ARR_TO_STR(gf2_8.makeMat(Print.strToIntArr(textBox34.Text)));
                richTextBox4.Text = Print.ARR_TO_STR(gf2_8.rowToColumn(Print.strToIntArr(textBox35.Text)));
                richTextBox5.Text = Print.ARR_TO_STR(gf2_8.rowToColumn(ans));
            }
            catch
            {

            }
        }
        private void textBox34_TextChanged(object sender, EventArgs e)
        {
            MATMUL4x4();
        }
        private void textBox35_TextChanged(object sender, EventArgs e)
        {
            MATMUL4x4();
        }
        private void textBox36_TextChanged(object sender, EventArgs e)
        {
            MATMUL4x4();
        }

        private void textBox38_TextChanged(object sender, EventArgs e)
        {
            MATMUL();
        }
        void MATMUL()
        {
            try
            {
                int p = num(textBox24.Text);
                int k = num(textBox22.Text);
                gfp_n.IRRSet((int)Math.Pow(p, k));
                int irr = gfp_n.getIRRbyIndex(p, k, num(textBox40.Text));
                gfp_n.mulFastSet(p, k, irr);
                int[][] ans = gfp_n.GaloisMatMulFast(Matrix.squareMat(Print.strToIntArr(textBox38.Text)), Matrix.rowToColumn(Print.strToIntArr(textBox39.Text)), p, k, irr);
                label68.Text = "(" + irr + ")";
                textBox41.Text = Print.ARR_TO_STR(ans);
                richTextBox6.Text = Print.ARR_TO_STR((Matrix.squareMat(Print.strToIntArr(textBox38.Text))));
                richTextBox7.Text = Print.ARR_TO_STR((Matrix.rowToColumn(Print.strToIntArr(textBox39.Text))));
                richTextBox8.Text = Print.ARR_TO_STR(ans);
            }
            catch
            {

            }
        }
        private void textBox39_TextChanged(object sender, EventArgs e)
        {
            MATMUL();
        }
        private void textBox40_TextChanged(object sender, EventArgs e)
        {
            MATMUL();
        }

        private void textBox42_TextChanged(object sender, EventArgs e)
        {
            SentToNum();
        }
        void SentToNum()
        {
            try
            {
                textBox43.Text = Print.ARR_TO_STR(Print.SentToNum(textBox42.Text));
            }
            catch
            {

            }
        }

        private void textBox46_TextChanged(object sender, EventArgs e)
        {
            ADD();
        }
        void ADD()
        {
            try
            {
                textBox45.Text = NBASE.add(num(textBox46.Text), num(textBox44.Text), num(textBox47.Text)) + "";
            }
            catch
            {

            }
        }
        private void textBox44_TextChanged(object sender, EventArgs e)
        {
            ADD();
        }
        private void textBox47_TextChanged(object sender, EventArgs e)
        {
            ADD();
        }

        private void textBox48_TextChanged(object sender, EventArgs e)
        {
            prime();
        }
        void prime()
        {
            try
            {
                textBox49.Text = gf2_n.bPrime(num(textBox48.Text)) + "";
            }
            catch
            {

            }
        }

        private void textBox51_TextChanged(object sender, EventArgs e)
        {
            NumToSent();
        }
        void NumToSent()
        {
            try
            {
                textBox50.Text = Print.NumToSent(Print.strToIntArr(textBox51.Text));
            }
            catch
            {

            }
        }

        private void panel1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            Data_Analysis ascii;
            for (int i = 0; i < files.Length; i++)
            {
                ascii = new Data_Analysis(File.ReadAllBytes(files[i]));
                if (radioButton1.Checked)
                {
                    string[] findings = ascii.nBaseAsciiAll(); //does everything, should probably have min stride at 2
                    for (int j = 0; j < findings.Length; j++)
                    {
                        File.WriteAllText("Decode\\" + ascii.dict.language[j] + ".txt", findings[i]);
                    }
                }
                else if (radioButton2.Checked)
                {
                    int lang = 0;
                    string findings = ascii.modAsciiLanIndex(lang);
                    File.WriteAllText("Decode\\" + ascii.dict.language[lang] + ".txt", findings);
                }
            }
        } //AsciiDecoder
        private void panel1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox53_TextChanged(object sender, EventArgs e)
        {
            try
            {
                textBox52.Text = Print.ARR_TO_STR(irr.pFact(Convert.ToInt64(textBox53.Text)));
            }
            catch
            {

            }
        } //Prime Factorization

        private void textBox54_TextChanged(object sender, EventArgs e)
        {
            try
            {
                EnigmaEncrypt();
            }
            catch
            {

            }
        } //ENIGMA
        private void textBox59_TextChanged(object sender, EventArgs e)
        {
            EnigmaBuild();
        }
        void EnigmaBuild()
        {
            EnigmaCustom();
            try
            {
                eg.buildMachine(num(textBox60.Text), Print.strToIntArr(textBox56.Text));
                eg.buildRots(textBox59.Text);
                try
                {
                    EnigmaEncrypt();
                }
                catch
                {

                }
                EnigmaStats();
            }
            catch
            {

            }

        }
        void EnigmaCustom()
        {
            label49.Enabled = custom.Checked;
            label50.Enabled = custom.Checked;
            textBox60.Enabled = custom.Checked;
            textBox61.Enabled = custom.Checked;
            if (custom.Checked)
            {
                textBox54.CharacterCasing = CharacterCasing.Normal;
            }
            else
            {
                textBox60.Text = 26 + "";
                textBox61.Text = 65 + "";
                if (inv.Checked && plugboard.Checked)
                {
                    textBox54.CharacterCasing = CharacterCasing.Normal;
                }
                else
                {
                    textBox54.CharacterCasing = CharacterCasing.Upper;
                }
            }
        }
        void EnigmaStats()
        {
            try
            {
                if (stat.Checked)
                {
                    int phase = num(textBox61.Text);
                    richTextBox10.Text = eg.ToString(phase);
                    textBox57.Text = eg.getCurrComb();
                    textBox58.Text = eg.getLatestSeq(phase);
                    label47.Text = eg.getElements(phase);
                }
            }
            catch
            {

            }
        }
        void EnigmaEncrypt()
        {
            string input = textBox54.Text;
            int phase = num(textBox61.Text);
            if (inv.Checked)
            {
                if (custom.Checked)
                {
                    if (plugboard.Checked)
                    {
                        string[] board = textBox66.Text.Split(' ');
                        textBox55.Text = eg.dec(input, phase, board);
                    }
                    else
                    {
                        textBox55.Text = eg.dec(input, phase);
                    }
                }
                else //traditional
                {
                    if (plugboard.Checked)
                    {
                        string[] board = textBox66.Text.Split(' ');
                        textBox55.Text = eg.dec(input, board);
                    }
                    else
                    {
                        textBox55.Text = eg.dec(input);
                    }
                }
            }
            else
            {
                if (custom.Checked)
                {
                    if (plugboard.Checked)
                    {
                        string[] board = textBox66.Text.Split(' ');
                        textBox55.Text = eg.enc(input, phase, board);
                    }
                    else
                    {
                        textBox55.Text = eg.enc(input, phase);
                    }
                }
                else
                {
                    if (plugboard.Checked)
                    {
                        string[] board = textBox66.Text.Split(' ');
                        textBox55.Text = eg.enc(input, board);
                    }
                    else
                    {
                        textBox55.Text = eg.enc(input);
                    }

                }
            }
            EnigmaStats();
        } //Enigma Interface
        private void textBox56_TextChanged(object sender, EventArgs e) //Initial Combination
        {
            EnigmaBuild();
        }
        private void checkBox6_CheckedChanged(object sender, EventArgs e) //Custom Checkbox
        {
            EnigmaBuild();
        }
        private void textBox60_TextChanged(object sender, EventArgs e)
        {
            EnigmaBuild();
        }
        private void textBox61_TextChanged(object sender, EventArgs e)
        {
            EnigmaBuild();
        }
        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            EnigmaBuild();
        }
        private void richTextBox12_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string[] board = richTextBox12.Text.Split(' ');
                if (plug.valid(board))
                {
                    label55.Text = "The plugboard is valid with " + board.Length + " character-unique words";
                }
                else
                {
                    label55.Text = "The plugboard is not valid";
                }
            }
            catch
            {

            }
        } //Plugboard Maker
        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            textBox66.Enabled = plugboard.Checked;
            label56.Enabled = plugboard.Checked;
            EnigmaBuild();
        }
        private void textBox66_TextChanged(object sender, EventArgs e)
        {
            labelCorrect(label56, plug.valid(textBox66.Text.Split(' ')));
            EnigmaBuild();
        }
        private void textBox67_TextChanged(object sender, EventArgs e)
        {

        }
        private void textBox71_TextChanged(object sender, EventArgs e)
        {
        }
        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void textBox62_TextChanged(object sender, EventArgs e) //CAESAR
        {
            richTextBox11.Text = caes.AnalyzeCipher(Print.strToIntArr(textBox62.Text));
            try
            {
                
            }
            catch
            {

            }
        }
        private void textBox63_TextChanged(object sender, EventArgs e)
        {
            caesar();
        }
        void caesar()
        {
            try
            {
                if (checkBox7.Checked)
                {
                    textBox65.Text = caes.enc(textBox63.Text, num(textBox64.Text));
                }
                else
                {
                    textBox65.Text = caes.dec(textBox63.Text, num(textBox64.Text));
                }

            }
            catch
            {

            }
        }
        private void textBox64_TextChanged(object sender, EventArgs e)
        {
            caesar();
        }
        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            caesar();
        }

        private void textBox52_TextChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox13_TextChanged(object sender, EventArgs e) //Paragraph decoder
        {
            richTextBox14.Text = para.histDecode(richTextBox13.Text);
        }

        private void textBox68_TextChanged(object sender, EventArgs e)
        {

        }

        void labelCorrect(Label a, bool b)
        {
            if (b)
            {
                a.ForeColor = Color.Green;
                a.Text = "✓";
            }
            else
            {
                a.ForeColor = Color.Red;
                a.Text = "X";
            }
        }

        private void textBox72_TextChanged(object sender, EventArgs e)
        {
            getFields();
        }
        private void textBox73_TextChanged(object sender, EventArgs e)
        {
            getFields();
        }
        void getFields()
        {
            try
            {
                int[] pfields = gfp_n.getPrimeFields(num(textBox72.Text), num(textBox73.Text));
                int[] cfields = gfp_n.getCompositeFields(num(textBox72.Text), num(textBox73.Text));
                textBox75.Text = Print.ARR_TO_STR(pfields);
                textBox76.Text = Print.ARR_TO_STR(cfields);
                textBox74.Text = pfields.Length + "";
                textBox77.Text = cfields.Length + "";
            }
            catch
            {

            }
        }

        private void textBox70_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox79_TextChanged(object sender, EventArgs e)
        {
            int[] phasecorrect = Print.strToNumArray(textBox79.Text);
            Print.phase(ref phasecorrect, num(textBox80.Text), false);
            textBox78.Text = Print.ARR_TO_STR(phasecorrect);
        }
        void FieldHop() 
        {
        
            try
            {
                int[] a = Print.strToIntArr(textBox78.Text);
                if (checkBox6.Checked)
                {
                    fh.matUnHop(richTextBox15.Text, ref a);
                }
                else
                {
                    fh.matHop(richTextBox15.Text, ref a);
                }
                richTextBox16.Text = fh.db;
                textBox81.Text = Print.ARR_TO_STR(a);
                textBox82.Text = caes.enc(Print.IntARRtoSTR(a), num(textBox80.Text));
            }
            catch
            {

            }
        }

        private void textBox78_TextChanged(object sender, EventArgs e)
        {
            FieldHop();
        }

        private void checkBox6_CheckedChanged_1(object sender, EventArgs e)
        {
            FieldHop();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox9_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            solveInt();
        }
        void solveInt()
        {

            try
            {
                integer.Open(longnum(textBox83.Text));
                richTextBox17.Text = integer.getLog();
            }
            catch
            {

            }
        }

        private void panel2_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }
        private void panel2_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

        }

        private void panel3_DragDrop(object sender, DragEventArgs e) //Number set  gf decoder
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            numsetgfdecode.Set(File.ReadAllBytes(files[0]));
            richTextBox18.Text = numsetgfdecode.log;
        }
        void NumSetGFDecode(int[] a)
        {
            numsetgfdecode.Set_a_Z_b(a, num(textBox86.Text));
            richTextBox18.Text = numsetgfdecode.log;

        }
        private void panel3_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            NumSetGFDecode(Print.strToIntArr(textBox84.Text));
        }
        private void button4_Click(object sender, EventArgs e)
        {
            NumSetGFDecode(Print.hexStrToIntArr(textBox85.Text));
        }

        private void textBox86_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            int gf = num(textBox67.Text);
            if (gfp_n.isValidField(gf))
            {
                label60.Text = "ok";
            }
            else
            {
                label60.Text = "invalid field";
            }

            richTextBox22.Text = Print.ARR_TO_STR(gfp_n.IRR(gf));
        }

        private void textBox84_TextChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox19_TextChanged(object sender, EventArgs e)
        {
            MAT_MUL();
        }
        void MAT_MUL()
        {
            try
            {
                int[][] A = Print.strToNumArray(richTextBox19.Text.Split('\n'));
                int[][] B = Print.strToNumArray(richTextBox20.Text.Split('\n'));
                gfp_n.GaloisMatMulFastFastSet(num(textBox68.Text), num(textBox69.Text), A.Length, B[0].Length);
                int[][] C = gfp_n.GaloisMatMulFastFast(A, B);
                label65.Text = Print.ARR_TO_STR(gfp_n.irr_[num(textBox69.Text) % gfp_n.irr_.Length]);
                richTextBox21.Text = Print.ARR_TO_STR(C);
                richTextBox23.Text = caes.enc(C, 65);
                label66.Text = "pCount: " + gfp_n.irr_.Length;
            }
            catch { }
        }

        private void textBox68_TextChanged_1(object sender, EventArgs e)
        {
            MAT_MUL();
        }

        private void richTextBox20_TextChanged(object sender, EventArgs e)
        {
            MAT_MUL();
        }

        private void richTextBox24_TextChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox25_TextChanged(object sender, EventArgs e)
        {
            invMat();
        }
        void invMat()
        {
            EJMA();
            int[][] junk = gfp_n.GaloisFieldMatINVFull(Print.strToNumArray(richTextBox25.Text.Split('\n'), 16), num(textBox71.Text), num(textBox70.Text));
            richTextBox26.Text = Print.intArrayToHexadecimalString(gfp_n.aug);
            label84.Text = gfp_n.irr_.Length + " irreducibles";
            try
            {

            }
            catch
            {

            }
        }

        private void textBox70_TextChanged_1(object sender, EventArgs e)
        {
            invMat();
        }

        private void textBox71_TextChanged_1(object sender, EventArgs e)
        {
            invMat();
        }

        private void richTextBox27_TextChanged(object sender, EventArgs e)
        {
            EJMA();
        }

        void EJMA()
        {
            byte[] poop = Print.strToByteArray(richTextBox27.Text);
            ejma.R_GFM(ref poop, num(textBox70.Text));
            richTextBox28.Text = ejma.GFM_ToString();
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
            string path = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
        }
    }
}
