using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace FRun
{
    class Program
    {
        const int HWND_BROADCAST = 0xffff;
        const uint WM_SETTINGCHANGE = 0x001a;

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool SendNotifyMessage(IntPtr hWnd, uint Msg, UIntPtr wParam, string lParam);

        public static string ShowVersion(int VersionNumbers)
        {
            string[] ver = Application.ProductVersion.Split('.');
            string[] builderVer = new string[VersionNumbers];

            for (int i = 0; i < VersionNumbers; i++)
            {
                builderVer.SetValue(ver[i], i);
            }

            return string.Join(".", builderVer);
        }

        public static void RunFile(string FilePath, string Arguments, bool UAC, bool Hidden, bool WaitForExit)
        {
            Process process = new Process();
            ProcessStartInfo processStartInfo = new ProcessStartInfo();

            processStartInfo.WindowStyle = !Hidden ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden;
            processStartInfo.FileName = FilePath;
            processStartInfo.Arguments = Arguments;

            if (UAC) processStartInfo.Verb = "runas";

            process.StartInfo = processStartInfo;
            process.Start();

            if (WaitForExit) process.WaitForExit();
        }
        public static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                      .IsInRole(WindowsBuiltInRole.Administrator);
        }
        public static void HelpMsg()
        {
            MessageBox.Show($"FastRun [FRun] v{ShowVersion(2)} beta\nРазработчик: DesConnet\nСайт: https://ds1nc.ru\n\nДля быстрого запуска программы можно использовать окно выполнить или командную строку.\n\nПример использования: frun dnspy(Регистр не важен)\n\nДоступные аргументы:\n--about - Открывает окно помощи\n--settings - Открывает окно настроек\n--add - Добавить новую программу\n--clear - Очистить список программ\n--install - Выполнить установку FRun в данной директории\n--uninstall - Удаление FRun\n--prefix [prefix] - Изменение префикса\n--remove [name] - Удалить приложение из списка", $"FRun v{ShowVersion(2)} beta (c0d9d by DesConnet)", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Environment.Exit(1);
        }

        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 0) HelpMsg();

            #region Команды
            switch (args[0])
            {
                case "--about":
                    HelpMsg();
                    break;

                case "--settings":
                    if (IsAdministrator()) new Settings().ShowDialog();
                    else
                    {
                        RunFile(Application.ExecutablePath, "--settings", true, false, false);
                        Environment.Exit(1);
                    }
                    break;

                case "--add":
                    if (IsAdministrator())
                    {
                        new NewApp().ShowDialog();
                        Environment.Exit(1);
                    }
                    else
                    {
                        RunFile(Application.ExecutablePath, "--add", true, false, false);
                        Environment.Exit(1);
                    }
                    break;

                case "--clear":
                    if (IsAdministrator())
                    {
                        if (MessageBox.Show("Вы действительно хотите удалить все приложения из FRun?", "Очистка программ [FRun]", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            using (RegistryKey frun = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\FRun", true))
                            {
                                try
                                {
                                    string[] Apps = frun.GetSubKeyNames();

                                    foreach (string app in Apps)
                                    {
                                        frun.DeleteSubKeyTree(app);
                                    }
                                    MessageBox.Show("Очистка прошла успешно!", "Очистка программ [FRun]", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message, "Ошибка [FRun]", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                }
                            }
                        }
                        Environment.Exit(1);
                    } 
                    else
                    {
                        RunFile(Application.ExecutablePath, "--clear", true, false, false);
                        Environment.Exit(1);
                    }
                    break;

                case "--install":
                    if (IsAdministrator())
                    {
                        using (RegistryKey frun = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\FRun", true))
                        {
                            if (frun.GetValue("InstalledPath") == null)
                            {
                                if (MessageBox.Show("Вы действительно хотите установить FRun в данной директории?", "Установка [FRun]", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                {
                                    try
                                    {
                                        frun.SetValue("InstalledPath", Application.ExecutablePath);
                                        using (RegistryKey Env = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment", true))
                                        {
                                            string oldEnv = Env.GetValue("Path").ToString();
                                            Env.SetValue("Path", $"{(oldEnv.EndsWith(";") ? oldEnv : $"{oldEnv};")}{Path.GetDirectoryName(Application.ExecutablePath)};");
                                            SendNotifyMessage((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE, (UIntPtr)0, "Environment");
                                        }

                                        MessageBox.Show("Установка прошла успешно!\nТеперь вы можете вызывать FRun из окна выполнить и командной строки!", "Установка [FRun]", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.Message, "Ошибка [FRun]", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                    }
                                }
                            } 
                            else
                            {
                                MessageBox.Show("FRun уже установлен!", "Ошибка [FRun]", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            }
                        }

                        Environment.Exit(1);
                    }
                    else
                    {
                        RunFile(Application.ExecutablePath, "--install", true, false, false);
                        Environment.Exit(1);
                    }
                    break;

                case "--uninstall":
                    if (IsAdministrator())
                    {
                        using (RegistryKey frun = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\FRun", true))
                        {
                            if (frun.GetValue("InstalledPath") != null)
                            {
                                if (MessageBox.Show("Вы действительно хотите удалить FRun?", "Установка [FRun]", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                                {
                                    try
                                    {
                                        frun.DeleteValue("InstalledPath");
                                        using (RegistryKey Env = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Environment", true))
                                        {
                                            string[] oldEnv = Env.GetValue("Path").ToString().Split(';');
                                            oldEnv = oldEnv.Where(x => x != Path.GetDirectoryName(Application.ExecutablePath)).ToArray();

                                           

                                            Env.SetValue("Path", $"{string.Join(";", oldEnv)}");
                                            SendNotifyMessage((IntPtr)HWND_BROADCAST, WM_SETTINGCHANGE, (UIntPtr)0, "Environment");
                                        }

                                        MessageBox.Show("Удаление прошло успешно!", "Установка [FRun]", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show(ex.Message, "Ошибка [FRun]", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("FRun ещё не установлен!", "Ошибка [FRun]", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            }
                        }

                        Environment.Exit(1);
                    }
                    else
                    {
                        RunFile(Application.ExecutablePath, "--uninstall", true, false, false);
                        Environment.Exit(1);
                    }
                    break;

                case "--prefix":
                    if (IsAdministrator())
                    {
                        if (args[1] == null)
                        {
                            MessageBox.Show("Не указан префикс", "Ошибка [FRun]", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            Environment.Exit(1);
                        }

                        File.Move(Application.ExecutablePath, $@"{Path.GetDirectoryName(Application.ExecutablePath)}\{args[1]}.exe");
                        return;
                    } 
                    else
                    {
                        RunFile(Application.ExecutablePath, $"--prefix {(args[1] != null ? args[1] : "")}", true, false, false);
                        Environment.Exit(1);
                    }
                    break;

                case "--remove":
                    if (IsAdministrator())
                    {
                        if (args[1] == null)
                        {
                            MessageBox.Show("Не указана программа", "Ошибка [FRun]", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            Environment.Exit(1);
                        }

                        using (RegistryKey frun = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\FRun", true))
                        {
                            string[] Apps = frun.GetSubKeyNames();

                            if (Apps.Any(x => x == args[1]))
                            {
                                frun.DeleteSubKeyTree(args[1]);
                            } 
                            else
                            {
                                MessageBox.Show($"Не удалось найти программу с названием \"{args[1]}\"", "Ошибка [FRun]", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            }
                        }
                        return;
                    }
                    else
                    {
                        RunFile(Application.ExecutablePath, $"--remove {(args[1] != null ? args[1] : "")}", true, false, false);
                        Environment.Exit(1);
                    }
                    break;
            }
            #endregion

            #region Запуск приложений
            using (RegistryKey frun = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\FRun", true))
            {
                try
                {
                    string[] Apps = frun.GetSubKeyNames();

                    if (Apps.Any(x => x == args[0]))
                    {
                        using (RegistryKey AppKey = frun.OpenSubKey(Apps.Single(x => x == args[0]), true))
                        {
                            switch ((int)AppKey.GetValue("RunType"))
                            {
                                case (int)Types.runType.FILE:
                                    RunFile(AppKey.GetValue("ExecParam").ToString(), AppKey.GetValue("Args") != null ? AppKey.GetValue("Args").ToString() : "", AppKey.GetValue("UAC") != null ? (int)AppKey.GetValue("UAC") == 0 ? false : true : false, false, false);
                                    Environment.Exit(1);
                                    break;

                                case (int)Types.runType.FOLDER:
                                    RunFile("explorer.exe", AppKey.GetValue("ExecParam").ToString(), false, false, false);
                                    Environment.Exit(1);
                                    break;

                                case (int)Types.runType.LINK:
                                    RunFile(AppKey.GetValue("ExecParam").ToString(), "", false, false, false);
                                    Environment.Exit(1);
                                    break;
                            }
                        }
                    }
                } 
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка [FRun]", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }
            #endregion

            HelpMsg();
        }
    }
}
