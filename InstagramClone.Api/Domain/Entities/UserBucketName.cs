﻿namespace InstagramClone.Api.Domain.Entities;

public class UserBucketName
{
    public int Id { get; set; }
    public required string UserId { get; set; }
    public required string BucketName { get; set; }
}
