using System;
using ElMariachi.Http.Header;

namespace ElMariachi.Http.Server.Services
{
    public class DefaultResponseHeadersSetter : IDefaultResponseHeadersSetter
    {
        public void Set(IHttpResponseHeaders headers)
        {
            headers.Date.Value = DateTime.UtcNow;
            headers.Server = "ElMariachi";
        }
    }
}