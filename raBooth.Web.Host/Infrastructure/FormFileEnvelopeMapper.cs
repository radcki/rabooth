using raBooth.Web.Core.Types;

namespace raBooth.Web.Host.Infrastructure
{
    public interface IFormFileEnvelopeMapper
    {
        FileDto MapFormFile(IFormFile formFile);
    }

    public class FormFileEnvelopeMapper : IFormFileEnvelopeMapper
    {
        public FileDto MapFormFile(IFormFile formFile)
        {
            using var ms = new MemoryStream();
            formFile.CopyTo(ms);
            var data = ms.ToArray();
            
            return new FileDto(formFile.FileName, data);
        }
    }
}
