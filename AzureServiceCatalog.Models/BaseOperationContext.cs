using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureServiceCatalog.Models
{
    public class BaseOperationContext : BaseContext
    {
        private readonly string _operationName;

        public BaseOperationContext(string operationName) : this(Guid.NewGuid(), operationName)
        {
        }

        public BaseOperationContext(Guid operationId, string operationName) : base(operationId)
        {
            _operationName = operationName;
        }

        public BaseOperationContext(BaseOperationContext parentContext, string operationName) : this(parentContext.OperationId, operationName)
        {
            UserId = parentContext.UserId;
            UserName = parentContext.UserName;
            IpAddress = parentContext.IpAddress;
        }

        /// <summary>
        /// Gets the operation name. Should be in the format {classname}:{methodname}.
        /// </summary>
        public string OperationName
        {
            get
            {
                return _operationName;
            }
        }

        /// <summary>
        /// Gets or sets the user id performing the operation. Guid.Empty when the user is anonymous.
        /// </summary>
        public string UserId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the user name performing the operation. Null when user is anonymous.
        /// </summary>
        public string UserName
        {
            get;
            set;
        }

        public string IpAddress
        {
            get;
            set;
        }

        public void CalculateTimeTaken ()
        {
            DateTime startTime = Timestamp;
            DateTime endTime = DateTime.UtcNow;
            TimeSpan span = endTime.Subtract(startTime);
            double timeTaken = span.TotalSeconds;
            TimeTaken = timeTaken;
        }

        public override IDictionary<string, object> Export()
        {
            var dictionary = base.Export();
            dictionary.Add("OperationName", OperationName);
            dictionary.Add("UserId", UserId);
            dictionary.Add("UserName", UserName);
            dictionary.Add("IpAddress", IpAddress);
            return dictionary;
        }
    }
}