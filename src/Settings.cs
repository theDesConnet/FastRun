using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FRun
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        public void getListApplications()
        {
            listView1.Items.Clear();
            listView2.Items.Clear();
            listView3.Items.Clear();
            using (RegistryKey frun = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\FRun", true))
            {
                try
                {
                    foreach (string keyName in frun.GetSubKeyNames())
                    {
                        using (RegistryKey appKey = frun.OpenSubKey(keyName, true))
                        {
                            var row = new string[5];
                            var listview = new ListViewItem();

                            switch ((int)appKey.GetValue("RunType"))
                            {
                                case (int)Types.runType.FILE:
                                    row = new string[] { keyName, appKey.GetValue("ExecParam").ToString(), appKey.GetValue("Args") != null ? appKey.GetValue("Args").ToString() : "", appKey.GetValue("UAC") != null ? (int)appKey.GetValue("UAC") == 0 ? "False" : "True" : "False" };
                                    listview = new ListViewItem(row);


                                    listview.Tag = appKey.Name.Split('\\')[3];
                                    listView1.Items.Add(listview);
                                    break;

                                case (int)Types.runType.LINK:
                                    row = new string[] { keyName, appKey.GetValue("ExecParam").ToString() };
                                    listview = new ListViewItem(row);


                                    listview.Tag = appKey.Name.Split('\\')[3];
                                    listView2.Items.Add(listview);
                                    break;

                                case (int)Types.runType.FOLDER:
                                    row = new string[] { keyName, appKey.GetValue("ExecParam").ToString() };
                                    listview = new ListViewItem(row);


                                    listview.Tag = appKey.Name.Split('\\')[3];
                                    listView3.Items.Add(listview);
                                    break;
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            getListApplications();
        }

        private void Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                Environment.Exit(1);
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            string item = "";

            if (tabControl2.SelectedTab == tabPage3 && listView1.SelectedIndices.Count != 0 || tabControl2.SelectedTab == tabPage4 && listView2.SelectedIndices.Count != 0 || tabControl2.SelectedTab == tabPage5 && listView3.SelectedIndices.Count != 0)
            {
                if (tabControl2.SelectedTab == tabPage3) item = (string)listView1.FocusedItem.Tag;
                if (tabControl2.SelectedTab == tabPage4) item = (string)listView2.FocusedItem.Tag;
                if (tabControl2.SelectedTab == tabPage5) item = (string)listView3.FocusedItem.Tag;

                using (RegistryKey AppsKey = Registry.CurrentUser.CreateSubKey($@"SOFTWARE\FRun", true))
                {
                    AppsKey.DeleteSubKeyTree(item);
                }

                getListApplications();
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (new NewApp().ShowDialog() == DialogResult.OK) getListApplications();
        }
    }
}
