using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        private static byte[] _buffer = new byte[1024];
        private static List<Socket> _clientSockets = new List<Socket>();
        private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static void Main(string[] args)
        {
            Console.Title = "Server";
            SetupServer();

            Console.ReadLine();
        }

        private static void SetupServer()
        {
            Console.WriteLine("Starting Server");
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any,100));
            _serverSocket.Listen(5);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);

        }

        private static void AcceptCallBack(IAsyncResult AR)
        {
            Socket socket = _serverSocket.EndAccept(AR);
            _clientSockets.Add(socket);
            Console.WriteLine("Client Connected to Server");
            socket.BeginReceive(_buffer,0,_buffer.Length,SocketFlags.None, new AsyncCallback(ReceiveCallback),socket);
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);

        }

        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;
            int received = socket.EndReceive(AR);
            byte[] dataBuf = new byte[received];
            Array.Copy(_buffer,dataBuf,received);
            string text = Encoding.ASCII.GetString(dataBuf);

            Console.WriteLine("Text received : " + text);

            string response = string.Empty;
            String[] strs = ReadData(text);

            if (strs[0].ToLower() == "login")
            {
                if (strs[1] == "MinhHien" && strs[2] == "123456")
                {
                    response = "Login Success";
                }
                else
                {
                    response = "Wrong username or password";
                }
            }
            else if (strs[0].ToLower() == "logout")
            {
                response = "Logout Success";
            }
            else if (strs[0].ToLower() == "checkout")
            {
                response = "checkout/{}";
            }
            else
            {
                response = "Invalid Request";
                
            }

            byte[] data = Encoding.ASCII.GetBytes(response);
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket);

        }
        /// <summary>
        /// modifile function write here
        /// </summary>
        private static string[] ReadData(string text)
        {
           String[] splitStr = text.Split(" ");

            return splitStr;
        }

        private static void GetInput(string text, IAsyncResult AR)
        {
            switch(text)
            {
                case "login":
                    break;
                case "signup":
                    break;
                case "logout":
                    break;
                case "checkout":
                    break;
            }
        }

        private static void Register(IAsyncResult AR)
        {
            
        }

        private static void Login(IAsyncResult AR)
        {

        }

        private static void CheckOut(IAsyncResult AR)
        {

        }

        private static void Booking(IAsyncResult AR)
        {

        }

        /// <summary>
        /// End Modified function
        /// </summary>

        private static void SendCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState;

            socket.EndSend(AR);
        }
    }

    class Customer{
        public string userName;
        public string password;
        public string creditNumber;

        public Customer(string userName, string password,string number)
        {
            this.userName = userName;
            this.password = password;
            this.creditNumber = number;
        }

    }
}
