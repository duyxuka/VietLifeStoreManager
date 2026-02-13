// MediaAppService.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;
using VietlifeStore.Entity.MediaContainers;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;
using Volo.Abp;
using System.Runtime.InteropServices;

namespace VietlifeStore.Entity.UploadFile
{
    [AllowAnonymous]
    public class MediaAppService : ApplicationService, IMediaAppService
    {
        private readonly IBlobContainer<MediaContainer> _mediaContainer;

        public MediaAppService(IBlobContainer<MediaContainer> mediaContainer)
        {
            _mediaContainer = mediaContainer;
        }

        // ================= UPLOAD =================
        public async Task<UploadResultDto> UploadAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new UserFriendlyException("File không hợp lệ");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!Array.Exists(allowedExtensions, x => x == extension))
                throw new UserFriendlyException("Chỉ cho phép upload ảnh");

            if (file.Length > 5 * 1024 * 1024)
                throw new UserFriendlyException("File quá lớn (tối đa 5MB)");

            var fileName = Guid.NewGuid() + extension;
            using (var stream = file.OpenReadStream())
            {
                await _mediaContainer.SaveAsync(fileName, stream, overrideExisting: true);
            }

            return new UploadResultDto { Result = fileName };
        }

        [HttpGet]
        [Route("files/{fileName}")]
        public async Task<IActionResult> GetFileAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new UserFriendlyException("File không hợp lệ");

            try
            {
                var stream = await _mediaContainer.GetAsync(fileName);

                if (stream == null)
                    throw new UserFriendlyException("Không tìm thấy file");

                var contentType = GetContentType(fileName);
                return new FileStreamResult(stream, contentType)
                {
                    EnableRangeProcessing = true 
                };
            }
            catch
            {
                throw new UserFriendlyException("Không tìm thấy file");
            }
        }

        // ================= GET - Legacy (giữ lại cho backward compatibility) =================
        public async Task<IRemoteStreamContent> GetAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new UserFriendlyException("File không hợp lệ");

            var stream = await _mediaContainer.GetAsync(fileName);

            if (stream == null)
                throw new UserFriendlyException("Không tìm thấy file");

            return new RemoteStreamContent(
                stream,
                fileName,
                GetContentType(fileName)
            );
        }

        // ================= DELETE =================
        public async Task DeleteAsync(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return;

            await _mediaContainer.DeleteAsync(fileName);
        }

        private string GetContentType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();

            return ext switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
    }
}