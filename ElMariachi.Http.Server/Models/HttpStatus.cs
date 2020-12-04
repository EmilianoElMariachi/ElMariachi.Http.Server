namespace ElMariachi.Http.Server.Models
{
    public class HttpStatus
    {

        public HttpStatus(int code, string description)
        {
            Code = code;
            Description = description;
        }

        public int Code { get; }

        public string Description { get; }


        public static HttpStatus Continue { get; } = new HttpStatus(100, "Continue");
        public static HttpStatus SwitchingProtocols { get; } = new HttpStatus(101, "Switching Protocols");


        public static HttpStatus Ok { get; } = new HttpStatus(200, "OK");
        public static HttpStatus Created { get; } = new HttpStatus(201, "Created");
        public static HttpStatus Accepted { get; } = new HttpStatus(202, "Accepted");
        public static HttpStatus NonAuthoritativeInformation { get; } = new HttpStatus(203, "Non-Authoritative Information");
        public static HttpStatus NoContent { get; } = new HttpStatus(204, "No Content");
        public static HttpStatus ResetContent { get; } = new HttpStatus(205, "Reset Content");
        public static HttpStatus PartialContent { get; } = new HttpStatus(206, "Partial Content");


        public static HttpStatus MultipleChoices { get; } = new HttpStatus(300, "Multiple Choices");
        public static HttpStatus MovedPermanently { get; } = new HttpStatus(301, "Moved Permanently");
        public static HttpStatus Found { get; } = new HttpStatus(302, "Found");
        public static HttpStatus SeeOther { get; } = new HttpStatus(303, "See Other");
        public static HttpStatus NotModified { get; } = new HttpStatus(304, "Not Modified");
        public static HttpStatus UseProxy { get; } = new HttpStatus(305, "Use Proxy");
        public static HttpStatus TemporaryRedirect { get; } = new HttpStatus(307, "Temporary Redirect");


        public static HttpStatus BadRequest { get; } = new HttpStatus(400, "Bad Request");
        public static HttpStatus Unauthorized { get; } = new HttpStatus(401, "Unauthorized");
        public static HttpStatus PaymentRequired { get; } = new HttpStatus(402, "Payment Required");
        public static HttpStatus Forbidden { get; } = new HttpStatus(403, "Forbidden");
        public static HttpStatus NotFound { get; } = new HttpStatus(404, "Not Found");
        public static HttpStatus MethodNotAllowed { get; } = new HttpStatus(405, "Method Not Allowed");
        public static HttpStatus NotAcceptable { get; } = new HttpStatus(406, "Not Acceptable");
        public static HttpStatus ProxyAuthenticationRequired { get; } = new HttpStatus(407, "Proxy Authentication Required");
        public static HttpStatus RequestTimeout { get; } = new HttpStatus(408, "Request Time-out");
        public static HttpStatus Conflict { get; } = new HttpStatus(409, "Conflict");
        public static HttpStatus Gone { get; } = new HttpStatus(410, "Gone");
        public static HttpStatus LengthRequired { get; } = new HttpStatus(411, "Length Required");
        public static HttpStatus PreconditionFailed { get; } = new HttpStatus(412, "Precondition Failed");
        public static HttpStatus RequestEntityTooLarge { get; } = new HttpStatus(413, "Request Entity Too Large");
        public static HttpStatus RequestUriTooLarge { get; } = new HttpStatus(414, "Request-URI Too Large");
        public static HttpStatus UnsupportedMediaType { get; } = new HttpStatus(415, "Unsupported Media Type");
        public static HttpStatus RequestedRangeNotSatisfiable { get; } = new HttpStatus(416, "Requested range not satisfiable");
        public static HttpStatus ExpectationFailed { get; } = new HttpStatus(417, "Expectation Failed");


        public static HttpStatus InternalServerError { get; } = new HttpStatus(500, "Internal Server Error");
        public static HttpStatus NotImplemented { get; } = new HttpStatus(501, "Not Implemented");
        public static HttpStatus BadGateway { get; } = new HttpStatus(502, "Bad Gateway");
        public static HttpStatus ServiceUnavailable { get; } = new HttpStatus(503, "Service Unavailable");
        public static HttpStatus GatewayTimeout { get; } = new HttpStatus(504, "Gateway Time-out");
        public static HttpStatus HttpVersionNotSupported { get; } = new HttpStatus(505, "HTTP Version not supported");

    }
}