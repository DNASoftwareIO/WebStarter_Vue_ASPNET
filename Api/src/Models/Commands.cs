using System.ComponentModel.DataAnnotations;

// Disabling warning because all properties that have warnings have a Required attribute and won't be null when used in a controller.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

public class RegisterUserCommand
{
  [Required]
  public string UserName { get; set; }
  [Required]
  public string Password { get; set; }
  [Required]
  public string Email { get; set; }
  public string? PromoCode { get; set; }
}

public class LoginUserCommand
{
  [Required]
  public string UserName { get; set; }
  [Required]
  public string Password { get; set; }
  public string? TfaCode { get; set; }
}

public struct RefreshJwtCommand
{
  [Required]
  public Guid RefreshToken { get; set; }
}

public struct EndSessionCommand
{
  [Required]
  public Guid Id { get; set; }
}

public struct ChangePasswordCommand
{
  [Required]
  public string OldPassword { get; set; }

  [Required]
  public string NewPassword { get; set; }

  public string? TfaCode { get; set; }
}

public struct ToggleTfaCommand
{
  [Required]
  public string TfaCode { get; set; }
}

public struct ForgotPasswordCommand
{
  [Required]
  public string Email { get; set; }
}

public struct ResetPasswordCommand
{
  [Required]
  public string Token { get; set; }

  [Required]
  public string Password { get; set; }

  [Required]
  public string Email { get; set; }
}

public struct ConfirmEmailCommand
{
  [Required]
  public string UserId { get; set; }

  [Required]
  public string Token { get; set; }
}

public struct SetEmailCommand
{
  [Required]
  public string Email { get; set; }
  public string? TfaCode { get; set; }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
