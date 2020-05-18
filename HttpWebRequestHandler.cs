using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HttpWebReqInterceptor
{
    public class HttpWebRequestHandler: IHttpWebRequestHandler
    {
        string newUrl;

        public HttpWebRequestHandler(string mockServerUrl)
        {
            newUrl = mockServerUrl;
        }

        void IHttpWebRequestHandler.Add(HttpWebRequest request)
        {
            //check feature toggle here

            Uri newUri = new Uri(newUrl);

            request.GetType().InvokeMember("_OriginUri"
                , BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField
                , null
                , request
                , new object[] { newUri });

            request.GetType().InvokeMember("_Uri"
                , BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField
                , null
                , request
                , new object[] { newUri });

            typeof(WebRequest).InvokeMember("SetupCacheProtocol"
                , BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod
                , null
                , request
                , new object[] { newUri });
        }
    }
}
