using MistoxServer.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MistoxServer {
    public class ClientInterface : IMistoxServer {

        TCPClient SlowUpdateClient;
        UDPClient FastUpdate;

        public event EventHandler onReceive;

        void onClientReceived(object packet, EventArgs e) {
            onReceive.Invoke(packet, e);
        }

        public ClientInterface(string ServerIP, int ClientPort, string UserName) {

            // Create the web network UDP client
            FastUpdate = new UDPClient(ClientPort);
            FastUpdate.onReceived += onClientReceived;

            // Make a connection to the server
            try {
                SlowUpdateClient = new TCPClient(ServerIP, ClientPort, UserName, FastUpdate);
                SlowUpdateClient.onReceived += onClientReceived;
            }catch(Exception e) {
                Console.WriteLine("An error has occured with the connection to the server. Error { ");
                Console.WriteLine(e.ToString());
                Console.WriteLine("}");
            }

            Console.WriteLine("The client is initilized and trying to connect to the server at ip : " + ServerIP);
        }

        public void SendToServer<Packet>(Packet data) {
            SlowUpdateClient.Send(data);
        }

        public void SendToUser<Packet>(string user, Packet data) {
            FastUpdate.SendTo(user, data);
        }

        public void SendToAllUsers<Packet>(Packet data) {
            FastUpdate.SendAll(data);
        }
    }
}
