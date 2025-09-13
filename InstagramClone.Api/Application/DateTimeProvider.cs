namespace InstagramClone.Api.Application;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime GetDateTime()
    {
        return DateTime.UtcNow;
    }
}
