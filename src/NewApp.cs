using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FRun
{
    public partial class NewApp : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);
        const int EM_SETCUEBANNER = 0x1501;

        public NewApp()
        {
            InitializeComponent();
            SendMessage(textBox1.Handle, EM_SETCUEBANNER, 1, "Укажите название которые вы будете указывать для запуска");
            SendMessage(textBox2.Handle, EM_SETCUEBANNER, 1, @"[Путь до файла, Пример: C:\Path\to\File.exe]");
            SendMessage(textBox3.Handle, EM_SETCUEBANNER, 1, @"[Пример: -o test.exe]");
        }

        Types.runType rType = Types.runType.FILE;

        private void button1_Click(object sender, EventArgs e)
        {
            switch (rType)
            {
                case Types.runType.FILE:
                    using (OpenFileDialog fileChoose = new OpenFileDialog())
                    {
                        fileChoose.Filter = "Исполняемые файлы | *.exe";
                        fileChoose.Title = "Выберите файл который будет открываться";

                        if (fileChoose.ShowDialog() == DialogResult.OK)
                        {
                            textBox2.Text = fileChoose.FileName;
                            textBox2.SelectionStart = textBox2.Text.Length;
                            textBox2.ScrollToCaret();
                        }
                    }
                    break;

                case Types.runType.FOLDER:
                    using (FolderBrowserDialog folderChoose = new FolderBrowserDialog())
                    {
                        folderChoose.Description = "Выберите папку которая будет открыватся";

                        if (folderChoose.ShowDialog() == DialogResult.OK)
                        {
                            textBox2.Text = folderChoose.SelectedPath;
                            textBox2.SelectionStart = textBox2.Text.Length;
                            textBox2.ScrollToCaret();
                        }
                    }
                    break;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(textBox1.Text) || String.IsNullOrEmpty(textBox2.Text))
            {
                MessageBox.Show("Вы не указали один из параметров!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            using (RegistryKey AppsKey = Registry.CurrentUser.CreateSubKey($@"SOFTWARE\FRun", true))
            {
                if (AppsKey.GetSubKeyNames().Any(x => x == textBox1.Text.ToLower()))
                {
                    MessageBox.Show($"Название \"{textBox1.Text.ToLower()}\" уже используется");
                    return;
                }
                using (RegistryKey appKey = AppsKey.CreateSubKey(textBox1.Text.ToLower(), true))
                {
                    appKey.SetValue("ExecParam", textBox2.Text);
                    appKey.SetValue("RunType", (int)rType);

                    if (rType == Types.runType.FILE)
                    {
                        appKey.SetValue("Args", textBox3.Text != "" ? textBox3.Text : "");
                        appKey.SetValue("UAC", checkBox1.Checked ? 1 : 0);
                    }
                }
            }

            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                rType = Types.runType.FILE;
                textBox2.Text = @"";
                SendMessage(textBox2.Handle, EM_SETCUEBANNER, 1, @"[Путь до файла, Пример: C:\Path\to\File.exe]");
                button1.Text = "Выберите файл";
                button1.Enabled = true;
                textBox3.Enabled = true;
                checkBox1.Enabled = true;
            }
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked)
            {
                rType = Types.runType.FOLDER;
                textBox2.Text = @"";
                SendMessage(textBox2.Handle, EM_SETCUEBANNER, 1, @"[Путь до папки, Пример: C:\Path\to\Folder]");
                button1.Text = "Выберите папку";
                button1.Enabled = true;
                textBox3.Enabled = false;
                checkBox1.Enabled = false;
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                rType = Types.runType.LINK;
                textBox2.Text = @"";
                SendMessage(textBox2.Handle, EM_SETCUEBANNER, 1, @"[Пример: https://ya.ru или tg://resolve?domain=ds1nc]");
                button1.Text = "Выберите ссылку";
                button1.Enabled = false;
                textBox3.Enabled = false;
                checkBox1.Enabled = false;
            }
        }
    }
}
