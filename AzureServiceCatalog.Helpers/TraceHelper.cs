using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureServiceCatalog.Models;

namespace AzureServiceCatalog.Helpers
{
    public class TraceHelper
    {
        public static void TraceError (Guid operationId, string operationName, Exception ex)
        {
            var errorLog = string.Format("Operation Id:{0}.\n Error in occured in Method:{1}.\n Error Message: {2}.\n Stack Trace: {3}", operationId, operationName, ex.Message, ex.StackTrace);
            Trace.TraceError(errorLog);
        }

        public static void TraceInformation(Guid operationId, string operationName, string message)
        {
            var infoLog = string.Format("Operation Id:{0}.\n Method:{1}.\n Information: {2}", operationId, operationName, message);
            Trace.TraceInformation(infoLog);
        }

        public static void TraceWarning(Guid operationId, string operationName)
        {
            var warningLog = string.Format("Operation Id:{0}.\n Warning at Method:{1}.\n", operationId, operationName);
            Trace.TraceWarning(warningLog);
        }

        public static void TraceOperation(BaseOperationContext operationContext)
        {
            var dictionary = operationContext.Export();
            //var infoLog = string.Format("[{0}] - Operation Id:{1}.\n Executed Method:{2}.\n Time Taken:{3} \n User Id:{4}.\n", operationContext.Timestamp, operationContext.OperationId, operationContext.OperationName, operationContext.TimeTaken, operationContext.UserId);
            //Trace.WriteLine(infoLog);
            string log = "";
            foreach (KeyValuePair<string, object> item in dictionary)
            {
                log += string.Format("{0}: {1}. ", item.Key, item.Value);
            }
            Trace.WriteLine(log);
        }
    }
}
