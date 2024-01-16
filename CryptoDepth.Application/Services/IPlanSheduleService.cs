using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoDepth.Application.Services
{
    public interface IPlanSheduleService
    {
        Task CreateRecordsForPlan();
    }
}
