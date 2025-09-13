namespace InstagramClone.Api.Domain.Entities;

public class UserMedia
{
    public Guid Id { get; set; }
    public required string ObjectName { get; set; }
    public required string BucketName { get; set; }
    public required string UserId { get; set; }
    public required DateTime CreatedAt { get; set; }
    public string? Title { get; set; }
}
