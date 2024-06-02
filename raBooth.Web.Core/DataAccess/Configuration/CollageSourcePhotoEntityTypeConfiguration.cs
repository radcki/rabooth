using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using raBooth.Web.Core.Entities;

namespace raBooth.Web.Core.DataAccess.Configuration;

public class CollageSourcePhotoEntityTypeConfiguration : IEntityTypeConfiguration<CollageSourcePhoto>
{
    public void Configure(EntityTypeBuilder<CollageSourcePhoto> builder)
    {
        builder.HasKey(x => x.CollageSourcePhotoId);
        builder.Property(x => x.CollageSourcePhotoId).ValueGeneratedOnAdd();
    }
}