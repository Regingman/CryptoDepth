﻿using CryptoDepth.Application.Services.Interface;
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
using System.Collections.Immutable;
using CryptoDepth.Domain.Data.Adapters;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoDepth.Application.Services
{
    public class BackgroundService : IBackgroundService, IJob
    {
        private int index = 1;
        private bool flag = true;
        private bool flagLA = true;

        public List<TopCoinsInfo> topCoinsInfos { get; set; } = new List<TopCoinsInfo>();
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public BackgroundService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public BackgroundService()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<CryptoDepthDbContext>();
                //Берем данные из БД
                topCoinsInfos = _context.TopCoinsInfos.ToList();

                //Очистка данных при новом запуске системы ( Очистка depth+2, depth-2, название биржи)
                if (topCoinsInfos.Count > 0)
                {
                    foreach (TopCoinsInfo info in topCoinsInfos)
                    {
                        // Преобразование в TopCoinsInfo
                        info.Name1 = null;
                        info.CostToMoveUpUsd1 = 0;
                        info.CostToMoveDownUsd1 = 0;

                        info.Name2 = null;
                        info.CostToMoveUpUsd2 = 0;
                        info.CostToMoveDownUsd2 = 0;

                        info.Name3 = null;
                        info.CostToMoveUpUsd3 = 0;
                        info.CostToMoveDownUsd3 = 0;

                        _context.Update(info);
                    }
                    _context.SaveChanges();
                }
            }
        }

        public void ModifyExcelFile()
        {
            if (topCoinsInfos.Count == 0 && flag)
            {
                flag = false;
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
                        using (var scope = _serviceScopeFactory.CreateScope())
                        {
                            var _context = scope.ServiceProvider.GetRequiredService<CryptoDepthDbContext>();
                            _context.TopCoinsInfos.AddRange(topCoinsInfos);
                            _context.SaveChanges();
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
            List<CoinDetails> list = new List<CoinDetails>();
            var client = new RestClient("https://api.coingecko.com/api/v3");

            var listCheck = topCoinsInfos.Where(e => e.Id != null).ToList();
            if (index > 1 && listCheck.Count == 0)
            {
                index = 1;
            }
            if (flagLA)
            {
                //Ищет Id на LATOKEN
                var requestLA = new RestRequest($"/exchanges/latoken/tickers", Method.Get);
                var responseLA = client.Execute<CoinInfo>(requestLA);
                // Фильтрация по symbols
                if (responseLA.IsSuccessful)
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var _context = scope.ServiceProvider.GetRequiredService<CryptoDepthDbContext>();
                        var tokensLA = responseLA.Data.Tickers;
                        foreach (var coinDetails in tokensLA)
                        {
                            //Находим нужный символ в нужной строке по Symbol
                            var matchingSymbol = topCoinsInfos.Where(e => e.Id == null).FirstOrDefault(s => string.Equals(s.Symbol, coinDetails.Base, StringComparison.OrdinalIgnoreCase));

                            if (matchingSymbol != default)
                            {
                                matchingSymbol.Id = coinDetails.coin_id;
                                _context.Update(matchingSymbol);
                            }
                        }

                        _context.SaveChanges();
                    }
                }
                flag = false;
            }


            for (; index <= 40; index++)
            {
                var request = new RestRequest($"/coins/markets?vs_currency=usd&order=market_cap_desc&per_page=250&page={index}&sparkline=false&locale=en", Method.Get);
                var response = client.Execute<List<CoinDetails>>(request);

                if (response.IsSuccessful)
                {
                    list.AddRange(response.Data);
                }
                else
                {
                    continue;
                }
            }
            // Фильтрация по symbols
            var selectedCoins = list
                .Where(coin => topCoinsInfos.Where(e => e.Id == null).Any(symbol => string.Equals(symbol.Symbol, coin.Symbol, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(e => e.market_cap_rank)
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
                model.Name1 = sortedCoins.Count >= 1 ? sortedCoins[0].Market.Name : null;
                model.CostToMoveUpUsd1 = sortedCoins.Count >= 1 ? sortedCoins[0].cost_to_move_up_usd : 0;
                model.CostToMoveDownUsd1 = sortedCoins.Count >= 1 ? sortedCoins[0].cost_to_move_down_usd : 0;

                model.Name2 = sortedCoins.Count >= 2 ? sortedCoins[1].Market.Name : null;
                model.CostToMoveUpUsd2 = sortedCoins.Count >= 2 ? sortedCoins[1].cost_to_move_up_usd : 0;
                model.CostToMoveDownUsd2 = sortedCoins.Count >= 2 ? sortedCoins[1].cost_to_move_down_usd : 0;

                model.Name3 = sortedCoins.Count >= 3 ? sortedCoins[2].Market.Name : null;
                model.CostToMoveUpUsd3 = sortedCoins.Count >= 3 ? sortedCoins[2].cost_to_move_up_usd : 0;
                model.CostToMoveDownUsd3 = sortedCoins.Count >= 3 ? sortedCoins[2].cost_to_move_down_usd : 0;
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<CryptoDepthDbContext>();
                    _context.Update(model);
                    _context.SaveChanges();
                }
                return model;
            }
            else
            {
                return null;
            }
        }

        public Task Execute(IJobExecutionContext context)
        {
            //На случай отключения интернета
            try
            {
                //Получение списка пар из excel файла
                ModifyExcelFile();
                if (topCoinsInfos != null && !flag)
                {
                    //Получение Id для списка пар
                    GetCoinDetailsList();
                    //Получение depth для первой взятой пары, запрос будет работать раз
                    //в 5 секунд, поэтому будет обновляться depth для одной пары раз в 5 минут
                    BackgroundCalculateDepth();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return Task.CompletedTask;
        }

        public void BackgroundCalculateDepth()
        {
            var value = topCoinsInfos.FirstOrDefault(e => e.Name1 == null && e.Id != null);
            if (value != null)
            {
                var tempvalue = CalculateDepth(value);
            }
        }
    }
}
