using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HttpWebReqInterceptor
{
    internal sealed class ConnectionArrayList : ArrayListWrapper
    {
        private readonly ServicePointContext context;
        private readonly IHttpWebRequestHandler handler;
        public ConnectionArrayList(ArrayList list, ServicePointContext servicePointContext, IHttpWebRequestHandler httpHandler)
            : base(list)
        {
            context = servicePointContext;
            handler = httpHandler;
        }

        public override int Add(object value)
        {
            if (context.ConnectionType.IsInstanceOfType(value))
            {
                // Replace the HttpWebRequest arraylist inside this Connection object,
                // which allows us to intercept each new HttpWebRequest object added under
                // this Connection.
                ArrayList originalArrayList = context.WriteListField.GetValue(value) as ArrayList;
                HttpWebRequestArrayList newArrayList = new HttpWebRequestArrayList(originalArrayList ?? new ArrayList(), handler);

                context.WriteListField.SetValue(value, newArrayList);
            }

            return base.Add(value);
        }
    }
}
