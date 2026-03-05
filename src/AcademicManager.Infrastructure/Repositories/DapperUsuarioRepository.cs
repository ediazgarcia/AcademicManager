using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using AcademicManager.Infrastructure.Data;
using Dapper;

namespace AcademicManager.Infrastructure.Repositories;

public class DapperUsuarioRepository : GenericDapperRepository<Usuario>, IUsuarioRepository
{
    protected override string TableName => "Usuarios";

    public DapperUsuarioRepository(DbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }

    public async Task<Usuario?> GetByNombreUsuarioAsync(string nombreUsuario)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Usuarios WHERE NombreUsuario = @NombreUsuario";
        return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { NombreUsuario = nombreUsuario });
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Usuarios WHERE Email = @Email";
        return await connection.QueryFirstOrDefaultAsync<Usuario>(sql, new { Email = email });
    }

    public override async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM Usuarios ORDER BY NombreUsuario";
        return await connection.QueryAsync<Usuario>(sql);
    }

    public async Task<bool> UpdateUltimoAccesoAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE Usuarios SET UltimoAcceso = @UltimoAcceso WHERE Id = @Id";
        var rows = await connection.ExecuteAsync(sql, new { Id = id, UltimoAcceso = DateTime.UtcNow });
        return rows > 0;
    }

    protected override string BuildInsertQuery(Usuario entity)
    {
        return @"
            INSERT INTO Usuarios (NombreUsuario, Email, PasswordHash, Rol, Activo, FechaCreacion, AlumnoId, DocenteId, TwoFactorEnabled, TwoFactorSecret)
            VALUES (@NombreUsuario, @Email, @PasswordHash, @Rol, @Activo, @FechaCreacion, @AlumnoId, @DocenteId, @TwoFactorEnabled, @TwoFactorSecret);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";
    }

    protected override string BuildUpdateQuery(Usuario entity)
    {
        return @"
            UPDATE Usuarios SET
                NombreUsuario = @NombreUsuario,
                Email = @Email,
                PasswordHash = @PasswordHash,
                Rol = @Rol,
                Activo = @Activo,
                AlumnoId = @AlumnoId,
                DocenteId = @DocenteId,
                TwoFactorEnabled = @TwoFactorEnabled,
                TwoFactorSecret = @TwoFactorSecret
            WHERE Id = @Id";
    }
}
