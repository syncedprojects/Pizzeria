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
    public partial class Form1 : Form
    {
        /* создать экземпляры классов для дальнейшего использования */
        PizzeriaTcpServer service = new PizzeriaTcpServer();
        TcpFileHandler fh = new TcpFileHandler();

        public Form1()
        {
            InitializeComponent();
            this.SizeChanged += new EventHandler(Form1_SizeChanged);
            // передать "оригинальный" listBox1 объект
            TcpFileHandler.SetControls(listBox1);
            fh.GetFolderForOrders();
            folderBrowserDialog1.Description = @"Создать/Изменить папку для хранения файлов заказов
*При изменении папки все файлы будут копированы в новую папку. Старая не будет удалена в целях безопасности.";
        }

        void Form1_SizeChanged(object sender, EventArgs e)
        {
            listBox1.Width = this.Width - 23;
        }

        private void openFilesFolderToolStripMenuItem1_Click(
            object sender, EventArgs e)
        {
            if (TcpFileHandler.actualFolder == null)
            {
                MessageBox.Show(@"Нет ни одной папки для хранения данных. Сначала создайте папку!");
            }
            else
            {
                if (Directory.Exists(TcpFileHandler.actualFolder))
                {
                    openFileDialog1.InitialDirectory =
                        TcpFileHandler.actualFolder;
                    openFileDialog1.ShowDialog();
                }
                else
                {
                    MessageBox.Show(@"Папка для хранения файлов заказов не обнаружена. Она либо была удалена вручную, или перенесена в другое место!");
                }
            }
        }

        private void StarTcpServer_Click(object sender, EventArgs e)
        {
            if (TcpFileHandler.actualFolder == null)
            {
                MessageBox.Show(@"Папка для хранения файлов заказов не обнаружена. Пожалуйста, создайте новую!");
                while (true)
                {
                    DialogResult res = folderBrowserDialog1.ShowDialog();
                    if (res == DialogResult.OK)
                    {
                        /* создать файл для хранения настроек,
                           если нет ни одной */
                        TcpFileHandler.CreateOrAlterFolderForOrders(
                            folderBrowserDialog1);
                        fh.GetFolderForOrders();
                        break;
                    }
                    else if (res == DialogResult.Cancel)
                    {
                        MessageBox.Show(@"Вы ещё не создали папку для хранения файлов заказов. Чтобы начать работу, пожалуйста, создайте одну!");
                        break;
                    }
                }
            }
            else
            {
                if (Directory.Exists(TcpFileHandler.actualFolder))
                {
                    // и только потом, запускать сервер
                    if (!PizzeriaTcpServer.serviceOn)
                    {
                        service.StartTcpServer();
                    }
                    else
                    {
                        MessageBox.Show("Сервер уже запущен!");
                    }
                }
                else
                {
                    MessageBox.Show(@"Папка для хранения файлов заказов не обнаружена. Она либо была удалена вручную, или перенесена в другое место! Создайте новую папку чтобы начать работу.");
                }
            }
        }

        private void StopTcpServer_Click(object sender, EventArgs e)
        {
            if (PizzeriaTcpServer.serviceOn)
            {
                service.StopTcpServer();
            }
            else
            {
                MessageBox.Show("Сервер уже остановлен!");
            }
        }

        private void exitToolStripMenuItem_Click(
            object sender, EventArgs e)
        {
            if (!PizzeriaTcpServer.serviceOn)
            {
                menuStrip1.Dispose();
                listBox1.Dispose();
                folderBrowserDialog1.Dispose();
                Application.Exit();
            }
            else
            {
                MessageBox.Show(
                    "Сначала закройте активное соединение...");
            }
        }

        private void listBox1_MouseDoubleClick(
            object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                var match = Regex.Match(listBox1.SelectedItem.ToString(),
                    @"\d{18}.doc");
                fh.OpenOrderFile(match.ToString()); 
            }
        }

        private void openOrAlterFolderToolStripMenuItem_Click(
            object sender, EventArgs e)
        {
            if (!PizzeriaTcpServer.serviceOn)
            {
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    TcpFileHandler.CreateOrAlterFolderForOrders(
                        folderBrowserDialog1);
                    // перезаписать файл для хранения настроек
                    TcpFileHandler.SetSettingsFile(folderBrowserDialog1);
                    fh.GetFolderForOrders();
                }
            }
            else
            {
                MessageBox.Show("Сначала отключите соединение!");
            }
        }

        private void aboutToolStripMenuItem_Click(
            object sender, EventArgs e)
        {
            AboutBox1 ab1 = new AboutBox1();
            ab1.Text = "Пиццерия \"Elite Pizzas\"";
            ab1.ShowDialog();
        }
    }
}
