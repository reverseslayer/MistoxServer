using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MistoxServer {

    public class ConnectionRequestPacket {

        public Guid GUID {
            get; set;
        }

        public string UserName {
            get; set;
        }

        public string LocalIP {
            get; set;
        } = new WebClient().DownloadString("https://ipv4.icanhazip.com/").TrimEnd();

        public int ClientPort {
            get; set;
        }
    }

}
