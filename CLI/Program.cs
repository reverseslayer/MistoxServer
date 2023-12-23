using System;
using System.Collections.Generic;
using MistoxServer;
using System.Runtime.InteropServices;

namespace MistoxHolePunch {
    class Program {

        static IMistoxServer serverObj;

        static void Main(string[] args) {
            string Task = args.Length > 0 ? args[0].ToLower() : null;

            if (Task == "/?" || Task == "--help") {
                Console.WriteLine(HelpDocumentation.HelpText);
            } else if (Task == "/s" || Task == "-s") {
                Console.Clear();
                serverObj = mServer.newServer();  
            } else if (Task == "/c" || Task == "-c") {
                Console.Clear();
                string IPAddress = args[1];
                int Port = 25567;
                string UserName = "username";
                if (args.Length >= 2) {
                    for (int i = 0; i < args.Length; i++) {
                        string arg = args[i];
                        if (arg == "/p" || arg == "-p") {
                            Port = Convert.ToInt32(args[i + 1]);
                        } else if (arg == "/u" || arg == "-u") {
                            UserName = args[i + 1];
                        }
                    }
                }
                serverObj = mServer.newClient(IPAddress, Port, UserName);
            } else if (Task == "/q" || Task == "-q") {
                Console.WriteLine("The server or client has been stopped successfully");
            } else if (Task == "/r" || Task == "-r") {
                // Need to save args and then stop and start the server using old args
                
            } else {
                Console.WriteLine( "If you need help please type MistServer /?" );
                //serverObj = mServer.newClient("mistox.net", 25567, "Mistox");
                //serverObj = mServer.newServer();

                Console.WriteLine( ConnectionStatics.IPV4 );
            }
        }

    }

}