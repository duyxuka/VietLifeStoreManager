using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VietlifeStore.ChucNang.DatLichs.Emails;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Domain.Repositories;

namespace VietlifeStore.Controllers
{
    [Route("api/email")]
    [ApiController]
    public class EmailTrackingController : AbpController
    {
        private readonly IRepository<EmailQueue, Guid> _queueRepo;
        private readonly IRepository<EmailLog, Guid> _logRepo;

        public EmailTrackingController(
            IRepository<EmailQueue, Guid> queueRepo,
            IRepository<EmailLog, Guid> logRepo)
        {
            _queueRepo = queueRepo;
            _logRepo = logRepo;
        }

        // Tracking pixel — gọi khi khách mở mail
        [HttpGet("track/open/{queueId}")]
        [AllowAnonymous]
        public async Task<IActionResult> TrackOpen(Guid queueId)
        {
            try
            {
                var queue = await _queueRepo.FindAsync(queueId);
                if (queue != null && !queue.DaMo)
                {
                    queue.DaMo = true;
                    queue.ThoiGianMo = DateTime.Now;
                    await _queueRepo.UpdateAsync(queue, autoSave: true);

                    // Cập nhật SoMo trên campaign
                    var log = await _logRepo.FirstOrDefaultAsync(
                        x => x.QueueId == queueId);
                    if (log != null)
                    {
                        log.DaMo = true;
                        await _logRepo.UpdateAsync(log, autoSave: true);
                    }
                }
            }
            catch { /* Không throw — tracking không được làm hỏng UX */ }

            // Trả về ảnh 1x1 pixel trong suốt
            var pixel = Convert.FromBase64String(
                "R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");
            return File(pixel, "image/gif");
        }
    }
}
