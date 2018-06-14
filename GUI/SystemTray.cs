using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace app.GUI
{
    public class SystemTray : ApplicationContext
    {
        public event OnExitEvent OnExit = null;
        public delegate void OnExitEvent();

        public event OnClickEvent OnClick = null;
        public delegate void OnClickEvent();

        //Component declarations
        private NotifyIcon TrayIcon;
        private ContextMenuStrip TrayIconContextMenu;
        private ToolStripMenuItem CloseMenuItem;
        private string Name = "";

        public SystemTray(string name)
        {
            Name = name;
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            InitializeComponent();
            TrayIcon.Visible = true;
        }

        private void InitializeComponent()
        {
            TrayIcon = new NotifyIcon();
            TrayIcon.Text = Name;

            ///////////////////////////////////////////////////////////////////////////
            // Set m_icon from resource
            string resourceName = "icon.ico";
            var assembly = Assembly.GetExecutingAssembly();
            resourceName = typeof(App).Namespace + "." + resourceName.Replace(" ", "_")
                .Replace("\\", ".").Replace("/", ".");
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                //The icon is added to the project resources.
                //Here I assume that the name of the file is 'TrayIcon.ico' 
                    TrayIcon.Icon = new Icon(stream); //Properties.Resources.TrayIcon;

            //Optional - handle doubleclicks on the icon:
            TrayIcon.Click += TrayIcon_DoubleClick;

            //Optional - Add a context menu to the TrayIcon:
            TrayIconContextMenu = new ContextMenuStrip();
            CloseMenuItem = new ToolStripMenuItem();
            TrayIconContextMenu.SuspendLayout();

            // 
            // TrayIconContextMenu
            // 
            this.TrayIconContextMenu.Items.AddRange(new ToolStripItem[] {
            this.CloseMenuItem});
            this.TrayIconContextMenu.Name = "TrayIconContextMenu";
            this.TrayIconContextMenu.Size = new Size(153, 70);
            // 
            // CloseMenuItem
            // 
            this.CloseMenuItem.Name = "CloseMenuItem";
            this.CloseMenuItem.Size = new Size(152, 22);
            this.CloseMenuItem.Text = "Close " + Name;
            this.CloseMenuItem.Click += new EventHandler(this.CloseMenuItem_Click);

            TrayIconContextMenu.ResumeLayout(false);
            TrayIcon.ContextMenuStrip = TrayIconContextMenu;
        }

        public void Hide()
        {
            TrayIcon.Visible = false;
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            //Cleanup so that the icon will be removed when the application is closed
            TrayIcon.Visible = false;
        }

        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            if (OnClick != null) OnClick();
        }

        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            if (OnExit != null) OnExit();
        }
    }
}
