using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Models.Billing
{
    public class UsageFilterParameters : BaseFilterParameters
    {
        public const string DailyAggregration = "Daily";
        public const string HourlyAggregration = "Hourly";
        private const string billingFilterFormat = "api-version={0}&reportedStartTime={1}&reportedEndTime={2}&aggregationGranularity={3}&showDetails={4}";
        private const int currentDayUsageDefaultHoursOffset = 2;

        public DateTime ReportedStartTime { get; set; }
        public DateTime ReportedEndTime { get; set; }
        public string AggregationGranularity { get; set; }
        public bool ShowDetails { get; set; }
        public int HoursOffset { get; private set; }

        public bool IsValidReportedTime
        {
            get { return ReportedEndTime > ReportedStartTime; }
        }

        public UsageFilterParameters(int hoursOffset): base()
        {
            ShowDetails = true;
            HoursOffset = hoursOffset;
        }

        public void PrepareReportedTimeFortheCurrentMonth()
        {
            ReportedStartTime = GetFirstDayOfTheCurrentMonth();
            ReportedEndTime = GetCurrentDateWithMidnightTime();

            ApplyDayOffset();
        }

        public void PrepareReportedTimeFortheLastNoOfDays(int lastNoOfDays = 30)
        {
            ReportedEndTime = GetCurrentDateWithMidnightTime();
            ReportedStartTime = ReportedEndTime.AddDays(-lastNoOfDays);

            ApplyDayOffset();
        }

        /// <summary>
        /// Handles edge conditions when getting the Daily data when the current time is 12:00 AM. A day is subracted to handle that scenario
        /// </summary>
        private void ApplyDayOffset()
        {
            if (ReportedEndTime >= DateTime.UtcNow.AddHours(-HoursOffset))
                ReportedEndTime = ReportedEndTime.AddDays(-1);
        }

        /// <summary>
        /// Handles edge conditions when getting the Hourly data when the current time is 12:00 AM. A day is subracted to handle that scenario
        /// </summary>
        private void ApplyHourOffset()
        {
            if (ReportedEndTime >= DateTime.UtcNow.AddHours(-HoursOffset))
                ReportedEndTime = ReportedEndTime.AddHours(-HoursOffset);

            if (ReportedEndTime <= ReportedStartTime)
                ReportedStartTime = ReportedStartTime.AddDays(-1)
;        }
        /// <summary>
        /// Set the StartTime and EndTime for the current day
        /// </summary>
        /// <remarks>Using the Current Time as end time will return an error from the Azure Billing Service. So make sure to use the hoursOffset (at least 2 Hours)</remarks>
        public void PrepareReportedTimeForToday()
        {
            ReportedStartTime = GetCurrentDateWithMidnightTime();
            ReportedEndTime = GetCurrentDateTimeUptoTheHour();

            ApplyHourOffset();
        }

        /// <summary>
        /// Azure accepts datetime only with time set to midnight for Daily Aggregation Granuality
        /// </summary>
        /// <returns></returns>
        private static DateTime GetFirstDayOfTheCurrentMonth()
        {
            DateTime currentDateTime = DateTime.UtcNow;
            return new DateTime(currentDateTime.Year, currentDateTime.Month, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        }

        private static DateTime GetCurrentDateTimeUptoTheHour()
        {
            DateTime currentDateTime = DateTime.UtcNow;
            return new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, 0, 0, 0, DateTimeKind.Utc);
        }

        /// <summary>
        /// Azure accepts datetime only with time set to midnight for Daily Aggregation Granuality
        /// </summary>
        /// <returns></returns>
        private static DateTime GetCurrentDateWithMidnightTime()
        {
            DateTime currentDateTime = DateTime.UtcNow;
            return new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, 0, 0, 0, 0, DateTimeKind.Utc);
        }

        public override string GetFormattedFilter()
        {
            return string.Format(billingFilterFormat, 
                ApiVersion, 
                FormatForQueryParameter(ReportedStartTime), 
                FormatForQueryParameter(ReportedEndTime), 
                AggregationGranularity, 
                ShowDetails.ToString().ToLower());
        }

        public static UsageFilterParameters GetFilterParametersForLast30Days()
        {
            UsageFilterParameters filterParameters = new UsageFilterParameters(Config.BillingHoursOffsetFromCurrentTimeForDataRequest);
            filterParameters.AggregationGranularity = DailyAggregration;
            filterParameters.ShowDetails = true;
            filterParameters.PrepareReportedTimeFortheLastNoOfDays();
            return filterParameters;
        }

        public static UsageFilterParameters GetFilterParametersForToday()
        {
            UsageFilterParameters filterParameters = new UsageFilterParameters(Config.BillingHoursOffsetFromCurrentTimeForDataRequest);
            filterParameters.AggregationGranularity = HourlyAggregration;
            filterParameters.ShowDetails = true;
            filterParameters.PrepareReportedTimeForToday();
            return filterParameters;
        }
        
    }
}