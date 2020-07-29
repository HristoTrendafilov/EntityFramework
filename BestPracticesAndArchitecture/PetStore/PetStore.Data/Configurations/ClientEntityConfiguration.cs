using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PetStore.Common;
using PetStore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PetStore.Data.Configurations
{
    public class ClientEntityConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.Property(x => x.Username)
                .HasMaxLength(GlobalConstants.UserNameMaxLength)
                .IsUnicode(false);

            builder.Property(x => x.Email)
                .HasMaxLength(GlobalConstants.EmailMaxLength)
                .IsUnicode(false);

            builder.Property(x => x.FirstName)
                .HasMaxLength(GlobalConstants.ClientNameMaxLength)
                .IsUnicode(true);

            builder.Property(x => x.LastName)
                .HasMaxLength(GlobalConstants.ClientNameMaxLength)
                .IsUnicode(true);
        }
    }
}
