using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using raBooth.Web.Core.Entities;

namespace raBooth.Web.Core.DataAccess.Configuration
{
    public class CollageEntityTypeConfiguration : IEntityTypeConfiguration<Collage>
    {
        public void Configure(EntityTypeBuilder<Collage> builder)
        {

            builder.HasKey(x => x.CollageId);

            builder.OwnsOne(x => x.CollagePhoto);
            builder.HasMany(x => x.SourcePhotos).WithOne().HasForeignKey(x => x.CollageId);
        }
    }
}
