using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace PyDoodle
{
    class NetReceiver
    {
        public delegate void OnPinged(object sender, EventArgs e);

        public event OnPinged Pinged;

        private Form _form;
        private int _port;
        private Socket _listenSocket;

        public NetReceiver(Form form, int port)
        {
            _form = form;
            _port = port;

            _listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            _listenSocket.Bind(new IPEndPoint(IPAddress.Any, port));
            _listenSocket.BeginAccept(this.OnAccept, null);
        }

        private void OnAccept(IAsyncResult ar)
        {
            Socket socket = _listenSocket.EndAccept(ar);

            socket.Close();

            _form.BeginInvoke(new EventHandler(this.PingedForwarder), null);
        }

        private void PingedForwarder(object sender, EventArgs e)
        {
            if (this.Pinged != null)
                this.Pinged(this, null);
        }
    }
}
