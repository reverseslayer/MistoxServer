using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;

// Client Connections

namespace MistoxServer.Server {
    public class TCPServer {

        public event EventHandler onReceived;
        public List<Connection> Clients;

        TcpListener Listener;
        bool Alive;

        public TCPServer() {
            Clients = new List<Connection>();
            Alive = true;

            Thread ConnectionThread = new Thread(ListenerThread);
            ConnectionThread.Start();
        }

        public void SendAll<T>(T packet) {
            foreach (Connection cur in Clients) {
                NetworkStream Stream = cur.tcpClient.GetStream();
                byte[] typeName = Encoding.UTF8.GetBytes(typeof(T).FullName);
                byte[] typeLength = BitConverter.GetBytes(typeName.Length);
                byte[] packetdata = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(packet));
                byte[] length = BitConverter.GetBytes(packetdata.Length);
                Stream.Write(typeLength, 0, 4);
                Stream.Write(typeName, 0, typeName.Length);
                Stream.Write(length, 0, 4);
                Stream.Write(packetdata, 0, packetdata.Length);
            }
        }

        public void SendTo<Packet>(Guid user, Packet packet) {
            foreach (Connection cur in Clients) {
                if (cur.ID == user) {
                    NetworkStream Stream = cur.tcpClient.GetStream();
                    byte[] typeName = Encoding.UTF8.GetBytes(typeof(Packet).FullName);
                    byte[] typeLength = BitConverter.GetBytes(typeName.Length);
                    byte[] packetdata = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(packet));
                    byte[] length = BitConverter.GetBytes(packetdata.Length);
                    Stream.Write(typeLength, 0, 4);
                    Stream.Write(typeName, 0, typeName.Length);
                    Stream.Write(length, 0, 4);
                    Stream.Write(packetdata, 0, packetdata.Length);
                    break;
                }
            }
        }

        void AddUser(ConnectionRequestPacket conRequest, TcpClient tcp) {
            foreach (Connection cur in Clients) {
                if (cur.tcpClient == tcp) {
                    cur.ID = conRequest.GUID;
                    cur.UserName = conRequest.UserName;
                    break;
                }
            }
        }

        void ReceiveThread(Connection Client) {
            try {
                byte[] BufferedData = new byte[0];
                while (Alive) {
                    byte[] StreamData = new byte[1024];
                    int bytesRead = Client.tcpClient.GetStream().Read(StreamData, 0, StreamData.Length);                                                                    // Read data off the network
                    BufferedData = BufferedData.Join(StreamData.Sub(0, bytesRead));                                                                                         // Pull the recieved data out and buffer it
                                                                                                                                                                            // [TypeLength, TypeName, DataLength, Data]
                    if (BufferedData.Length > 4) {                                                                                                                          // If the packet length data has been received
                        int typeLength = BitConverter.ToInt32(BufferedData);                                                                                                // Gets the first 4 bytes off the data and puts it into the data length int
                        if (BufferedData.Length >= typeLength + 8) {                                                                                                        // If the type and the data length of the packet has been received
                            int dataLength = BitConverter.ToInt32(BufferedData.Sub(typeLength + 4, 4));                                                                     // Get the data length off the packet
                            if (BufferedData.Length >= typeLength + dataLength + 8) {                                                                                       // If the whole packet has been received
                                Type dType = Type.GetType(Encoding.UTF8.GetString(BufferedData.Sub(4, typeLength)));                                                        // Get the type of the data
                                dynamic dData = JsonConvert.DeserializeObject(Encoding.UTF8.GetString(BufferedData.Sub(typeLength + 8, dataLength)), dType);                // Get the packet as the correct type sent
                                if (dType == typeof(ConnectionRequestPacket)) {
                                    AddUser(dData, Client.tcpClient);
                                    SendAll(dData);
                                    Console.WriteLine("User Added with username of [" + ((ConnectionRequestPacket)dData).UserName + "] via slowupdate");
                                } else {
                                    onReceived.Invoke(dData, new EventArgs());                                                                                              // Split out packet and send it up
                                }
                                BufferedData = BufferedData.Sub(dataLength, BufferedData.Length - dataLength);                                                              // Remove the packet from the Buffered data
                            }
                        }
                    }
                }
            } catch (Exception e) {
                Clients.Remove(Client);
                Console.WriteLine("A user has disconnected for reason : " + e.ToString());
            }
        }


        void ListenerThread() {
            Listener = new TcpListener(IPAddress.Any, 25566);
            Listener.Start();
            while (Alive) {
                TcpClient client = Listener.AcceptTcpClient();
                Console.WriteLine("New User Connected");
                Connection user = new Connection() {
                    tcpClient = client
                };
                Clients.Add(user);
                Thread receiveThread = new Thread(() => ReceiveThread(user));
                receiveThread.Start();
            }
        }

        public void Dispose() {
            Alive = false;
            Listener.Stop();
            foreach (Connection cur in Clients) {
                cur.tcpClient.Close();
            }
            Clients = null;
            Listener = null;
        }
    }
}
