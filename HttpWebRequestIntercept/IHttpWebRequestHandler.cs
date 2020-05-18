using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace HttpWebReqInterceptor
{
    public interface IHttpWebRequestHandler
    {
        void Add(HttpWebRequest httpWebRequest);
    }
}
