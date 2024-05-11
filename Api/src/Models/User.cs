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
