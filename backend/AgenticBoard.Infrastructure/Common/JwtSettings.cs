namespace AgenticBoard.Infrastructure.Common;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string SecretKey { get; set; } = "AgenticBoard_Super_Secret_Key_For_Development_Tokens_2026_AtLeast32BytesLong!";
    public string Issuer { get; set; } = "AgenticBoard";
    public string Audience { get; set; } = "AgenticBoard";
    public int ExpiryMinutes { get; set; } = 1440; // 24 hours
}
