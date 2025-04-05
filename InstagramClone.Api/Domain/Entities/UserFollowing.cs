namespace InstagramClone.Api.Domain.Entities;

public class UserFollowing
{
    public Guid Id { get; set; }
    public required string UserId { get; set; }
    public required string FollowerUserId { get; set; }
}
