using CryptoDepth.Domain.Entities.Base;
using CryptoDepth.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoDepth.Domain.Entities
{
    public class TradingPairDepth : BaseEntityGuid
    {
        public string TradingPair { get; set; }
        public decimal DepthPlus2Percent { get; set; }
        public decimal DepthMinus2Percent { get; set; }
        public decimal TotalDepth { get; set; }
        public CryptoSite CryptoSite { get; set; }
    }
}
