using System.Security.Cryptography;
using System.Text;
using AcademicManager.Application.Interfaces;
using AcademicManager.Application.Services;
using AcademicManager.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace AcademicManager.Tests.Unit.Services;

public class AuthServiceTests
{
    [Fact]
    public void HashPassword_ShouldGenerateVersionedHashFormat()
    {
        var hash = AuthService.HashPassword("StrongPass123");
        var parts = hash.Split('$');

        parts.Length.Should().Be(5);
        parts[0].Should().Be("v2");
        parts[1].Should().Be("pbkdf2-sha256");
        parts[2].Should().Be("210000");
    }

    [Fact]
    public async Task LoginAsync_ShouldAuthenticateLegacyHashAndRehashPassword()
    {
        var password = "LegacyPass123";
        var legacyHash = CreateLegacyHash(password);
        var user = new Usuario
        {
            Id = 1,
            NombreUsuario = "legacy-user",
            PasswordHash = legacyHash,
            Rol = "Admin",
            Activo = true,
            TwoFactorEnabled = false
        };

        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(r => r.GetByNombreUsuarioAsync("legacy-user")).ReturnsAsync(user);
        repository.Setup(r => r.UpdateAsync(It.IsAny<Usuario>())).ReturnsAsync(true);
        repository.Setup(r => r.UpdateUltimoAccesoAsync(user.Id)).ReturnsAsync(true);

        var service = new AuthService(repository.Object, new TwoFactorService());

        var result = await service.LoginAsync("legacy-user", password);

        result.Success.Should().BeTrue();
        result.RequiresTwoFactor.Should().BeFalse();
        result.User.Should().NotBeNull();

        repository.Verify(
            r => r.UpdateAsync(It.Is<Usuario>(u => u.PasswordHash.StartsWith("v2$pbkdf2-sha256$210000$", StringComparison.Ordinal))),
            Times.Once);
        repository.Verify(r => r.UpdateUltimoAccesoAsync(user.Id), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldNotRehashWhenPasswordIsAlreadyCurrentFormat()
    {
        var password = "CurrentPass123";
        var user = new Usuario
        {
            Id = 2,
            NombreUsuario = "current-user",
            PasswordHash = AuthService.HashPassword(password),
            Rol = "Admin",
            Activo = true,
            TwoFactorEnabled = false
        };

        var repository = new Mock<IUsuarioRepository>();
        repository.Setup(r => r.GetByNombreUsuarioAsync("current-user")).ReturnsAsync(user);
        repository.Setup(r => r.UpdateUltimoAccesoAsync(user.Id)).ReturnsAsync(true);

        var service = new AuthService(repository.Object, new TwoFactorService());

        var result = await service.LoginAsync("current-user", password);

        result.Success.Should().BeTrue();
        repository.Verify(r => r.UpdateAsync(It.IsAny<Usuario>()), Times.Never);
        repository.Verify(r => r.UpdateUltimoAccesoAsync(user.Id), Times.Once);
    }

    private static string CreateLegacyHash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            100000,
            HashAlgorithmName.SHA256,
            32);

        return $"{Convert.ToHexString(hash)};{Convert.ToHexString(salt)}";
    }
}
