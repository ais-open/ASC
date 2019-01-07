using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Models.Billing
{
    public class BracketCost : BaseCost, ICost
    {
        IDictionary<string, double> usageBrackets = null;
        protected override double CalculateCosts()
        {
            SetupBrackets();
            return CalculateCostsByBracket();
        }

        private double CalculateCostsByBracket()
        {
            double totalCosts = 0;
            foreach (var bracket in usageBrackets)
            {
                totalCosts += Meter.MeterRates[bracket.Key] * bracket.Value;
            }
            return totalCosts;
        }

        private void SetupBrackets()
        {
            usageBrackets = new Dictionary<string, double>();

            double remainingQuantity = BillableQuantity;

            for (int keyIndex = 0; keyIndex < Meter.MeterRates.Count; keyIndex++)
            {
                int nextIndex = keyIndex + 1;
                string key = Meter.MeterRates.Keys.ElementAt(keyIndex);
                double bracketStart = Utils.ParseDouble(key);
                double bracketEnd = 0;

                bool nextIndexExists = nextIndex < Meter.MeterRates.Count;

                if (nextIndexExists)
                {
                    bracketEnd = Utils.ParseDouble(Meter.MeterRates.Keys.ElementAt(nextIndex)) - 1;

                    var bracketQuantity = bracketEnd - bracketStart;

                    //Adjust the quantity (Ex: 0, 256, 512 is used, in first case (255-0) is fine, in second case (511-256+1) must be used
                    if (bracketStart > 0)
                    {
                        bracketQuantity += 1;
                    }

                    if (remainingQuantity <= bracketQuantity)
                    {
                        usageBrackets.Add(key, remainingQuantity);
                        return;
                    }
                    else
                    {
                        usageBrackets.Add(key, bracketQuantity);
                        remainingQuantity = remainingQuantity - bracketQuantity;
                    }
                }
                else
                {
                    //last bracket. Just add the remaining quantity and return
                    usageBrackets.Add(key, remainingQuantity);
                }
            }
        }
    }
}