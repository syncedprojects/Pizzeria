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
    class TcpConnectionHandler
    {
        public TcpListener serverCopy;
        public static int connections = 0;
        int recv;
        byte[] data = new byte[1024];

        public void HandleConnection()
        {
            try
            {
                /* функция AcceptTcpClient() блокирующий,
                   поэтому надо исполнять код в блоке try-catch! */ 
                TcpClient client = serverCopy.AcceptTcpClient();
                NetworkStream ns = client.GetStream();
                connections++;

                recv = ns.Read(data, 0, data.Length);
                ns.Write(data, 0, recv);
                ns.Flush();

                ns.Close();
                client.Close();

                string info = Encoding.Default.GetString(data, 0, recv);
                
                string index = " " + connections.ToString() + ".";
                string time = DateTime.Now.ToString();
                string orderId = DateTime.Now.Ticks.ToString() + ".doc";

                TcpFileHandler fh = new TcpFileHandler();
                fh.CreateOrderFile(orderId, info);
                string fullOrderInfo = index + "\t\t" +
                    time + "\t\t" + orderId;
                fh.ListBoxAddItem(fullOrderInfo);
            }
            catch (SocketException)
            {
                /* алт.: ничего не предпринимать */
                return;
            }
        }
    }
}
