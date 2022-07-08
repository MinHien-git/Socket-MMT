using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Text.Json;

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
            }else if(request.ToLower() == "checkout")
            {
                Checkout();
            }else if(request.ToLower() == "signup")
            {
                SignupForm();
            }else if(request.ToLower() == "booking")
            {
                Booking();
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

        private static void SignupForm()
        {
            Console.WriteLine("Enter your user name: "); //cout << endl
            string name = Console.ReadLine(); //cin >> 

            Console.WriteLine("Enter your password: ");
            string password = Console.ReadLine();
            Console.WriteLine("Enter your credit card number: ");
            string creditCardNumber = Console.ReadLine();
            
            if(Int64.TryParse(creditCardNumber,out long result))
            {
                SendResigterData(name, password, creditCardNumber);
            }else
            {
                SendString("Your username or creditCardNumber is wrong please check again");
            }
            
        }

        private static void Booking()
        {
            Console.WriteLine("Hotel id : ");
            string hotelId = Console.ReadLine();

            Console.WriteLine("Enter room id name : ");
            string roomId = Console.ReadLine();

            Console.WriteLine("date in : ");
            string date = Console.ReadLine();
            Console.WriteLine("month in : ");
            string month = Console.ReadLine();

            Console.WriteLine("date out : ");
            string dateOut = Console.ReadLine();
            Console.WriteLine("month out : ");
            string monthOut = Console.ReadLine();
            //booking hotel1 12/12 13/12
            if(CheckDate(date,month) && CheckDate(dateOut, monthOut)) { 
                SendString("booking" + " " + hotelId + " " + roomId +" " + date + "/" + month + " " + dateOut + "/" + monthOut);
            }else
            {
                SendString("Wrong date or month Try again");
            }
        }
        
        private static bool CheckDate(string date,string month)
        {
            int dateI = Int32.Parse(date);
            switch (Int32.Parse(month))
            {
                case 1:
                case 3:
                case 5:
                case 7:
                case 8:
                case 10:
                case 12:
                    if (dateI <= 31)
                    {
                        return true;
                    }
                    break;
                case 2:
                    if (dateI <= 28)
                        return true;
                    break;
                case 4: case 6: case 9: case 11:
                    if (dateI <= 30)
                        return true;
                    break;
                default:
                    return false;
            }
            return false;
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
            byte[] buffer = Encoding.ASCII.GetBytes("signup" + " " + name + " " + password + " " + creditNumber);
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        private static void Checkout()
        {
             byte[] buffer = Encoding.ASCII.GetBytes("checkout");
             ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        private static void CheckoutInfo(string name, int hotelId,int id)
        {
            byte[] buffer = Encoding.ASCII.GetBytes("checkout" + " " + hotelId + " " + id + " " + customer.name);
            ClientSocket.Send(buffer, 0, buffer.Length, SocketFlags.None);
            
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
            

            if (text == "Login Success")
            {
                Console.WriteLine("Login Success to " + customer.readName());
                Console.WriteLine(@"<Type ""logout"" to logout client>");
            }else if(text == "Logout Success")
            {
                Console.WriteLine("Logout Success! Check out as a guest");
                customer = new Customer("","");
            }else if (text.Split("/")[0] == "checkout") //checkout/{}
            {
                Console.WriteLine("Your in checkout section");
                var rooms = JsonSerializer.Deserialize<List<Hotel>>(text.Split("/")[1]);
                //Console.WriteLine(rooms[0].rooms[1].isBooking);
                WriteHotelRoom(rooms);

            }else
            {
                Console.WriteLine(text);
            }
                     
        }

        private static void WriteHotelRoom(List<Hotel> hotels)
        {
            for(int i = 0;i < hotels.Count;++i)
            {
                Console.WriteLine(hotels[i].name + " : " + hotels[i].id);
                for(int j =0; j < hotels[i].rooms.Count;++j)
                {
                    Console.WriteLine("Room : " + hotels[i].rooms[j].id );
                }
            }
        }

        class Customer
        {
            public string name;
            public string creditCard;

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

        public class Room
        {
            public int id { get; set; }
            public bool isBooking { get; set; }
        }


        public class Hotel
        {
            public int id { get; set; }
            public string name { get; set; }
            public List<Room> rooms { get; set; }

        }

    }
}