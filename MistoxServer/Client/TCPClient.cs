using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json;

//IP Range Updater

namespace MistoxServer.Client {
    public class TCPClient {

        TcpClient Server;
        UDPClient Client;

        public event EventHandler onReceived;
        bool Alive;


        public TCPClient(string ServerAddress, int ClientPort, string UserName, UDPClient udpClient) {
            Server = new TcpClient(ServerAddress, 25566);
            Alive = true;
            Client = udpClient;
            Thread RThread = new Thread(ReceiveThread);
            RThread.Start();

            // Send Connection Request Packet
            Send(new ConnectionRequestPacket { GUID = Guid.NewGuid(), UserName = UserName, ClientPort = ClientPort });
        }

        public void Send<Packet>(Packet packet) {
            NetworkStream Stream = Server.GetStream();
            byte[] typeName = Encoding.UTF8.GetBytes(typeof(Packet).FullName);
            byte[] typeLength = BitConverter.GetBytes(typeName.Length);
            byte[] packetdata = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(packet));
            byte[] length = BitConverter.GetBytes(packetdata.Length);
            Stream.Write(typeLength, 0, 4);
            Stream.Write(typeName, 0, typeName.Length);
            Stream.Write(length, 0, 4);
            Stream.Write(packetdata, 0, packetdata.Length);
        }

        void AddUser(ConnectionRequestPacket conRequest) {
            Client.Endpoints.Add(new Connection() {
                ID = conRequest.GUID,
                UserName = conRequest.UserName,
                udpClient = new IPEndPoint(IPAddress.Parse(conRequest.LocalIP), conRequest.ClientPort)
            });
        }

        void ReceiveThread() {
            try {
                byte[] BufferedData = new byte[0];
                while (Alive) {
                    byte[] StreamData = new byte[1024];
                    int bytesRead = Server.GetStream().Read(StreamData, 0, StreamData.Length);                                                                              // Read data off the network
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
                                    AddUser(dData);
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
                Console.WriteLine("A user has disconnected for reason : " + e.ToString());
            }
        }

        public void Dispose() {
            Alive = false;
            Server.Close();
            Server = null;
        }
    }
}
