using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// Network Sync

namespace MistoxServer.Client {
    public class UDPClient {

        public List<Connection> Endpoints;
        public event EventHandler onReceived;

        UdpClient _udpClient;
        bool _alive;
        int _Port;

        public UDPClient(int Port) {
            Endpoints = new List<Connection>();

            // Setup of the socket
            _alive = true;
            _Port = Port;

            // Send and reply the connections that exist
            Thread Client = new Thread(ReceiveThread);
            Client.Start();
        }

        void ReceiveThread() {
            _udpClient = new UdpClient(_Port);
            IPEndPoint _anyone = new IPEndPoint(IPAddress.Any, _Port);
            while (_alive) {
                // [TypeLength, TypeName, Data
                byte[] packetData = _udpClient.Receive(ref _anyone);
                int typeLength = BitConverter.ToInt32(packetData);                                                                                                      // Gets the first 4 bytes off the data and puts it into the data length int
                Type dType = Type.GetType(Encoding.UTF8.GetString(packetData.Sub(4, typeLength)));                                                                      // Get the type of the data
                object x = mSerialize.Deserialize(packetData.Sub(typeLength + 4, packetData.Length - (typeLength + 4)), dType);                                         // Get the packet as the correct type sent
                onReceived.Invoke(x, new EventArgs());                                                                                                                  // Split out packet and send it up
            }
        }

        public void SendTo<Packet>(string User, Packet Data) {
            foreach (Connection cur in Endpoints) {
                if (cur.ID == User) {
                    byte[] typeName = Encoding.UTF8.GetBytes(typeof(Packet).FullName);
                    byte[] packetData = mSerialize.Serialize(Data);
                    byte[] typeLength = BitConverter.GetBytes(typeName.Length);
                    byte[] fullmsg = typeLength.Join(typeName).Join(packetData);
                    _udpClient.Send(fullmsg, fullmsg.Length, cur.udpClient);
                    break;
                }
            }
            Console.WriteLine("User {" + User.ToString() + "} not found");
        }

        public void SendAll<Packet>(Packet Data) {
            foreach (Connection cur in Endpoints) {
                byte[] typeName = Encoding.UTF8.GetBytes(typeof(Packet).FullName);
                byte[] packetData = mSerialize.Serialize(Data);
                byte[] typeLength = BitConverter.GetBytes(typeName.Length);
                byte[] fullmsg = typeLength.Join(typeName).Join(packetData);
                _udpClient.Send(fullmsg, fullmsg.Length, cur.udpClient);
            }
        }

        public void Dispose() {
            _alive = false;
            _udpClient.Close();
            _udpClient = null;
        }
    }
}
