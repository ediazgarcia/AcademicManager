using AcademicManager.Application.Interfaces;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

/// <summary>
/// Generic repository base class for Dapper-based data access
/// Provides common CRUD operations for all entities
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public abstract class GenericDapperRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly DbConnectionFactory _connectionFactory;
    protected abstract string TableName { get; }

    protected GenericDapperRepository(DbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"SELECT * FROM {TableName} WHERE Id = @Id";
        return await connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"SELECT * FROM {TableName}";
        return await connection.QueryAsync<T>(sql);
    }

    public virtual async Task<int> CreateAsync(T entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = BuildInsertQuery(entity);
        return await connection.QuerySingleAsync<int>(sql, entity);
    }

    public virtual async Task<bool> UpdateAsync(T entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = BuildUpdateQuery(entity);
        var rows = await connection.ExecuteAsync(sql, entity);
        return rows > 0;
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"DELETE FROM {TableName} WHERE Id = @Id";
        var rows = await connection.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }

    public virtual async Task<int> GetTotalCountAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = $"SELECT COUNT(*) FROM {TableName}";
        return await connection.QuerySingleAsync<int>(sql);
    }

    /// <summary>
    /// Builds INSERT query dynamically from entity properties
    /// Override in derived classes for custom queries
    /// </summary>
    protected virtual string BuildInsertQuery(T entity)
    {
        var properties = typeof(T).GetProperties()
            .Where(p => p.Name != "Id" && IsSimpleType(p.PropertyType))
            .ToList();

        var columns = string.Join(", ", properties.Select(p => p.Name));
        var values = string.Join(", ", properties.Select(p => $"@{p.Name}"));

        return $@"
            INSERT INTO {TableName} ({columns})
            VALUES ({values});
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
    }

    /// <summary>
    /// Builds UPDATE query dynamically from entity properties
    /// Override in derived classes for custom queries
    /// </summary>
    protected virtual string BuildUpdateQuery(T entity)
    {
        var properties = typeof(T).GetProperties()
            .Where(p => p.Name != "Id" && IsSimpleType(p.PropertyType))
            .ToList();

        var setClause = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));

        return $"UPDATE {TableName} SET {setClause} WHERE Id = @Id";
    }

    private static bool IsSimpleType(Type type)
    {
        if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) 
            || type == typeof(DateTime) || type == typeof(DateTime?) 
            || type == typeof(bool) || type == typeof(bool?)
            || type == typeof(int) || type == typeof(int?)
            || type == typeof(double) || type == typeof(double?)
            || type == typeof(float) || type == typeof(float?)
            || type == typeof(Guid) || type == typeof(Guid?))
        {
            return true;
        }
        return type.IsValueType;
    }
}
