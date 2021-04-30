using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Samples.Kinect.ColorBasics
{
    class socketUser
    {
        //public static int port = 0;
        public NetworkStream stream;
        public Socket remoteClient;
        public BinaryReader br;
        public BinaryWriter bw;

        public socketUser(Socket socketclient)
        {
            remoteClient = socketclient;
            stream = new NetworkStream(remoteClient);
            br = new BinaryReader(stream);
            bw = new BinaryWriter(stream);
        }

        public void Close()
        {
            br.Close();
            bw.Close();
            remoteClient.Close();
        }
    }
}
