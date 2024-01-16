using CryptoDepth.Application.Services.Interface;
using CryptoDepth.Domain.Dto;
using OfficeOpenXml;
using System.Collections.Generic;

namespace CryptoDepth.Application.Services
{
    public class ReportService : IReportService
    {
        public byte[] CreateExcelFile(List<CoinGekoPars.TopCoinsInfo> data)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("TopCoinsInfo");

                // Заголовки
                worksheet.Cells[1, 1].Value = "Пара";
                worksheet.Cells[1, 2].Value = "Топ 1";
                worksheet.Cells[1, 3].Value = "Depth+2";
                worksheet.Cells[1, 4].Value = "Depth-2";
                worksheet.Cells[1, 5].Value = "Топ 2";
                worksheet.Cells[1, 6].Value = "Depth+2";
                worksheet.Cells[1, 7].Value = "Depth-2";
                worksheet.Cells[1, 8].Value = "Топ 3";
                worksheet.Cells[1, 9].Value = "Depth+2";
                worksheet.Cells[1, 10].Value = "Depth-2";

                // Данные
                int row = 2;
                foreach (var item in data)
                {
                    if (item.Name1 != null)
                    {
                        worksheet.Cells[row, 1].Value = item.Name;
                        worksheet.Cells[row, 2].Value = item.Name1;
                        worksheet.Cells[row, 3].Value = item.CostToMoveUpUsd1;
                        worksheet.Cells[row, 4].Value = item.CostToMoveDownUsd1;
                        worksheet.Cells[row, 5].Value = item.Name2;
                        worksheet.Cells[row, 6].Value = item.CostToMoveUpUsd2;
                        worksheet.Cells[row, 7].Value = item.CostToMoveDownUsd2;
                        worksheet.Cells[row, 8].Value = item.Name3;
                        worksheet.Cells[row, 9].Value = item.CostToMoveUpUsd3;
                        worksheet.Cells[row, 10].Value = item.CostToMoveDownUsd3;

                        row++;
                    }
                }

                return package.GetAsByteArray();
            }
        }
    }
}
