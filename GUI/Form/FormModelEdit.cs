using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using app.Core;
using System.Linq.Dynamic;
using System.Threading;

namespace app.GUI
{
    public class FormModelEdit : FormBase
    {

        public delegate void EventLogin(string user, string pass);
        public EventLogin OnLogin;

        private readonly IDataFile db;

        public FormModelEdit(IDataFile _db)
        {
            db = _db;
            

            var fields = db.GetFields("Test");

            FlowLayoutPanel boi_Filter = new FlowLayoutPanel() { Dock = DockStyle.Fill, Height = (((fields.Length / 3) + 0) * 25), AutoScroll = false, Padding = new Padding(0), BackColor = Color.WhiteSmoke };
            boi_Filter.FlowDirection = FlowDirection.LeftToRight;
            boi_Filter.MouseDown += FormMove_MouseDown;

            Label lbl_Name = new Label() { Text = "Name", AutoSize = false, Width = 80, BackColor = Color.Gray, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };
            Label lbl_Type = new Label() { Text = "Type", AutoSize = false, Width = 80, BackColor = Color.Gray,ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };
            Label lbl_Caption = new Label() { Text = "Caption", AutoSize = false, Width = 80,BackColor = Color.Gray, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };
            boi_Filter.Controls.AddRange(new Control[] {
                            lbl_Name,
                            lbl_Type,
                            lbl_Caption,
                        });


            StringBuilder wh_Contain = new StringBuilder();
            for (int k = 0; k < fields.Length; k++)
            {
                var dp = fields[k];
                wh_Contain.Append(dp.Name + (dp.Type.Name == "String" ? string.Empty : ".ToString()") + ".Contains(@0) ");
                if (k < fields.Length - 1) wh_Contain.Append(" || ");

                TextBox lbl = new TextBox() { Name = "lbl" + k.ToString(), Text = dp.Name,  Width = 80, BorderStyle = BorderStyle.FixedSingle, TextAlign = HorizontalAlignment.Center };
                ComboBox cbo = new ComboBox() { Name = "cbo" + k.ToString(), Width = 80, DropDownStyle = ComboBoxStyle.DropDownList, };
                //if (dp.Type.Name == "String")
                //    for (int ki = 0; ki < OpString.Length; ki++) cbo.Items.Add(OpString[ki]);
                //else
                //    for (int ki = 0; ki < OpNumber.Length; ki++) cbo.Items.Add(OpNumber[ki]);
                //cbo.SelectedIndexChanged += (se, ev) => 
                //{
                //};
                //cbo.DataSource = dp.Type.Name == "String" ? OpString : OpNumber;

                TextBox txt = new TextBox() { Name = "txt" + k.ToString(), Width = 80, BorderStyle = BorderStyle.FixedSingle, TextAlign = HorizontalAlignment.Center };
                boi_Filter.Controls.AddRange(new Control[] {
                            lbl,
                            cbo,
                            txt,
                        });

                if (k != 0 && k % 3 == 0)
                {
                    Label sp = new Label() { Text = "", AutoSize = false, Width = App.Width, Height = 1, };
                    boi_Filter.Controls.Add(sp);
                }
            }//end for fields



            Panel boi_Footer = new Panel() { Dock = DockStyle.Bottom, Height = 25 };
            Button btn_Add = new Button() { Dock = DockStyle.Right, Text = "ADD", BackColor = Color.WhiteSmoke, Width = 60, TextAlign = ContentAlignment.MiddleCenter };
            Button btn_Remove = new Button() { Dock = DockStyle.Right, Text = "REMOVE", BackColor = Color.WhiteSmoke, Width = 70, TextAlign = ContentAlignment.MiddleCenter };
            Button btn_Submit = new Button() { Dock = DockStyle.Right, Text = "SUBMIT", BackColor = Color.WhiteSmoke, Width = 60, TextAlign = ContentAlignment.MiddleCenter };
            Button btn_Close = new Button() { Dock = DockStyle.Right, Text = "CLOSE", BackColor = Color.WhiteSmoke, Width = 60, TextAlign = ContentAlignment.MiddleCenter };
            boi_Footer.Controls.AddRange(new Control[] {
                btn_Add,
                btn_Remove,
                btn_Submit,
                btn_Close
            });


            this.Controls.AddRange(new Control[] {
                boi_Filter,
                boi_Footer,
            });
            boi_Filter.BringToFront();
            btn_Submit.Focus();

            btn_Close.Click += (se, ev) =>
            {
                this.Close();
            };
            btn_Submit.Click += (se, ev) =>
            {
            };

            new Thread(() => 
            {
                this.Width = 270;
            }).Start();
        }

    }
}
