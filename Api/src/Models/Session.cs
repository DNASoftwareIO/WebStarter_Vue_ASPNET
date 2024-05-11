public class Session
{
  public Guid Id { get; set; }
  public Guid UserId { get; set; }
  public Guid RefreshToken { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime ExpiredAt { get; set; }
  public string? Country { get; set; }
  public string? IpAddress { get; set; }
  public string? UserAgent { get; set; }
}
