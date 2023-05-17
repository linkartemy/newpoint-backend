using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using NewPoint.Models;
using NewPoint.Models.Requests;
using NewPoint.Services;

namespace NewPoint.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Response>> Register([FromBody] RegisterRequest request)
    {
        var response = new Response();
        try
        {
            var login = request.Login.Trim();

            if (login.Length < 4)
            {
                response.Error = "Username must be at least 4 symbols";
                return BadRequest(response);
            }

            if (login.Length > 32)
            {
                response.Error = "Username's length can't be over 32 symbols";
                return BadRequest(response);
            }

            var password = request.Password.Trim();

            if (password.Length < 8)
            {
                response.Error = "Password's length must be at least 8 symbols";
                return BadRequest(response);
            }

            if (password.Length > 32)
            {
                response.Error = "Password's length can't be over 32 symbols";
                return BadRequest(response);
            }

            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");

            if (!hasNumber.IsMatch(password))
            {
                response.Error = "Password must contain at least 1 digit";
                return BadRequest(response);
            }

            if (!hasUpperChar.IsMatch(password))
            {
                response.Error = "Password must contain at least 1 capital letter";
                return BadRequest(response);
            }

            if (password.Length > 32)
            {
                response.Error = "Password's length can't be over 32 symbols";
                return BadRequest(response);
            }

            var email = request.Email.Trim();
            var phone = request.Phone.Trim();

            if (email.Length == 0 && phone.Length == 0)
            {
                response.Error = "You must provide either email or phone number";
                return BadRequest(response);
            }

            var user = new User
            {
                Login = login, Name = request.Name, Surname = request.Surname, Email = email, Phone = phone,
                BirthDate = request.BirthDate
            };

            if (await _userService.LoginExists(login))
            {
                response.Error = "User with this name already exists";
                return BadRequest(response);
            }

            _userService.AssignPasswordHash(user, password);

            var token = _userService.CreateToken(user);

            user.Token = token;

            await _userService.InsertUser(user);

            Response.Headers.Add("token", token);

            var dataEntry = new DataEntry<User>()
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

    [HttpPost("login")]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Response>> Login([FromBody] LoginRequest request)
    {
        var response = new Response();
        try
        {
            var login = request.Login.Trim();
            var password = request.Password.Trim();

            var user = await _userService.GetUserByLogin(login);
            user.HashedPassword = await _userService.GetUserHashedPassword(user.Login);
            
            if (await _userService.LoginExists(login) is false || _userService.VerifyPassword(user, password) is false)
            {
                response.Error = "Wrong login or password.";
                return BadRequest(response);
            }
            

            Response.Headers.Add("token", _userService.CreateToken(user));

            var dataEntry = new DataEntry<User>()
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
    
    [HttpPost("profile/edit")]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Response>> EditProfile([FromBody] EditProfileRequest request)
    {
        var response = new Response();
        try
        {
            if (request.Description.Length > 255)
            {
                response.Error = "Description length can't be over 255 symbols";
                return BadRequest(response);
            }

            var id = _userService.GetIdFromToken(Request.Headers["token"]);
            await _userService.EditProfile(id, request);
            var user = await _userService.GetProfileById(id);
            var dataEntry = new DataEntry<User>()
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