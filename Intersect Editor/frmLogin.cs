﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Intersect_Editor
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
        }

        private void frmLogin_Load(object sender, EventArgs e)
        {
            Globals.GameSocket = new Socket();
            tmrSocket.Enabled = true;
            
        }

        private void tmrSocket_Tick(object sender, EventArgs e)
        {
            Globals.GameSocket.Update();
            string statusString = "Connecting to server...";
            if (Globals.GameSocket.isConnected)
            {
                statusString = "Connected to server.";
            }
            else if (Globals.GameSocket.isConnecting)
            {

            }
            else
            {
                statusString = "Failed to connect, retrying in " + ((Globals.GameSocket.reconnectTime - Environment.TickCount)/1000).ToString("0") + " seconds.";
            }
            Globals.loginForm.lblStatus.Text = statusString;
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (Globals.GameSocket.isConnected)
            {
                if (txtUsername.Text.Trim().Length > 0 && txtPassword.Text.Trim().Length > 0)
                {
                    PacketSender.SendLogin(txtUsername.Text.Trim(), txtPassword.Text.Trim());
                }
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            if (e.KeyChar == 13)
            {
                e.Handled = true;
                if (txtUsername.Text.Trim().Length > 0 && txtPassword.Text.Trim().Length > 0)
                {
                    PacketSender.SendLogin(txtUsername.Text.Trim(), txtPassword.Text.Trim());
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Exit();
        }
    }


}