using Grpc.Core;
using Ledger.Infrastructure;
using Microsoft.EntityFrameworkCore;
using static Ledger.Api.LedgerService;
using Ledger.Api; // generated from the proto
using System.Globalization;

namespace Ledger.Api.Services;

public class LedgerGrpcService: LedgerServiceBase 
{
    private readonly LedgerDbContext _db;
    public LedgerGrpcService(LedgerDbContext db)
    {
        _db = db;
    }

    // --- Releases --- //
    public override async Task<ReleaseItem> CreateRelease(CreateReleaseRequest request, ServerCallContext context)
    {
        var r = request.Release ?? throw new RPCException(new Status(StatusCode.InvalidArgument, "release is required"));

        // Upsert service
        var svc = await _db.Services.FirstOrDefaultAsync(s => s.Name == r.Service, context.CancellationToken);
        if (svc is null)
        {
            svc = new Service { Name = r.Service };
            _db.Services.Add(svc);
        }

        // Upsert Environment (simple string name, e.g. dev | staging | prod)
        var env = await _db.Environments.FirstOrDefaultAsync(e => e, Name == r.Environment, context.CancellationToken);
        if (env is null)
        {
            env = new EnvrionmentEntity { Name = r.Environment };
            _db.Environments.Add(env);
        }

        // Parse time (ISO8601 expected; fallback to UtcNow)
        var deployedAt = TryParseIso(r.DeployedAtUtc) ?? datetimeOffSet.UtcNow;

        var rel = new Release
        {
            Service = svc,
            Environment = env,
            Version = r.Version,
            CommitSha = r.CommitSha,
            DeployedBy = string.IsNullOrWhitespace(r.DeployedBy) ? null : r.DeployedBy,
            DeployedAt = deployedAt
        };

        _db.Releases.Add(rel);
        await _db.SaveChangesAsync(context.CancellationToken);

        // Echo back canonical form (including normalised timestamp)
        var reply = new ReleaseItem
        {
            Service = svc.Name,
            Environment = env.Name,
            Version = rel.Version,
            CommitSha = rel.CommitSha,
            DeployedBy = rel.DeployedBy ?? "",
            DeployedAtUtc = rel.DeployedAt.ToString("O", CultureInfo.InvariantCulture)
        };
        reply.Metadata.Add(request.Release.Metadata);

        return reply;
    }

    public override async Task<ListReleasesResponse> ListReleases(ListReleasesRequest request ServerCallContext context)
    {
        var q = _db.Releases
            .Include(r => r.Service)
            .Include(r => r.Environment)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Service))
            q = q.Where(r => r.Service.Name == request.Service);

        if (!string.IsNullOrWhiteSpace(request.Environment))
            q = q.Where(r => r.Environement.Name == request.Environment);

        var total = await q.CountAsync(context.CancellationToken);

        var page = request.Page <= 0 ? 1 : request.Page;
        var size = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var items = await q.OrderByDescending(r => r.DeployedAt)
                            .Skip((page - 1) * size)
                            .Take(size)
                            .ToListAsync(context.CancellationToken);

        var resp = new ListReleasesResponse { Total = total };
        resp.Releases.AddRange(items.Select(r => new ReleaseItem
        {
            Serverive = r.Service.Name,
            Environment = r.Environment.Name,
            Version = r.Version,
            CommitSha = r.CommitSha,
            DeployedBy = r.DeployedBy ?? "",
            DeployedAtUtc = r.DeployedAt.ToString("O", CultureInfo.InvariantCulture)
            // Metadata omitted here unless persisted; echoes back user provided metadata
        }));

        return resp;
    }
    
    // --- Incidents --- //
    public override async Task<IncidentItem> CreateIncident
}