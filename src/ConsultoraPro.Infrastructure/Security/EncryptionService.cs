using System.Security.Cryptography;
using System.Text;
using ConsultoraPro.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ConsultoraPro.Infrastructure.Security;

public class EncryptionService : IEncryptionService
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private const string Version = "v1";
    private readonly byte[] _key;

    public EncryptionService(IConfiguration configuration, IHostEnvironment environment)
    {
        var configuredKey = configuration["Encryption:Key"];
        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            if (environment.IsProduction())
                throw new InvalidOperationException("Encryption:Key debe configurarse en producción.");

            configuredKey = configuration["Jwt:Key"];
        }

        if (string.IsNullOrWhiteSpace(configuredKey))
            throw new InvalidOperationException("No hay una clave disponible para cifrado.");

        _key = SHA256.HashData(Encoding.UTF8.GetBytes(configuredKey));
    }

    public string Encrypt(string plainText)
    {
        if (plainText is null)
            throw new ArgumentNullException(nameof(plainText));

        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);

        return string.Join(':',
            Version,
            Convert.ToBase64String(nonce),
            Convert.ToBase64String(tag),
            Convert.ToBase64String(cipherBytes));
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrWhiteSpace(cipherText))
            throw new InvalidOperationException("El valor cifrado está vacío.");

        var parts = cipherText.Split(':');
        if (parts.Length != 4 || parts[0] != Version)
            throw new InvalidOperationException("El formato del valor cifrado no es válido.");

        var nonce = Convert.FromBase64String(parts[1]);
        var tag = Convert.FromBase64String(parts[2]);
        var cipherBytes = Convert.FromBase64String(parts[3]);
        var plainBytes = new byte[cipherBytes.Length];

        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }
}
