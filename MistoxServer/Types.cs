using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MistoxServer {

    public class Connection {
        public Guid ID;
        public string UserName;
        public IPEndPoint udpClient;
        public TcpClient tcpClient;
    }

    static class Extensions {
        public static T[] Join<T>(this T[] first, T[] second) {
            T[] bytes = new T[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, bytes, 0, first.Length);
            Buffer.BlockCopy(second, 0, bytes, first.Length, second.Length);
            return bytes;
        }

        public static T[] Sub<T>(this T[] data, int index, int length) {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}
