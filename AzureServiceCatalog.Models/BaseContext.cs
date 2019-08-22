using System;
using System.Collections.Generic;

namespace AzureServiceCatalog.Models
{
    public class BaseContext
    {
        private readonly DateTime _timestamp;
        private readonly Guid _operationId;

        public BaseContext() : this(Guid.NewGuid())
        {
        }

        public BaseContext(Guid operationId)
        {
            _timestamp = DateTime.UtcNow;
            _operationId = operationId;
        }

        /// <summary>
        /// Gets the operation id.
        /// </summary>
        public Guid OperationId
        {
            get
            {
                return _operationId;
            }
        }

        /// <summary>
        /// Gets the date time ticks (in UTC) when the operation started.
        /// </summary>
        public DateTime Timestamp
        {
            get
            {
                return _timestamp;
            }
        }

        /// <summary>
        /// Gets or sets the time taken to finish the operation.
        /// </summary>
        public double TimeTaken
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets any custom message with the operation.
        /// </summary>
        public string Message
        {
            get;
            set;
        }

        public virtual IDictionary<string, object> Export()
        {
            var dictionary = new Dictionary<string, object>
            {
                {"OperationId", OperationId.ToString()},
                {"OperationTick", Timestamp.Ticks},
                {"TimeTaken (s)", TimeTaken},
                {"Message", Message}
            };
            return dictionary;
        }
    }
}