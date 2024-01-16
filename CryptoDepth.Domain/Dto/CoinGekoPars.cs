using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoDepth.Domain.Dto
{
    public class CoinGekoPars
    {
        public class MarketInfo
        {
            public string Name { get; set; }
        }
        public class Ticker
        {
            public MarketInfo Market { get; set; }
            public string Base { get; set; }
            public string Target { get; set; }
            public decimal cost_to_move_up_usd { get; set; }
            public string coin_id { get; set; }
            public decimal cost_to_move_down_usd { get; set; }
        }
        public class CoinInfo
        {
            public string Name { get; set; }
            public List<Ticker> Tickers { get; set; }
        }
        public class CoinDetails
        {
            public string Id { get; set; }
            public string Symbol { get; set; }
            public string Name { get; set; }
            public int market_cap_rank { get; set; }
        }
        public class TopCoinsInfo
        {
            [Key]
            public int Uq_Id { get; set; }
            public string Name { get; set; }
            public string Symbol { get; set; }
            public string Id { get; set; }
            public string Name1 { get; set; }
            public decimal CostToMoveUpUsd1 { get; set; }
            public decimal CostToMoveDownUsd1 { get; set; }
            public string Name2 { get; set; }
            public decimal CostToMoveUpUsd2 { get; set; }
            public decimal CostToMoveDownUsd2 { get; set; }
            public string Name3 { get; set; }
            public decimal CostToMoveUpUsd3 { get; set; }
            public decimal CostToMoveDownUsd3 { get; set; }
        }

        public class FromParse
        {
            public string Symbol { get; set; }
            public int RowNumber { get; set; }
            public string BaseTarget { get; set; }
        }
    }
}
