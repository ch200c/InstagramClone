var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.InstagramClone_Api>("api");

await builder.Build().RunAsync();
