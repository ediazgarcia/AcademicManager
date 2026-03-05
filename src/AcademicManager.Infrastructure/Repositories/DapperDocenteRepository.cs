using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

public class DapperDocenteRepository : GenericDapperRepository<Docente>, IDocenteRepository
{
    protected override string TableName => "Docentes";

    public DapperDocenteRepository(DbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<Docente?> GetByCodigoAsync(string codigo)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Docentes WHERE Codigo = @Codigo";
        return await connection.QueryFirstOrDefaultAsync<Docente>(sql, new { Codigo = codigo });
    }

    public override async Task<IEnumerable<Docente>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Docentes ORDER BY Apellidos, Nombres";
        return await connection.QueryAsync<Docente>(sql);
    }

    public override async Task<int> GetTotalCountAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(*) FROM Docentes WHERE Activo = 1";
        return await connection.QuerySingleAsync<int>(sql);
    }

    protected override string BuildInsertQuery(Docente entity)
    {
        return @"
            INSERT INTO Docentes (Codigo, Nombres, Apellidos, FechaNacimiento, Genero, Direccion,
                Telefono, Email, Especialidad, GradoAcademico, FechaContratacion, Activo)
            VALUES (@Codigo, @Nombres, @Apellidos, @FechaNacimiento, @Genero, @Direccion,
                @Telefono, @Email, @Especialidad, @GradoAcademico, @FechaContratacion, @Activo);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
    }

    protected override string BuildUpdateQuery(Docente entity)
    {
        return @"
            UPDATE Docentes SET
                Codigo = @Codigo, Nombres = @Nombres, Apellidos = @Apellidos,
                FechaNacimiento = @FechaNacimiento, Genero = @Genero, Direccion = @Direccion,
                Telefono = @Telefono, Email = @Email, Especialidad = @Especialidad,
                GradoAcademico = @GradoAcademico, FechaContratacion = @FechaContratacion, Activo = @Activo
            WHERE Id = @Id";
    }
}
