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
    /// Helper class used for ServicePointManager.s_ServicePointTable. The goal here is to
    /// intercept each new ServicePoint object being added to ServicePointManager.s_ServicePointTable
    /// and replace its ConnectionGroupList hashtable field.
    /// </summary>
    internal sealed class ServicePointHashtable : HashtableWrapper
    {
        private readonly ServicePointContext context;
        private readonly IHttpWebRequestHandler handler;

        public ServicePointHashtable(Hashtable table, ServicePointContext servicePointContext, IHttpWebRequestHandler httpHandler)
            : base(table)
        {
            this.context = servicePointContext;
            this.handler = httpHandler;
        }

        public override object this[object key]
        {
            get => base[key];
            set
            {
                if (value is WeakReference weakRef && weakRef.IsAlive)
                {
                    if (weakRef.Target is ServicePoint servicePoint)
                    {
                        // Replace the ConnectionGroup hashtable inside this ServicePoint object,
                        // which allows us to intercept each new ConnectionGroup object added under
                        // this ServicePoint.
                        Hashtable originalTable = context.ConnectionGroupListField.GetValue(servicePoint) as Hashtable;
                        ConnectionGroupHashtable newTable = new ConnectionGroupHashtable(originalTable ?? new Hashtable(), context, handler);

                        context.ConnectionGroupListField.SetValue(servicePoint, newTable);
                    }
                }

                base[key] = value;
            }
        }
    }

}
