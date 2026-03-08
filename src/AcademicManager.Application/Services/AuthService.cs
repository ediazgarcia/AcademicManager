using AcademicManager.Application.Interfaces;
using AcademicManager.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

namespace AcademicManager.Application.Services;

public class AuthService
{
    private const string CurrentHashVersion = "v2";
    private const string CurrentHashAlgorithm = "pbkdf2-sha256";
    private const int SaltSize = 16; // 128 bit
    private const int KeySize = 32;  // 256 bit
    private const int LegacyIterations = 100000;
    private const int CurrentIterations = 210000;
    private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly TwoFactorService _twoFactorService;

    public AuthService(IUsuarioRepository usuarioRepository, TwoFactorService twoFactorService)
    {
        _usuarioRepository = usuarioRepository;
        _twoFactorService = twoFactorService;
    }

    public static (bool Valid, string Message) ValidarComplejidadPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) return (false, "La contraseña no puede estar vacía.");
        if (password.Length < 8) return (false, "La contraseña debe tener al menos 8 caracteres.");
        if (!password.Any(char.IsUpper)) return (false, "Debe contener al menos una letra mayúscula.");
        if (!password.Any(char.IsLower)) return (false, "Debe contener al menos una letra minúscula.");
        if (!password.Any(char.IsDigit)) return (false, "Debe contener al menos un número.");
        
        return (true, string.Empty);
    }

    public async Task<(bool Success, bool RequiresTwoFactor, Usuario? User)> LoginAsync(string nombreUsuario, string password)
    {
        var usuario = await _usuarioRepository.GetByNombreUsuarioAsync(nombreUsuario);
        if (usuario == null || !usuario.Activo)
            return (false, false, null);

        if (!VerifyPassword(password, usuario.PasswordHash, out var shouldRehash))
            return (false, false, null);

        if (shouldRehash)
        {
            usuario.PasswordHash = HashPassword(password);
            await _usuarioRepository.UpdateAsync(usuario);
        }

        if (usuario.TwoFactorEnabled)
        {
            return (true, true, usuario);
        }

        await _usuarioRepository.UpdateUltimoAccesoAsync(usuario.Id);
        return (true, false, usuario);
    }

    public async Task<Usuario?> ValidateTwoFactorAsync(int userId, string code)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(userId);
        if (usuario == null || !usuario.Activo || !usuario.TwoFactorEnabled)
            return null;

        if (!_twoFactorService.ValidateCode(usuario.TwoFactorSecret!, code))
            return null;

        await _usuarioRepository.UpdateUltimoAccesoAsync(usuario.Id);
        return usuario;
    }

    public async Task<(string Secret, string AuthUrl)> GenerateTwoFactorSetupAsync(int userId)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(userId);
        if (usuario == null)
            throw new InvalidOperationException("Usuario no encontrado");

        var secret = _twoFactorService.GenerateSecret();
        var authUrl = _twoFactorService.GetOtpAuthUrl(usuario.Email, secret);

        return (secret, authUrl);
    }

    public async Task<bool> EnableTwoFactorAsync(int userId, string secret, string code)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(userId);
        if (usuario == null)
            return false;

        if (!_twoFactorService.ValidateCode(secret, code))
            return false;

        usuario.TwoFactorEnabled = true;
        usuario.TwoFactorSecret = secret;

        return await _usuarioRepository.UpdateAsync(usuario);
    }

    public async Task<bool> DisableTwoFactorAsync(int userId, string password)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(userId);
        if (usuario == null)
            return false;

        if (!VerifyPassword(password, usuario.PasswordHash, out _))
            return false;

        usuario.TwoFactorEnabled = false;
        usuario.TwoFactorSecret = null;

        return await _usuarioRepository.UpdateAsync(usuario);
    }

    public async Task<bool> VerificarTwoFactorCodeAsync(int userId, string code)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(userId);
        if (usuario == null || !usuario.TwoFactorEnabled || string.IsNullOrEmpty(usuario.TwoFactorSecret))
            return false;

        return _twoFactorService.ValidateCode(usuario.TwoFactorSecret, code);
    }

    public async Task<int> RegistrarUsuarioAsync(Usuario usuario, string password)
    {
        // Verificar que no exista
        var existente = await _usuarioRepository.GetByNombreUsuarioAsync(usuario.NombreUsuario);
        if (existente != null)
            throw new InvalidOperationException("El nombre de usuario ya existe.");

        var existenteEmail = await _usuarioRepository.GetByEmailAsync(usuario.Email);
        if (existenteEmail != null)
            throw new InvalidOperationException("El email ya está registrado.");

        usuario.PasswordHash = HashPassword(password);
        return await _usuarioRepository.CreateAsync(usuario);
    }

    public async Task<bool> CambiarPasswordAsync(int userId, string passwordActual, string nuevoPassword)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(userId);
        if (usuario == null)
            return false;

        if (!VerifyPassword(passwordActual, usuario.PasswordHash, out _))
            return false;

        usuario.PasswordHash = HashPassword(nuevoPassword);
        return await _usuarioRepository.UpdateAsync(usuario);
    }

    public static string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            CurrentIterations,
            HashAlgorithm,
            KeySize);

        return $"{CurrentHashVersion}${CurrentHashAlgorithm}${CurrentIterations}${Convert.ToHexString(salt)}${Convert.ToHexString(hash)}";
    }

    private static bool VerifyPassword(string password, string hashWithSalt, out bool shouldRehash)
    {
        shouldRehash = false;

        if (string.IsNullOrWhiteSpace(hashWithSalt))
            return false;

        if (TryVerifyVersionedHash(password, hashWithSalt, out var isCurrentFormat))
        {
            shouldRehash = !isCurrentFormat;
            return true;
        }

        if (TryVerifyLegacyHash(password, hashWithSalt))
        {
            shouldRehash = true;
            return true;
        }

        return false;
    }

    private static bool TryVerifyVersionedHash(string password, string hashValue, out bool isCurrentFormat)
    {
        isCurrentFormat = false;

        var parts = hashValue.Split('$');
        if (parts.Length != 5)
            return false;

        if (!string.Equals(parts[1], CurrentHashAlgorithm, StringComparison.Ordinal))
            return false;

        if (!int.TryParse(parts[2], out var iterations) || iterations <= 0)
            return false;

        try
        {
            var salt = Convert.FromHexString(parts[3]);
            var expectedHash = Convert.FromHexString(parts[4]);

            byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterations,
                HashAlgorithm,
                expectedHash.Length);

            var isValid = CryptographicOperations.FixedTimeEquals(expectedHash, inputHash);
            isCurrentFormat = string.Equals(parts[0], CurrentHashVersion, StringComparison.Ordinal)
                && iterations == CurrentIterations
                && expectedHash.Length == KeySize;

            return isValid;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool TryVerifyLegacyHash(string password, string hashWithSalt)
    {
        var parts = hashWithSalt.Split(';');
        if (parts.Length != 2)
            return false;

        try
        {
            byte[] hash = Convert.FromHexString(parts[0]);
            byte[] salt = Convert.FromHexString(parts[1]);

            byte[] inputHash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                LegacyIterations,
                HashAlgorithm,
                hash.Length);

            return CryptographicOperations.FixedTimeEquals(hash, inputHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
