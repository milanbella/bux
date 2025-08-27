using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bux.Controllers.Model.Api.UserController;
using Bux.Dbo;
using Bux.Dbo.Model;
using Bux.Sessionn;
using Serilog;

namespace Bux.Controllers.Api
{
    [Route("api/user")]
    public class UserController : Controller
    {
        public static string CLASS_NAME = typeof(UserController).Name;

        private SessionService sessionService;

        public UserController(SessionService sessionService)
        {
            this.sessionService = sessionService;
        }

        [HttpGet]
        [Route("hello")]
        public IActionResult Hello()
        {
            return Content("Hello", "text/plain");
        }

        [HttpGet("get-user")]
        public async Task<ActionResult<GetUserResponse>> GetUser()
        {
            var user = await sessionService.GetLoggedInUser();
            if (user == null)
            {
                return Unauthorized("No user logged in");
            }
            var getUserResponse = new GetUserResponse(user.Name, user.Email, user.FirstName, user.LastName);

            return Ok(getUserResponse);
        }

        [HttpGet("get-total-bux-earned")]
        public async Task<ActionResult<GetTotalBuxEarnedResponse>> GetTotalBuxEarned([FromServices] Db db)
        {
            var user = await sessionService.GetLoggedInUser();
            if (user == null)
            {
                return Unauthorized("No user logged in");
            }

            var buxEarned = await db.BuxEarned.FirstOrDefaultAsync(b => b.UserId == user.Id);
            double total = buxEarned?.Amount ?? 0;

            var response = new GetTotalBuxEarnedResponse(total);
            return Ok(response);
        }

        // Adjust to your needs (per-file)
        private const long MaxAvatarFileBytes = 5L * 1024 * 1024; // 5 MB per variant

        // JPEG magic numbers
        private static readonly byte[] JpegSig = new byte[] { 0xFF, 0xD8, 0xFF };

        public sealed class AvatarUploadForm
        {
            // These names match your FormData keys
            public IFormFile? avatar256 { get; set; }
            public IFormFile? avatar128 { get; set; }
            public IFormFile? avatar64 { get; set; }
        }

        [HttpPost("avatar-upload")]
        [Consumes("multipart/form-data")]
        // Cap the whole request (sum of 3 files); also set global MultipartBodyLengthLimit in Program.cs
        [RequestSizeLimit(20L * 1024 * 1024)]
        public async Task<ActionResult<AvatarUploadResponse>> AavatarUpload([FromForm] AvatarUploadForm form, [FromServices] Db db, [FromServices] IConfiguration configuration, CancellationToken ct)
        {
            const string METHOD_NAME = "AavatarUpload()";

            var user = await sessionService.GetLoggedInUser();
            if (user == null)
            {
                return Unauthorized("No user logged in");
            }

            // Require at least one variant (you can make 256 mandatory if you want)
            var files = new (int Size, IFormFile? File)[] {
                (256, form.avatar256),
                (128, form.avatar128),
                ( 64, form.avatar64)
            };

            if (files.All(t => t.File is null))
            {
                Log.Warning($"{CLASS_NAME}:{METHOD_NAME}: No files provided. Expect avatar256, avatar128 and/or avatar64.");
                return BadRequest("No files provided. Expect avatar256, avatar128 and/or avatar64.");
            }

            var id = Guid.NewGuid().ToString("N");
            //var baseDir = Path.Combine(AppContext.BaseDirectory, "uploads", "avatars", id);
            var baseDir = configuration["file_store_base_dir"];
            if (baseDir == null)
            {
                Log.Error($"{CLASS_NAME}:{METHOD_NAME}: missing config variable: file_store_base_dir");
                throw new Exception("missing config variable: file_store_base_dir");
            }
            var urlFilesBase = configuration["file_store_url"];
            if (urlFilesBase == null)
            {
                Log.Error($"{CLASS_NAME}:{METHOD_NAME}: missing config variable: file_store_url");
                throw new Exception("missing config variable: file_store_url");
            }
            //Directory.CreateDirectory(baseDir);

            string? path64 = null;
            string? file64 = null;
            string? path128 = null;
            string? file128 = null;
            string? path256 = null;
            string? file256 = null;



            foreach (var (size, file) in files)
            {
                if (file is null)
                {
                    continue;
                }

                if (file.Length == 0)
                {
                    Log.Warning($"{CLASS_NAME}:{METHOD_NAME}: avatar{size}: empty file.");
                    return BadRequest($"avatar{size}: empty file.");
                }

                if (file.Length > MaxAvatarFileBytes)
                {
                    Log.Warning($"{CLASS_NAME}:{METHOD_NAME}: avatar{size}: file too large.");
                    return StatusCode(StatusCodes.Status413PayloadTooLarge, $"avatar{size}: file too large.");
                }

                var fileName = Guid.NewGuid().ToString("N");
                // take the first 2 characters from fileName to serve as directory name
                var dirName = fileName.Substring(0, 2);

                var dirFullName = Path.Combine(baseDir, dirName);
                // create dirFullName if does not exist 
                Directory.CreateDirectory(dirFullName);

                // Minimal server-side validation: verify JPEG by signature (don’t trust ContentType)
                await using (var s = file.OpenReadStream())
                {
                    var head = await ReadHeadAsync(s, 8, ct);
                    if (!LooksLikeJpeg(head))
                    {
                        Log.Warning($"{CLASS_NAME}:{METHOD_NAME}: avatar{size}: only JPEG accepted.");
                        return StatusCode(StatusCodes.Status415UnsupportedMediaType, $"avatar{size}: only JPEG accepted.");
                    }
                }

                // Save to disk as {size}.jpg
                fileName = $"{fileName}_{size}.jpg";
                var savePath = Path.Combine(dirFullName, fileName);

                await using var src = file.OpenReadStream();
                await using var dst = System.IO.File.Create(savePath);
                await src.CopyToAsync(dst, ct);

                if (size == 64)
                {
                    path64 = dirName;
                    file64 = fileName;
                }
                else if (size == 128)
                {
                    path128 = dirName;
                    file128 = fileName;
                }
                else if (size == 256)
                {
                    path256 = dirName;
                    file256 = fileName;
                }
            }

            // start db transaction
            using var transaction = await db.Database.BeginTransactionAsync(ct);
            Avatar existingAvatar = null;
            try
            {
                existingAvatar = await db.Avatar
                    .Where(a => a.UserId == user.Id)
                    .FirstOrDefaultAsync();


                if (existingAvatar != null)
                {
                    db.Avatar.Remove(existingAvatar);
                }
                Avatar avatar = new()
                {
                    Id = 0,
                    UserId = user.Id,
                    path64 = path64,
                    file64 = file64,
                    path128 = path128,
                    file128 = file128,
                    path256 = path256,
                    file256 = file256
                };
                db.Avatar.Add(avatar);
                await db.SaveChangesAsync(ct);
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Log.Error(ex, $"{CLASS_NAME}:{METHOD_NAME}: Error saving avatar to database.");
                throw new Exception("Error saving avatar to database.");
            }

            // remove existing avatar files
            if (existingAvatar != null)
            {
                // remove files
                if (existingAvatar.path64 != null && existingAvatar.file64 != null)
                {
                    var fullPath = Path.Combine([baseDir, existingAvatar.path64, existingAvatar.file64]);
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }
                if (existingAvatar.path128 != null && existingAvatar.file128 != null)
                {
                    var fullPath = Path.Combine(baseDir, existingAvatar.path128, existingAvatar.file128);
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }
                if (existingAvatar.path256 != null && existingAvatar.file256 != null)
                {
                    var fullPath = Path.Combine(baseDir, existingAvatar.path256, existingAvatar.file256);
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }
            }

            var result = new AvatarUploadResponse(
                url64: path64 != null && file64 != null ? $"{urlFilesBase}/{path64}/{file64}" : null,
                url128: path128 != null && file128 != null ? $"{urlFilesBase}/{path128}/{file128}" : null,
                url256: path256 != null && file256 != null ? $"{urlFilesBase}/{path256}/{file256}" : null
            );

            return Ok(result);
        }

        private static async Task<byte[]> ReadHeadAsync(Stream s, int max, CancellationToken ct)
        {
            var buf = new byte[Math.Min(max, 64)];
            int read = 0;
            while (read < buf.Length)
            {
                var n = await s.ReadAsync(buf.AsMemory(read, buf.Length - read), ct);
                if (n <= 0) break;
                read += n;
            }
            if (read < buf.Length) Array.Resize(ref buf, read);
            return buf;
        }

        private static bool LooksLikeJpeg(ReadOnlySpan<byte> head)
            => head.Length >= JpegSig.Length && head[..JpegSig.Length].SequenceEqual(JpegSig);

        [HttpGet("get-avatars")]
        public async Task<ActionResult<GetAvatarsResponse>> GetAvatars([FromServices] Db db, [FromServices] IConfiguration configuration)
        {
            const string METHOD_NAME = "GetAvatars()";
            var user = await sessionService.GetLoggedInUser();
            if (user == null)
            {
                return Unauthorized("No user logged in");
            }

            // Get the latest avatar for the user (by Id descending)
            var avatar = await db.Avatar
                .Where(a => a.UserId == user.Id)
                .FirstOrDefaultAsync();

            if (avatar == null)
            {
                // No avatar uploaded yet
                return Ok(new GetAvatarsResponse(null, null, null));
            }

            // Get config for URL base
            var urlFilesBase = configuration["file_store_url"];
            if (string.IsNullOrEmpty(urlFilesBase))
            {
                Log.Error($"{CLASS_NAME}:{METHOD_NAME}: Missing configuration: file_store_url");
                throw new Exception("Missing configuration: file_store_url");
            }


            string? url64 = (avatar.path64 != null && avatar.file64 != null)
                ? $"{urlFilesBase}/{avatar.path64}/{avatar.file64}"
                : null;
            string? url128 = (avatar.path128 != null && avatar.file128 != null)
                ? $"{urlFilesBase}/{avatar.path128}/{avatar.file128}"
                : null;
            string? url256 = (avatar.path256 != null && avatar.file256 != null)
                ? $"{urlFilesBase}/{avatar.path256}/{avatar.file256}"
                : null;

            var response = new GetAvatarsResponse(url64, url128, url256);
            return Ok(response);
        }

        [HttpGet("get-avatar-64")]
        public async Task<ActionResult<GetAvatar64Response>> GetAvatar64([FromServices] Db db, [FromServices] IConfiguration configuration)
        {
            const string METHOD_NAME = "GetAvatar64()";
            var user = await sessionService.GetLoggedInUser();
            if (user == null)
            {
                return Unauthorized("No user logged in");
            }

            // Get the latest avatar for the user (by Id descending)
            var avatar = await db.Avatar
                .Where(a => a.UserId == user.Id)
                .FirstOrDefaultAsync();

            if (avatar == null)
            {
                // No avatar uploaded yet
                return Ok(new GetAvatarsResponse(null, null, null));
            }

            // Get config for URL base
            var urlFilesBase = configuration["file_store_url"];
            if (string.IsNullOrEmpty(urlFilesBase))
            {
                Log.Error($"{CLASS_NAME}:{METHOD_NAME}: Missing configuration: file_store_url");
                throw new Exception("Missing configuration: file_store_url");
            }


            string? url64 = (avatar.path64 != null && avatar.file64 != null)
                ? $"{urlFilesBase}/{avatar.path64}/{avatar.file64}"
                : null;
            var response = new GetAvatar64Response(url64);
            return Ok(response);
        }

    }

}
