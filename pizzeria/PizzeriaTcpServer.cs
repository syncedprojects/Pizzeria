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
    class PizzeriaTcpServer
    {
        private TcpListener server;
        public static bool serviceOn;
        private static Thread waitThread;
        private Thread conThread;

        public void StartTcpServer()
        {
            // создать серверный сокет
            server = new TcpListener(IPAddress.Any, 65535);
            // запустить TCP сервер
            server.Start();
            // если сокет соединен к порту
            if (server.Server.IsBound)
            {
                serviceOn = true;
                MessageBox.Show("Сервер успешно запущен!");
            }
            // новый поток для ожидания клиентов
            waitThread = new Thread(
                new ThreadStart(WaitForClients));
            waitThread.IsBackground = true;
            waitThread.Start();
        }

        void WaitForClients()
        {
            while (true)
            {
                // пока клиентов нет...
                while (!server.Pending())
                {
                    // ...пусть процессор отдыхает
                    Thread.Sleep(1000);
                }
                TcpConnectionHandler conHandler = new
                    TcpConnectionHandler();
                conHandler.serverCopy = this.server;
                // новый поток для обработки соединения с клиентом
                conThread = new Thread(new ThreadStart(
                    conHandler.HandleConnection));
                conThread.IsBackground = true;
                conThread.Start();
            }
        }

        // закрыть соединение
        public void StopTcpServer()
        {
            // прерывать поток waitThread
            waitThread.Abort();
            // теперь остановить службу
            server.Stop();
            // если сокет отсоединен
            if (!server.Server.IsBound)
            {
                serviceOn = false;
                MessageBox.Show("Сервер успешно остановлен!");
            }
        }
    }
}
