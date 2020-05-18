using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HttpWebReqInterceptor
{
    /// <summary>
    /// Helper class used for Connection.m_WriteList. The goal here is to
    /// intercept all new HttpWebRequest objects being added to Connection.m_WriteList
    /// and notify the listener about the HttpWebRequest that's about to send a request.
    /// It also intercepts all HttpWebRequest objects that are about to get removed from
    /// Connection.m_WriteList as they have completed the request.
    /// </summary>
    public sealed class HttpWebRequestArrayList : ArrayListWrapper
    {
        private readonly IHttpWebRequestHandler handler;

        public HttpWebRequestArrayList(ArrayList list, IHttpWebRequestHandler handler)
            : base(list)
        {
            this.handler = handler;
        }

        public override int Add(object value)
        {
            // Add before firing events so if some user code cancels/aborts the request it will be found in the outstanding list.
            var index = base.Add(value);

            if (value is HttpWebRequest request)
            {
                //Instance.RaiseRequestEvent(request);
                handler.Add(request);
            }

            return index;
        }

        public override void RemoveAt(int index)
        {
            object request = this[index];

            base.RemoveAt(index);

            if (request is HttpWebRequest webRequest)
            {
                //HookOrProcessResult(webRequest);
            }
        }

        public override void Clear()
        {
            ArrayList oldList = this.Swap();
            for (int i = 0; i < oldList.Count; i++)
            {
                if (oldList[i] is HttpWebRequest request)
                {
                   // HookOrProcessResult(request);
                }
            }
        }
    }
}
