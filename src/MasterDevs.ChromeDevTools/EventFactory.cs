using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace MasterDevs.ChromeDevTools
{
    public class EventFactory : IEventFactory
    {
        private readonly IMethodTypeMap _methodTypeMap;

        public EventFactory(IMethodTypeMap methodTypeMap)
        {
            _methodTypeMap = methodTypeMap;
        }

        public IEvent Create(byte[] responseBytes)
        {
            return Create(Encoding.UTF8.GetString(responseBytes));
        }

        public IEvent Create(string responseText)
        {
            var jObject = JObject.Parse(responseText);
            var methodString = jObject["method"].GetSafeString();
            if (null == methodString)
            {
                return null;
            }
            var typeInferredFromMethod = _methodTypeMap.GetEvent(methodString);
            if (null == typeInferredFromMethod)
            {
                return null;
            }
            var genericEventType = typeof(Event<>);
            var commandResponseType = genericEventType.MakeGenericType(typeInferredFromMethod);
            var result = jObject.ToObject(commandResponseType);
            return result as IEvent;
        }
    }
}