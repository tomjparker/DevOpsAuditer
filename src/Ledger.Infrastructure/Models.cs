namespace Ledger.Infrastructure;

public class Service
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public ICollection<Release> Releases { get; set; } = new List<Release>();
}

public class EnvironmentEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public ICollection<Release> Releases { get; set; } = new List<Release>();
}

public class Release
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public Service Service { get; set; } = default!;
    public int EnvironmentId { get; set; }
    public EnvironmentEntity Environment { get; set; } = default!;
    public string Version { get; set; } = default!;
    public string CommitSha { get; set; } = default!;
    public string? DeployedBy { get; set; }
    public DateTimeOffset DeployedAt { get; set; }
}

public class Incident
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Key { get; set; } = default!;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }
}
