using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using citi_core.Data;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _dbContext;

    /// <summary>
    /// List of critical tables to check.
    /// </summary>
    private readonly string[] _criticalTables = new[]
    {
        "Users",
    };

    public DatabaseHealthCheck(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await _dbContext.Database.CanConnectAsync(cancellationToken))
            {
                return HealthCheckResult.Unhealthy("Cannot connect to the database");
            }
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Database connection failed: {ex.Message}");
        }

        var failedTables = new List<string>();

        foreach (var table in _criticalTables)
        {
            if (!await IsTableReachableAsync(table, cancellationToken))
            {
                failedTables.Add(table);
            }
        }

        if (failedTables.Any())
        {
            return HealthCheckResult.Unhealthy(
                description: "One or more critical tables are not reachable",
                data: new Dictionary<string, object>
                {
                    { "failedTables", failedTables }
                });
        }

        return HealthCheckResult.Healthy("All critical tables are reachable");
    }

    private async Task<bool> IsTableReachableAsync(string tableName, CancellationToken cancellationToken)
    {
        try
        {
            if (!_criticalTables.Contains(tableName))
                return false;

            var sql = $"SELECT TOP 1 1 FROM [{tableName}]";

            await _dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);

            return true;
        }
        catch
        {
            return false;
        }
    }
}
