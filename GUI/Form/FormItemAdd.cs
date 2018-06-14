using System.Windows.Forms;
using System.Drawing;
using app.Core;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System;

namespace app.GUI
{

    public class FormItemAdd : FormBase
    {
        const int fWidth = 800;
        const int fHeight = 550;

        public delegate void EventSubmit(string dbName, Dictionary<string, object> data);
        public EventSubmit OnSubmit;

        private readonly IDataFile db;

        public FormItemAdd(IDataFile _db, string dbName)
            : base(true)
        {
            int _Hi = 0;
            db = _db;
            ClientSize = new Size(fWidth, Screen.PrimaryScreen.WorkingArea.Height - 80);
            Top = 40;

            var model = db.GetModel(dbName);

            #region [ === CONTROLS UI === ]

            FlowLayoutPanel boi_Filter = new FlowLayoutPanel()
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                Padding = new Padding(0),
                BackColor = Color.WhiteSmoke,
                FlowDirection = FlowDirection.LeftToRight,
            };
            boi_Filter.MouseDown += FormMove_MouseDown;

            var fields = model.Fields.Where(x => x.IsKeyAuto == false).ToArray();

            for (int ki = 0; ki < fields.Length; ki++)
            {
                var uc = new ucDataItemAdd(ki, fields[ki]) { Name = "uc" + ki.ToString(), };
                uc.Height = uc._Height;
                uc.Width = uc._Width;
                boi_Filter.Controls.Add(uc);
            }

            int kitMin = ((fields.Select(x => x.Kit).Where(x => x != ControlKit.HTML && x != ControlKit.TEXTAREA)
                .Count() / 2) + 1);
            int hi_Min = (kitMin * ucDataItemAdd.Height_Min) + (kitMin * 10);
            int hi_Max = fields.Select(x => x.Kit).Where(x => x == ControlKit.HTML || x == ControlKit.TEXTAREA)
                .Count() * ucDataItemAdd.Height_Max;
            _Hi = hi_Min + hi_Max + 60;

            Panel boi_Action = new Panel() { Dock = DockStyle.Bottom, Height = 25 };
            Button btn_Submit = new Button() { Dock = DockStyle.Right, Text = "SUBMIT", BackColor = Color.WhiteSmoke, Width = 60, TextAlign = ContentAlignment.MiddleCenter };
            Button btn_Reset = new Button() { Dock = DockStyle.Right, Text = "RESET", BackColor = Color.WhiteSmoke, Width = 60, TextAlign = ContentAlignment.MiddleCenter };
            boi_Action.Controls.AddRange(new Control[] { btn_Submit, btn_Reset });
            boi_Action.MouseDown += FormMove_MouseDown;

            this.Controls.AddRange(new Control[] { boi_Filter, boi_Action });
            boi_Action.BringToFront();
            boi_Filter.BringToFront();
            btn_Submit.Focus();

            #endregion

            if (_Hi < Screen.PrimaryScreen.WorkingArea.Height)
            {
                if (_Hi < 200) _Hi = 200;
                ClientSize = new System.Drawing.Size(fWidth, _Hi);
            }

            btn_Submit.Click += (se, ev) => form_Submit(dbName, boi_Filter);
            HideScrollBar(boi_Filter.Handle, ScrollBarHide.SB_HORZ);
        }

        private readonly Type m_TypeField = typeof(dbField);
        private void form_Submit(string dbName, FlowLayoutPanel boi_Filter)
        {
            if (string.IsNullOrEmpty(dbName))
            {
                MessageBox.Show("Please input Model Name and fields: name, type, caption.");
                return;
            }

            var dt = new Dictionary<string, object>();
            foreach (Control c in boi_Filter.Controls)
            {
                foreach (Control fi in c.Controls)
                {
                    if (fi.Tag != null && fi.Tag.GetType() == m_TypeField)
                    {
                        object val = null;
                        var fo = (IDbField)fi.Tag;
                        switch (fo.Kit)
                        {
                            case ControlKit.LABEL: // Label 
                                val = fi.Text;
                                break;
                            case ControlKit.CHECK: // CheckBox 
                                val = (fi as CheckBox).Checked;
                                break;
                            case ControlKit.RADIO: // RadioButton 
                                break;
                            case ControlKit.COLOR: // Label 
                                val = fi.Text;
                                break;
                            case ControlKit.SELECT: // ComboBox 
                                val = (fi as ComboBox).SelectedValue;
                                break;
                            case ControlKit.TEXT_PASS: // TextBox 
                                val = fi.Text;
                                break;
                            case ControlKit.TEXT_DATE:
                            case ControlKit.TEXT_DATETIME:
                            case ControlKit.TEXT_TIME: // DateTimePicker 
                                val = (fi as DateTimePicker).Value;
                                break;
                            case ControlKit.TEXT_EMAIL: // TextBox 
                                val = fi.Text;
                                break;
                            case ControlKit.TEXT_FILE: // TextBox 
                                val = fi.Text;
                                break;
                            case ControlKit.TEXTAREA:
                            case ControlKit.HTML: // TextBox 
                                val = fi.Text;
                                break;
                            case ControlKit.LOOKUP: // TextBox  
                                val = fi.Text;
                                break;
                            default:  // TextBox  
                                val = fi.Text;
                                break;
                        }
                        if (val != null) dt.Add(fo.Name, val);
                    }
                }//end for fields

            }//end for controls
            if (dt.Count > 0)
                if (OnSubmit != null) OnSubmit(dbName, dt); 
            else
            {
                MessageBox.Show("Please input fields.");
            }
        }

    }
}
