using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace WamlDemos
{
    public class WpfAppTracingInterceptor : Microsoft.WindowsAzure.ICloudTracingInterceptor
    {
        private Action<string> _logAction;

        public WpfAppTracingInterceptor(Action<string> logAction)
        {
            _logAction = logAction;
        }

        private void Write(string message, params object[] arguments)
        {
            _logAction(string.Format(message, arguments));
        }

        public void Information(string message)
        {
            Write(message);
        }

        public void Configuration(string source, string name, string value)
        {
            Write("Configuration(" + source + "): " + name + " = " + value);
        }

        public void Enter(string invocationId, object instance, string method, IDictionary<string, object> parameters)
        {
            Write("{0}: Enter {1}({4}) on 0x{3:X}:{2}",
                invocationId,
                method,
                instance,
                instance.GetHashCode(),
                string.Join(
                    ", ",
                    parameters.Select(p => p.Key + "=" + p.Value.ToString())));
        }

        public void SendRequest(string invocationId, HttpRequestMessage request)
        {
            Write("{0}: SendRequst {1}", invocationId, request.ToString());
        }

        public void ReceiveResponse(string invocationId, HttpResponseMessage response)
        {
            Write("{0}: ReceiveResponse {1}", invocationId, response.ToString());
        }

        public void Error(string invocationId, Exception ex)
        {
            Write("{0}: Error {1}", invocationId, ex.ToString());
        }

        public void Exit(string invocationId, object result)
        {
            Write("{0}: Exit {1}", invocationId, result);
        }
    }
}
