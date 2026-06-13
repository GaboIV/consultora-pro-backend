using System.IO;
using System.Threading.Tasks;

namespace ConsultoraPro.Application.Interfaces;

public interface IStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType);
    Task DeleteFileAsync(string fileUrl);
}
