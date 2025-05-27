using System.Security.Claims;
using ArtworkStoreApi.DTOs;
using ArtworkStoreApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtworkStoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ResultDto<string>>> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            var validationResult = ResultDto<string>.Failure("Validation failed", errors);
            return BadRequest(validationResult);
        }

        var result = await _authService.LoginAsync(request);
        return result.IsSuccess ? Ok(result) : Unauthorized(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult<ResultDto<UserDto>>> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
            var validationResult = ResultDto<UserDto>.Failure("Validation failed", errors);
            return BadRequest(validationResult);
        }

        var result = await _authService.RegisterAsync(request);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ResultDto<UserDto>>> GetCurrentUser()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _authService.GetCurrentUserAsync(userId);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
}