using QRDine.Domain.Catalog;
using QRDine.Infrastructure.Persistence.Constants;

namespace QRDine.Infrastructure.Persistence.Configurations.Catalog
{
    public class ToppingConfiguration : IEntityTypeConfiguration<Topping>
    {
        public void Configure(EntityTypeBuilder<Topping> builder)
        {
            builder.ToTable("Toppings", SchemaNames.Catalog);

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(x => x.Name).IsRequired().HasMaxLength(256);

            builder.Property(x => x.Price).HasColumnType("decimal(18,2)");

            builder.HasIndex(x => x.ToppingGroupId);

            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.HasIndex(t => t.ToppingGroupId)
                .IncludeProperties(t => new { t.Name, t.Price, t.DisplayOrder, t.IsAvailable });
        }
    }
}
