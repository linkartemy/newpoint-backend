using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewPoint.Models;
using NewPoint.Models.Requests;
using NewPoint.Services;

namespace NewPoint.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly IUserService _userService;
    private readonly ILogger<PostController> _logger;

    public PostController(IPostService postService, IUserService userService, ILogger<PostController> logger)
    {
        _postService = postService;
        _userService = userService;
        _logger = logger;
    }

    [Authorize]
    [HttpPost("get")]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Response>> Get()
    {
        var response = new Response();
        try
        {
            var posts = (await _postService.GetPosts()).OrderByDescending(post => post.CreationTimestamp).Select(
                async post =>
                {
                    var user = await _userService.GetPostUserDataById(post.AuthorId);
                    if (user is null)
                    {
                        post.Login = "Unknown";
                        post.Name = "Unknown";
                        post.Surname = "";
                    }
                    else
                    {
                        post.Login = user.Login;
                        post.Name = user.Name;
                        post.Surname = user.Surname;
                        post.Liked = await _postService.IsLikedByUser(post.Id, user.Id);
                    }

                    return post;
                }).Select(post => post.Result).ToList();

            var dataEntry = new DataEntry<List<Post>>
            {
                Data = posts,
                Type = "posts"
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
    [HttpPost("get/post")]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Response>> GetPost([FromBody] GetPostRequest request)
    {
        var response = new Response();
        try
        {
            var post = await _postService.GetPost(request.Id);


            var user = await _userService.GetPostUserDataById(post.AuthorId);
            if (user is null)
            {
                post.Login = "Unknown";
                post.Name = "Unknown";
                post.Surname = "";
            }
            else
            {
                post.Login = user.Login;
                post.Name = user.Name;
                post.Surname = user.Surname;
                post.Liked = await _postService.IsLikedByUser(post.Id, user.Id);
            }

            var dataEntry = new DataEntry<Post>
            {
                Data = post,
                Type = "post"
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