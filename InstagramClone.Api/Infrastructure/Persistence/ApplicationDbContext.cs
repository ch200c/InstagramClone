﻿using InstagramClone.Api.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InstagramClone.Api.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public DbSet<UserBucketName> UserBucketNames { get; set; }

    public DbSet<UserMedia> UserMedia { get; set; }

    public DbSet<UserFollowing> UserFollowing { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<UserBucketName>().HasIndex(u => u.UserId);
    }
}
