using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace Client
{
    class Program
    {
        private static readonly Socket ClientSocket = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        private const int PORT = 100;

        private static bool isLoggedIn = false;
        private static Customer customer;

        static void Main()
        {
            Console.Title = "Client";
            ConnectToServer();
            RequestLoop();
            Exit();
        }

        private static void ConnectToServer()
        {
            int attempts = 0;

            while (!ClientSocket.Connected)
            {
                try
                {
                    attempts++;
                    Console.WriteLine("Connection attempt " + attempts);
                    // Change IPAddress.Loopback to a remote IP to connect to a remote host.
                    ClientSocket.Connect(IPAddress.Loopback, PORT);
                }
                catch (SocketException)
                {
                    Console.Clear();
                }
            }

            Console.Clear();
            Console.WriteLine("Connected");
        }

        private static void RequestLoop()
        {
            Console.WriteLine(@"<Type ""exit"" to properly disconnect client>");

            while (true)
            {
                SendRequest();
                ReceiveResponse();
            }
        }

        /// <summary>
        /// Close socket and exit program.
        /// </summary>
        private static void Exit()
        {
            SendString("exit"); // Tell the server we are exiting
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
            Environment.Exit(0);
        }

        private static void SendRequest()
        {
            Console.Write("Send a request: ");
            string request = Console.ReadLine();


            if (request.ToLower() == "exit")
            {
                Exit();
            }else if(request.ToLower() == "login")
            {
                LoginForm();
            }else if(request.ToLower() == "logout")
            {
                Logout();
            }
        }

        /// <summary>
        /// Sends a string to the server with ASCII encoding.
        /// </summary>
        private static void SendString(string text)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(text);
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        private static void LoginForm()
        {
            Console.WriteLine("Enter your user name: ");
            string name = Console.ReadLine();

            Console.WriteLine("Enter your password: ");
            string password = Console.ReadLine();
            customer = new Customer(name, password);
            SendLoginData(name, password);
        } 

        private static void Logout()
        {
            Console.WriteLine("Do you wanted to logout");
            Console.WriteLine(@"<Type ""yes"" or ""no"" ");
            string choice = Console.ReadLine();
            if(choice.ToLower() == "no")
            {
                return;
            }else if(choice.ToLower() == "yes")
            {
                byte[] buffer = Encoding.ASCII.GetBytes("logout");
                ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
            }
            
        }

        private static void SendLoginData(string name,string password)
        {
            byte[] buffer = Encoding.ASCII.GetBytes("login"+ " " + name + " " + password);
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        private static void SendResigterData(string name,string password,string creditNumber)
        {

        }
        /// <summary>
        /// end modifiler for socket
        /// </summary>
        private static void ReceiveResponse()
        {
            var buffer = new byte[2048];
            int received = ClientSocket.Receive(buffer, SocketFlags.None);
            if (received == 0) return;
            var data = new byte[received];
            Array.Copy(buffer, data, received);
            string text = Encoding.ASCII.GetString(data);
            Console.WriteLine(text);

            if (text == "Login Success")
            {
                Console.WriteLine("Login Success to " + customer.readName());
                Console.WriteLine(@"<Type ""logout"" to logout client>");
            }else if(text == "Logout Success")
            {
                customer = new Customer("","");
            }
                     
        }

        class Customer
        {
            string name;
            string creditCard;

            public Customer(string name,string creditcard)
            {
                this.name = name;
                this.creditCard = creditcard;
            }

            public string readName()
            {
                return name;
            }
        }
    }
}