namespace InstagramClone.Api.Domain.Entities;

public class UserMedia
{
    public Guid Id { get; set; }
    public required string ObjectName { get; set; }
    public required string BucketName { get; set; }
}
