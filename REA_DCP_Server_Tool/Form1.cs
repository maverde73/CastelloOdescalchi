using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using REA_DCP_Server_Tool;
using System.Net.Sockets;
using System.IO;

namespace REA_DCP_Server_Tool
{
    public partial class Form1 : Form
    {
        private UDP_server UDP_srv;
        public Form1()
        {
            this.InitializeComponent();
            IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            for (int i = (int)addressList.Length - 1; i >= 0; i--)
            {
                if (addressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    this.comboBox1.Items.Add(addressList[i]);
                }
            }
            if (this.comboBox1.Items.Count > 0)
            {
                this.comboBox1.SelectedIndex = 0;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (this.UDP_srv != null)
            {
                //STOP
                //UDP_server uDPSrv = this.UDP_srv;
                //uDPSrv.e_comminfo -= new REA_DCP_Server_Tool.UDP_server.CommunInfo(this.CommInfo);
                //this.UDP_srv.Close();
                //this.UDP_srv = null;

                //UDP_server uDPSrv = this.UDP_srv;
                this.UDP_srv.e_comminfo -= new REA_DCP_Server_Tool.UDP_server.CommunInfo(this.CommInfo);
                this.UDP_srv.Close();
                this.UDP_srv = null;
                this.button1.Text = "Start";
                return;
            }
            else
            {
                //START
                this.UDP_srv = new UDP_server((IPAddress)this.comboBox1.SelectedItem, (int)this.numericUpDown1.Value, (int)this.numericUpDown2.Value);
                //UDP_server uDPServer = this.UDP_srv;
                //uDPServer.e_comminfo += new REA_DCP_Server_Tool.UDP_server.CommunInfo(this.CommInfo);


                this.UDP_srv.e_comminfo += new REA_DCP_Server_Tool.UDP_server.CommunInfo(this.CommInfo);

                this.button1.Text = "Stop";
                if (!this.radioButton1.Checked)
                {
                    if (!this.radioButton2.Checked)
                    {
                        this.UDP_srv.act = 2;
                    }
                    else
                    {
                        this.UDP_srv.act = 1;
                    }
                }
                else
                {
                    this.UDP_srv.act = 0;
                }
                this.UDP_srv.cnt = (int)this.numericUpDown3.Value;
                return;
            }
        }

        private void CommInfo(IPAddress ip, string rcvdata, string snddata)
        {
            if (!base.InvokeRequired)
            {
                this.richTextBox1.SelectionColor = Color.Black;
                DateTime now = DateTime.Now;
                string[] str = new string[5];
                str[0] = "time - ";
                str[1] = now.ToString("HH:mm:ss.");
                str[2] = string.Format("{0:d3},   IP - ", now.Millisecond);
                str[3] = ip.ToString();
                str[4] = ":\n";
                this.richTextBox1.AppendText(string.Concat(str));
                this.richTextBox1.SelectionColor = Color.Red;
                this.richTextBox1.AppendText(string.Concat(rcvdata, "\n"));
                this.richTextBox1.SelectionColor = Color.Blue;
                this.richTextBox1.AppendText(string.Concat(snddata, "\n"));
                this.richTextBox1.ScrollToCaret();

                #region Lettura Barcode
                DataSet ds = new DataSet();
                StringReader SR = new StringReader(rcvdata);
                ds.ReadXml(SR);
                if (ds.Tables.Count > 1)
                {
                    var dt = ds.Tables[1];
                    if (dt.Columns.Contains("chip"))
                    {
                        //lettura barcode
                        var barcode = dt.Rows[0]["chip"];
                        //todo: verifica barcode e fare send
                    }
                }
                #endregion

                return;
            }
            else
            {
                //la chiamata è stata effettuata da un altro thread
                object[] objArray = new object[3];
                objArray[0] = ip;
                objArray[1] = rcvdata;
                objArray[2] = snddata;
                base.BeginInvoke(new REA_DCP_Server_Tool.UDP_server.CommunInfo(this.CommInfo), objArray);
                return;
            }


            
        }

      

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.UDP_srv != null)
            {
                UDP_server uDPSrv = this.UDP_srv;
                uDPSrv.e_comminfo -= new REA_DCP_Server_Tool.UDP_server.CommunInfo(this.CommInfo);
                this.UDP_srv.Close();
            }
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            if (this.UDP_srv != null)
            {
                this.UDP_srv.cnt = (int)this.numericUpDown3.Value;
                return;
            }
            else
            {
                return;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (this.UDP_srv != null)
            {
                if (!this.radioButton1.Checked)
                {
                    if (!this.radioButton2.Checked)
                    {
                        this.UDP_srv.act = 2;
                        return;
                    }
                    else
                    {
                        this.UDP_srv.act = 1;
                        return;
                    }
                }
                else
                {
                    this.UDP_srv.act = 0;
                    return;
                }
            }
            else
            {
                return;
            }
        }

        private void btnPanic_Click(object sender, EventArgs e)
        {
            string state = "on";

            if (btnPanic.Text == "Disattiva i tornelli")
            {
                btnPanic.Text = "Riattiva i tornelli";
                state = "on";
            }
            else
            {
                btnPanic.Text = "Disattiva i tornelli";
                state = "off";
            }

            this.UDP_srv.SetPanicState(state);
        }

    }
}
