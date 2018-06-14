using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Permissions;
using System.Threading;
using System.Net;
using System.Reflection;
using System.IO;
using ProtoBuf;
using System.Net.Sockets;
using System.Collections.Specialized;
using System.Windows.Forms;
using ProtoBuf.Meta;
using app.GUI;
using app.Core;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Dynamic;
using System.Collections;

namespace app
{

    [PermissionSet(SecurityAction.LinkDemand, Name = "Everything"), PermissionSet(SecurityAction.InheritanceDemand, Name = "FullTrust")]
    class App
    {
        public const int col_Left_Width = 99;
        public const int col_Splitter_Width = 3;
        public const int Width = 999;
        public const string Name = "TEST SYSTEM";
        public static Font Font = new Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        public static Padding FormBorder = new Padding(1, 1, 1, 1);
        public static Color ColorBorder = SystemColors.ControlDarkDark;
        public static Color ColorBg = Color.Black;
        public static Color ColorControl = Color.LightGray;

        static App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (se, ev) =>
            {
                Assembly asm = null;
                string comName = ev.Name.Split(',')[0];
                string resourceName = @"DLL\" + comName + ".dll";
                var assembly = Assembly.GetExecutingAssembly();
                resourceName = typeof(App).Namespace + "." + resourceName.Replace(" ", "_").Replace("\\", ".").Replace("/", ".");
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        byte[] buffer = new byte[stream.Length];
                        using (MemoryStream ms = new MemoryStream())
                        {
                            int read;
                            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                                ms.Write(buffer, 0, read);
                            buffer = ms.ToArray();
                        }
                        asm = Assembly.Load(buffer);
                    }
                }
                return asm;
            };
        }

        ////////////////////////////////////////////
        ////static Log log;
        ////static HostListener host;  
        static DataHost db;

        ////////////////////////////////////////
        static SystemTray icon_tray;
        static FormNotification fm_noti;
        static FormLogger fm_log;
        static FormLogin fm_login;
        static FormDB fm_DB;

        public class demo
        {
            public string time { set; get; }
        }
         

        public static void Start()
        {
            ////Func<string, DateTime> func = delegate (string item)
            ////{
            ////    return DateTime.ParseExact(item, "yyMMddHHmmss", null);
            ////};

            ////demo[] dt = new demo[] { new demo() { time = DateTime.Now.ToString("yyMMddHHmmss") } };

            //////var l0 = dt.AsQueryable().Select(typeof(Result), @"new @out (@0(it.time) as data)", func);

            ////var l0 = dt.AsQueryable().Select(@"new (@0(it.time) as data)", func);

            //////var l1 = (IList)typeof(List<>).MakeGenericType(l0.ElementType).GetConstructor(Type.EmptyTypes).Invoke(null);
            //////foreach (var elem in l0) l1.Add(elem);

            ////IList l2 = new List<object>();
            ////foreach (var elem in l0) l2.Add(elem);


            Application.EnableVisualStyles();

            db = new DataHost();

            //////////////////////////////////////////////
            ////RuntimeTypeModel.Default.Add(typeof(Msg), false).SetSurrogate(typeof(MsgSurrogate));

            ////////////////////////////////////////////
            ////////log = new Log();  
            ////////host = new HostListener(log);
            ////host.Start();

            //////////////////////////////////////////////
            noti_Init();
            icon_tray = new SystemTray("Host");
            fm_noti = new FormNotification();
            fm_log = new FormLogger();
            fm_login = new FormLogin();
            fm_login.OnExit += () => Exit();

            icon_tray.OnClick += () => show_Form(fm_DB);
            icon_tray.OnExit += () => Exit();

            //////////////////////////////////////////

            db.OnOpen += (string[] a) =>
            {
                if (db.Open)
                {
                    fm_DB = new FormDB(db);
                    fm_DB.OnExit += () => Exit();

                    init_Form();

                    show_Form(fm_DB);

                    //show_Form(new FormModelAdd(db));
                    //show_Form(new FormItemAdd(db, "ITEM"));

                    ////show_Form(new FormColorPicker(true));
                    //show_Form(new FormLookupItem(db));


                    //fm_login.OnLogin += (user, pass) =>
                    //{
                    //    bool login = db.Login(user, pass);
                    //    if (login)
                    //        show_Form(fm_DB);
                    //    else
                    //    {
                    //        show_Form(fm_login);
                    //        MessageBox.Show("Username and Password wrong. Please input again.", "LOGIN AGAIN");
                    //    }
                    //};
                    //show_Form(fm_login);
                }
                else
                {
                    MessageBox.Show("SYSTEM CAN NOT OPEN. PLEASE CHECK DATA FILE.", "SYSTEM OPEN");
                }
            };
            db.Start();

            Application.Run(icon_tray);
        }


        #region [ === FORM === ]

        private static void Exit()
        {
            DialogResult ok = MessageBox.Show("Application will exit ?", "Are you sure ?",
                MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button2);
            if (ok == DialogResult.Yes)
            {
                if (db != null) db.Close();

                if (fm_DB != null) fm_DB.Close();
                if (fm_login != null) fm_login.Close();
                if (fm_log != null) fm_log.Close();
                if (fm_noti != null) fm_noti.Close();

                if (icon_tray != null) icon_tray.Hide();

                Thread.Sleep(300);

                int pi = Process.GetCurrentProcess().Id;
                Process p = Process.GetProcessById(pi);
                p.Kill();
            }
        }

        private static void init_Form()
        {
            fm_login.ShowInTaskbar = false;
            fm_login.Padding = new Padding(2, 0, 2, 2);
            fm_login.BackColor = App.ColorBg;
            fm_login.FormBorderStyle = FormBorderStyle.None;
            fm_login.Width = 200;
            fm_login.Height = 111;

            //fm_DB.Padding = new Padding(2, 3, 2, 2);
            //fm_DB.BackColor = App.ColorBg;
            //fm_DB.FormBorderStyle = FormBorderStyle.None;
            //fm_DB.Width = App.Width;
            //fm_DB.Height = 555;
        }

        public static void show_Form(Form fm)
        {
            fm.Show();
            fm.Left = (Screen.PrimaryScreen.WorkingArea.Width - fm.Width) / 2;
            fm.Top = (Screen.PrimaryScreen.WorkingArea.Height - fm.Height) / 2;
        }

        public static void show_FormDialog(Form fm)
        {
            fm.ShowDialog(); 
        }

        // very simple method to create new forms, controls ... is to use Invoke of this control
        private static Control _invoker;
        public static void show_Notification(string msg, int duration_ = 0)
        {
            FormNotification form = new FormNotification(msg, duration_);
            _invoker.Invoke((MethodInvoker)delegate()
            {
                form.Show();
            });
        }

        static void noti_Init()
        {
            _invoker = new Control();
            _invoker.CreateControl();
        }

        #endregion
    }//end class 
}
