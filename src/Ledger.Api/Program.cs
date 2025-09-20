using Microsoft.EntityFrameworkCore;
using Ledger.Api.Services;
using Ledger.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();

// For EF core (parses sql easily) with SQL Server
builder.Services.AddDbContext<LedgerDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("LedgerDb")));

// Temp Cors, will need to be restricted at some point
builder.Services.AddCores(o =>
{
    o.AddPolicy("grpcweb", p => p
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("grpcweb");



// Configure the HTTP request pipeline.
app.MapGrpcService<LedgerGrpcService>();

// NB: Communication with gRPC endpoints must be made through a gRPC client - we will need to make one later
app.MapGet("/", () => "Ledger API running");
app.Run();

// Making this public and partial allows us to run tests on it
public partial class Program { }