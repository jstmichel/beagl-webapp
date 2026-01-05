// MIT License - Copyright (c) 2025 Jonathan St-Michel

IDistributedApplicationBuilder builder =
    DistributedApplication.CreateBuilder(args);

IResourceBuilder<IResourceWithConnectionString> connectionString =
    builder.AddConnectionString("beagl-db");

// Register PostgreSQL database
IResourceBuilder<PostgresServerResource> postgresServer = builder
    .AddPostgres("beagl-db-server")
    .WithHostPort(5432);

IResourceBuilder<PostgresDatabaseResource> postgresdb = postgresServer
    .WithConnectionStringRedirection(connectionString.Resource)
    .WithVolume("postgres-data", "/var/lib/postgresql/data")
    .AddDatabase("beagl-database");

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
