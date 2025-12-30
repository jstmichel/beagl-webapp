// MIT License - Copyright (c) 2025 Jonathan St-Michel

IDistributedApplicationBuilder builder =
    DistributedApplication.CreateBuilder(args);

// Register PostgreSQL database
IResourceBuilder<PostgresServerResource> postgresServer = builder.AddPostgres("postgres-server");
IResourceBuilder<PostgresDatabaseResource> postgresdb = postgresServer.AddDatabase("postgres-db");

// Register Beagl web application
builder.AddProject("beagl-webapp", "../../src/Beagl.WebApp")
    .WaitFor(postgresdb)
    .WithReference(postgresdb);

// builder.AddDockerfile("beagl-webapp-docker", "../../src/Beagl.WebApp")
//     .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
//     .WithHttpEndpoint(8080, 8080)
//     .WaitFor(postgresdb)
//     .WithReference(postgresdb);

builder.Build().Run();
