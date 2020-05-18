using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HttpWebReqInterceptor
{
    public class ServicePointContext
    {
        public Type ConnectionType { get; set; }
        public FieldInfo WriteListField { get; set; }
        public Type ConnectionGroupType { get; set; }
        public FieldInfo ConnectionListField { get; set; }
        public FieldInfo ConnectionGroupListField { get; set; }

    }
}
