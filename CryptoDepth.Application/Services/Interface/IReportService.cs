using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CryptoDepth.Domain.Dto.CoinGekoPars;

namespace CryptoDepth.Application.Services.Interface
{
    public interface IReportService
    {
        byte[] CreateExcelFile(List<TopCoinsInfo> data);
    }
}
