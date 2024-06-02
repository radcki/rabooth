using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using raBooth.Web.Core.Entities;

namespace raBooth.Web.Core.DataAccess
{
    public interface IPhotoStorage
    {
        public Task StoreImage(IPhoto reference, byte[] data);
        public Task<byte[]> GetImage(IPhoto reference);
    }
}
