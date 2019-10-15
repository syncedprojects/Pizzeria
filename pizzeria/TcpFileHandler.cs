using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace pizzeria
{
    class TcpFileHandler
    {
        private static ListBox listBoxObject;
        delegate void ListBoxDelegate(string data);

        public static void SetControls(ListBox lb)
        {
            listBoxObject = lb;
        }

        public void ListBoxAddItem(string data)
        {
            if (listBoxObject.InvokeRequired)
            {
                ListBoxDelegate d = new ListBoxDelegate(ListBoxAddItem);
                listBoxObject.Invoke(d, new object[] { data });
            }
            else
            {
                listBoxObject.Items.Add(data);
            }
        }
        
        public static void SetSettingsFile(FolderBrowserDialog fbd)
        {
            if (!Directory.Exists(@"settings\"))
            {
                Directory.CreateDirectory(@"settings\");
            }
            FileStream fs = new FileStream(
                    @"settings\settings.dat",
                    FileMode.Create,
                    FileAccess.ReadWrite);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(fbd.SelectedPath + @"\");
            sw.Flush();

            sw.Close();
            fs.Close();
        }

        public static void CreateOrAlterFolderForOrders(
            FolderBrowserDialog fbDialog)
        {
            string oldFolder = actualFolder;

            if (actualFolder == null)
            {
                SetSettingsFile(fbDialog);
            }
            else
            {
                if (Directory.Exists(actualFolder))
                {
                    if (fbDialog.SelectedPath + @"\" ==
                        oldFolder)
                    {
                        MessageBox.Show(@"Невозможно создавать уже созданную папку! Введите другое имя папки.");
                    }
                    else
                    {
                        string[] dirs = Directory.GetDirectories(
                            actualFolder);
                        foreach (string dir in dirs)
                        {
                            var fFolder = Regex.Match(dir,
                                @"\d{2}-\d{2}-\d{2}");
                            if (!Directory.Exists(
                                fbDialog.SelectedPath +
                                @"\" + fFolder.ToString()))
                            {
                                Directory.CreateDirectory(
                                    fbDialog.SelectedPath +
                                    @"\" + fFolder.ToString());
                            }

                            string[] files = Directory.GetFiles(dir);
                            foreach (string file in files)
                            {
                                FileStream readfs = new FileStream(file,
                                    FileMode.Open, FileAccess.Read);
                                StreamReader sr = new
                                    StreamReader(readfs);
                                string info = sr.ReadToEnd();

                                sr.Close();
                                readfs.Close();

                                var fname = Regex.Match(
                                    file, @"\d{18}.doc");

                                FileStream writefs = new FileStream(
                                    fbDialog.SelectedPath +
                                    @"\" + fFolder.ToString() + @"\" +
                                    fname.ToString(),
                                    FileMode.Create, FileAccess.Write);
                                StreamWriter sw = new
                                    StreamWriter(writefs);
                                sw.Write(info);
                                sw.Flush();

                                sw.Close();
                                writefs.Close();
                            }
                        }
                        SetSettingsFile(fbDialog);
                        MessageBox.Show(@"Создана новая папка. Данные из старой папки скопированы в нее.");
                    }
                }
                else
                {
                    SetSettingsFile(fbDialog);
                    MessageBox.Show(@"Создана новая пустая папка.");
                }
            }
        }

        public static string actualFolder;

        public void GetFolderForOrders()
        {
            try
            {
                FileStream fs = new FileStream(
                    @"settings\settings.dat",
                    FileMode.Open,
                    FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                actualFolder = sr.ReadToEnd();
                sr.Close();
                fs.Close();
            }
            catch (Exception)
            {
                return;
            }
        }

        public static string fileFolder;

        public void CreateOrderFile(string fname, string info)
        {
            string date = DateTime.Now.Date.ToShortDateString();
            string correctDate = date.Replace('/', '-');
            string folder = actualFolder;
            correctDate += @"\";
            fileFolder = correctDate;
            folder += correctDate;
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            string path = folder + fname;
            
            FileStream fs = new FileStream(path,
                FileMode.CreateNew, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(info);
            sw.Flush();

            sw.Close();
            fs.Close();
        }

        public void OpenOrderFile(string fname)
        {
            string fullPath = actualFolder + fileFolder + fname;
            try
            {
                Process newProc = Process.Start(fullPath);
            }
            catch (Exception)
            {
                MessageBox.Show("Запрашиваемый файл не найден!");
            }
        }
    }
}
