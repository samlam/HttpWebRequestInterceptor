using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace HttpWebReqInterceptor
{
    public class HttpWebRequestInterceptor
    {
        // Fields for reflection
        private static FieldInfo connectionGroupListField;
        private static Type connectionGroupType;
        private static FieldInfo connectionListField;
        private static Type connectionType;
        private static FieldInfo writeListField;
        private static Func<object, IAsyncResult> writeAResultAccessor;
        private static Func<object, IAsyncResult> readAResultAccessor;

        // LazyAsyncResult & ContextAwareResult
        private static Func<object, AsyncCallback> asyncCallbackAccessor;
        private static Action<object, AsyncCallback> asyncCallbackModifier;
        private static Func<object, object> asyncStateAccessor;
        private static Action<object, object> asyncStateModifier;
        private static Func<object, bool> endCalledAccessor;
        private static Func<object, object> resultAccessor;
        private static Func<object, bool> isContextAwareResultChecker;

        // HttpWebResponse
        private static Func<object[], HttpWebResponse> httpWebResponseCtor;
        private static Func<HttpWebResponse, Uri> uriAccessor;
        private static Func<HttpWebResponse, object> verbAccessor;
        private static Func<HttpWebResponse, string> mediaTypeAccessor;
        private static Func<HttpWebResponse, bool> usesProxySemanticsAccessor;
        private static Func<HttpWebResponse, object> coreResponseDataAccessor;
        private static Func<HttpWebResponse, bool> isWebSocketResponseAccessor;
        private static Func<HttpWebResponse, string> connectionGroupNameAccessor;

        private static IHttpWebRequestHandler handler;

        public HttpWebRequestInterceptor(IHttpWebRequestHandler httpWebRequestHandler)
        {
            handler = httpWebRequestHandler;
            try
            {
                // This flag makes sure we only do this once. Even if we failed to initialize in an
                // earlier time, we should not retry because this initialization is not cheap and
                // the likelihood it will succeed the second time is very small.
                //this.initialized = true;

                PrepareReflectionObjects();
                PerformInjection();
            }
            catch (Exception ex)
            {
                // If anything went wrong, just no-op. Write an event so at least we can find out.

                //this.Write(InitializationFailed, new { Exception = ex });
                throw;
            }
        }


        private static void PrepareReflectionObjects()
        {
            // At any point, if the operation failed, it should just throw. The caller should catch all exceptions and swallow.

            Type servicePointType = typeof(ServicePoint);
            Assembly systemNetHttpAssembly = servicePointType.Assembly;
            connectionGroupListField = servicePointType.GetField("m_ConnectionGroupList", BindingFlags.Instance | BindingFlags.NonPublic);
            connectionGroupType = systemNetHttpAssembly?.GetType("System.Net.ConnectionGroup");
            connectionListField = connectionGroupType?.GetField("m_ConnectionList", BindingFlags.Instance | BindingFlags.NonPublic);
            connectionType = systemNetHttpAssembly?.GetType("System.Net.Connection");
            writeListField = connectionType?.GetField("m_WriteList", BindingFlags.Instance | BindingFlags.NonPublic);

            writeAResultAccessor = CreateFieldGetter<IAsyncResult>(typeof(HttpWebRequest), "_WriteAResult", BindingFlags.NonPublic | BindingFlags.Instance);
            readAResultAccessor = CreateFieldGetter<IAsyncResult>(typeof(HttpWebRequest), "_ReadAResult", BindingFlags.NonPublic | BindingFlags.Instance);

            // Double checking to make sure we have all the pieces initialized
            if (connectionGroupListField == null ||
                connectionGroupType == null ||
                connectionListField == null ||
                connectionType == null ||
                writeListField == null ||
                writeAResultAccessor == null ||
                readAResultAccessor == null ||
                !PrepareAsyncResultReflectionObjects(systemNetHttpAssembly) ||
                !PrepareHttpWebResponseReflectionObjects(systemNetHttpAssembly))
            {
                // If anything went wrong here, just return false. There is nothing we can do.
                throw new InvalidOperationException("Unable to initialize all required reflection objects");
            }

        }

        private static void PerformInjection()
        {
            FieldInfo servicePointTableField = typeof(ServicePointManager).GetField("s_ServicePointTable", BindingFlags.Static | BindingFlags.NonPublic);
            if (servicePointTableField == null)
            {
                // If anything went wrong here, just return false. There is nothing we can do.
                throw new InvalidOperationException("Unable to access the ServicePointTable field");
            }

            Hashtable originalTable = servicePointTableField.GetValue(null) as Hashtable;
            ServicePointHashtable newTable = new ServicePointHashtable(originalTable ?? new Hashtable(), new ServicePointContext
            {
                ConnectionGroupListField = connectionGroupListField,
                ConnectionGroupType = connectionGroupType,
                ConnectionListField = connectionListField,
                ConnectionType = connectionType,
                WriteListField = writeListField
            }
            , httpHandler: handler);

            servicePointTableField.SetValue(null, newTable);
        }

















        /// <summary>
        /// Creates getter for a field defined in private or internal type
        /// repesented with classType variable.
        /// </summary>
        private static Func<object, TField> CreateFieldGetter<TField>(Type classType, string fieldName, BindingFlags flags)
        {
            FieldInfo field = classType.GetField(fieldName, flags);
            if (field != null)
            {
                string methodName = classType.FullName + ".get_" + field.Name;
                DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(TField), new[] { typeof(object) }, true);
                ILGenerator generator = getterMethod.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Castclass, classType);
                generator.Emit(OpCodes.Ldfld, field);
                generator.Emit(OpCodes.Ret);

                return (Func<object, TField>)getterMethod.CreateDelegate(typeof(Func<object, TField>));
            }

            return null;
        }

        private static Func<TClass, TField> CreateFieldGetter<TClass, TField>(string fieldName, BindingFlags flags) where TClass : class
        {
            FieldInfo field = typeof(TClass).GetField(fieldName, flags);
            if (field != null)
            {
                string methodName = field.ReflectedType.FullName + ".get_" + field.Name;
                DynamicMethod getterMethod = new DynamicMethod(methodName, typeof(TField), new[] { typeof(TClass) }, true);
                ILGenerator generator = getterMethod.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, field);
                generator.Emit(OpCodes.Ret);
                return (Func<TClass, TField>)getterMethod.CreateDelegate(typeof(Func<TClass, TField>));
            }

            return null;
        }


        /// <summary>
        /// Creates setter for a field defined in private or internal type
        /// repesented with classType variable.
        /// </summary>
        private static Action<object, TField> CreateFieldSetter<TField>(Type classType, string fieldName, BindingFlags flags)
        {
            FieldInfo field = classType.GetField(fieldName, flags);
            if (field != null)
            {
                string methodName = classType.FullName + ".set_" + field.Name;
                DynamicMethod setterMethod = new DynamicMethod(methodName, null, new[] { typeof(object), typeof(TField) }, true);
                ILGenerator generator = setterMethod.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Castclass, classType);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Stfld, field);
                generator.Emit(OpCodes.Ret);

                return (Action<object, TField>)setterMethod.CreateDelegate(typeof(Action<object, TField>));
            }

            return null;
        }

        /// <summary>
        /// Creates an "is" method for the private or internal type.
        /// </summary>
        private static Func<object, bool> CreateTypeChecker(Type classType)
        {
            string methodName = classType.FullName + ".typeCheck";
            DynamicMethod setterMethod = new DynamicMethod(methodName, typeof(bool), new[] { typeof(object) }, true);
            ILGenerator generator = setterMethod.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Isinst, classType);
            generator.Emit(OpCodes.Ldnull);
            generator.Emit(OpCodes.Cgt_Un);
            generator.Emit(OpCodes.Ret);

            return (Func<object, bool>)setterMethod.CreateDelegate(typeof(Func<object, bool>));
        }

        /// <summary>
        /// Creates an instance of T using a private or internal ctor.
        /// </summary>
        private static Func<object[], T> CreateTypeInstance<T>(ConstructorInfo constructorInfo)
        {
            Type classType = typeof(T);
            string methodName = classType.FullName + ".ctor";
            DynamicMethod setterMethod = new DynamicMethod(methodName, classType, new Type[] { typeof(object[]) }, true);
            ILGenerator generator = setterMethod.GetILGenerator();

            ParameterInfo[] ctorParams = constructorInfo.GetParameters();
            for (int i = 0; i < ctorParams.Length; i++)
            {
                generator.Emit(OpCodes.Ldarg_0);
                switch (i)
                {
                    case 0: generator.Emit(OpCodes.Ldc_I4_0); break;
                    case 1: generator.Emit(OpCodes.Ldc_I4_1); break;
                    case 2: generator.Emit(OpCodes.Ldc_I4_2); break;
                    case 3: generator.Emit(OpCodes.Ldc_I4_3); break;
                    case 4: generator.Emit(OpCodes.Ldc_I4_4); break;
                    case 5: generator.Emit(OpCodes.Ldc_I4_5); break;
                    case 6: generator.Emit(OpCodes.Ldc_I4_6); break;
                    case 7: generator.Emit(OpCodes.Ldc_I4_7); break;
                    case 8: generator.Emit(OpCodes.Ldc_I4_8); break;
                    default: generator.Emit(OpCodes.Ldc_I4, i); break;
                }

                generator.Emit(OpCodes.Ldelem_Ref);
                Type paramType = ctorParams[i].ParameterType;
                generator.Emit(paramType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, paramType);
            }

            generator.Emit(OpCodes.Newobj, constructorInfo);
            generator.Emit(OpCodes.Ret);

            return (Func<object[], T>)setterMethod.CreateDelegate(typeof(Func<object[], T>));
        }


























        private static bool PrepareAsyncResultReflectionObjects(Assembly systemNetHttpAssembly)
        {
            Type lazyAsyncResultType = systemNetHttpAssembly?.GetType("System.Net.LazyAsyncResult");
            if (lazyAsyncResultType != null)
            {
                asyncCallbackAccessor = CreateFieldGetter<AsyncCallback>(lazyAsyncResultType, "m_AsyncCallback", BindingFlags.NonPublic | BindingFlags.Instance);
                asyncCallbackModifier = CreateFieldSetter<AsyncCallback>(lazyAsyncResultType, "m_AsyncCallback", BindingFlags.NonPublic | BindingFlags.Instance);
                asyncStateAccessor = CreateFieldGetter<object>(lazyAsyncResultType, "m_AsyncState", BindingFlags.NonPublic | BindingFlags.Instance);
                asyncStateModifier = CreateFieldSetter<object>(lazyAsyncResultType, "m_AsyncState", BindingFlags.NonPublic | BindingFlags.Instance);
                endCalledAccessor = CreateFieldGetter<bool>(lazyAsyncResultType, "m_EndCalled", BindingFlags.NonPublic | BindingFlags.Instance);
                resultAccessor = CreateFieldGetter<object>(lazyAsyncResultType, "m_Result", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            Type contextAwareResultType = systemNetHttpAssembly?.GetType("System.Net.ContextAwareResult");
            if (contextAwareResultType != null)
            {
                isContextAwareResultChecker = CreateTypeChecker(contextAwareResultType);
            }

            return asyncCallbackAccessor != null
                && asyncCallbackModifier != null
                && asyncStateAccessor != null
                && asyncStateModifier != null
                && endCalledAccessor != null
                && resultAccessor != null
                && isContextAwareResultChecker != null;
        }

        private static bool PrepareHttpWebResponseReflectionObjects(Assembly systemNetHttpAssembly)
        {
            Type knownHttpVerbType = systemNetHttpAssembly?.GetType("System.Net.KnownHttpVerb");
            Type coreResponseData = systemNetHttpAssembly?.GetType("System.Net.CoreResponseData");

            if (knownHttpVerbType != null && coreResponseData != null)
            {
                var constructorParameterTypes = new Type[]
                {
                    typeof(Uri), knownHttpVerbType, coreResponseData, typeof(string),
                    typeof(bool), typeof(DecompressionMethods),
                    typeof(bool), typeof(string),
                };

                ConstructorInfo ctor = typeof(HttpWebResponse).GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    constructorParameterTypes,
                    null);

                if (ctor != null)
                {
                    httpWebResponseCtor = CreateTypeInstance<HttpWebResponse>(ctor);
                }
            }

            uriAccessor = CreateFieldGetter<HttpWebResponse, Uri>("m_Uri", BindingFlags.NonPublic | BindingFlags.Instance);
            verbAccessor = CreateFieldGetter<HttpWebResponse, object>("m_Verb", BindingFlags.NonPublic | BindingFlags.Instance);
            mediaTypeAccessor = CreateFieldGetter<HttpWebResponse, string>("m_MediaType", BindingFlags.NonPublic | BindingFlags.Instance);
            usesProxySemanticsAccessor = CreateFieldGetter<HttpWebResponse, bool>("m_UsesProxySemantics", BindingFlags.NonPublic | BindingFlags.Instance);
            coreResponseDataAccessor = CreateFieldGetter<HttpWebResponse, object>("m_CoreResponseData", BindingFlags.NonPublic | BindingFlags.Instance);
            isWebSocketResponseAccessor = CreateFieldGetter<HttpWebResponse, bool>("m_IsWebSocketResponse", BindingFlags.NonPublic | BindingFlags.Instance);
            connectionGroupNameAccessor = CreateFieldGetter<HttpWebResponse, string>("m_ConnectionGroupName", BindingFlags.NonPublic | BindingFlags.Instance);

            return httpWebResponseCtor != null
                && uriAccessor != null
                && verbAccessor != null
                && mediaTypeAccessor != null
                && usesProxySemanticsAccessor != null
                && coreResponseDataAccessor != null
                && isWebSocketResponseAccessor != null
                && connectionGroupNameAccessor != null;
        }

    }

}
