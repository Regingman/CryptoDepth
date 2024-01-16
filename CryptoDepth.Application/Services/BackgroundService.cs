using CryptoDepth.Application.Services.Interface;
using CryptoDepth.Domain.Dto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using Org.BouncyCastle.Asn1.Crmf;
using static CryptoDepth.Domain.Dto.CoinGekoPars;
using RestSharp;
using Quartz;
using DocumentFormat.OpenXml.Drawing.Charts;

namespace CryptoDepth.Application.Services
{
    public class BackgroundService : IBackgroundService, IJob
    {
        public List<TopCoinsInfo> topCoinsInfos { get; set; } = new List<TopCoinsInfo>();

        public void ModifyExcelFile()
        {
            if (topCoinsInfos.Count == 0)
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                try
                {
                    string excelFilePath = "PairList.xlsx";
                    List<FromParse> symbols = ReadSymbolsFromExcel(excelFilePath, 1);

                    if (symbols != null && symbols.Any())
                    {
                        foreach (var temp in symbols)
                        {
                            topCoinsInfos.Add(
                                       new TopCoinsInfo
                                       {
                                           Name = temp.BaseTarget,
                                           Symbol = temp.Symbol,
                                       });
                        }
                    }
                    else
                    {
                        Console.WriteLine("Не удалось прочитать символы из файла Excel.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public void GetCoinDetailsList()
        {
            var count = topCoinsInfos.Where(e => e.Id == null).Count();
            if (count != 0)
            {
                List<CoinDetails> list = new List<CoinDetails>();
                var client = new RestClient("https://api.coingecko.com/api/v3");
                for (int i = 1; i <= 20; i++)
                {
                    var request = new RestRequest($"/coins/markets?vs_currency=usd&order=market_cap_desc&per_page=250&page={i}&sparkline=false&locale=en", Method.Get);
                    var response = client.Execute<List<CoinDetails>>(request);

                    if (response.IsSuccessful)
                    {
                        list.AddRange(response.Data);
                    }
                }

                // Фильтрация по symbols
                var selectedCoins = list
                    .OrderByDescending(e => e.market_cap_rank)
                    .Where(coin => topCoinsInfos.Where(e => e.Id == null).Any(symbol => string.Equals(symbol.Symbol, coin.Symbol, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                foreach (var coinDetails in selectedCoins.OrderByDescending(e => e.market_cap_rank))
                {
                    //Находим нужный символ в нужной строке по Symbol
                    var matchingSymbol = topCoinsInfos.Where(e => e.Id == null).FirstOrDefault(s => string.Equals(s.Symbol, coinDetails.Symbol, StringComparison.OrdinalIgnoreCase));

                    if (matchingSymbol != default)
                    {
                        matchingSymbol.Id = coinDetails.Id;
                    }
                }
            }
        }

        public string GetSymbolFromPair(string pair, string value)
        {
            // 
            int index = pair.IndexOf(value, StringComparison.OrdinalIgnoreCase);

            if (index != -1)
            {
                return pair.Substring(0, index);
            }
            return null;
        }

        public List<FromParse> ReadSymbolsFromExcel(string filePath, int col)
        {
            try
            {
                FileInfo file = new FileInfo(filePath);

                using (ExcelPackage package = new ExcelPackage(file))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    List<FromParse> symbolList = new List<FromParse>();

                    for (int row = 1; row <= rowCount; row++)
                    {
                        string checkId = worksheet.Cells[row, 2].Text.Trim();
                        if (checkId == "")
                        {
                            string cellValue = worksheet.Cells[row, col].Text.Trim();
                            string symbol = GetSymbolFromPair(cellValue, "USDT");

                            if (!string.IsNullOrEmpty(symbol))
                            {
                                symbolList.Add(new FromParse { Symbol = symbol, RowNumber = row, BaseTarget = cellValue });
                            }
                            else
                            {
                                string symbolBTC = GetSymbolFromPair(cellValue, "BTC");

                                if (!string.IsNullOrEmpty(symbolBTC))
                                {
                                    symbolList.Add(new FromParse { Symbol = symbolBTC, RowNumber = row, BaseTarget = cellValue });
                                }
                                else
                                {
                                    string symbolETH = GetSymbolFromPair(cellValue, "ETH");

                                    if (!string.IsNullOrEmpty(symbolETH))
                                    {
                                        symbolList.Add(new FromParse { Symbol = symbolETH, RowNumber = row, BaseTarget = cellValue });
                                    }
                                    else
                                    {
                                        string symbolLA = GetSymbolFromPair(cellValue, "LA");

                                        if (!string.IsNullOrEmpty(symbolLA))
                                        {
                                            symbolList.Add(new FromParse { Symbol = symbolLA, RowNumber = row, BaseTarget = cellValue });
                                        }
                                        else
                                        {
                                            string symbolTRX = GetSymbolFromPair(cellValue, "TRX");

                                            if (!string.IsNullOrEmpty(symbolTRX))
                                            {
                                                symbolList.Add(new FromParse { Symbol = symbolTRX, RowNumber = row, BaseTarget = cellValue });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return symbolList;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при чтении из Excel: {ex.Message}");
                return null;
            }
        }

        public TopCoinsInfo CalculateDepth(TopCoinsInfo model)
        {
            var client = new RestClient("https://api.coingecko.com/api/v3");
            var request = new RestRequest($"/coins/{model.Id}/tickers?depth=true", Method.Get);
            var response = client.Execute<CoinInfo>(request);

            if (response.IsSuccessful)
            {
                var allCoins = response.Data.Tickers;

                // Фильтрация по symbols (в данном случае, просто оставим первые 10 монет)
                var filteredCoins = allCoins.Take(10).ToList();

                // Сортировка по сумме CostToMoveUpUsd + CostToMoveDownUsd
                var sortedCoins = filteredCoins
                    .OrderByDescending(coin => coin.cost_to_move_up_usd + coin.cost_to_move_down_usd)
                    .Where(e => string.Equals((e.Base + e.Target), model.Name, StringComparison.OrdinalIgnoreCase))
                    .Take(3)
                    .ToList();

                // Преобразование в TopCoinsInfo
                var topCoinsInfo = new TopCoinsInfo
                {
                    Name1 = sortedCoins.Count >= 1 ? sortedCoins[0].Market.Name : null,
                    CostToMoveUpUsd1 = sortedCoins.Count >= 1 ? sortedCoins[0].cost_to_move_up_usd : 0,
                    CostToMoveDownUsd1 = sortedCoins.Count >= 1 ? sortedCoins[0].cost_to_move_down_usd : 0,

                    Name2 = sortedCoins.Count >= 2 ? sortedCoins[1].Market.Name : null,
                    CostToMoveUpUsd2 = sortedCoins.Count >= 2 ? sortedCoins[1].cost_to_move_up_usd : 0,
                    CostToMoveDownUsd2 = sortedCoins.Count >= 2 ? sortedCoins[1].cost_to_move_down_usd : 0,

                    Name3 = sortedCoins.Count >= 3 ? sortedCoins[2].Market.Name : null,
                    CostToMoveUpUsd3 = sortedCoins.Count >= 3 ? sortedCoins[2].cost_to_move_up_usd : 0,
                    CostToMoveDownUsd3 = sortedCoins.Count >= 3 ? sortedCoins[2].cost_to_move_down_usd : 0
                };
                return topCoinsInfo;
            }
            else
            {
                return null;
            }
        }

        public Task Execute(IJobExecutionContext context)
        {
            //Получение списка пар из excel файла
            ModifyExcelFile();
            //Получение Id для списка пар
            GetCoinDetailsList();
            //Получение depth для первой взятой пары, запрос будет работать раз
            //в 5 секунд, поэтому будет обновляться depth для одной пары раз в 5 минут
            BackgroundCalculateDepth();

            return Task.CompletedTask;
        }

        public void BackgroundCalculateDepth()
        {
            var value = topCoinsInfos.FirstOrDefault(e => e.Name1 == null && e.Id != null);
            if (value != null)
            {
                var tempvalue = CalculateDepth(value);
                if (tempvalue != null)
                {
                    value = tempvalue;
                }
            }
        }
    }
}
