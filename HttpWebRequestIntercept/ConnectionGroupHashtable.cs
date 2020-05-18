using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HttpWebReqInterceptor
{
    /// <summary>
    /// Helper class used for ServicePoint.m_ConnectionGroupList. The goal here is to
    /// intercept each new ConnectionGroup object being added to ServicePoint.m_ConnectionGroupList
    /// and replace its m_ConnectionList arraylist field.
    /// </summary>
    internal sealed class ConnectionGroupHashtable : HashtableWrapper
    {
        private readonly ServicePointContext context;
        private readonly IHttpWebRequestHandler handler;

        public ConnectionGroupHashtable(Hashtable table, ServicePointContext servicePointContext, IHttpWebRequestHandler httpHandler)
            : base(table)
        {
            this.context = servicePointContext;
            handler = httpHandler;
        }

        public override object this[object key]
        {
            get => base[key];
            set
            {
                if (context.ConnectionGroupType.IsInstanceOfType(value))
                {
                    // Replace the Connection arraylist inside this ConnectionGroup object,
                    // which allows us to intercept each new Connection object added under
                    // this ConnectionGroup.
                    ArrayList originalArrayList = context.ConnectionListField.GetValue(value) as ArrayList;
                    ConnectionArrayList newArrayList = new ConnectionArrayList(originalArrayList ?? new ArrayList(), context, handler);

                    context.ConnectionListField.SetValue(value, newArrayList);
                }

                base[key] = value;
            }
        }
    }
}
