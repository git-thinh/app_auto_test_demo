using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using app.Core;
using System.Linq.Dynamic;
using Deveck.Ui.Controls;
using Deveck.Ui.Controls.Scrollbar;
using System.Collections;
using System.Threading;

namespace app.GUI
{
    public class FormDB : Form
    {

        public delegate void EventExit();
        public EventExit OnExit;

        private readonly IDataFile db;
        private string[] dbName;

        public FormDB(IDataFile _db)
        {
            Font = App.Font;
            Control.CheckForIllegalCrossThreadCalls = false;
            this.Text = App.Name;
            db = _db;
            dbName = db.GetListDB();
            init_Control();
        }


        #region [ === MOVE FORM === ]

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

        #endregion

        private ToolStripLabel lbl_Title;
        private CustomListView listDB;
        private TabControlCustom tab;

        private void init_Control()
        {
            this.Padding = App.FormBorder;
            this.BackColor = App.ColorBorder;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ClientSize = new Size(App.Width, 555);

            ////////////////////////////////////////////////////

            #region [ === MENU === ]

            MenuStrip menu = new MenuStrip() { Dock = DockStyle.Top, BackColor = App.ColorBg };
            //ToolStripTextBox mn_DbSearch = new ToolStripTextBox() { Width = left_Width, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.LightGray };
            lbl_Title = new ToolStripLabel() { Text = "// ... ", ForeColor = Color.White, };
            ToolStripMenuItem mn = new ToolStripMenuItem() { Text = "MODEL", ForeColor = Color.White, Alignment = ToolStripItemAlignment.Right, };
            var mn_ModelAdd = new ToolStripMenuItem() { Text = "Model Add" };
            mn_ModelAdd.Click += (se, ev) => model_Add();
            var mn_ModelConfig = new ToolStripMenuItem() { Text = "Model Config" };
            mn_ModelConfig.Click += (se, ev) => model_Config();
            var mn_ModelEdit = new ToolStripMenuItem() { Text = "Model Edit" };
            mn_ModelEdit.Click += (se, ev) => model_Edit();
            var mn_ModelRemove = new ToolStripMenuItem() { Text = "Model Remove" };
            mn_ModelRemove.Click += (se, ev) => model_Remove();
            var mn_ItemAdd = new ToolStripMenuItem() { Text = "Item Add" };
            mn_ItemAdd.Click += (se, ev) => item_Add();
            mn.DropDownItems.AddRange(new ToolStripItem[] { mn_ModelAdd, mn_ModelConfig, mn_ModelEdit, mn_ModelRemove, mn_ItemAdd });
            ToolStripItem mn_Hide = new ToolStripMenuItem() { Text = "HIDE", ForeColor = Color.White, Alignment = ToolStripItemAlignment.Right };
            ToolStripItem mn_Exit = new ToolStripMenuItem() { Text = "EXIT", ForeColor = Color.White, Alignment = ToolStripItemAlignment.Right, };
            mn_Hide.Click += (se, ev) => { this.Hide(); };
            mn_Exit.Click += (se, ev) => { if (OnExit != null) OnExit(); };
            menu.Items.AddRange(new ToolStripItem[] { lbl_Title, mn_Exit, mn_Hide, mn, });
            menu.MouseDown += Label_MouseDown;
            this.Controls.Add(menu);

            #endregion
             
            ////////////////////////////////////////////////////

            #region [ === FORM - BOX LEFT === ]

            Panel box_Left = new Panel() { Dock = DockStyle.Left, Width = App.col_Left_Width, BackColor = Color.White, Margin = new Padding(0), Padding = new Padding(0), };
            TextBox txt_Search = new TextBox() { Dock = DockStyle.Top, BackColor = App.ColorControl, Width = 100, Height = 20, Margin = new Padding(10, 10, 0, 0), Text = "", BorderStyle = BorderStyle.FixedSingle };

            CustomScrollbar scrollbar1 = new CustomScrollbar()
            {
                Dock = DockStyle.Left,

                ActiveBackColor = Color.White,
                BackColor = Color.White,

                LargeChange = 10,
                Location = new Point(306, 12),
                Maximum = 99,
                Minimum = 0,
                Size = new Size(13, 303),
                SmallChange = 1,
                TabIndex = 1,
                ThumbStyle = CustomScrollbar.ThumbStyleEnum.Auto,
                Value = 0,
                Margin = new Padding(0),
                Padding = new Padding(0),
            };
            //ScrollbarStyleHelper.ApplyStyle(scrollbar1, ScrollbarStyleHelper.StyleTypeEnum.Blue);
            ScrollbarStyleHelper.ApplyStyle(scrollbar1, ScrollbarStyleHelper.StyleTypeEnum.Black);

            listDB = new CustomListView(true)
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = Color.WhiteSmoke,

                HeaderStyle = ColumnHeaderStyle.None,
                FullRowSelect = true,
                //HideSelection = false,
                //Location = new Point(12, 12),
                MultiSelect = false,
                //Size = new Size(288, 303),
                UseCompatibleStateImageBehavior = false,
                View = View.Details,
                VScrollbar = new ScrollbarCollector(scrollbar1),
            };
            listDB.Columns.AddRange(new ColumnHeader[] { new ColumnHeader() { Text = "", Width = App.col_Left_Width } });

            box_Left.Controls.AddRange(new Control[] {
                txt_Search,
                listDB,
                scrollbar1,
            });

            listDB.BeginUpdate();
            for (int x = 0; x < dbName.Length; x++)
                listDB.Items.Add(dbName[x]);
            //for (int x = dbName.Length; x < 999; x++) listDB.Items.Add("Auto test " + x.ToString());
            listDB.EndUpdate();

            this.Controls.Add(box_Left);
            scrollbar1.BringToFront();

            #endregion

            ////////////////////////////////////////////////////

            #region [ === FORM - BOX RIGHT === ]

            Splitter splitter = new Splitter() { Dock = DockStyle.Left, BackColor = App.ColorBg, Width = App.col_Splitter_Width };

            Panel box_Right = new Panel() { Dock = DockStyle.Fill, BackColor = Color.White, };
            tab = new TabControlCustom() { Dock = DockStyle.Fill };

            Panel box_Footer = new Panel() { Dock = DockStyle.Bottom, Height = 20, Padding = new Padding(1, 1, 1, 0), BackColor = App.ColorBg };
            box_Footer.MouseDown += Label_MouseDown;

            ButtonCustom btn_ModelAdd = new ButtonCustom() { Text = "Tab Add", Dock = DockStyle.Left, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = App.ColorBg, ForeColor = Color.White, };
            ButtonCustom btn_ModelEdit = new ButtonCustom() { Text = "Tab Edit", Dock = DockStyle.Left, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = App.ColorBg, ForeColor = Color.White, };
            ButtonCustom btn_ModelRemove = new ButtonCustom() { Text = "Tab Remove", Dock = DockStyle.Left, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = App.ColorBg, ForeColor = Color.White, };

            ButtonCustom btn_ItemAdd = new ButtonCustom() { Text = "Item Add", Dock = DockStyle.Left, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = App.ColorBg, ForeColor = Color.White, };
            ButtonCustom btn_ItemEdit = new ButtonCustom() { Text = "Item Edit", Dock = DockStyle.Left, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = App.ColorBg, ForeColor = Color.White, };
            ButtonCustom btn_ItemRemove = new ButtonCustom() { Text = "Item Remove", Dock = DockStyle.Left, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = App.ColorBg, ForeColor = Color.White, };

            box_Footer.Controls.AddRange(new Control[] {
                btn_ItemRemove,
                btn_ItemEdit,
                btn_ItemAdd,

                btn_ModelRemove,
                btn_ModelEdit,
                btn_ModelAdd,
            });

            box_Right.Controls.AddRange(new Control[] {
                tab,
                //box_Footer,
            });

            #endregion

            ////////////////////////////////////////////////////
            // LOAD CONTROL

            this.Controls.Add(box_Left);
            this.Controls.Add(splitter);
            this.Controls.Add(box_Right);
            box_Left.BringToFront();
            listDB.BringToFront();
            splitter.BringToFront();
            box_Right.BringToFront();

            ////////////////////////////////////////////////////

            listDB_Init();
        }

        #region [ === LIST DB === ]

        private void listDB_Init()
        {
            listDB.SelectedIndexChanged += (se, ev) => listDB_SelectedIndexChanged();
            if (dbName.Length > 0)
            {
                for (int k = 0; k < dbName.Length; k++) tabPage_CreateUI(dbName[k]);
                listDB.Items[dbName.Length - 1].Selected = true;
            }
            //else 
            //model_Add();
        }

        private void listDB_SelectedIndexChanged()
        {
            int tabIndex = -1;
            if (listDB.SelectedItems.Count > 0)
                tabIndex = listDB.Items.IndexOf(listDB.SelectedItems[0]);
            else return;

            //tabIndex = list_DB.SelectedIndex;
            if (tabIndex >= dbName.Length)
            {
                lbl_Title.Text = "// ";
                tab.Visible = false;
                return;
            }

            string dbNameCurrent = dbName[tabIndex];
            lbl_Title.Text = "// " + dbNameCurrent.ToUpper();

            tab.SelectedIndex = tabIndex;
        }

        #endregion

        #region [ === MODEL: ADD - EDIT - REMOVE === ]

        private void model_Config()
        {
        }

        private void model_Add()
        {
            var fm = new FormModelAdd(db);
            fm.OnSubmit += (dbModel m) =>
            {
                bool ok = db.CreateDb(m);
                if (ok)
                {
                    listDB.Items.Add(m.Name);
                    dbName = db.GetListDB();
                    tabPage_CreateUI(m.Name);
                    MessageBox.Show("Create model: " + m.Name + " successfully.");
                    fm.Close();
                }
                else
                    MessageBox.Show("Create model: " + m.Name + " fail.");
            };
            App.show_FormDialog(fm);

        }
        private void model_Edit() { }
        private void model_Remove() { }

        #endregion

        #region [ === ITEM: ADD - EDIT - REMOVE === ]

        private void item_Add()
        {
            string modelName = dbName[tab.SelectedIndex];
            var fm = new FormItemAdd(db, modelName);
            fm.OnSubmit += (model, data) =>
            {
                var ok = db.AddItem(model, data );
                if (ok == EditStatus.SUCCESS)
                {
                    MessageBox.Show("Add item model: " + model + " successfully.");
                    fm.Close();
                    (tab.SelectedTab as TabPageCustom).OnLoadData();
                }
                else
                    MessageBox.Show("Add item model: " + model + " fail: " + ok.ToString());
            };
            fm.ShowDialog();
        }
        private void item_Edit() { }
        private void item_Remove() { }

        #endregion
         
        private const int selectTop = 100; 
        private string[] OpString = "Equals,NotEquals,Contains,StartsWith,EndsWith".Split(',');
        private string[] OpNumber = "Equals,NotEquals,Contains,GreaterThan,LessThan,GreaterThanOrEqual,LessThanOrEqual".Split(',');
        private void tabPage_CreateUI(string modelName)
        {
            var info = db.GetInfoSelectTop(modelName, selectTop);
            if (info == null)
            {
                tab.TabPages.Add(new TabPageCustom());
            }
            else
            {
                TabPageCustom page = new TabPageCustom(info.Fields) { BackColor = Color.White, };

                #region [ === BOX FILTER === ]

                int hi = (((info.Fields.Length / 3) + (info.Fields.Length % 3 == 0 ? 0 : 1) + 0) * 27);
                if (info.Fields.Length < 4) hi = 27;
                FlowLayoutPanel boi_Filter = new FlowLayoutPanel() { Visible = false, Dock = DockStyle.Top, Height = hi, AutoScroll = false, Padding = new Padding(0), BackColor = App.ColorBg };
                boi_Filter.FlowDirection = FlowDirection.LeftToRight;
                boi_Filter.MouseDown += Label_MouseDown;

                for (int k = 1; k <= info.Fields.Length; k++)
                {
                   var dp = info.Fields[k - 1];

                    Label lbl = new Label() { Name = "lbl" + k.ToString(), Text = dp.Name, AutoSize = false, Width = 120, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleRight };
                    ComboBox cbo = new ComboBox() { Name = "cbo" + k.ToString(), Width = 80, BackColor = App.ColorControl, DropDownStyle = ComboBoxStyle.DropDownList };
                    if (dp.Type.Name == "String")
                        for (int ki = 0; ki < OpString.Length; ki++) cbo.Items.Add(OpString[ki]);
                    else
                        for (int ki = 0; ki < OpNumber.Length; ki++) cbo.Items.Add(OpNumber[ki]);
                    TextBox txt = new TextBox() { Name = "txt" + k.ToString(), Width = 80, BackColor = App.ColorControl, BorderStyle = BorderStyle.FixedSingle };
                    boi_Filter.Controls.AddRange(new Control[] {
                            lbl,
                            cbo,
                            txt,
                        });

                    if (k != 0 && k % 3 == 0)
                    {
                        Label sp = new Label() { Text = "", AutoSize = false, Width = App.Width - App.col_Left_Width, Height = 1, };
                        boi_Filter.Controls.Add(sp);
                    }
                }//end for fields

                #endregion

                #region [ === BOX SEARCH - SHOW | HIDE FILTER === ]

                Panel boi_Action = new Panel() { Dock = DockStyle.Top, Height = 27, AutoScroll = false, Padding = new Padding(0, 5, 10, 3), BackColor = App.ColorBg };
                boi_Action.MouseDown += Label_MouseDown;
                TextBox txt_Keyword = new TextBox() { Dock = DockStyle.Right, Width = 166, BorderStyle = BorderStyle.FixedSingle, BackColor = App.ColorControl };
                ButtonCustom btn_Search = new ButtonCustom() { Text = "Search", Dock = DockStyle.Right, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 60, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = Color.Gray, ForeColor = Color.White, };
                ButtonCustom btn_Filter = new ButtonCustom() { Text = "Filter", Dock = DockStyle.Right, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 50, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = Color.Gray, ForeColor = Color.White, };

                boi_Action.Controls.AddRange(new Control[] {
                        txt_Keyword,
                        new Label() { Dock = DockStyle.Right, AutoSize = false, Width = 2, Height = 20, BackColor = App.ColorBg },
                        btn_Search,
                        new Label() { Dock = DockStyle.Right, AutoSize = false, Width = 2, Height = 20, BackColor = App.ColorBg },
                        btn_Filter,
                    });

                #endregion

                #region [ === GRID === ]
                 
                var grid = new ListViewModelItem(){ Dock = DockStyle.Fill };
                grid.SetDataBinding(info.Fields, info.DataSelectTop);

                #endregion

                #region [ === BOX FOOTER === ]

                Panel boi_Footer = new Panel() { Dock = DockStyle.Bottom, Height = 18, AutoScroll = false, Padding = new Padding(0), BackColor = App.ColorBg };
                boi_Footer.MouseDown += Label_MouseDown;

                Label lbl_TotalRecord = new Label() { Dock = DockStyle.Left, Text = "Records: " + info.DataSelectTop.Count.ToString() + " / " + info.TotalRecord.ToString() + " ", AutoSize = false, Width = 199, Height = 18, Padding = new Padding(0), Font = new Font(new FontFamily("Arial"), 7F, FontStyle.Regular), ForeColor = Color.WhiteSmoke, TextAlign = ContentAlignment.MiddleLeft };
                Label lbl_Port = new Label() { Dock = DockStyle.Left, Text = "Port HTTP: " + info.PortHTTP.ToString(), AutoSize = false, Width = 110, Height = 18, Padding = new Padding(4, 0, 0, 0), Font = new Font(new FontFamily("Arial"), 7F, FontStyle.Regular), ForeColor = Color.WhiteSmoke, TextAlign = ContentAlignment.MiddleLeft };
                //ButtonCustom btn_Search = new ButtonCustom() { Text = "search", Dock = DockStyle.Right, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = Color.Gray, ForeColor = Color.White, };
                ButtonCustom btn_PagePrev = new ButtonCustom() { Text = "<<<", Dock = DockStyle.Right, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = App.ColorBg, ForeColor = Color.White, };
                ButtonCustom btn_PageNext = new ButtonCustom() { Text = ">>>", Dock = DockStyle.Right, FlatStyle = System.Windows.Forms.FlatStyle.Flat, Width = 80, TextAlign = System.Drawing.ContentAlignment.MiddleCenter, BackColor = App.ColorBg, ForeColor = Color.White, };
                Label spa = new Label() { Text = "", AutoSize = false, Width = App.Width, Height = 1, };
                Label lbl_PageCurrent = new Label() { Text = "1", Dock = DockStyle.Right, AutoSize = false, Width = 30, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter };
                Label lbl_PageSP = new Label() { Text = " | ", Dock = DockStyle.Right, AutoSize = false, Width = 30, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter };
                Label lbl_PageTotal = new Label() { Text = "1", Dock = DockStyle.Right, AutoSize = false, Width = 30, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter };
                lbl_PageTotal.Text = ((int)(info.TotalRecord / selectTop) + (info.TotalRecord % selectTop == 0 ? 0 : 1)).ToString();

                boi_Footer.Controls.AddRange(new Control[] {
                        lbl_TotalRecord,
                        lbl_Port,

                        btn_PagePrev,
                        lbl_PageCurrent,
                        lbl_PageSP,
                        lbl_PageTotal,
                        btn_PageNext, 
                    });

                #endregion

                page.Controls.AddRange(new Control[] { boi_Filter, boi_Action, grid, boi_Footer, });

                tab.TabPages.Add(page); 
                boi_Filter.BringToFront();
                boi_Action.BringToFront();
                grid.BringToFront(); 
                 
                btn_Filter.Click += (se, ev) => { if (boi_Filter.Visible) boi_Filter.Visible = false; else boi_Filter.Visible = true; };
                ////////////////////////////////////////////////// 
                btn_PageNext.Click += (se, ev) => tabPage_Next(modelName, info, page, grid, lbl_PageCurrent, lbl_PageTotal, lbl_TotalRecord);
                btn_PagePrev.Click += (se, ev) => tabPage_Prev(modelName, info, page, grid, lbl_PageCurrent, lbl_PageTotal, lbl_TotalRecord);
                ////////////////////////////////////////////////// 
                txt_Keyword.KeyDown += (se, ev) =>
                {
                    if (ev.KeyCode == Keys.Enter) tabPage_Search(txt_Keyword.Text, modelName, info, page, grid, lbl_PageCurrent, lbl_PageTotal, lbl_TotalRecord);
                };
                btn_Search.Click += (se, ev) => tabPage_Search(txt_Keyword.Text, modelName, info, page, grid, lbl_PageCurrent, lbl_PageTotal, lbl_TotalRecord);
                page.OnLoadData += () =>
                {
                    info = db.GetInfoSelectTop(modelName, selectTop);
                    tabPage_Search(txt_Keyword.Text, modelName, info, page, grid, lbl_PageCurrent, lbl_PageTotal, lbl_TotalRecord);
                };
            }//end bind info Model
        }

        #region [ === FUNCTION: SEARCH - PREVIEW - NEXT === ]

        private void tabPage_Search(
            string kw, string modelName, DataFileInfoSelectTop info,
            TabPageCustom page, ListViewModelItem grid,
            Label lbl_PageCurrent, Label lbl_PageTotal, Label lbl_TotalRecord)
        {
            //grid.DataSource = null;

            string predicate = "";
            if (string.IsNullOrEmpty(kw))
            {
                page.SearchRequest = null;
                page.SearchResult = null;

                page.PageCurrent = 1;
                lbl_PageCurrent.Text = page.PageCurrent.ToString();
                int countPage = (int)(info.TotalRecord / selectTop) + (info.TotalRecord % selectTop == 0 ? 0 : 1);
                lbl_PageTotal.Text = countPage.ToString();
                lbl_TotalRecord.Text = "Records: " + info.DataSelectTop.Count.ToString() + " / " + info.TotalRecord.ToString() + " ";
                grid.SetDataBinding(info.Fields, info.DataSelectTop);
            }
            else
            {
                StringBuilder wh_Contain = new StringBuilder();
                for (int k = 0; k < info.Fields.Length; k++)
                {
                    var dp = info.Fields[k];
                    wh_Contain.Append(dp.Name + (dp.Type.Name == "String" ? string.Empty : ".ToString()") + ".Contains(@0) ");
                    if (k < info.Fields.Length - 1) wh_Contain.Append(" || ");
                }

                List<object> lp = new List<object>();
                predicate = wh_Contain.ToString();
                lp.Add(kw);

                SearchRequest sr = new SearchRequest(selectTop, 1, predicate, lp.Count == 0 ? null : lp.ToArray());
                SearchResult rs = db.Search(modelName, sr);

                page.SearchRequest = sr;
                page.SearchResult = rs;

                if (rs != null)
                {
                    page.PageCurrent = rs.PageNumber;
                    lbl_PageCurrent.Text = page.PageCurrent.ToString();
                    int countPage = (int)(rs.Total / selectTop) + (rs.Total % selectTop == 0 ? 0 : 1);
                    lbl_PageTotal.Text = countPage.ToString();
                    lbl_TotalRecord.Text = "Records: " + rs.IDs.Length.ToString() + " / " + info.TotalRecord.ToString() + " ";
                    grid.SetDataBinding(info.Fields, (IList)rs.Message);
                }
            }
        }

        private void tabPage_Prev(
            string modelName, DataFileInfoSelectTop info,
            TabPageCustom page, ListViewModelItem grid,
            Label lbl_PageCurrent, Label lbl_PageTotal, Label lbl_TotalRecord)
        {
            SearchRequest sr = page.SearchRequest;
            SearchResult rs = page.SearchResult;
            if (sr == null)
            {
                int PageNumber = page.PageCurrent - 1;
                if (PageNumber == 0) return;

                page.PageCurrent = PageNumber;
                lbl_PageCurrent.Text = page.PageCurrent.ToString();
                grid.SetDataBinding(info.Fields, db.GetSelectPage(modelName, PageNumber, selectTop));
            }
            else
            {
                sr.PageNumber = rs.PageNumber - 1;
                if (sr.PageNumber == 0) return;

                rs = db.Search(modelName, sr);

                page.SearchRequest = sr;
                page.SearchResult = rs;

                if (rs != null)
                {
                    page.PageCurrent = rs.PageNumber;
                    lbl_PageCurrent.Text = page.PageCurrent.ToString();
                    lbl_TotalRecord.Text = "Records: " + rs.IDs.Length.ToString() + " / " + info.TotalRecord.ToString() + " ";
                    grid.SetDataBinding(info.Fields, (IList)rs.Message);
                }
            }
        }

        private void tabPage_Next(
            string modelName, DataFileInfoSelectTop info,
            TabPageCustom page, ListViewModelItem grid,
            Label lbl_PageCurrent, Label lbl_PageTotal, Label lbl_TotalRecord)
        {
            SearchRequest sr = page.SearchRequest;
            SearchResult rs = page.SearchResult;
            if (sr == null)
            {
                int PageNumber = page.PageCurrent + 1;
                if (PageNumber > int.Parse(lbl_PageTotal.Text)) return;

                page.PageCurrent = PageNumber;
                lbl_PageCurrent.Text = page.PageCurrent.ToString();
                grid.SetDataBinding(info.Fields, db.GetSelectPage(modelName, PageNumber, selectTop));
            }
            else
            {
                sr.PageNumber = sr.PageNumber + 1;
                if (sr.PageNumber > int.Parse(lbl_PageTotal.Text)) return;

                rs = db.Search(modelName, sr);

                page.SearchRequest = sr;
                page.SearchResult = rs;

                if (rs != null)
                {
                    page.PageCurrent = rs.PageNumber;
                    lbl_PageCurrent.Text = page.PageCurrent.ToString();
                    lbl_TotalRecord.Text = "Records: " + rs.IDs.Length.ToString() + " / " + info.TotalRecord.ToString() + " ";
                    grid.SetDataBinding(info.Fields, (IList)rs.Message);
                }
            }
        }

        #endregion


    }
}
