using System.Text.RegularExpressions;
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
    private readonly ILogger<PostController> _logger;

    public PostController(IPostService postService, ILogger<PostController> logger)
    {
        _postService = postService;
        _logger = logger;
    }

    [HttpPost("get")]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Response>> Get()
    {
        var response = new Response();
        try
        {
            var posts = await _postService.GetPosts();

            var dataEntry = new DataEntry<List<Post>>()
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
}