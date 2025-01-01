using Minio;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddMinio(client =>
{
    client
        .WithEndpoint("localhost:9000")
        .WithCredentials("ROOTUSER", "CHANGEME123")
        .WithSSL(false)
        .Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
