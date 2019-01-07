using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AzureServiceCatalog.Web.Models.Billing
{
    public class RateCard
    {
        public RateCard()
        {
            OfferTerms = new List<OfferTerm>();
            Meters = new List<Meter>();
        }
        public List<OfferTerm> OfferTerms { get; internal set; }
        public List<Meter> Meters { get; internal set; }
    }
}