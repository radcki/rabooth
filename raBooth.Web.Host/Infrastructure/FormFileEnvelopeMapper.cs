using raBooth.Web.Core.Entities;

namespace raBooth.Web.Host.Infrastructure
{
    public interface IFormFileEnvelopeMapper
    {
        FileEnvelope MapFormFile(IFormFile formFile);
    }

    public class FormFileEnvelopeMapper : IFormFileEnvelopeMapper
    {
        public FileEnvelope MapFormFile(IFormFile formFile)
        {
            using var ms = new MemoryStream();
            formFile.CopyTo(ms);
            var data = ms.ToArray();
            
            return new FileEnvelope(formFile.FileName, data);
        }
    }
}
