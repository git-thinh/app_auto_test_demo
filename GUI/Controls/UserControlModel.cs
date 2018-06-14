using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using app.Core;
using System.Linq;
using System.Linq.Dynamic;

namespace app.GUI
{
    public class ComboboxItem
    {
        public string Text { get; set; }
        public object Value { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }

    public partial class ucModelTitle : UserControl
    {
        public ucModelTitle()
        {
            Label lbl_Name = new Label() { Left = 4, Top = 0, Width = 80, Text = "Name", AutoSize = false, BackColor = SystemColors.Control, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };
            Label lbl_Type = new Label() { Left = 88, Top = 0, Width = 60, Text = "Type", AutoSize = false, BackColor = SystemColors.Control, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };
            Label lbl_Auto = new Label() { Left = 152, Top = 0, Width = 36, Text = "Auto", AutoSize = false, BackColor = SystemColors.Control, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };
            Label lbl_Control = new Label() { Left = 192, Top = 0, Width = 80, Text = "Control", AutoSize = false, BackColor = SystemColors.Control, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };

            Label lbl_LinkType = new Label() { Left = 276, Top = 0, Width = 84, Text = "Join Value", AutoSize = false, BackColor = SystemColors.Control, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };
            Label lbl_LinkModel = new Label() { Left = 362, Top = 0, Width = 100, Text = "Join Model", AutoSize = false, BackColor = SystemColors.Control, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };
            Label lbl_LinkField = new Label() { Left = 464, Top = 0, Width = 100, Text = "Join Field", AutoSize = false, BackColor = SystemColors.Control, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };

            Label lbl_Caption = new Label() { Left = 568, Top = 0, Width = 100, Text = "Caption", AutoSize = false, BackColor = SystemColors.Control, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };
            Label lbl_Index = new Label() { Left = 670, Top = 0, Width = 38, Text = "Index", AutoSize = false, BackColor = SystemColors.Control, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };
            Label lbl_Null = new Label() { Left = 710, Top = 0, Width = 30, Text = "Null", AutoSize = false, BackColor = SystemColors.Control, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };

            this.Controls.AddRange(new Control[] { lbl_Name, lbl_Type, lbl_Auto, lbl_Control,
                lbl_LinkType,lbl_LinkModel, lbl_LinkField,
                lbl_Caption,
                lbl_Index, lbl_Null,
            });
        }
    }

    public partial class ucModelFieldAdd : UserControl
    {
        public delegate void EventRemoveField(int index);
        public EventRemoveField OnRemoveField;

        private ControlKit[] Kits = Enum.GetValues(typeof(ControlKit)).OfType<ControlKit>().ToArray();
        private JoinType[] JoinTypes = Enum.GetValues(typeof(JoinType)).OfType<JoinType>().ToArray();
        const int _top = 7;

        #region [ === VARIABLE === ]

        private TextBoxCustom txt_Name;
        private ComboBox cbo_Type;
        private CheckBox chk_Auto;
        private ComboBox cbo_Kit;

        private ComboBox cbo_LinkType;
        private TextBoxCustom txt_ValueDefault;
        private ComboBox cbo_LinkModel;
        private ComboBox cbo_LinkField;

        private TextBoxCustom txt_Caption;
        private CheckBox chk_Index;
        private CheckBox chk_Null;

        private Button btn_Ext;
        private TextBoxCustom txt_CaptionShort;
        private TextBoxCustom txt_Des;
        private CheckBox chk_MobiShow;
        private CheckBox chk_TabletShow;
        private CheckBox chk_Duplicate;
        private CheckBox chk_Encrypt;

        private Button btn_Remove;

        #endregion

        public ucModelFieldAdd(int index, IDataFile db)
        {
            if (index != 0 && index % 2 == 0) BackColor = Color.Gray;

            string[] models = db.GetListDB();

            #region [ === UI === ]

            txt_Name = new TextBoxCustom() { Left = 4, Top = _top, Width = 80, Name = "name" + index.ToString(), WaterMark = "Name ...", BorderStyle = BorderStyle.FixedSingle, TextAlign = HorizontalAlignment.Center };
            cbo_Type = new ComboBox() { Left = 88, Top = _top, Width = 60, Name = "type" + index.ToString(), DropDownStyle = ComboBoxStyle.DropDownList, };
            chk_Auto = new CheckBox() { Left = 164, Top = _top, Width = 22, Name = "auto" + index.ToString() };
            for (int k = 0; k < dbType.Types.Length; k++) cbo_Type.Items.Add(dbType.Types[k]);
            cbo_Type.SelectedIndex = 0;
            cbo_Kit = new ComboBox() { Left = 192, Top = _top, Width = 80, Name = "kit" + index.ToString(), DropDownStyle = ComboBoxStyle.DropDownList, };
            foreach (ControlKit kit in Kits)
                cbo_Kit.Items.Add(new ComboboxItem() { Text = kit.ToString().ToUpper(), Value = ((int)kit) });
            cbo_Kit.SelectedIndex = 0;

            cbo_LinkType = new ComboBox() { Left = 276, Top = _top, Width = 84, Name = "link_type" + index.ToString(), DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (JoinType ti in JoinTypes)
                cbo_LinkType.Items.Add(new ComboboxItem() { Text = ti.ToString().ToUpper(), Value = ((int)ti) });
            cbo_LinkType.SelectedIndex = 0;
            txt_ValueDefault = new TextBoxCustom() { Left = 363, Top = _top, Width = 200, Name = "value_default" + index.ToString(), WaterMark = "Default value: v1|v2|...", BorderStyle = BorderStyle.FixedSingle };
            cbo_LinkModel = new ComboBox() { Visible = false, Left = 363, Top = _top, Width = 100, Name = "link_model" + index.ToString(), DropDownStyle = ComboBoxStyle.DropDownList, };
            cbo_LinkField = new ComboBox() { Visible = false, Left = 465, Top = _top, Width = 100, Name = "link_field" + index.ToString(), DropDownStyle = ComboBoxStyle.DropDownList, };
            cbo_LinkModel.Items.Add(new ComboboxItem() { Text = "", Value = "" });
            for (int k = 0; k < models.Length; k++)
                cbo_LinkModel.Items.Add(new ComboboxItem() { Text = models[k].ToUpper(), Value = models[k] });
            cbo_LinkModel.SelectedIndex = 0;
            txt_Caption = new TextBoxCustom() { Left = 568, Top = _top, Width = 100, Name = "caption" + index.ToString(), WaterMark = "Caption title ...", BorderStyle = BorderStyle.FixedSingle, TextAlign = HorizontalAlignment.Center };
            chk_Index = new CheckBox() { Left = 684, Top = _top, Width = 20, Name = "index" + index.ToString(), Checked = false };
            chk_Null = new CheckBox() { Left = 720, Top = _top, Width = 15, Name = "null" + index.ToString(), Checked = true };

            ///////////////////////////////////////////////////////////////////////////////////////////////

            btn_Ext = new Button() { Text = "+", Left = 744, Top = _top, Width = 20 };
            int hiBox = 30;

            txt_CaptionShort = new TextBoxCustom() { Left = 4, Top = _top + 30, Width = 100, Name = "caption_short" + index.ToString(), WaterMark = "Caption short ...", BorderStyle = BorderStyle.FixedSingle };
            txt_Des = new TextBoxCustom() { Left = 112, Top = _top + 30, Width = 150, Name = "des" + index.ToString(), WaterMark = "Description ...", BorderStyle = BorderStyle.FixedSingle, TextAlign = HorizontalAlignment.Center };
            chk_MobiShow = new CheckBox() { Left = 268, Top = _top + 30, Name = "mobi" + index.ToString(), Text = "Show Mobi", Width = 90, Checked = true };
            chk_TabletShow = new CheckBox() { Left = 358, Top = _top + 30, Name = "tablet" + index.ToString(), Text = "Show Tablet", Width = 100, Checked = true };

            chk_Duplicate = new CheckBox() { Left = 460, Top = _top + 30, Name = "duplicate" + index.ToString(), Text = "Duplicate", Width = 100, Checked = true };
            chk_Encrypt = new CheckBox() { Left = 560, Top = _top + 30, Name = "encrypt" + index.ToString(), Text = "Encrypt", Width = 100, Checked = false };

            btn_Remove = new Button() { Text = "Remove", Left = 704, Top = _top + 30, Width = 60 };
            btn_Remove.Click += (se, ev) => remove_Field(index);
            btn_Ext.Click += (se, ev) =>
            {
                if (btn_Ext.Text == "-")
                {
                    btn_Ext.Text = "+";
                    this.Height = this.Height - hiBox;
                }
                else
                {
                    btn_Ext.Text = "-";
                    this.Height = this.Height + hiBox;
                }
            };

            this.Controls.AddRange(new Control[] { txt_Name, cbo_Type, chk_Auto, cbo_Kit,
                cbo_LinkType,txt_ValueDefault, cbo_LinkModel,cbo_LinkField,
                txt_Caption,
                chk_Index, chk_Null,
                btn_Ext, txt_CaptionShort, txt_Des, chk_MobiShow, chk_TabletShow, chk_Duplicate, chk_Encrypt,
                btn_Remove
            });

            #endregion

            ///////////////////////////////////////////////////////////////////////////////////////////////

            #region [ === EVENT === ]

            cbo_LinkModel.Visible = false;
            cbo_LinkField.Visible = false;
            int ijt = JoinTypes.FindIndex(x => x == JoinType.DEF_VALUE);
            cbo_LinkType.SelectedIndex = ijt;

            cbo_Type.SelectedIndexChanged += (se, ev) =>
            {
                string type = dbType.Types[cbo_Type.SelectedIndex];
                if (type == typeof(Boolean).Name)
                {
                    chk_Auto.Checked = false;
                    chk_Auto.Visible = false;
                    int iKit = Kits.FindIndex(x => x == ControlKit.CHECK);
                    cbo_Kit.SelectedIndex = iKit;
                    cbo_Kit.Enabled = false;
                    cbo_LinkType.Visible = false;
                    return;
                }
                else
                {
                    chk_Auto.Visible = true;
                    cbo_Kit.Enabled = true;
                    cbo_Kit.SelectedIndex = 0;
                    cbo_LinkType.Visible = true;
                    return;
                }

                if (type == typeof(DateTime).Name)
                {
                    int iKit = Kits.FindIndex(x => x == ControlKit.TEXT_DATETIME);
                    cbo_Kit.SelectedIndex = iKit;
                    cbo_Kit.Enabled = false;
                    return;
                }
                else
                {
                    cbo_Kit.Enabled = true;
                    cbo_Kit.SelectedIndex = 0;
                    return;
                }
            };

            cbo_Kit.SelectedIndexChanged += (se, ev) => kit_Change();

            cbo_LinkType.SelectedIndexChanged += (se, ev) =>
            {
                JoinType ji = JoinTypes[cbo_LinkType.SelectedIndex];
                switch (ji)
                {
                    case JoinType.NONE:
                        cbo_LinkType.SelectedIndex = 1;
                        break;
                    case JoinType.DEF_VALUE:
                        txt_ValueDefault.Visible = true;
                        cbo_LinkModel.Visible = false;
                        cbo_LinkField.Visible = false;
                        break;
                    case JoinType.JOIN_MODEL:
                        txt_ValueDefault.Visible = false;
                        cbo_LinkModel.Visible = true;
                        cbo_LinkField.Visible = true;
                        break;
                }
            };
            cbo_LinkModel.SelectedIndexChanged += (se, ev) =>
            {
                cbo_LinkField.Items.Clear();
                if (cbo_LinkModel.SelectedIndex > 0)
                {
                    string m = models[cbo_LinkModel.SelectedIndex - 1];
                    if (!string.IsNullOrEmpty(m))
                    {
                        var fs = db.GetFields(m).ToArray();
                        if (fs.Length > 0)
                        {
                            for (int k = 0; k < fs.Length; k++)
                                cbo_LinkField.Items.Add(new ComboboxItem() { Text = fs[k].Name.ToUpper() + " - " + fs[k].Type.Name, Value = fs[k].Name });
                            cbo_LinkField.SelectedIndex = 0;
                        }
                    }
                }
            };

            chk_Auto.CheckedChanged += (se, ev) =>
            {
                kit_Change();
                if (chk_Auto.Checked)
                {
                    // FIELD KEY AUTO
                    chk_Null.Checked = false;
                    chk_Null.Visible = false;
                    cbo_Kit.Visible = false;
                    cbo_LinkType.Visible = false;
                    txt_ValueDefault.Visible = false;
                    cbo_LinkModel.Visible = false;
                    cbo_LinkField.Visible = false;
                }
                else
                {
                    // FIELD DATA
                    chk_Null.Visible = true;
                    cbo_Kit.Visible = true;
                    cbo_LinkType.Visible = true;
                    txt_ValueDefault.Visible = true;
                }
            };

            #endregion

            ///////////////////////////////////////////////////////////////////////////////////////////////
        }

        private void kit_Change()
        {
            ControlKit kit = Kits[cbo_Kit.SelectedIndex];
            switch (kit)
            {
                case ControlKit.SELECT:
                    cbo_LinkType.Visible = true;
                    cbo_LinkType.Enabled = true;
                    cbo_LinkType.SelectedIndex = 1;
                    txt_ValueDefault.Visible = true;
                    cbo_LinkModel.Visible = false;
                    cbo_LinkField.Visible = false;
                    break;
                case ControlKit.LOOKUP:
                    cbo_LinkType.Enabled = false;
                    cbo_LinkType.Visible = true;
                    cbo_LinkType.SelectedIndex = 2;
                    txt_ValueDefault.Visible = false;
                    cbo_LinkModel.Visible = true;
                    cbo_LinkField.Visible = true;
                    break;
                default:
                    if (kit == ControlKit.TEXT_PASS)
                        chk_Encrypt.Checked = true;
                    else
                        chk_Encrypt.Checked = false;

                    cbo_LinkType.Visible = true;
                    cbo_LinkType.Enabled = true;
                    txt_ValueDefault.Visible = true;
                    cbo_LinkModel.Visible = false;
                    cbo_LinkField.Visible = false;
                    break;
            }
        }

        private void remove_Field(int index)
        {
            if (OnRemoveField != null && MessageBox.Show("Are you sure remove this fields ?", "Remove Field",
                MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                OnRemoveField(index);
        }

    }

    public partial class ucDataItemAdd : UserControl
    {
        public const int Height_Min = 26;
        public const int Height_Max = 150;

        public int _Height { private set; get; }
        public int _Width { private set; get; }
        public ucDataItemAdd(int index, dbField field)
        {
            int wi_Name = 110, wi_Max = 630, wi_ = 200;
            _Height = Height_Min;
            _Width = wi_ + wi_Name + 8;
            string caption = string.IsNullOrEmpty(field.Caption) ? field.Name : field.Caption;
            Label lbl_Name = new Label() { Left = 4, Top = 0, Text = caption, AutoSize = false, Width = wi_Name, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleRight };
            this.Controls.Add(lbl_Name);

            switch (field.Kit)
            {
                case ControlKit.LABEL:
                    Label lbl = new Label() { Left = wi_Name + 8, AutoSize = false, Width = wi_, Height = _Height - 5, BackColor = SystemColors.Control, Tag = field, };
                    this.Controls.Add(lbl);
                    break;
                case ControlKit.CHECK:
                    CheckBox chk = new CheckBox() { Left = wi_Name + 8, Top = 0, Tag = field, };
                    this.Controls.Add(chk);
                    break;
                case ControlKit.RADIO:
                    RadioButton radio = new RadioButton() { Left = wi_Name + 8, Top = 0, Tag = field, };
                    this.Controls.Add(radio);
                    break;
                case ControlKit.COLOR:
                    Label lbl_Color = new Label() { Left = wi_Name + 8, AutoSize = false, Width = 44, Height = _Height - 5, BackColor = Color.Gray, Tag = field, };
                    this.Controls.Add(lbl_Color);
                    break;
                case ControlKit.SELECT:
                    ComboBox cbo = new ComboBox() { Left = wi_Name + 8, Top = 0, Width = wi_, Height = _Height, DropDownStyle = ComboBoxStyle.DropDownList, Tag = field, };
                    this.Controls.Add(cbo);
                    break;
                case ControlKit.TEXT_PASS:
                    TextBox txt_Pass = new TextBox() { Left = wi_Name + 8, Top = 0, Width = wi_, Height = _Height, PasswordChar = '*', BorderStyle = BorderStyle.FixedSingle, Multiline = false, ScrollBars = ScrollBars.None, WordWrap = false, Tag = field, };
                    this.Controls.Add(txt_Pass);
                    break;
                case ControlKit.TEXT_DATE:
                case ControlKit.TEXT_DATETIME:
                case ControlKit.TEXT_TIME:
                    DateTimePicker dt = new DateTimePicker() { Left = wi_Name + 8, Top = 0, CustomFormat = "dd-MM-yyyy HH:mm:ss", Format = DateTimePickerFormat.Custom, Tag = field, };
                    this.Controls.Add(dt);
                    break;
                case ControlKit.TEXT_EMAIL:
                    TextBox txt_Email = new TextBox() { Left = wi_Name + 8, Top = 0, Width = wi_, Height = _Height, BorderStyle = BorderStyle.FixedSingle, Multiline = false, ScrollBars = ScrollBars.None, WordWrap = false, Tag = field, };
                    this.Controls.Add(txt_Email);
                    break;
                case ControlKit.TEXT_FILE:
                    TextBox txt_File = new TextBox() { Left = wi_Name + 8, Top = 0, Width = wi_, Height = _Height, ReadOnly = true, BorderStyle = BorderStyle.FixedSingle, Multiline = false, ScrollBars = ScrollBars.None, WordWrap = false, Tag = field, };
                    this.Controls.Add(txt_File);
                    break;
                case ControlKit.TEXTAREA:
                case ControlKit.HTML:
                    _Height = Height_Max;
                    _Width = wi_Max + wi_Name + 8;
                    TextBox text_area = new TextBox() { Left = wi_Name + 8, Top = 0, Width = wi_Max, Height = _Height, BorderStyle = BorderStyle.FixedSingle, Multiline = true, ScrollBars = ScrollBars.Vertical, WordWrap = true, Tag = field, };
                    this.Controls.Add(text_area);
                    break;
                case ControlKit.LOOKUP:
                    TextBox txt_Lookup = new TextBox() { Left = wi_Name + 8, Top = 0, Width = wi_, Height = _Height, ReadOnly = true, BorderStyle = BorderStyle.FixedSingle, Multiline = false, ScrollBars = ScrollBars.None, WordWrap = false, Tag = field, };
                    this.Controls.Add(txt_Lookup);
                    break;
                default: //case ControlKit.TEXT: break;
                    TextBox txt = new TextBox() { Left = wi_Name + 8, Top = 0, Width = wi_, Height = _Height, BorderStyle = BorderStyle.FixedSingle, Multiline = false, ScrollBars = ScrollBars.None, WordWrap = false, Tag = field, };
                    this.Controls.Add(txt);
                    break;
            }
        }
    }
}


