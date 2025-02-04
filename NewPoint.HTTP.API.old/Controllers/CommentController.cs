using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewPoint.Models;
using NewPoint.Models.Requests;
using NewPoint.Services;

namespace NewPoint.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentController : ControllerBase
{
    private readonly ICommentService _commentService;
    private readonly IUserService _userService;
    private readonly ILogger<CommentController> _logger;
    
    public CommentController(ICommentService commentService, IUserService userService, ILogger<CommentController> logger)
    {
        _commentService = commentService;
        _userService = userService;
        _logger = logger;
    }
    
    [Authorize]
    [HttpPost("get")]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Response>> GetComments([FromBody] GetCommentsRequest request)
    {
        var response = new Response();
        try
        {
            var comments = (await _commentService.GetCommentsByPostId(request.Id)).OrderByDescending(post => post.CreationTimestamp).Select(
                async comment =>
                {
                    var user = await _userService.GetPostUserDataById(comment.UserId);
                    if (user is null)
                    {
                        comment.Login = "Unknown";
                        comment.Name = "Unknown";
                        comment.Surname = "";
                    }
                    else
                    {
                        comment.Login = user.Login;
                        comment.Name = user.Name;
                        comment.Surname = user.Surname;
                    }

                    var token = Request.Headers["Authorization"][0]!.Split(' ')[1];
                    comment.Liked =
                        await _commentService.IsLikedByUser(comment.Id, (await _userService.GetUserByToken(token)).Id);

                    return comment;
                }).Select(comment => comment.Result).ToList();
            var dataEntry = new DataEntry<List<Comment>>
            {
                Data = comments,
                Type = "comments"
            };
            response.Data = new[] { dataEntry };
            return Ok(response);
        }
        catch (Exception)
        {
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return StatusCode(500, response.Error);
        }
    }
    
    [Authorize]
    [HttpPost("add")]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Response>> Add([FromBody] CommentAddRequest request)
    {
        var response = new Response();
        try
        {
            var token = Request.Headers["Authorization"][0]!.Split(' ')[1];
            var user = await _userService.GetUserByToken(token);
            if (user == null)
            {
                response.Error = "User doesn't exist. Server error. Please contact with us";
                return BadRequest(response);
            }

            await _commentService.Add(request.Id, user.Id, request.Content);

            var dataEntry = new DataEntry<bool>
            {
                Data = true,
                Type = "bool"
            };
            response.Data = new[] { dataEntry };
            return Ok(response);
        }
        catch (Exception)
        {
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return StatusCode(500, response.Error);
        }
    }
    
    [Authorize]
    [HttpPost("like")]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Response>> Like([FromBody] CommentLikeRequest request)
    {
        var response = new Response();
        try
        {
            var token = Request.Headers["Authorization"][0]!.Split(' ')[1];
            var user = await _userService.GetUserByToken(token);
            if (user == null)
            {
                response.Error = "User doesn't exist. Server error. Please contact with us";
                return BadRequest(response);
            }

            await _commentService.Like(request.Id, user.Id);

            var dataEntry = new DataEntry<bool>
            {
                Data = true,
                Type = "bool"
            };
            response.Data = new[] { dataEntry };
            return Ok(response);
        }
        catch (Exception)
        {
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return StatusCode(500, response.Error);
        }
    }
    
    [Authorize]
    [HttpPost("unlike")]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Response>> UnLike([FromBody] CommentLikeRequest request)
    {
        var response = new Response();
        try
        {
            var token = Request.Headers["Authorization"][0]!.Split(' ')[1];
            var user = await _userService.GetUserByToken(token);
            if (user == null)
            {
                response.Error = "User doesn't exist. Server error. Please contact with us";
                return BadRequest(response);
            }

            await _commentService.UnLike(request.Id, user.Id);

            var dataEntry = new DataEntry<bool>
            {
                Data = false,
                Type = "bool"
            };
            response.Data = new[] { dataEntry };
            return Ok(response);
        }
        catch (Exception)
        {
            response.Error = "Something went wrong. Please try again later. We are sorry";
            return StatusCode(500, response.Error);
        }
    }
}