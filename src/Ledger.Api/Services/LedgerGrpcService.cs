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
    public override async Task<ReleaseItem> CreateRelease(CreateReleaseRequest request, ServerCallContext context) {
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
    }
}