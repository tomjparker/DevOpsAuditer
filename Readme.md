NB: All in all this would be the ideal structure to build (but the priority is testing gRPC in .NET first)

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

2. gRPC API (Backend Core)

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