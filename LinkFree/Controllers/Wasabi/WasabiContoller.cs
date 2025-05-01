using Application.VMs.Wasabi;
using CommonOperations.Methods;
using Microsoft.AspNetCore.Mvc;

namespace LinkFree.Controllers.Wasabi
{
    [Route("api/[controller]")]
    [ApiController]
    public class WasabiContoller : ControllerBase
    {
        [HttpPost("UploadBase64")]
        public IActionResult Upload(UploadToWasabi modal)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var res = CommonMethods.UploadToWasabi(modal.base64, modal.folderName, modal.fileName);
            return Ok(res);
        }


        [HttpPost("UpdateBase64")]
        public IActionResult Update(UpdateOnWasabi modal)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var res = CommonMethods.UpdateOnWasabi(modal.base64, modal.filePath);
            return Ok(res);
        }


        [HttpGet("GetBase64")]
        public IActionResult Get(string filepath)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var res = CommonMethods.RetriveFromWasabi(filepath);
            return Ok(res);
        }


        [HttpPost("DeleteBase64")]
        public IActionResult Delete(string filepath)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var res = CommonMethods.DeleteFromWasabi(filepath);
            return Ok(res);
        }


        [HttpPost("GetPreSignedUrl")]
        public IActionResult PreSignedUrl(string fileKey,string bucketName)
        {
            if (!ModelState.IsValid)
                return BadRequest();
            var res = CommonMethods.GetSignedUrl(fileKey, bucketName);
            return Ok(res);
        }




        [HttpPost("upload-wasabi-async")]
        public async Task<IActionResult> UploadFile([FromForm] FileVM data)
        {
            if (data.File == null || data.File.Length == 0)
            {
                return BadRequest("File is required");
            }

            var currentDate = DateTime.UtcNow;

            //using (var stream = data.File.OpenReadStream())
            //{

                var response = await CommonMethods.UploadWasabiFileAsync(data);
                if (response.responseCode == 200)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            //}
        }
    }
}
