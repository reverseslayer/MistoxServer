﻿using MistoxServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MistoxServer {
    public class ServerInterface : IMistoxServer {

        public TCPServer SlowUpdateServer;

        public event EventHandler onReceive;

        public ServerInterface() {
            SlowUpdateServer = new TCPServer();
            SlowUpdateServer.onReceived += (object packet, EventArgs e) => {
                onReceive.Invoke(packet, e);
            };
            Console.WriteLine( "The server initilized successfully and is awaiting connections at " + ConnectionStatics.IPV4 + " & " + ConnectionStatics.IPV6 );
        }

        public void SendToServer<Packet>(Packet data) {
            onReceive(data, new EventArgs());
        }

        public void SendToUser<Packet>(string user, Packet data) {
            SlowUpdateServer.SendTo(user, data);
        }

        public void SendToAllUsers<Packet>(Packet data) {
            SlowUpdateServer.SendAll(data);
        }

    }
}
