using Microsoft.AspNetCore.Mvc;
using FreedomITAS.API_Serv;

namespace FreedomITAS.Controllers
{
    [Route("oauth/callback")]
    public class OAuthCallbackController : Controller
    {
        private readonly GoHighLevelService _ghlService;

        public OAuthCallbackController(GoHighLevelService ghlService)
        {
            _ghlService = ghlService;
        }

        [HttpGet]
        public async Task<IActionResult> Callback([FromQuery] string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return BadRequest("Missing authorization code.");

            try
            {
                await _ghlService.ExchangeCodeForTokensAsync(code);
                return Content("Authorization successful. You can now close this tab.");
            }
            catch (Exception ex)
            {
                return Content("Authorization failed: " + ex.Message);
            }
        }
    }
}
