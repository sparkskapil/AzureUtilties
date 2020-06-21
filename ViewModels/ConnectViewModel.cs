using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ViewModels.Helper;

namespace ViewModels
{
    public class ConnectViewModel : ViewModelBase
    {
        private string m_connectionUrl;
       
        public string ConnectionUrl
        {
            get { return m_connectionUrl; }
            set
            {
                m_connectionUrl = value;
                RaiseEvent();
            }
        }

        private Command m_connectCommand;

        public Command ConnectCommand
        {
            get {
                if (m_connectCommand == null)
                    m_connectCommand = new Command(Connect, CanConnect);
                return m_connectCommand;
            }
        }

        private bool CanConnect(object arg)
        {
            return Uri.IsWellFormedUriString(ConnectionUrl, UriKind.RelativeOrAbsolute);
        }

        public void Connect(object arg)
        {
            SessionManager.Instance.Reset(ConnectionUrl);
            SessionManager.Instance.StageSwitch(1);
        }
        public ConnectViewModel()
        {
            ConnectionUrl = "https://";
        }
    }
}
