using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestioneTornelloREA.Helpers
{
    public delegate void TerminalEventHandler(object sender, string request);
    public class TerminalHelper
    {
        #region events

        public event TerminalEventHandler OnTestReceived = null;
        private void _onTestReceived(object sender, string request)
        {
            if (OnTestReceived != null)
                OnTestReceived(sender, request);
        }

        public event TerminalEventHandler OnEntryReceived = null;
        private void _onEntryReceived(object sender, string request)
        {
            if (OnEntryReceived != null)
                OnEntryReceived(sender, request);
        }

        public event TerminalEventHandler OnEventReceived = null;
        private void _onEventReceived(object sender, string request)
        {
            if (OnEventReceived != null)
                OnEventReceived(sender, request);
        }

        public event TerminalEventHandler OnPassReceived = null;
        private void _onPassReceived(object sender, string request)
        {
            if (OnPassReceived != null)
                OnPassReceived(sender, request);
        }

        public event TerminalEventHandler OnReturnReceived = null;
        private void _onReturnReceived(object sender, string request)
        {
            if (OnReturnReceived != null)
                OnReturnReceived(sender, request);
        }

        #endregion

        #region properties

        private UDPServer _terminal;
        public UDPServer Terminal
        {
            set { _terminal = value; }
            get { return _terminal; }
        }

        private string _transactionId;
        public string TansactionId
        {
            set { _transactionId = value; }
            get { return _transactionId; }
        }


        #endregion

        #region methods

        public TerminalHelper()
        {
            _terminal = new UDPServer();
        }

        public void ListOnPort(int port)
        {
            _terminal.StartPort = port;
            _terminal.RequestReceived += _terminal_RequestReceived;
            _terminal.connect();
        }

        public void ListOnPort()
        {
            _terminal.RequestReceived += _terminal_RequestReceived;
            _terminal.connect();
        }

        private void _terminal_RequestReceived(object sender, string request)
        {
            _transactionId = DataStructure.GetAttributeFromNode(request, "cmf", "id");

            if (DataStructure.NodeHasChild(request, "cmf", "test"))
            {
                _onTestReceived(this, DataStructure.GetInnerFromNode(request, @"//cmf/test", "time"));
            }
            else if (DataStructure.NodeHasChild(request, "cmf", "entry"))
            {
                _onEntryReceived(this, DataStructure.GetInnerFromNode(request, @"//cmf/entry", "chip"));
            }
            else if (DataStructure.NodeHasChild(request, "cmf", "event"))
            {
                _onEventReceived(this, DataStructure.GetInnerFromNode(request, @"//cmf/event", "code"));
            }
            else if (DataStructure.NodeHasChild(request, "cmf", "pass"))
            {
                _onPassReceived(this, DataStructure.GetInnerFromNode(request, @"//cmf/pass", "act"));
            }
            else if (DataStructure.NodeHasChild(request, "cmf", "return"))
            {
                _onReturnReceived(this, DataStructure.GetInnerFromNode(request, "cmf", "return"));
            }

        }

        public void SendOk()
        {
            _terminal.SendResponse(DataStructure.GetResponseTest(_transactionId));
        }

        public void SendResponseEntry(string direction,string message,string passage)
        {
            _terminal.SendResponse(DataStructure.GetResponseEntry(_transactionId, direction, message, passage));
        }

        public void SendResponsePass()
        {
            _terminal.SendResponse(DataStructure.GetResponsePass(_transactionId));
        }

        public void SendRequestOpen(string direction, string message, string passage)
        {
            _transactionId = (int.Parse(_transactionId) + 1).ToString();
            _terminal.SendResponse(DataStructure.GetRequestOpen(_transactionId, direction, message, passage));
        }

        public void StopListen()
        {
            Terminal.Close();
        }
        #endregion


    }
}

