using OtpNet;

namespace AcademicManager.Application.Services;

public class TwoFactorService
{
    private const int TimeStep = 30;
    private const int CodeLength = 6;

    public string GenerateSecret()
    {
        var secretKey = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(secretKey);
    }

    public string GetOtpAuthUrl(string username, string secret)
    {
        return $"otpauth://totp/AcademicManager:{username}?secret={secret}&issuer=AcademicManager&digits={CodeLength}";
    }

    public bool ValidateCode(string secret, string code)
    {
        if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(code))
            return false;

        try
        {
            var secretBytes = Base32Encoding.ToBytes(secret);
            var totp = new Totp(secretBytes, TimeStep, OtpHashMode.Sha1, CodeLength);
            var isValid = totp.VerifyTotp(code, out _);
            return isValid;
        }
        catch
        {
            return false;
        }
    }
}
