# my-cinebook

My cinema booking system

## Development

This solution was developed on Windows, instruction for other OS are not detailed.

### Prerequisites

- .NET 9 [Install on Windows](https://learn.microsoft.com/en-us/dotnet/core/install/windows)
- Docker Desktop [Install on Windwos](https://docs.docker.com/desktop/setup/install/windows-install/)

### Run

This solution is configured to use .Net Aspire to orchestrate local environment.

#### Visual Studio

- Set MyCinebook.AppHost as default startup project
- Launch the *https* (or *http* if preferable) and the deafult browser should open the Aspire dashboard

#### dotnet CLI

- From the root of the project use *dotnet cli* to run the *AppHost* project: `dotnet run --project MyCinebook.AppHost\MyCinebook.AppHost.csproj`
- In the logs look for the message with the logged url for the Apire dashboard (Example: `Login to the dashboard at https://localhost:17298/login?t=0173024e9363ee53b0c76e91b8d1392b`)

### Web client to managed database

- Uncomment line **6** in the file `MyCinebook.AppHost\Program.cs` before running.
- In the Aspire dashboard look for the service name postgreSQLServer-pgadmin
- Click on this service url to open PgAdmin4 page with admin privilege to managed the databse server
- Database container has no volumes, so each launch will be reset to demo seeding data

### OpenAPI Docs and client

- In Aspire dashboard look for the 2 web api services named `bookapiservice` and `scheduleapiservice`
- On the right in the *Azioni* column click the 3 dots icon to open the context menu
- Click on the **Scalar** menu entry to open the Scalar API Reference for the service
- Navigate the endpoints documentation and look for the *Test Request* button to make request using the buil-in client

### Tests

- MyCinebook.Tests is a xUnit test suite with unit test and integration tests for the developed functionality
- Run the tests from Visual Studio or with the cli by running `dotnet test`

### Automation

- Build, test and publish are automated with GitHub Action
- Docker images are pushed to the github container registry and publicly available
	- `docker pull ghcr.io/lorenzobaronio22/my-cinebook-booking-api:latest`
	- `docker pull ghcr.io/lorenzobaronio22/my-cinebook-schedule-api:latest`
	- `docker pull ghcr.io/lorenzobaronio22/my-cinebook-migration-worker:latest`

### Project management

- The Github project is publicly available [here](https://github.com/users/lorenzobaronio22/projects/2)