using Microsoft.AspNetCore.Mvc;
using sql_sink_api.Interfaces;


namespace sql_sink_api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IDataHandler _dataHandler;

        public UserController(IDataHandler dataHandler)
        {
            _dataHandler = dataHandler;
        }
        [HttpGet("actions")]
        public async Task<IActionResult> GetUserActions(
    [FromQuery] DateTime? startTimeStamp = null,
    [FromQuery] DateTime? endTimeStamp = null)
        {
            var start = startTimeStamp; // null means no filter
            var end = endTimeStamp;

            if (start.HasValue && end.HasValue && start > end)
                return BadRequest("startTimeStamp must be earlier than endTimeStamp");

            var results = await _dataHandler.Get(start, end);
            return Ok(results);
        }
    }
}
