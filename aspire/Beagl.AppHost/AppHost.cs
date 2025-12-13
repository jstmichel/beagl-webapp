// MIT License - Copyright (c) 2025 Jonathan St-Michel

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

builder.AddProject("beagl-webapp", "../../src/Beagl.WebApp");

// builder.AddDockerfile("beagl-webapp-docker", "../../src/Beagl.WebApp")
//     .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
//     .WithHttpEndpoint(8080, 8080);

builder.Build().Run();
