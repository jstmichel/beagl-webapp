# Beagl-WebApp

Beagl-WebApp is an open source application designed for animal centers. It centralizes the management of animals, owners, employees, provides a self-service portal, facilitates adoption and annual fee payments, and offers reporting tools.

## Main Features

- Employee and profile management
- Owner and animal tracking
- Adoption workflow
- Owner portal (self-service, annual fee payment)
- Reporting and statistics
- Authentication and role management
- Fee payment and centralized data management

## Technologies

- .NET 10 / ASP.NET Core / Blazor
- Entity Framework Core / PostgreSQL
- Docker (deployment)
- Identity with EF Core

## Quick Start

1. **Clone the repository**
	```sh
	git clone https://github.com/jstmichel/beagl-webapp.git
	```

2. **Install dependencies**
	Follow the specific instructions in each project (`README.fr.md`).

3. **Run the web application**
	```sh
	dotnet run --project src/Beagl.WebApp
	```

## PostgreSQL Database with Docker

1. **Create a `.env` file at the root**
	```
	POSTGRES_PASSWORD=strong-password
	```
	Replace with a secure password.

2. **Start the database**
	```sh
	docker-compose up -d
	```

3. **Stop the database**
	```sh
	docker-compose down
	```

## User Secrets Management

For development, use [dotnet user-secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) to provide admin credentials and the connection string.

Example:
```sh
dotnet user-secrets init
dotnet user-secrets set "SeedData:SeedUser:Email" "admin@localhost"
dotnet user-secrets set "SeedData:SeedUser:Password" "strong-password"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=beagl-webapp;Username=beagl_admin;Password=strong-password"
```
> Note: The password used in the connection string (user-secrets) must be exactly the same as the one defined in the `.env` file (`POSTGRES_PASSWORD`). Both locations must be synchronized to allow the application to connect properly to the database created by Docker.

## Entity Framework Core & Migrations

Install the EF Core tool:
```sh
dotnet tool install --global dotnet-ef
```

Common commands:
- Add a migration:
  ```sh
  dotnet ef migrations add <MigrationName> --project src/Beagl.Infrastructure --startup-project src/Beagl.WebApp
  ```
- Update the database:
  ```sh
  dotnet ef database update --project src/Beagl.Infrastructure --startup-project src/Beagl.WebApp
  ```
- Remove the last migration:
  ```sh
  dotnet ef migrations remove --project src/Beagl.Infrastructure --startup-project src/Beagl.WebApp
  ```

## Front-end Dependencies Management (npm)

This project uses [npm](https://www.npmjs.com/) to manage front-end libraries.

Common commands:
- Install dependencies:
	```sh
	npm install
	```
- Add a library:
	```sh
	npm install <library-name>
	```
- Update dependencies:
	```sh
	npm update
	```
- Remove a library:
	```sh
	npm uninstall <library-name>
	```

Dependency configuration is found in the `package.json` file at the project root or in the relevant folder.

## Tests

To run unit and integration tests:
```sh
dotnet test
```

## Contribution

Contributions are welcome!
See [CONTRIBUTING.md](CONTRIBUTING.md) for rules and process.

## Security

To report a vulnerability, see [SECURITY.md](SECURITY.md).

## License

This project is licensed under the [MIT](LICENSE) license.
