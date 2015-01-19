using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ELEBEATMusicEditer
{

    public partial class Form3 : Form
    {

        public double NewBPM;
        public double TextBoxOldBPM;

        public Form3()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double i=0;
            try
            {
                i = double.Parse(textBox1.Text);
            }
            catch
            {
                return;
            }
            if (i > 0)
            {
                NewBPM = i;
                this.DialogResult = DialogResult.OK;
            }

        }

        private void Form3_Load(object sender, EventArgs e)
        {
            textBox1.Text = TextBoxOldBPM.ToString();
        }
    }
}
