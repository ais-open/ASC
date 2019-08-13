using System;
using System.Web;
using Newtonsoft.Json;

namespace AzureServiceCatalog.Web.Models
{
    public class ErrorInformation
    {
        public ErrorInformation()
        {
            RequestId = Guid.NewGuid().ToString();
            RequestDate = DateTime.UtcNow;
        }

        public ErrorInformation(Guid requestId, DateTime timestamp)
        {
            RequestId = requestId.ToString();
            RequestDate = timestamp;
        }

        [JsonProperty(PropertyName = "code")]
        public string Code { get; set; }
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
        [JsonProperty(PropertyName = "requestId")]
        public string RequestId { get; set; }
        [JsonProperty(PropertyName = "requestDate")]
        //[JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime RequestDate { get; set; }

        public static ErrorInformation GetInternalServerErrorInformation()
        {
            var errorInformation = new ErrorInformation();
            errorInformation.Code = "InternalServerError";
            errorInformation.Message = "An internal server error occurred. Please retry your request.";
            return errorInformation;
        }

        public static ErrorInformation GetInternalServerErrorInformation(Guid requestId, DateTime timestamp)
        {
            var errorInformation = new ErrorInformation(requestId, timestamp);
            errorInformation.Code = "InternalServerError";
            errorInformation.Message = "An internal server error occurred. Please retry your request.";
            return errorInformation;
        }
    }
}
