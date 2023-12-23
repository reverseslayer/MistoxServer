using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using MistoxServer.Client;
using MistoxServer.Server;

namespace MistoxServer {
    public partial class mServer {
        public static IMistoxServer newServer() {
            Console.WriteLine("Initilizing server");
            return new ServerInterface();
        }

        public static IMistoxServer newClient(string ServerIPOrHostName, int ClientPort, string UserName) {
            int index = 0;
            IPAddress[] x = Dns.GetHostAddresses(ServerIPOrHostName);
            for( int i=0; i<x.Length; i++ ) {
                if( x[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ) {
                    index = i;
                }
            }
            string ipAddress = x[index].ToString();
            if (x.Length > 0) {
                Console.WriteLine("Initilizing connection to server at IP : " + ipAddress);
                return new ClientInterface(ipAddress, ClientPort, UserName);
            } else {
                Console.WriteLine("The server at " + ServerIPOrHostName + " doesn't exit or cannot be found");
                return null;
            }
        }  
    }

    public interface IMistoxServer {

        public event EventHandler onReceive;
        public void SendToServer<Packet>(Packet data);
        public void SendToUser<Packet>(string user, Packet data);
        public void SendToAllUsers<Packet>(Packet data);

    }

}
