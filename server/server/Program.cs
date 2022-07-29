using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using Newtonsoft.Json;

namespace Server
{
    class Program
    {
        private static byte[] _buffer = new byte[1024];
        private static List<Socket> _clientSockets = new List<Socket>();
        // lấy 1 list socket client mục đích là hỗ trợ multiple client
        private static Socket _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //AddressFamily.InterNetwork địa chỉ gia đình SocketType.Stream: gửi và nhận (đi và về) ưu điểm ổn định  
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
            socket.BeginReceive(_buffer,0,_buffer.Length,SocketFlags.None, new AsyncCallback(ReceiveCallback),socket); //SocketFlags.None : Chỉ định các hành vi gửi và nhận của socket
            _serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), null);

        }

        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket socket = (Socket)AR.AsyncState; //object lưu hay 1 tiêu chuẩn về bất đồng bộ
            int received = socket.EndReceive(AR);// cách thức sẽ chặn đến khi dữ liệu xuất hiện hoặc tìm thấy dữ liệu 
            byte[] dataBuf = new byte[received];//Đại diện cho trạng thái của một hoạt động không đồng bộ.
            Array.Copy(_buffer,dataBuf,received);//Copy dữ lữ liệu từ databuf -> _buffer với chiều dài dữ liệu là recevied
            string text = Encoding.ASCII.GetString(dataBuf);// chuyển dữ liệu thành string

            Console.WriteLine("Text received : " + text);

            string response = string.Empty;
            String[] strs = ReadData(text);//login usernam password 

            if (strs[0].ToLower() == "login")
            {
                string path = Path.GetFullPath("account.json");
                string json =File.ReadAllText(path);
                var accounts =System.Text.Json.JsonSerializer.Deserialize<List<Customer>>(json); //Deserialize : biến file json thành 1 cái class

                bool haveAccount = false;
                if(accounts != null)
                {
                    for (int i = 0; i < accounts.Count; ++i)
                    {
                        if (strs[1] == accounts[i].name && strs[2] == accounts[i].password)
                        {
                            haveAccount = true;
                            break;
                        }
                    }
                    Console.WriteLine(haveAccount);
                    response = haveAccount ? "Login Success" : "Wrong username or password";
                }else
                {
                    response = "Couldn't access to data";
                }
                
                
            }
            else if (strs[0].ToLower() == "logout")
            {
                response = "Logout Success";
            }
            else if (strs[0].ToLower() == "checkout")
            {
                if(strs.Length == 1) { 
                    string path = Path.GetFullPath("hotelCollection.json");
                    string json =File.ReadAllText(path);
                    response = "checkout/"+json;
                }else if (strs.Length == 3)
                {

                }
               
            }else if (strs[0].ToLower() == "signup")
            {
                Customer customer = new Customer()
                {
                    id = 10,
                    name = strs[1],
                    password = strs[2],
                    creditnumber = strs[3],
                    room = new List<string>()
                };
                try
                {
                    
                    string path = Path.GetFullPath("account.json");
                    string json = File.ReadAllText(path);
                    var accounts = System.Text.Json.JsonSerializer.Deserialize<List<Customer>>(json);
                    accounts.Add(customer);
                    var jsonToWrite = JsonConvert.SerializeObject(accounts, Formatting.Indented); //SerializeObject: biến 1 list class thành 1 định dạng json
                    using (var writer = new StreamWriter(path))
                    {
                        writer.Write(jsonToWrite);
                        response = "signup complete";
                    }
                }catch(Exception ex)
                {
                    response = "signup fail";
                }
            }else if(strs[0].ToLower() == "booking")
            {
                string path = Path.GetFullPath("hotelCollection.json");
                string json = File.ReadAllText(path);
                var hotels = System.Text.Json.JsonSerializer.Deserialize<List<Hotel>>(json);
                bool catchComplete = false;
                for(int i = 0; i < hotels.Count;++i)
                {
                    if (Int32.Parse(strs[1]) == hotels[i].id)
                    {
                        if(hotels[i].rooms[Int32.Parse(strs[2]) - 1].isBooking == false)
                        {
                            hotels[i].rooms[Int32.Parse(strs[2]) - 1].isBooking = true;
                            var jsonToWrite = JsonConvert.SerializeObject(hotels, Formatting.Indented);
                            using (var writer = new StreamWriter(path))
                            {
                                writer.Write(jsonToWrite);
                                response = "booking complete";
                            }
                            

                        }else
                        {
                            response = "room have been taken";
                        }
                        catchComplete = true;
                        
                        break;
                    }
                }

                if(!catchComplete)
                {
                    response = "Could Not Found hotel id";
                }
            }
            else
            {
                response = "Invalid Request";
                
            }

            byte[] data = Encoding.ASCII.GetBytes(response);
            socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), socket); // gửi dữ liệu bất đồng bộ đến client
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), socket); // nhận dữ liệu bất đồng bộ từ client

        }
        /// <summary>
        /// modifile function write here
        /// </summary>
        private static string[] ReadData(string text)
        {
           String[] splitStr = text.Split(" ");// biến những request thành 1 mảng string để phân tích

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
            Socket socket = (Socket)AR.AsyncState;// trạng thái gửi bất đồng bộ

            socket.EndSend(AR);//đợi dữ liệu đến
        }
    }

    public class Customer{
        public int id { get;set;}
        public string name { get; set; }
        public string password { get; set; }
        public string creditnumber { get; set; }
        public List<string> room { get; set; }
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
