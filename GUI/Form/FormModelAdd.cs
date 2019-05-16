using System.Windows.Forms;
using System.Drawing;
using app.Core;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System;
using Newtonsoft.Json;
using System.IO;

namespace app.GUI
{

    public class FormModelAdd : FormBase
    {
        const int fWidth = 800;
        const int fHeight = 500;

        public delegate void EventSubmit(dbModel model);
        public EventSubmit OnSubmit;

        private readonly IDataFile db;
        Label lbl_MODEL_NAME_KEY_TYPE;
        TextBoxCustom txt_MODEL_NAME_KEY_TYPE;

        Label lbl_MODEL_NAME_KEY_NAME;
        TextBoxCustom txt_MODEL_NAME_KEY_NAME;


        public FormModelAdd(IDataFile _db)
            : base("Model Add", true)
        {
            db = _db;
            ClientSize = new Size(fWidth, fHeight);

            countField = 0;
            listIndexRemove.Clear();

            #region [ === CONTROLS UI === ]

            Panel boi_DbName = new Panel() { Dock = DockStyle.Top, Height = 62, };

            boi_DbName.MouseDown += FormMove_MouseDown;
            Label lbl_Name = new Label() { Left = 4, Width = 120, Top = 7, Text = "Model name", AutoSize = false, Height = 20, BackColor = Color.Gray, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };
            TextBoxCustom txt_Name = new TextBoxCustom() { Left = 124, Top = 7, Width = 120, WaterMark = "Model name ...", BorderStyle = BorderStyle.FixedSingle, TextAlign = HorizontalAlignment.Center };
            Label lbl_Caption = new Label() { Visible = false, Left = 252, Top = 7, Width = 120, Text = "Model caption1", AutoSize = false, Height = 20, BackColor = Color.Gray, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };
            TextBoxCustom txt_Caption = new TextBoxCustom() { Visible = false, Left = 372, Top = 7, Width = 120, WaterMark = "Model caption ...", BorderStyle = BorderStyle.FixedSingle, TextAlign = HorizontalAlignment.Center };

            lbl_MODEL_NAME_KEY_TYPE = new Label() { Left = 252, Top = 7, Width = 120, Text = "KEY_TYPE", AutoSize = false, Height = 20, BackColor = Color.Gray, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };
            txt_MODEL_NAME_KEY_TYPE = new TextBoxCustom() { Left = 372, Top = 7, Width = 120, WaterMark = "KEY_TYPE", BorderStyle = BorderStyle.FixedSingle, TextAlign = HorizontalAlignment.Center };

            lbl_MODEL_NAME_KEY_NAME = new Label() { Left = 492, Top = 7, Width = 120, Text = "KEY_NAME", AutoSize = false, Height = 20, BackColor = Color.Gray, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter };
            txt_MODEL_NAME_KEY_NAME = new TextBoxCustom() { Left = 612, Top = 7, Width = 120, WaterMark = "KEY_NAME", BorderStyle = BorderStyle.FixedSingle, TextAlign = HorizontalAlignment.Center };


            boi_DbName.Controls.AddRange(new Control[] { lbl_Name, lbl_Caption, txt_Name, txt_Caption ,
                lbl_MODEL_NAME_KEY_TYPE,txt_MODEL_NAME_KEY_TYPE,lbl_MODEL_NAME_KEY_NAME,txt_MODEL_NAME_KEY_NAME,
                new ucModelTitle() { Left = 4, Top = 39, Height = 25, Width = fWidth - (SystemInformation.VerticalScrollBarWidth + 20)} });

            FlowLayoutPanel boi_Filter = new FlowLayoutPanel()
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = false,
                Padding = new Padding(0),
                BackColor = Color.WhiteSmoke,
                FlowDirection = FlowDirection.TopDown,
            };
            boi_Filter.MouseDown += FormMove_MouseDown;

            form_Add(boi_Filter);

            Panel boi_Action = new Panel() { Dock = DockStyle.Bottom, Height = 25 };
            Button btn_Add = new Button() { Dock = DockStyle.Right, Text = "ADD", BackColor = Color.WhiteSmoke, Width = 60, TextAlign = ContentAlignment.MiddleCenter };
            Button btn_Remove = new Button() { Dock = DockStyle.Right, Text = "REMOVE", BackColor = Color.WhiteSmoke, Width = 70, TextAlign = ContentAlignment.MiddleCenter };
            Button btn_Submit = new Button() { Dock = DockStyle.Right, Text = "SUBMIT", BackColor = Color.WhiteSmoke, Width = 60, TextAlign = ContentAlignment.MiddleCenter };
            boi_Action.Controls.AddRange(new Control[] { btn_Add, btn_Remove, btn_Submit });

            this.Controls.AddRange(new Control[] { boi_DbName, boi_Filter, boi_Action });
            boi_DbName.BringToFront();
            boi_Action.BringToFront();
            boi_Filter.BringToFront();
            btn_Submit.Focus();

            btn_Add.Click += (se, ev) => form_Add(boi_Filter);
            btn_Remove.Click += (se, ev) => form_Remove(boi_Filter);

            #endregion

            btn_Submit.Click += (se, ev) => form_Submit(txt_Name.Text, txt_Caption.Text, boi_Filter);
            HideScrollBar(boi_Filter.Handle, ScrollBarHide.SB_HORZ);
        }//end function init()

        private int countField = 0;
        private List<int> listIndexRemove = new List<int>() { };
        private void form_Add(FlowLayoutPanel boi_Filter)
        {
            countField++;
            var uc = new ucModelFieldAdd(countField, db) { Name = "uc" + countField.ToString(), Height = 35, Width = fWidth };
            uc.OnRemoveField += (index) => form_RemoveAt(boi_Filter, index);
            boi_Filter.Controls.Add(uc);
        }

        private void form_RemoveAt(FlowLayoutPanel boi_Filter, int index)
        {
            if (index == countField)
                form_Remove(boi_Filter);
            else
            {
                listIndexRemove.Add(index);
                foreach (Control c in boi_Filter.Controls)
                {
                    if (c.Name == "uc" + index.ToString())
                    {
                        c.Visible = false;
                        break;
                    }
                }
            }
        }

        private void form_Remove(FlowLayoutPanel boi_Filter)
        {
            int id = countField;
            countField--;
            if (countField == -1) return;
            foreach (Control c in boi_Filter.Controls)
                if (c.Name == "uc" + id.ToString())
                {
                    boi_Filter.Controls.Remove(c);
                    break;
                }
        }

        private void form_Submit(string dbName, string dbCaption, FlowLayoutPanel boi_Filter)
        {
            //string dbName = txt_Name.Text, dbCaption = txt_Caption.Text;
            if (string.IsNullOrEmpty(dbName))
            {
                MessageBox.Show("Please input Model Name and fields: name, type, caption.");
                return;
            }

            dbName = dbName.ToLower().Trim();
            bool exist = db.ExistModel(dbName);
            if (exist)
            {
                MessageBox.Show("Model Name exist. Please choose other name.");
                return;
            }

            var li = new List<dbField>();
            int index = 1;
            foreach (Control c in boi_Filter.Controls)
            {
                if (listIndexRemove.IndexOf(index) != -1) continue;

                var o = new dbField();
                int ki = 0;
                foreach (Control fi in c.Controls)
                {
                    if (fi.Name == "name" + index.ToString())
                        o.Name = (fi as TextBox).Text;
                    else if (fi.Name == "type" + index.ToString())
                    {
                        int ix = (fi as ComboBox).SelectedIndex;
                        o.TypeName = (fi as ComboBox).Items[ix].ToString();
                    }
                    else if (fi.Name == "auto" + index.ToString())
                    {
                        o.IsKeyAuto = (fi as CheckBox).Checked;
                    }
                    else if (fi.Name == "kit" + index.ToString())
                    {
                        #region

                        if (o.IsKeyAuto) continue;

                        object _coltrol = (fi as ComboBox).SelectedItem;
                        if (_coltrol != null)
                        {
                            try
                            {
                                o.Kit = (ControlKit)((int)(_coltrol as ComboboxItem).Value);
                            }
                            catch { }
                        }

                        #endregion
                    }
                    else if (fi.Name == "link_type" + index.ToString())
                    {
                        if (o.IsKeyAuto) continue;
                        o.JoinType = JoinType.NONE;
                        object ct = (fi as ComboBox).SelectedItem;
                        if (ct != null)
                        {
                            try
                            {
                                o.JoinType = (JoinType)((int)(ct as ComboboxItem).Value);
                            }
                            catch { }
                        }
                    }
                    else if (fi.Name == "value_default" + index.ToString())
                    {
                        if (o.IsKeyAuto) continue;
                        string vd = (fi as TextBox).Text;
                        o.ValueDefault = vd == null ? new string[] { } : vd.Split('|');
                    }
                    else if (fi.Name == "link_model" + index.ToString())
                    {
                        if (o.IsKeyAuto) continue;
                        object ct = (fi as ComboBox).SelectedItem;
                        if (ct != null)
                            o.JoinModel = (ct as ComboboxItem).Value as string;
                    }
                    else if (fi.Name == "link_field" + index.ToString())
                    {
                        if (o.IsKeyAuto) continue;
                        object ct = (fi as ComboBox).SelectedItem;
                        if (ct != null)
                            o.JoinField = (ct as ComboboxItem).Value as string;
                    }
                    else if (fi.Name == "caption" + index.ToString())
                    {
                        o.Caption = (fi as TextBox).Text;
                    }
                    else if (fi.Name == "index" + index.ToString())
                    {
                        o.IsIndex = (fi as CheckBox).Checked;
                    }
                    else if (fi.Name == "null" + index.ToString())
                    {
                        o.IsAllowNull = (fi as CheckBox).Checked;
                        if (o.IsKeyAuto || o.IsIndex) o.IsAllowNull = false;
                    }
                    else if (fi.Name == "caption_short" + index.ToString())
                    {
                        o.CaptionShort = (fi as TextBox).Text;
                    }
                    else if (fi.Name == "des" + index.ToString())
                    {
                        o.Description = (fi as TextBox).Text;
                    }
                    else if (fi.Name == "mobi" + index.ToString())
                    {
                        o.Mobi = (fi as CheckBox).Checked;
                    }
                    else if (fi.Name == "tablet" + index.ToString())
                    {
                        o.Tablet = (fi as CheckBox).Checked;
                    }
                    else if (fi.Name == "duplicate" + index.ToString())
                    {
                        o.IsDuplicate = (fi as CheckBox).Checked;
                    }
                    else if (fi.Name == "encrypt" + index.ToString())
                    {
                        o.IsEncrypt = (fi as CheckBox).Checked;
                    }

                    ki++;
                }//end for fields 

                if (!string.IsNullOrEmpty(o.Name) && o.Type != null)
                {
                    switch (o.Kit)
                    {
                        case ControlKit.CHECK:
                        case ControlKit.RADIO:
                            o.JoinType = JoinType.DEF_VALUE;
                            if (o.ValueDefault == null || o.ValueDefault.Length == 0 || (o.ValueDefault.Length == 1 && o.ValueDefault[0] == ""))
                            {
                                MessageBox.Show("Please input field [ " + o.Name + " ] attributed [ Value Default ]");
                                return;
                            }
                            break;
                        case ControlKit.SELECT:
                            if (o.JoinType == JoinType.DEF_VALUE && (o.ValueDefault == null || o.ValueDefault.Length == 0 || (o.ValueDefault.Length == 1 && o.ValueDefault[0] == "")))
                            {
                                MessageBox.Show("Please input field [ " + o.Name + " ] attributed [ Value Default ]");
                                return;
                            }
                            if (o.JoinType == JoinType.JOIN_MODEL && (string.IsNullOrEmpty(o.JoinModel) || string.IsNullOrEmpty(o.JoinField)))
                            {
                                MessageBox.Show("Please input field [ " + o.Name + " ] attributed [ JOIN MODEL - JOIN FIELD ]");
                                return;
                            }
                            break;
                        case ControlKit.LOOKUP:
                            if (o.JoinType == JoinType.JOIN_MODEL && (string.IsNullOrEmpty(o.JoinModel) || string.IsNullOrEmpty(o.JoinField)))
                            {
                                MessageBox.Show("Please input field [ " + o.Name + " ] attributed [ JOIN MODEL - JOIN FIELD ]");
                                return;
                            }
                            break;
                    }

                    if (o.JoinType == JoinType.JOIN_MODEL && !string.IsNullOrEmpty(o.JoinModel) && !string.IsNullOrEmpty(o.JoinField))
                    {
                        string[] types = db.GetFields(o.JoinModel).Where(x => x.Name == o.JoinField).Select(x => x.TypeName).ToArray();
                        if (types.Length > 0) o.TypeName = types[0];
                    }

                    if (o.JoinType == JoinType.DEF_VALUE && o.ValueDefault != null)
                        o.TypeName = typeof(Int32).Name;

                    li.Add(o);
                }
                else
                {
                    MessageBox.Show("Please input fields: name, type, caption.");
                    c.Focus();
                    return;
                }

                index++;
            }//end for controls
            if (li.Count > 0)
            {
                dbModel m = new dbModel()
                {
                    Name = dbName.Replace(" ", "_").Trim().ToUpper(),
                    Fields = li.ToArray(),
                };
                //if (OnSubmit != null) OnSubmit(m);
                generalApiController(dbName, li);
            }
            else
            {
                MessageBox.Show("Please input fields: name, type, caption.");
            }
        }

        private void generalApiController(string modelName, List<dbField> listFields)
        {
            modelName = modelName.ToLower().Trim();
            if (!modelName.StartsWith("ogen_")) modelName = "ogen_" + modelName;

            foreach (var it in listFields) it.Name = it.Name.ToLower().Trim();
            string keyType = txt_MODEL_NAME_KEY_TYPE.Text.Trim().ToLower();
            if (keyType != "int" && keyType != "long" && keyType != "string") {
                MessageBox.Show("MODEL_NAME_KEY_TYPE are int|long|string ?");
                return;
            }

            var api = new oApiControoler()
            {
                Name = modelName.ToLower(),
                keyType = keyType,
                KeyName = txt_MODEL_NAME_KEY_NAME.Text.Trim().ToLower(),
                Model = listFields,
            };

            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "api");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            string file = JsonConvert.SerializeObject(api, Formatting.Indented);
            string fiJson = Path.Combine(path, modelName + ".json");
            string fiCS = Path.Combine(path, modelName + ".cs");
            File.WriteAllText(fiJson, file);
             
            string con = File.ReadAllText("sample.txt");
            con = con
                .Replace("[MODEL_NAME]", api.Name)
                .Replace("[MODEL_NAME_KEY_TYPE]", api.keyType)
                .Replace("[MODEL_NAME_KEY_NAME]", api.KeyName);
            File.WriteAllText(fiCS, con);

            MessageBox.Show("OK");
        }
    }

    public class oApiControoler
    {
        public string Name { set; get; }
        public string KeyName { set; get; }
        public string keyType { set; get; }
        public List<dbField> Model { set; get; }
    }

}
