using Hangfire;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.ChucNang.DatLichs.DatLichGiamGiaSanPhams.ChuongTrinhGiamGias;
using Volo.Abp.DependencyInjection;

namespace VietlifeStore.ChucNang.DatLichs
{
    public interface IGiamGiaSingleJob
    {
        Task ActivateSingleAsync(Guid ctId);
        Task ExpireSingleAsync(Guid ctId);
    }

    [Queue("giam-gia")]
    public class GiamGiaSingleJob : IGiamGiaSingleJob, ITransientDependency
    {
        private readonly IChuongTrinhGiamGiaAppService _service;
        private readonly ILogger<GiamGiaSingleJob> _logger;

        public GiamGiaSingleJob(
            IChuongTrinhGiamGiaAppService service,
            ILogger<GiamGiaSingleJob> logger)
        {
            _service = service;
            _logger = logger;
        }

        [JobDisplayName("Kích hoạt giảm giá: {0}")]
        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 60, 120 })]
        public async Task ActivateSingleAsync(Guid ctId)
        {
            _logger.LogInformation("Bắt đầu kích hoạt chương trình giảm giá {Id}", ctId);
            try
            {
                await _service.ActivateSingleAsync(ctId);
                _logger.LogInformation("Đã kích hoạt thành công chương trình {Id}", ctId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi activate chương trình {Id}", ctId);
                throw; // Hangfire sẽ retry
            }
        }

        [JobDisplayName("Hết hạn giảm giá: {0}")]
        [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 60, 120 })]
        public async Task ExpireSingleAsync(Guid ctId)
        {
            _logger.LogInformation("Bắt đầu expire chương trình giảm giá {Id}", ctId);
            try
            {
                await _service.ExpireSingleAsync(ctId);
                _logger.LogInformation("Đã expire thành công chương trình {Id}", ctId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi expire chương trình {Id}", ctId);
                throw;
            }
        }
    }
}
