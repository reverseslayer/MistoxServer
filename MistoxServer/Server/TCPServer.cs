using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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
            Parallel.ForEach(Clients, ( cur ) => {
                Extensions.StreamSend( cur.tcpClient.GetStream(), packet );
            } );
        }

        public void SendTo<Packet>(string user, Packet packet) {
            Parallel.ForEach( Clients, ( cur ) => {
                if( cur.ID == user ) {
                    Extensions.StreamSend( cur.tcpClient.GetStream(), packet );
                }
            } );
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
                    byte[] StreamData = new byte[2048];
                    int bytesRead = Client.tcpClient.GetStream().Read(StreamData, 0, StreamData.Length);                                                                    // Read data off the network
                    BufferedData = BufferedData.Join(StreamData.Sub(0, bytesRead));                                                                                         // Pull the recieved data out and buffer it
                                                                                                                                                                            // [TypeLength, TypeName, DataLength, Data]
                    if (BufferedData.Length > 4) {                                                                                                                          // If the packet length data has been received
                        int typeLength = BitConverter.ToInt32(BufferedData);                                                                                                // Gets the first 4 bytes off the data and puts it into the data length int
                        if (BufferedData.Length >= typeLength + 8) {                                                                                                        // If the type and the data length of the packet has been received
                            int dataLength = BitConverter.ToInt32(BufferedData.Sub(typeLength + 4, 4));                                                                     // Get the data length off the packet
                            if (BufferedData.Length >= typeLength + dataLength + 8) {                                                                                       // If the whole packet has been received
                                Type dType = Type.GetType(Encoding.UTF8.GetString(BufferedData.Sub(4, typeLength)));                                                        // Get the type of the data
                                dynamic dData = mSerialize.Deserialize(BufferedData.Sub(typeLength + 8, dataLength), dType);                                                // Get the packet as the correct type sent
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
