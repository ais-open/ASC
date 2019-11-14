using AzureServiceCatalog.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AzureServiceCatalog.Helpers.BudgetHelper
{
    public class ChartHelper
    {
        double budgetAmount;
        double variance;
        double totalCost;
        string strbudgetAmount = "";
        string strtotalCost = "";
        string strvariance = "";
        string strvarPercentage = "";
        double varPercentage;
        string doughnutchartTitle = "";

        public UsageRequest usageRequest;
        public UsageResponse resourceCostResponse;

        public List<Budget> organizationBudgetList;
        public List<Usage> resourcecostList;

        public DateTime xValue;
        public double yValue;
        public string serviceName;

        public List<TrendlineData> chartData = new List<TrendlineData>();
        public TrendlineData t = new TrendlineData();

        public List<TrendlineData> budgetchartData = new List<TrendlineData>();
        public double monthlyBudget = 0;

        public List<DoughnutData> doughnutDataSource = new List<DoughnutData>();
        public DoughnutData d = new DoughnutData();

        public double bAmount = 0;
        public string bRepeatTypeString;

        UsageHelper usageHelper = new UsageHelper();

        public async Task<BudgetChartData> GetBudgetChartData(UsageRequest requestParams)
        {
            //Get Azure Usage
            requestParams.StartDate = DateTime.Now.AddMonths(-6);
            resourceCostResponse = await usageHelper.GetUsageData(requestParams);
            resourcecostList = resourceCostResponse.Value;
            var summaryByMonth = resourcecostList.GroupBy(t => t.Month, (key, t) =>
            {
                var transactionArray = t as Usage[] ?? t.ToArray();
                return new
                {
                    Month = key,
                    Amount = transactionArray.Sum(ta => ta.Cost),
                };
            }).ToList();
            var summaryByService = resourcecostList.GroupBy(t => t.ServiceName.ToLower(), (key, t) =>
            {
                var transactionArray = t as Usage[] ?? t.ToArray();
                return new
                {
                    Category = key,
                    Count = transactionArray.Length,
                    Amount = transactionArray.Sum(ta => ta.Cost),
                };
            }).ToList();
            totalCost = summaryByService.Sum(a => a.Amount);
            strtotalCost = totalCost.ToString("C", CultureInfo.CreateSpecificCulture("en-IN"));
            doughnutchartTitle = "Cost Distribution by Service - Total Cost as of Today: " + totalCost.ToString("C", CultureInfo.CreateSpecificCulture("en-IN"));

            variance = budgetAmount - totalCost;
            varPercentage = Math.Round((variance / budgetAmount), 2) * 100;
            strvariance = variance.ToString("C", CultureInfo.CreateSpecificCulture("en-IN"));
            strvarPercentage = varPercentage + "%";

            summaryByMonth.ForEach(
                row => chartData.Add(new TrendlineData
                {
                    x = new DateTime(2019, row.Month, 1),
                    y = Math.Round(row.Amount, 2)
            }));
            if (requestParams.Budget != null)
            {
                bAmount = requestParams.Budget.Amount;
                bRepeatTypeString = requestParams.Budget.RepeatTypeString;

                if (bRepeatTypeString.ToLower() == "monthly")//Monthly
                {
                    monthlyBudget = bAmount;
                    budgetAmount = bAmount * 12;
                }
                else if (bRepeatTypeString.ToLower() == "quarterly") //Quarterly
                {
                    monthlyBudget = bAmount / 3;
                    budgetAmount = bAmount * 4;
                }
                else //(bRepeatTypeString.ToLower() == "yearlyfixed") //YearlyFixed
                {
                    monthlyBudget = bAmount / 12;
                    budgetAmount = bAmount;
                }
            }
            strbudgetAmount = budgetAmount.ToString("C", CultureInfo.CreateSpecificCulture("en-IN"));
            summaryByMonth.ForEach(
               row =>
                 budgetchartData.Add(new TrendlineData
                 {
                     x = new DateTime(2019, row.Month, 1),
                     y = Math.Round(monthlyBudget, 2)
            }));
            totalCost = summaryByService.Sum(a => a.Amount);
            summaryByService.ForEach(
                row => doughnutDataSource.Add(new DoughnutData
                {
                    xValue = row.Category,
                    yValue = Math.Round(row.Amount, 2),
                    text = String.Format("{0:0.00}", (row.Amount / totalCost) * 100) + "%"

            }));
            BudgetChartData data = new BudgetChartData
            {
                CostData = chartData,
                BudgetData = budgetchartData,
                CostDoughnutData = doughnutDataSource,
                BudgetAmount = strbudgetAmount,
                TotalCost = strtotalCost,
                Variance = strvariance,
                VariancePercentage = strvarPercentage,
                DoughnutchartTitle = doughnutchartTitle
            };
            return data;
        }
    }  
}
