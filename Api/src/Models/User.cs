using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

public class User : IdentityUser<Guid>
{
  public bool Deleted { get; set; }
  public DateTime DateJoined { get; set; }
  public string? PromoCode { get; set; }
}

public static class ClaimsPrincipalExtensions
{
  public static Guid GetId(this ClaimsPrincipal user)
  {
    var id = user.FindFirst(ClaimTypes.NameIdentifier) ?? throw new ArgumentNullException();
    return new Guid(id.Value);
  }

  public static Guid? GetSessionId(this ClaimsPrincipal user)
  {
    var sessionIdClaim = user.Claims.FirstOrDefault(c => c.Type == "sessionId");
    if (sessionIdClaim != null && Guid.TryParse(sessionIdClaim.Value, out var sessionId))
    {
      return sessionId;
    }
    return null;
  }
}

public class UserDto
{
  public Guid Id { get; set; }
  public string? UserName { get; set; }
  public DateTime DateJoined { get; set; }
  public string? Email { get; set; }
  public bool EmailConfirmed { get; set; }
  public bool TfaEnabled { get; set; }

  public UserDto(User user)
  {
    Id = user.Id;
    UserName = user.UserName;
    DateJoined = user.DateJoined;
    Email = user.Email;
    EmailConfirmed = user.EmailConfirmed;
    TfaEnabled = user.TwoFactorEnabled;
  }
}
