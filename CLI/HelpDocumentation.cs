using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MistoxHolePunch {
    class HelpDocumentation {

public static string HelpText = @"
-------------- Help Page for Mistox Game Server --------------

    Usage: MistoxHolePunch.exe ServerIP [Command] [Options]

        Command     Linux Style Command                             Meaning
        /c          -c                                              Start The Client
        /s          -s                                              Start The Server
        /q          -q                                              Stop The Server
        /r          -r                                              Restart The Server With The Current Settings

        /s -s [options]
            /p          -p                                          The port that will be used for the server [Includes {port} + 1]
            /u          -u                                          The username of the client

    Examples:
        MistoxHolePunch.exe /c 127.0.0.1 /p 25550 /u Mistox         Start the client connecting to server 127.0.0.1 using port 25550
        MistoxHolePunch.exe /c 127.0.0.1                            Start the client connecting to server 127.0.0.1 using port 25567
        MistoxHolePunch.exe /s                                      Start the server
";

    }
}
