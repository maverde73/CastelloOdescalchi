using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GestioneTornelloREA.Helpers
{
    public delegate void UDPServerEventHandler(object sender, string request);

    public class UDPServer
    {
        #region events
        public event UDPServerEventHandler RequestReceived = null;
        private void OnRequestReceived(object sender, string request)
        {
            if (RequestReceived != null)
                RequestReceived(sender, request);
        }

        #endregion

        #region property
        private UdpClient _serverSocket;
        private IPEndPoint _startPoint;
        private IPEndPoint _endPoint;
        public IPEndPoint EndPoint
        {
            set
            {
                _endPoint = value;
            }
            get
            {
                return _endPoint;
            }
        }

        private int _endPort=1001;
        public int EndPort
        {
            set
            {
                _endPort = value;
            }
            get
            {
                return _endPort;
            }
        }

        private int _startPort=1000;
        public int StartPort
        {
            set
            {
                _startPort = value;
            }
            get
            {
                return _startPort;
            }
        }

        private bool disposed { get; set; }

        private DateTime turnstileEPList;

        private bool isActive=false;

        #endregion

        public UDPServer()
        {
            _serverSocket = new UdpClient();
            disposed = false;
        }

        public void connect(int sport, int eport)
        {
            _startPort = sport;
            _endPort = eport;
            connect();
        }

        public void connect()
        {
            _startPoint = new IPEndPoint(IPAddress.Any, _startPort);
            _serverSocket = new UdpClient(_startPoint);
            //_serverSocket.EnableBroadcast = true;
            _serverSocket.BeginReceive(new AsyncCallback(this.GetRequest), null);
            disposed = false;
            isActive = true;
        }

        private void GetRequest(IAsyncResult ar)
        {
            if (!disposed)
            {
                isActive = false;
                //if (DateTime.Now.Subtract(dtPrev).TotalMilliseconds > 3000)
                if (DateTime.Now.Subtract(turnstileEPList).TotalMilliseconds > 3000 || turnstileEPList==null)
                {
                    turnstileEPList = DateTime.Now;
                    byte[] numArray = _serverSocket.EndReceive(ar, ref _endPoint);
                    if (numArray != null && _endPoint.Address.ToString() != _startPoint.Address.ToString())
                    {
                        _endPoint.Port = _endPort;
                        string str2 = Encoding.UTF8.GetString(numArray);
                        System.Diagnostics.Debug.Write(str2 + "\n");
                        OnRequestReceived(this, str2);
                        isActive = true;
                        _serverSocket.BeginReceive(new AsyncCallback(this.GetRequest), null);
                    }
                }
                if (!isActive)
                {
                    isActive = true;
                    _serverSocket.BeginReceive(new AsyncCallback(this.GetRequest), null);
                }

            }

        }

        public bool SendResponse(byte[] packet)
        {
            if (_serverSocket != null)
            {
                int num = 0;
                try
                {
                    System.Diagnostics.Debug.Write(Encoding.UTF8.GetString(packet) + "\n");
                    num = _serverSocket.Send(packet, (int)packet.Length, _endPoint);
                }
                catch
                {
                }
                if (num != (int)packet.Length)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public void Close()
        {
            if (_serverSocket != null)
            {
                _serverSocket.Close();
                disposed = true;
                //this.Dispose();
            }
        }

        public void Dispose()
        {
            ((IDisposable)_serverSocket).Dispose();
        }



    }
}

