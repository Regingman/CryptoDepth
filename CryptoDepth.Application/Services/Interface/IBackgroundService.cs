using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CryptoDepth.Domain.Dto.CoinGekoPars;

namespace CryptoDepth.Application.Services.Interface
{
    public interface IBackgroundService
    {
        List<TopCoinsInfo> topCoinsInfos { get; set; }
        List<FromParse> ReadSymbolsFromExcel(string filePath, int col);
        string GetSymbolFromPair(string pair, string value);
        void GetCoinDetailsList();
        TopCoinsInfo CalculateDepth(TopCoinsInfo value);
        void BackgroundCalculateDepth();
        void ModifyExcelFile();
    }
}
