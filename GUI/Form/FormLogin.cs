using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace app.GUI
{
    public class FormLogin : Form
    {
        public event OnExitEvent OnExit = null;
        public delegate void OnExitEvent();

        public delegate void EventLogin(string user, string pass);
        public EventLogin OnLogin;

        public FormLogin()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            init_Control();
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        private void Label_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void init_Control()
        {
            Label lbl_Title = new Label() { Text = "LOGIN SYSTEM ", Dock = DockStyle.Top, AutoSize = false, Width = 80, Height = 24, BackColor = App.ColorBg, ForeColor = Color.White, TextAlign = System.Drawing.ContentAlignment.MiddleCenter };
            lbl_Title.MouseDown += Label_MouseDown;
            this.Controls.Add(lbl_Title);

            FlowLayoutPanel box = new FlowLayoutPanel() { Dock = DockStyle.Fill, BackColor = Color.White, FlowDirection = FlowDirection.LeftToRight };
            //box.MouseDown += Label_MouseDown;

            Label lbl_Username = new Label() { Text = "Username ", AutoSize = false, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleRight };
            Label lbl_Pass = new Label() { Text = "Password ", AutoSize = false, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleRight };

            TextBox txt_Username = new TextBox() { Width = 100, Text = "admin", BorderStyle = BorderStyle.FixedSingle };
            TextBox txt_Pass = new TextBox() { Width = 100, PasswordChar = '*', Text = "12345", BorderStyle = BorderStyle.FixedSingle };

            Label lbl_Space = new Label() { Text = "", AutoSize = false, Width = 10 }; 
             
            Button btn_Login = new Button() { Text = "LOGIN", Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter };
            Button btn_Close = new Button() { Text = "CLOSE", Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter };

            box.Controls.AddRange(new Control[] { 
                lbl_Username,
                txt_Username,
                lbl_Pass,
                txt_Pass, 
                lbl_Space,
                btn_Login,
                btn_Close,
            }); 
            this.Controls.Add(box);
            box.BringToFront();
            btn_Login.Focus();

            btn_Close.Click += (se, ev) =>
            {
                if (OnExit != null) OnExit();
            };
            btn_Login.Click += (se, ev) =>
            {
                this.Hide();
                if (OnLogin != null) OnLogin(txt_Username.Text, txt_Pass.Text);
            };
        }

    }
}
