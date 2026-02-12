using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

/// <summary>
/// Dapper repository for Seccion entity
/// Inherits common CRUD operations from GenericDapperRepository
/// </summary>
public class DapperSeccionRepository : GenericDapperRepository<Seccion>, ISeccionRepository
{
    protected override string TableName => "Secciones";

    public DapperSeccionRepository(DbConnectionFactory connectionFactory)
        : base(connectionFactory)
    {
    }

    public override async Task<Seccion?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT s.*, g.Id, g.Nombre, g.Nivel, g.Orden, g.Activo
            FROM Secciones s
            LEFT JOIN Grados g ON s.GradoId = g.Id
            WHERE s.Id = @Id";
        var result = await connection.QueryAsync<Seccion, Grado, Seccion>(
            sql,
            (seccion, grado) => { seccion.Grado = grado; return seccion; },
            new { Id = id },
            splitOn: "Id");
        return result.FirstOrDefault();
    }

    public override async Task<IEnumerable<Seccion>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT s.*, g.Id, g.Nombre, g.Nivel, g.Orden, g.Activo
            FROM Secciones s
            LEFT JOIN Grados g ON s.GradoId = g.Id
            ORDER BY g.Orden, s.Nombre";
        return await connection.QueryAsync<Seccion, Grado, Seccion>(
            sql,
            (seccion, grado) => { seccion.Grado = grado; return seccion; },
            splitOn: "Id");
    }

    public async Task<IEnumerable<Seccion>> GetByGradoIdAsync(int gradoId)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Secciones WHERE GradoId = @GradoId ORDER BY Nombre";
        return await connection.QueryAsync<Seccion>(sql, new { GradoId = gradoId });
    }
}

