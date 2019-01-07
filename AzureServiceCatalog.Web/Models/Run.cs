using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace AzureServiceCatalog.Web.Models
{
    public static class Run
    {
        private const int Second = 1000;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void WithProgressBackOff(int retries, int intervalMin, int intervalMax, Action routine)
        {
            int retryCount = retries;
            int minInterval = intervalMin;
            int maxInterval = intervalMax;
            int backOffInterval = intervalMin;
            int exponent = 2;
            int currentCount = 0;

            do
            {
                try
                {
                    routine();
                    break;
                }
                catch
                {
                    if (++currentCount >= retryCount)
                    {
                        break;
                    }
                    Thread.Sleep(backOffInterval * Second);
                    backOffInterval = Math.Min(maxInterval, backOffInterval * exponent);
                }
            } while (true);

        }
    }
}