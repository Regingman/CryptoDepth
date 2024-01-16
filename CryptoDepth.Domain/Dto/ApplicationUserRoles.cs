using CryptoDepth.Domain.Entities;
using CryptoDepth.Domain.Enum;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoDepth.Domain.Dto
{
    public class TradingPairDepthDto
    {
        public Guid Id { get; set; }
        public string TradingPair { get; set; }
        public decimal DepthPlus2Percent { get; set; }
        public decimal DepthMinus2Percent { get; set; }
        public decimal TotalDepth { get; set; }
        public CryptoSite CryptoSite { get; set; }
    }
}
