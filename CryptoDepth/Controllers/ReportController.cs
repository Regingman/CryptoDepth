using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using CryptoDepth.Application.Services.Interface;
using CryptoDepth.Application.Services;
using static CryptoDepth.Domain.Dto.CoinGekoPars;
using System.Linq;

namespace CryptoDepth.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly BackgroundService _serviceBackground;
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService, BackgroundService serviceBackground)
        {
            _reportService = reportService;
            _serviceBackground = serviceBackground;
        }

        [HttpGet("get_excel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status400BadRequest)]
        public ActionResult GetReport()
        {
            byte[] fileContents = _reportService.CreateExcelFile(_serviceBackground.topCoinsInfos);
            if (fileContents != null)
            {
                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TopCoinsInfo.xlsx");
            }
            else
            {
                // В случае ошибки возвращаем BadRequest или другой подходящий статус код
                return BadRequest("Не удалось создать файл Excel.");
            }
        }

        [HttpGet("get_status_background_service")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string[]), StatusCodes.Status400BadRequest)]
        public ActionResult GetStatus(int page = 1, int size = 20, bool IsNullId = true, bool IsNullDepth = true)
        {
            // Фильтрация данных в соответствии с условиями
            var filteredData = _serviceBackground.topCoinsInfos
                .Where(e => (!IsNullId ? e.Id != null : e.Id == null) && (!IsNullDepth ? e.Name1 != null : e.Name1 == null))
                .ToList();

            // Пагинация данных
            var paginatedData = filteredData.Skip((page - 1) * size).Take(size).ToList();

            return Ok(paginatedData);
        }
    }

}
