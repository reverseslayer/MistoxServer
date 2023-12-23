using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Net.Sockets;

namespace MistoxServer {

    public class ConnectionRequestPacket {

        [mSerialize(TypeTag.String)]
        public string GUID;
		[mSerialize(TypeTag.String)]
		public string UserName;
		[mSerialize(TypeTag.String)]
		public string IPV4;
		[mSerialize(TypeTag.String)]
		public string IPV6;
		[mSerialize(TypeTag.Integer)]
		public int ClientPort;

        public ConnectionRequestPacket() {
            GUID = Guid.NewGuid().ToString();
            UserName = "";
            ClientPort = 0;
            IPV4 = ConnectionStatics.IPV4;
            IPV6 = ConnectionStatics.IPV6;
        }

    }

	public class Connection {
		public string ID;
		public string UserName;
		public IPEndPoint udpClient;
		public TcpClient tcpClient;
	}

	public class ConnectionStatics{

		static string _IPV4 = "";
		public static string IPV4{
			get{
				if (string.IsNullOrEmpty(_IPV4)){
					string tempv4 = "";
					Task ip4Thread = Task.Run(async () => {
						HttpClient client = new HttpClient();
						HttpResponseMessage response = await client.GetAsync("https://ipv4.icanhazip.com/");
						tempv4 = (await response.Content.ReadAsStringAsync()).TrimEnd();
					});
					ip4Thread.Wait();
					_IPV4 = tempv4;
				}
				return _IPV4;
			}
		}

		static string _IPV6 = "";
		public static string IPV6{
			get{
				if(string.IsNullOrEmpty( _IPV6)){
					string tempv6 = "";
					Task ip6Thread = Task.Run(async () => {
						HttpClient client = new HttpClient();
						HttpResponseMessage response = await client.GetAsync("https://ipv6.icanhazip.com/");
						tempv6 = (await response.Content.ReadAsStringAsync()).TrimEnd();
					});
					ip6Thread.Wait();
					_IPV6 = tempv6;
				}
				return _IPV6;
			}
		}
		
	}

}
