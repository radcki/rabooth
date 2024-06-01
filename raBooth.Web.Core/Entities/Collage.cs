using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using raBooth.Web.Core.Types;

namespace raBooth.Web.Core.Entities
{
    public class Collage : BaseEntity
    {
        public Guid CollageId { get; set; }
        public CollagePhoto CollagePhoto { get; set; }
        public List<CollagePhoto> SourcePhotos { get; set; } = new();
        public DateTime CaptureDateTime { get; set; } = new();
        public DateTime AddedDateTime { get; set; } = new();
    }
}
