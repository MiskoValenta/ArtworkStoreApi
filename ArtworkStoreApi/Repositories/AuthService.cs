using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ArtworkStoreApi.DTOs;
using ArtworkStoreApi.Models;
using ArtworkStoreApi.Utils;
using AutoMapper;
using Microsoft.IdentityModel.Tokens;


namespace ArtworkStoreApi.Repositories;

public interface IAuthService
{
    Task<ResultDto<string>> LoginAsync(LoginRequestDto request);
    Task<ResultDto<UserDto>> RegisterAsync(RegisterRequestDto request);
    Task<ResultDto<UserDto>> GetCurrentUserAsync(int userId);
}

public class AuthService : IAuthService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IEmailSender _emailSender;
    private readonly IMapper _mapper;
    private readonly IAppLogger _logger;

    public AuthService(
        IGenericRepository<User> userRepository,
        IConfiguration configuration,
        IEmailSender emailSender,
        IMapper mapper,
        IAppLogger logger)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _emailSender = emailSender;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ResultDto<string>> LoginAsync(LoginRequestDto request)
    {
        try
        {
            var user = (await _userRepository.FindAsync(u => u.Email == request.Email))
                .FirstOrDefault();

            if (user == null)
                return ResultDto<string>.Failure("Invalid email or password");

            if (!VerifyPassword(request.Password, user.PasswordHash))
                return ResultDto<string>.Failure("Invalid email or password");

            if (!user.IsActive)
                return ResultDto<string>.Failure("Account is disabled");

            var token = GenerateJwtToken(user);
            _logger.LogInfo($"User {user.Email} logged in successfully");

            return ResultDto<string>.Success(token, "Login successful");
        }
        catch (Exception ex)
        {
            _logger.LogError("Login failed", ex);
            return ResultDto<string>.Failure("Login failed", ex.Message);
        }
    }

    public async Task<ResultDto<UserDto>> RegisterAsync(RegisterRequestDto request)
    {
        try
        {
            // Check if user already exists
            var existingUser = (await _userRepository.FindAsync(u => u.Email == request.Email))
                .FirstOrDefault();

            if (existingUser != null)
                return ResultDto<UserDto>.Failure("User with this email already exists");

            // Create new user
            var user = new User
            {
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                Role = "User",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createdUser = await _userRepository.AddAsync(user);
            var userDto = _mapper.Map<UserDto>(createdUser);

            // Send welcome email
            await _emailSender.SendWelcomeEmailAsync(user.Email);
            
            _logger.LogInfo($"New user registered: {user.Email}");
            return ResultDto<UserDto>.Success(userDto, "Registration successful");
        }
        catch (Exception ex)
        {
            _logger.LogError("Registration failed", ex);
            return ResultDto<UserDto>.Failure("Registration failed", ex.Message);
        }
    }

    public async Task<ResultDto<UserDto>> GetCurrentUserAsync(int userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return ResultDto<UserDto>.Failure("User not found");

            var userDto = _mapper.Map<UserDto>(user);
            return ResultDto<UserDto>.Success(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to get user {userId}", ex);
            return ResultDto<UserDto>.Failure("Failed to retrieve user", ex.Message);
        }
    }

    private string GenerateJwtToken(User user)
    {
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}