using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GestioneTornelloREA
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btn_exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            btn_start.Visible = true;
            btn_stop.Visible = false;
            Business.TransitionBusiness.StopService();
            lbl_status.Text = "Disconected";
        }

        private void btn_start_Click(object sender, EventArgs e)
        {
            btn_stop.Visible = true;
            btn_start.Visible = false;
            Business.TransitionBusiness.StartService();
            lbl_status.Text = "Started";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Business.TransitionBusiness.SendOpen();
        }

        private void button2_Click(object sender, EventArgs e)
        {
           // Business.TransitionBusiness.SendPassRes();
        }

        private void btnCanAccess_Click(object sender, EventArgs e)
        {
            var msg = Business.TransitionBusiness.GetCanPass(txtBarCode.Text);
            MessageBox.Show(msg);
        }
    }
}
