using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

public class DapperSolicitudRegistroRepository : GenericDapperRepository<SolicitudRegistro>, ISolicitudRegistroRepository
{
    protected override string TableName => "SolicitudesRegistro";

    public DapperSolicitudRegistroRepository(DbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public override async Task<IEnumerable<SolicitudRegistro>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM SolicitudesRegistro ORDER BY FechaSolicitud DESC";
        return await connection.QueryAsync<SolicitudRegistro>(sql);
    }

    public async Task<IEnumerable<SolicitudRegistro>> GetByEstadoAsync(string estado)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM SolicitudesRegistro WHERE Estado = @Estado ORDER BY FechaSolicitud DESC";
        return await connection.QueryAsync<SolicitudRegistro>(sql, new { Estado = estado });
    }

    public async Task<bool> UpdateEstadoAsync(int id, string estado)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE SolicitudesRegistro SET Estado = @Estado WHERE Id = @Id";
        var rows = await connection.ExecuteAsync(sql, new { Id = id, Estado = estado });
        return rows > 0;
    }

    protected override string BuildInsertQuery(SolicitudRegistro entity)
    {
        return @"
            INSERT INTO SolicitudesRegistro (NombreUsuario, Email, PasswordHash, RolSolicitado, FechaSolicitud, Estado, Mensaje)
            VALUES (@NombreUsuario, @Email, @PasswordHash, @RolSolicitado, @FechaSolicitud, @Estado, @Mensaje);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
    }
}
