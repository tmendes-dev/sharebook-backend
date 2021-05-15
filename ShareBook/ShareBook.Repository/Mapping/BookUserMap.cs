using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShareBook.Domain;

namespace ShareBook.Repository.Mapping
{
    public class BookUserMap
    {
        public BookUserMap(EntityTypeBuilder<BookRequest> entityBuilder)
        {

            entityBuilder
             .HasKey(bu => new { bu.Id, bu.BookRequestedId, bu.RequestUserId });

            entityBuilder
                .HasOne(bu => bu.BookRequested)
                .WithMany(b => b.BookUsers)
                .HasForeignKey(bu => bu.BookRequestedId);

            entityBuilder
                 .HasOne(bu => bu.DonorUser)
                .WithMany(u => u.BookUsers)
                .HasForeignKey(bu => bu.RequestUserId);

            entityBuilder.Property(bu => bu.DonorNote)
               .HasColumnType("varchar(2000)")
               .HasMaxLength(2000);

            entityBuilder.Property(bu => bu.Reason)
                .HasColumnType("varchar(2000)")
                .HasMaxLength(2000);

            entityBuilder.Property(bu => bu.NickName)
                .HasColumnType("varchar(64)")
                .HasMaxLength(64);
        }
    }
}
