using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Amazon.S3;
using Amazon.S3.Model;
using System.Net;
using Application.VMs;
using Application.VMs.Wasabi;
using Microsoft.VisualBasic.FileIO;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CommonOperations.Methods
{
    public static class CommonMethods
    {
        private static IAmazonS3 _s3Client;
        private static string _bucketName;
        public static void Initialize(IAmazonS3 s3Client, string wasabiBucket)
        {
            _s3Client = s3Client;
            _bucketName = wasabiBucket;
        }
        public static System.String ConvertStringToShah256(string value)
        {
            StringBuilder Sb = new StringBuilder();
            using (var hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }
            return Sb.ToString();
        }
        public static string EncrypthePassword(string Password)
        {
            string password = Base64Encode(Password);
            return ConvertStringToShah256(password);
        }
        public static string Base64Encode(string password)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(password);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string GenerateJwtToken(string UserEmail, long UserID, string UserName, IConfiguration config)
        {

            var authClaims = new List<Claim>
            {
                new Claim("Email", UserEmail),
                new Claim("ID", UserID.ToString()),
                new Claim("UserName", UserName)
            };
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWT:Secret"]));
            var token = new JwtSecurityToken(
                issuer: config["JWT:ValidIssuer"],
                audience: config["JWT:ValidAudience"],
                expires: DateTime.UtcNow.AddDays(5),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static string ExtractClaimFromToken(string token, string claimType)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var claim = jwtToken.Claims.FirstOrDefault(c => c.Type == claimType);
                return claim?.Value;
            }
            catch
            {
                return null; 
            }
        }





        public static string GetSignedUrl(string fileKey, string bucketName)
        {


            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = fileKey,
                Expires = DateTime.UtcNow.AddDays(1),
                Protocol = Protocol.HTTPS
            };
            return _s3Client.GetPreSignedURL(request);
        }
        public static string UploadToWasabi(string base64, string folderName, string name)
        {
            if (string.IsNullOrEmpty(base64))
            {
                return "Base64 can't be null or empty";
            }
            var filename = RemoveExtraChracters(name);

            string filePath = $"{folderName}/{filename}/Image{DateTime.Now.Ticks.ToString()}";
            string base64Data = base64;
            if (base64Data.Contains(","))
            {
                base64Data = base64Data.Split(',')[1];
            }

           
            byte[] fileBytes = Encoding.UTF8.GetBytes(base64Data); ;
            var contentType = "application/octet-stream";


            Console.WriteLine($"Uploading {fileBytes.Length} bytes to {filePath}");

            using (var stream = new MemoryStream(fileBytes))
            {
                //stream.Position = 0;
                var putRequest = new PutObjectRequest
                {
                    InputStream = stream,
                    BucketName = _bucketName,
                    Key = filePath,
                    ContentType = contentType,
                };
                _s3Client.PutObjectAsync(putRequest).GetAwaiter().GetResult();
            }

            return $"https://{_bucketName}.s3.wasabisys.com/{filePath}";
            //return $"https://cdn.banamcode.com/{filePath}";
        }
        public static string UpdateOnWasabi(string base64, string PreviousPath)
        {

            if (string.IsNullOrEmpty(base64))
            {
                return "Base64 can't be null or empty";
            }

            string base64Data = base64;
            byte[] fileBytes = Encoding.UTF8.GetBytes(base64Data);
            var fileName = "";
            var contentType = "application/octet-stream";


            if (base64Data.Contains(","))
            {
                base64Data = base64Data.Split(',')[1];
            }
            fileName = ExtractKeyFromUrl(PreviousPath);



            using (var stream = new MemoryStream(fileBytes))
            {
                var putRequest = new PutObjectRequest
                {
                    InputStream = stream,
                    BucketName = _bucketName,
                    Key = fileName,
                    ContentType = contentType,
                };
                _s3Client.PutObjectAsync(putRequest).GetAwaiter().GetResult();
            }

            return $"https://{_bucketName}.s3.wasabisys.com/{fileName}";
        }
        public static string RetriveFromWasabi(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                return "fileUrl can't be empty";
            }
            try
            {
                string fileName = ExtractKeyFromUrl(fileUrl);
                var getRequest = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileName
                };

                using (var response = _s3Client.GetObjectAsync(getRequest).GetAwaiter().GetResult())
                {

                    if (response.HttpStatusCode != HttpStatusCode.OK)
                    {
                        return "no file found.";
                    }
                    using (var reader = new StreamReader(response.ResponseStream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (AmazonS3Exception s3Ex) when (s3Ex.ErrorCode == "NoSuchKey")
            {

                return "no file found.";
            }

        }
        public static string DeleteFromWasabi(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                return "The FileUrl can't be empty";

            }
            string key = ExtractKeyFromUrl(fileUrl);
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            var response = _s3Client.DeleteObjectAsync(deleteRequest).GetAwaiter().GetResult();

            return $"Object '{fileUrl}' deleted successfully from bucket '{_bucketName}'.";

        }
        private static string ExtractKeyFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return "no file found";
            }

            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                return "";
            }

            return uri.AbsolutePath.TrimStart('/');
        }
        private static string RemoveExtraChracters(string name)
        {
            name = name.Replace("/", "-");
            name = name.Replace("\\", "-");
            name = name.Replace("%20", "-");
            name = name.Replace(" ", "-");

            return name;
        }





        #region AWS

        public static async Task<ResponseVM> UploadWasabiFileAsync(FileVM file)
        {
            ResponseVM response = ResponseVM.Instance;
            try
            {
                using (var stream = file.File.OpenReadStream())
                {
                    string moduleName = "mymodule";
                    string folderName = "myfolder";

                    if (stream == null || !stream.CanRead)
                    {
                        throw new ArgumentException("Invalid file stream.");
                    }
                    string fileExtension = Path.GetExtension(file.File.FileName).ToLowerInvariant();
                    string fileBaseName = Path.GetFileNameWithoutExtension(file.File.FileName);
                    string uniqueFileName = $"{Guid.NewGuid()}_{fileBaseName}{fileExtension}";

                    // Determine file type from extension
                    FileType detectedFileType = GetFileTypeFromExtension(fileExtension);

                    //string filename = Path.GetFileNameWithoutExtension(file.File.FileName);
                    string fileKey = $"{moduleName}/{folderName}/{uniqueFileName}";



                    var putRequest = new PutObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = fileKey,
                        InputStream = stream,
                        ContentType = GetFileTypeString(detectedFileType),
                        //      CannedACL = file.access == true
                        //? S3CannedACL.PublicRead
                        //: S3CannedACL.Private
                    };
                    //putRequest.Metadata.Add("FileName", EnsureAscii(file.Name));

                    var result = await _s3Client.PutObjectAsync(putRequest);
                    response.responseCode = 200;
                    response.responseMessage = "success";
                    response.data = result;
                    //if (result.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    //{
                    //    var dbResult = await UploadFileToDatabase(fileKey, file, detectedFileType, filename);
                    //    if (dbResult.responseCode == 200)
                    //    {
                    //        response.responseCode = 200;
                    //        response.responseMessage = "File uploaded successfully";
                    //    }
                    //    else
                    //    {
                    //        response.responseCode = dbResult.responseCode;
                    //        response.errorMessage = dbResult.errorMessage;
                    //    }
                    //}
                    //else
                    //{
                    //    response.responseCode = (int)result.HttpStatusCode;
                    //    response.errorMessage = "File upload failed";
                    //}
                }
            }
            catch (AmazonS3Exception ex)
            {
                response.responseCode = (int)ex.StatusCode;
                response.errorMessage = $"Amazon S3 Error: {ex.Message}";
            }
            catch (Exception ex)
            {
                response.responseCode = 500;
                response.errorMessage = $"Error: {ex.Message}";
            }

            return response;
        }

        private static FileType GetFileTypeFromExtension(string extension)
        {
            switch (extension)
            {
                case ".pdf":
                    return FileType.ApplicationPdf;
                case ".doc":
                case ".docx":
                    return FileType.WordDocument;
                case ".jpg":
                case ".jpeg":
                    return FileType.ImageJpeg;
                case ".png":
                    return FileType.ImagePng;
                case ".gif":
                    return FileType.ImageGif;
                case ".mp3":
                    return FileType.AudioMpeg;
                case ".mp4":
                    return FileType.VideoMp4;
                default:
                    throw new ArgumentException($"Unsupported file extension: {extension}");
            }
        }

        public enum FileType
        {
            ApplicationPdf,
            WordDocument,
            ImageJpeg,
            ImagePng,
            ImageGif,
            AudioMpeg,
            VideoMp4
        }


        public static string GetFileTypeString(FileType mimeType)
        {
            return mimeType switch
            {
                FileType.ApplicationPdf => "application/pdf",
                FileType.WordDocument => "application/msword",
                FileType.ImageJpeg => "image/jpeg",
                FileType.ImagePng => "image/png",
                FileType.ImageGif => "image/gif",
                FileType.AudioMpeg => "audio/mpeg",
                FileType.VideoMp4 => "video/mp4",
                _ => throw new ArgumentOutOfRangeException(nameof(mimeType), mimeType, null)
            };
        }


        #endregion

    }
}
