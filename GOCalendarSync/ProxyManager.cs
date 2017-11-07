using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace GOCalendarSync
{
    class ProxyManager : IDisposable
    {
        private const string TARGET_URL = "https://accounts.google.com/";
        private const string AUTH_TYPE = "Basic";

        private Uri _targetUrl = new Uri(TARGET_URL);
        private CredentialCache _proxyCredentialCache = new CredentialCache();
        private NetworkCredential _proxyCredential = null;
        private ICredentials _prevProxyCredentials;
        private int _pushCount = 0;

        public ProxyManager(string account, string password)
        {
            if ((string.IsNullOrEmpty(account) == false) || (string.IsNullOrEmpty(password) == false))
            {
                _proxyCredential = new NetworkCredential(account, password);
                var proxy = WebRequest.DefaultWebProxy.GetProxy(_targetUrl);
                var proxyHost = new Uri(proxy.AbsoluteUri);
                _proxyCredentialCache.Add(proxyHost, AUTH_TYPE, _proxyCredential);
            }
        }

        public void Apply()
        {
            _prevProxyCredentials = WebRequest.DefaultWebProxy.Credentials;
            //            WebRequest.DefaultWebProxy.Credentials = _proxyCredentialCache;
            WebRequest.DefaultWebProxy.Credentials = _proxyCredential;
            ++_pushCount;
        }

        public void Restore()
        {
            if (_pushCount > 0)
            {
                --_pushCount;
                if (_pushCount == 0)
                {
                    WebRequest.DefaultWebProxy.Credentials = _prevProxyCredentials;
                }
            }
        }

        #region IDisposable メンバー

        public void Dispose()
        {
            Restore();
        }

        #endregion
    }
}
