NB: All in all this would be the ideal structure to build (but the priority is testing gRPC in .NET first)

See: https://learn.microsoft.com/en-us/aspnet/core/grpc/basics?view=aspnetcore-9.0 

1. SQL Database (Source of Truth)

Purpose: Holds the core, authoritative data about deployments, changes, incidents, and services.

Tech: SQL Server (Microsoft native)

Used by:
- gRPC API backend (read/write)
- Internal reporting / BI
- Optional REST endpoints

Schema examples:
- Services – list of microservices tracked
- Environments – dev/staging/prod
- Releases – each deployment event (version, commit, timestamp, who)
- Changes – individual items within a release (e.g., migration, feature toggle)
- Incidents – logged production issues and their correlations to releases
- This is the system of record — relational DB ensures consistency and strong querying.

// Using docker here! `docker compose up -d`

2. gRPC API (Backend Core)

dotnet new grpc -n Ledger.Api

Purpose: The heart of the system — all ingestion and core business logic lives here.

Tech: ASP.NET Core gRPC

Responsibilities:
- Accept deployment/incident data from connectors (GitHub, GitLab, Azure DevOps, etc.).
- Validate and persist to SQL Server.
- Provide read APIs for consumers (UI, CLI, external tools).
- Optional server streaming for real-time feeds.
- This is where we will showcase gRPC contracts, service design, and clean architecture.

3. gRPC-Web + Frontend Dashboard

Purpose: Human-friendly view into what’s happening — timelines, deployments, incident correlations.

Tech Choices: React + gRPC-Web → browser calls gRPC through gRPC-Web middleware.

Main UI Features:
- View all services and environments.
- Timeline of deployments & incidents.
- Filter by service, environment, version, commit SHA.

(Future) live updates (SignalR or gRPC server-streaming).

The frontend is read-only at first, so no complex forms — just visualizing data.

4. Cloud Connectors (Ingestion Adapters)

Purpose: Collect deployment and pipeline events from external systems and send them to your gRPC backend. Each connector translates its provider's native webhook or pipeline output into your standard gRPC ingestion format.

- GitHub Actions: Custom step in a workflow to POST to the gRPC API or call directly via gRPC.
- GitLab:  pipeline job calls ingestion endpoint (Same approach).
- Azure DevOps: REST or gRPC call from pipeline or webhook.
- Jenkins: curl or gRPC client step in post-deploy phase.

NB: ingestion contract stays the same, no matter which provider is upstream.
This makes the backend cloud-agnostic and future-proof.



| Project Name                          | Type                       | Purpose                                                                      |
| ------------------------------------- | -------------------------- | ---------------------------------------------------------------------------- |
| **Ledger.Api**                        | ASP.NET Core gRPC Web Host | Exposes gRPC services + REST endpoints for the frontend or external systems. |
| **Ledger.Infrastructure**             | Class Library              | Data access layer: EF Core DbContext, migrations, repository code.           |
| **Ledger.Core** *(optional at first)* | Class Library              | Domain models, core business rules, interfaces shared across layers.         |
| **Ledger.UnitTests**                  | Test Project               | Unit and integration tests.                                                  |


Dotnet build pipeline phases:
// NB: This requires your base protos, services and program to be written to compile the rpc parts of gRPC
1. Restore:	Downloads all NuGet packages (Grpc.Tools, EF Core, etc.).
2. Generate gRPC cod:	Uses Grpc.Tools to read ledger.proto and generate C# classes like LedgerServiceBase, LedgerServiceClient, and your message classes (ReleaseItem, IncidentItem).
3. Compile:	Compiles your C# files (Program.cs, LedgerGrpcService.cs, EF Core models, etc.) together with the generated gRPC code.
4. Link and output:	Produces a .dll and executable in bin/Debug/net9.0/.
5. Check for errors: If there are syntax errors or missing references, it fails here.


`dotnet new classlib -n Ledger.Infrastructure`

// Sets up sub projects to be called into main project

# If Ledger.sln exists but is empty/corrupt, recreate it:
del .\Ledger.sln
dotnet new sln -n Ledger

# Add your projects to the solution
dotnet sln Ledger.sln add .\src\Ledger.Api\Ledger.Api.csproj
dotnet sln Ledger.sln add .\src\Ledger.Infrastructure\Ledger.Infrastructure.csproj

dotnet add .\src\Ledger.Api\Ledger.Api.csproj reference .\src\Ledger.Infrastructure\Ledger.Infrastructure.csproj
