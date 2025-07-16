using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

[ApiController]
public class UserController : ControllerBase
{
  private readonly UserManager<User> _userManager;
  private readonly ApplicationDbContext _context;
  private readonly IConfiguration _configuration;
  private readonly EmailService _emailService;

  public UserController(UserManager<User> userManager, ApplicationDbContext context, IConfiguration configuration, EmailService emailService)
  {
    _userManager = userManager;
    _context = context;
    _configuration = configuration;
    _emailService = emailService;
  }

  private async Task<Session> CreateSession(Guid userId)
  {
    var session = new Session
    {
      Id = Guid.NewGuid(),
      RefreshToken = Guid.NewGuid(),
      UserId = userId,
      CreatedAt = DateTime.UtcNow,
      ExpiredAt = DateTime.UtcNow.AddDays(28),
      // TODO get from http headers
      // Country = ,
      // IpAddress = ,
      // UserAgent = ,
    };

    await _context.AddAsync(session);

    return session;
  }

  private string CreateJwt(User user, Guid sessionId)
  {
    var authClaims = new List<Claim>
    {
      new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
      new (JwtRegisteredClaimNames.Name, user.UserName ?? ""),
      new ("sessionId", sessionId.ToString())
    };

    var authKey = _configuration["JWTSettings:Key"];
    if (string.IsNullOrEmpty(authKey))
      throw new ArgumentNullException();

    var symetricKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authKey));

    var token = new JwtSecurityToken(
        issuer: _configuration["JWTSettings:Issuer"],
        audience: _configuration["JWTSettings:Audience"],
        claims: authClaims,
        expires: DateTime.UtcNow.AddMinutes(5),
        signingCredentials: new SigningCredentials(symetricKey, SecurityAlgorithms.HmacSha256)
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
  }

  [HttpPost]
  [Route("[controller]/register")]
  public async Task<IActionResult> Register([FromBody] RegisterUserCommand cmd)
  {
    var user = new User
    {
      UserName = cmd.UserName.Trim(),
      Email = cmd.Email?.Trim().ToLower(),
      DateJoined = DateTime.UtcNow,
      PromoCode = cmd.PromoCode
    };

    var result = await _userManager.CreateAsync(user, cmd.Password);
    if (!result.Succeeded)
    {
      return StatusCode(StatusCodes.Status400BadRequest, "Error registering account. Please try again later.");
    }

    var session = await CreateSession(user.Id);

    await _context.SaveChangesAsync();

    await _emailService.SendEmailConfirmationToken(user);

    var jwt = CreateJwt(user, session.Id);

    return new JsonResult(new { jwt, refreshToken = session.RefreshToken, sessionId = session.Id, user = new UserDto(user) });
  }

  [HttpPost]
  [Route("[controller]/login")]
  public async Task<IActionResult> Login([FromBody] LoginUserCommand cmd)
  {
    var user = await _userManager.FindByNameAsync(cmd.UserName.Trim());
    if (user == null)
    {
      // Don't return username/email already used messages to prevent account enumeration attacks
      return StatusCode(StatusCodes.Status400BadRequest, "Error logging in. Please try again later.");
    }

		if (await _userManager.IsLockedOutAsync(user))
    {
        return StatusCode(StatusCodes.Status400BadRequest, "Error logging in. Please try again later.");
    }

    var result = await _userManager.CheckPasswordAsync(user, cmd.Password);
    if (!result)
    {

			await _userManager.AccessFailedAsync(user);
      // Don't return username/email already used messages to prevent account enumeration attacks
      return StatusCode(StatusCodes.Status400BadRequest, "Error logging in. Please try again later.");
    }

    if (user.Deleted)
    {
      // Don't return username/email already used messages to prevent account enumeration attacks
      return StatusCode(StatusCodes.Status400BadRequest, "Error logging in. Please try again later.");
    }

    if (user.TwoFactorEnabled)
    {
      // TODO If the user has tfa required we just ask them to send login request again with TfaCode.
      // Some sites will redirect to a login with tfa page and store session between requests.
      // I think the way here is the simplest way to do this especially with spa frontends.
      // If there's a good security reason to do things differently then we can change this.
      if (string.IsNullOrEmpty(cmd.TfaCode))
      {
        // TODO Is this the best status to send?
        return StatusCode(StatusCodes.Status400BadRequest, "tfa required");
      }

      // TODO check if tfa code has already been used!
      var tfaResult = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, cmd.TfaCode);
      if (!tfaResult)
      {
        return StatusCode(StatusCodes.Status400BadRequest, "Invalid tfa code");
      }
    }

		await _userManager.ResetAccessFailedCountAsync(user);

    var session = await CreateSession(user.Id);

    await _context.SaveChangesAsync();

    var jwt = CreateJwt(user, session.Id);

    return new JsonResult(new { jwt, refreshToken = session.RefreshToken, sessionId = session.Id, user = new UserDto(user) });
  }

  [HttpPost]
  [Route("[controller]/refresh-token")]
  public async Task<ActionResult> RefreshJwt([FromBody] RefreshJwtCommand cmd)
  {
    var session = await _context.Sessions.FirstOrDefaultAsync(x => x.RefreshToken == cmd.RefreshToken && x.ExpiredAt > DateTime.UtcNow);
    if (session == null)
    {
      return StatusCode(StatusCodes.Status400BadRequest, "Session not found or expired.");
    }

    var user = await _userManager.FindByIdAsync(session.UserId.ToString());
    if (user == null)
    {
      return StatusCode(StatusCodes.Status400BadRequest, "User not found."); // TODO needed? Should never get here
    }

    if (user.Deleted)
    {
      return StatusCode(StatusCodes.Status400BadRequest, "Session not found or expired.");
    }

    var jwt = CreateJwt(user, session.Id);
    return new JsonResult(new { jwt });
  }

  [HttpGet]
  [Authorize]
  [Route("[controller]/get-sessions")]
  public async Task<IActionResult> GetSessions()
  {
    var userId = User.GetId();
    var sessions = await _context.Sessions.Where(x => x.UserId == userId && x.ExpiredAt > DateTime.UtcNow).OrderByDescending(x => x.CreatedAt).Select(
        x => new
        {
          // do not return refresh token of the session
          id = x.Id,
          date = x.CreatedAt,
          ipAddress = x.IpAddress,
          device = x.UserAgent,
          country = x.Country
        }
    ).ToListAsync();

    return new JsonResult(new { sessions });
  }

  [HttpPost]
  [Authorize]
  [Route("[controller]/logout")]
  public async Task<IActionResult> Logout([FromBody] EndSessionCommand cmd)
  {
    var session = await _context.Sessions.FirstOrDefaultAsync(x => x.Id == cmd.Id && x.ExpiredAt > DateTime.UtcNow);
    if (session == null)
    {
      return StatusCode(StatusCodes.Status400BadRequest, "Session not found.");
    }

    session.ExpiredAt = DateTime.UtcNow;

    await _context.SaveChangesAsync();

    return Ok();
  }

  [HttpPost]
  [Authorize]
  [Route("[controller]/end-all-sessions")]
  public async Task<IActionResult> EndAllSessions()
  {
    var userId = User.GetId();
    var sessions = await _context.Sessions.Where(x => x.UserId == userId && x.ExpiredAt > DateTime.UtcNow).ToListAsync();
    if (sessions.Count == 0)
    {
      return StatusCode(StatusCodes.Status400BadRequest, "Sessions not found.");
    }

    for (var i = 0; i < sessions.Count; i++)
    {
      sessions[i].ExpiredAt = DateTime.UtcNow;
    }

    await _context.SaveChangesAsync();

    return Ok();
  }

  [HttpPost]
  [Authorize]
  [Route("[controller]/change-password")]
  public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand cmd)
  {
    var userId = User.GetId();
    var user = await _userManager.FindByIdAsync(userId.ToString());
    if (user == null)
    {
      return StatusCode(StatusCodes.Status400BadRequest, "User not found.");
    }

    if (user.TwoFactorEnabled)
    {
      if (string.IsNullOrEmpty(cmd.TfaCode))
      {
        return StatusCode(StatusCodes.Status400BadRequest, "tfa required");
      }

      // TODO check tfa code has not been used
      var tfaResult = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, cmd.TfaCode);
      if (!tfaResult)
      {
        return StatusCode(StatusCodes.Status400BadRequest, "tfa code invalid");
      }
    }

    var result = await _userManager.ChangePasswordAsync(user, cmd.OldPassword, cmd.NewPassword);
    if (!result.Succeeded)
    {
      return StatusCode(StatusCodes.Status400BadRequest, "Error changing password.");
    }

    var currentSessionId = User.GetSessionId();
    var sessions = await _context.Sessions.Where(x => x.UserId == userId && x.ExpiredAt > DateTime.UtcNow && x.Id != currentSessionId).ToListAsync();
    foreach (var session in sessions)
    {
      session.ExpiredAt = DateTime.UtcNow;
    }

    await _userManager.ResetAccessFailedCountAsync(user);

    await _context.SaveChangesAsync();

    return Ok();
  }

  // TODO This can probably be written better, it was copied from an old .net project which may have copied from SO. Asp .NET Core may have a better way to handle
  private string FormatTfaKey(string unformattedKey)
  {
    var result = new StringBuilder();
    var currentPosition = 0;
    while (currentPosition + 4 < unformattedKey.Length)
    {
      result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
      currentPosition += 4;
    }

    if (currentPosition < unformattedKey.Length)
    {
      result.Append(unformattedKey.Substring(currentPosition));
    }

    return result.ToString().ToLowerInvariant();
  }

  // TODO This can probably be written better, it was copied from an old .net project which may have copied from SO
  private string GenerateQRCodeUri(string userName, string unformattedKey)
  {
    var AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}";

    return string.Format(
        AuthenticatorUriFormat,
        _configuration["WebClientUrl"],
        userName,
        unformattedKey);
  }

  [HttpGet]
  [Authorize]
  [Route("[controller]/get-tfa-key")]
  public async Task<IActionResult> GetTfaKey()
  {
    var userId = User.GetId();
    var user = await _userManager.FindByIdAsync(userId.ToString());
    if (user == null || string.IsNullOrEmpty(user.UserName))
    {
      return StatusCode(StatusCodes.Status400BadRequest, "User not found.");
    }

    // don't return key to someone who has tfa enabled as an attacker could use to disable
    if (user.TwoFactorEnabled)
    {
      return StatusCode(StatusCodes.Status400BadRequest, "Cant get key if tfa enabled.");
    }

    var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
    if (string.IsNullOrEmpty(unformattedKey))
    {
      await _userManager.ResetAuthenticatorKeyAsync(user);
      unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
    }

    if (string.IsNullOrEmpty(unformattedKey))
    {
      return StatusCode(StatusCodes.Status400BadRequest, "Error getting key.");
    }

    var key = FormatTfaKey(unformattedKey);

    var authenticatorUri = GenerateQRCodeUri(user.UserName, key);

    return new JsonResult(new { tfaKey = key, uri = authenticatorUri });
  }

  [HttpPost]
  [Authorize]
  [Route("[controller]/toggle-tfa")]
  public async Task<IActionResult> ToggleTfa([FromBody] ToggleTfaCommand cmd)
  {
    var userId = User.GetId();
    var user = await _userManager.FindByIdAsync(userId.ToString());
    if (user == null)
    {
      return StatusCode(StatusCodes.Status400BadRequest, "User not found.");
    }

    var verifyResult = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, cmd.TfaCode);
    if (!verifyResult)
    {
      return StatusCode(StatusCodes.Status400BadRequest, "Code not valid.");
    }

    var setResult = await _userManager.SetTwoFactorEnabledAsync(user, !user.TwoFactorEnabled);
    if (!setResult.Succeeded)
    {
      throw new Exception(); 
    }

    await _context.SaveChangesAsync();

    return Ok();
  }

  [HttpPost]
  [Route("[controller]/forgot-password")]
  public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand cmd)
  {
    var user = await _userManager.FindByEmailAsync(cmd.Email);
    if (user == null)
    {
      // return ok so that user doesn't learn that email is used by someone
      return Ok();
    }

    // TODO throttle so that same email is not sent multiple times
    await _emailService.SendPasswordResetToken(user);

    return Ok();
  }

  [HttpPost]
  [Route("[controller]/reset-password")]
  public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand cmd)
  {
    var user = await _userManager.FindByEmailAsync(cmd.Email); 
    if (user == null)
    {
      // TODO do we need to do anything extra here to protect against enumeration attacks on this endpoint?
      return StatusCode(StatusCodes.Status400BadRequest, "Error resetting password. Please try again later.");
    }

    cmd.Token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(cmd.Token));

    var result = await _userManager.ResetPasswordAsync(user, cmd.Token, cmd.Password);
    if (!result.Succeeded)
    {
      return StatusCode(StatusCodes.Status400BadRequest, "Error resetting password. Please try again later.");
    }

    await _context.SaveChangesAsync();

    return Ok();
  }

  [HttpPost]
  [Authorize]
  [Route("[controller]/send-confirm-email-link")]
  public async Task<IActionResult> SendConfirmEmailLink()
  {
    var userId = User.GetId();
    var user = await _userManager.FindByIdAsync(userId.ToString());

    if (user == null || string.IsNullOrEmpty(user.Email) || user.EmailConfirmed)
    {
      return StatusCode(StatusCodes.Status400BadRequest, "Error sending email. Please try again later.");
    }

    // TODO throttle so that same email is not sent multiple times
    await _emailService.SendEmailConfirmationToken(user);

    return Ok();
  }

  [HttpPost]
  [Route("[controller]/confirm-email")]
  public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailCommand cmd)
  {
    var user = await _userManager.FindByIdAsync(cmd.UserId);
    if (user == null)
    {
      return StatusCode(StatusCodes.Status400BadRequest, "User not found.");
    }

    cmd.Token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(cmd.Token));

    var result = await _userManager.ConfirmEmailAsync(user, cmd.Token);
    if (!result.Succeeded)
    {
      return StatusCode(StatusCodes.Status400BadRequest, "Token invalid.");
    }

    await _context.SaveChangesAsync();

    return Ok();
  }

  [HttpPost]
  [Authorize]
  [Route("[controller]/set-email")]
  public async Task<IActionResult> SetEmail([FromBody] SetEmailCommand cmd)
  {
    var userId = User.GetId();
    var user = await _userManager.FindByIdAsync(userId.ToString());
    if (user == null)
    {
      return StatusCode(StatusCodes.Status400BadRequest, "User not found."); // TODO needed? Should never hit
    }

    var existingUser = await _userManager.FindByEmailAsync(cmd.Email.Trim());
    if (existingUser != null)
    {
      return StatusCode(StatusCodes.Status400BadRequest, "Error setting email. Please try again later."); // TODO potential enumeration attack?
    }

    if (user.TwoFactorEnabled)
    {
      if (string.IsNullOrEmpty(cmd.TfaCode))
      {
        return new JsonResult(new { success = false, errorMessage = "tfa required" });
      }

      // TODO check tfa code has not been used
      var tfaResult = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, cmd.TfaCode);
      if (!tfaResult)
      {
        return StatusCode(StatusCodes.Status400BadRequest, "Invalid tfa code");
      }
    }

    var result = await _userManager.SetEmailAsync(user, cmd.Email.Trim());
    if (!result.Succeeded)
    {
      return StatusCode(StatusCodes.Status400BadRequest, "Error setting email. Please try again later.");
    }

    await _context.SaveChangesAsync();

    // TODO throttle so that same email is not sent multiple times
    await _emailService.SendEmailConfirmationToken(user);

    return Ok();
  }
}

