using FresherMisa2026.Application.Interfaces.Services;
using FresherMisa2026.Entities;
using FresherMisa2026.Entities.FileUpload;
using Microsoft.AspNetCore.Mvc;

namespace FresherMisa2026.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FilesController : ControllerBase
    {
        #region Declare
        private readonly IFileService _fileService;
        #endregion

        #region Constructer
        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Upload file lên server
        /// </summary>
        /// <param name="file">File cần upload</param>
        /// <param name="folder">Thư mục con lưu file (ví dụ: candidates, employees)</param>
        /// <returns>URL tương đối để truy cập file</returns>
        /// Created By: Nguyen Thiet Do (09/05/2026)
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ServiceResponse>> Upload(IFormFile file, [FromQuery] string folder = "general")
        {
            var request = new FileUploadRequest
            {
                FileStream = file.OpenReadStream(),
                FileName = file.FileName,
                FileSize = file.Length
            };

            var url = await _fileService.SaveFileAsync(request, folder);
            return Ok(new ServiceResponse
            {
                IsSuccess = true,
                Code = 200,
                Data = new { url }
            });
        }

        /// <summary>
        /// Xóa file khỏi server
        /// </summary>
        /// <param name="relativeUrl">URL tương đối của file (ví dụ: /uploads/candidates/abc.pdf)</param>
        /// Created By: Nguyen Thiet Do (09/05/2026)
        [HttpDelete]
        public ActionResult<ServiceResponse> Delete([FromQuery] string relativeUrl)
        {
            _fileService.DeleteFile(relativeUrl);
            return Ok(new ServiceResponse { IsSuccess = true, Code = 200 });
        }
        #endregion
    }
}
