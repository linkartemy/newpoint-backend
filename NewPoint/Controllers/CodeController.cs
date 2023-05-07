using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using NewPoint.Models;
using NewPoint.Models.Requests;
using NewPoint.Services;

namespace NewPoint.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CodeController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICodeService _codeService;

    private readonly ILogger<CodeController> _logger;

    public CodeController(IUserService userService, ICodeService codeService, ILogger<CodeController> logger)
    {
        _userService = userService;
        _codeService = codeService;
        _logger = logger;
    }

    [HttpPost("create/email")]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Response>> SendCodeToEmail([FromBody] string email)
    {
        var response = new Response();
        try
        {
            email = email.Trim();
            if (email.Length > 255)
            {
                response.Error = "Email length can't be over 255 symbols";
                return BadRequest(response);
            }

            if (new EmailAddressAttribute().IsValid(email) is false)
            {
                response.Error = "Email is invalid";
                return BadRequest(response);
            }
            
            var id = _userService.GetIdFromToken(Request.Headers["token"]);
            await _codeService.SendCodeToEmail(id, email);
            var user = await _userService.GetProfileById(id);
            var dataEntry = new DataEntry<User>
            {
                Data = user,
                Type = "user"
            };
            response.Data = new[] { dataEntry };

            return Ok(response);
        }
        catch (Exception e)
        {
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }
    }
}