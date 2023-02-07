using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Mapping.Users
{
    public class UserMap : DiEntityTypeConfiguration<UserEntity>
    {
        public override void Configure(EntityTypeBuilder<UserEntity> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(p => p.Id);
            //builder.HasOne(p => p.Employee).WithMany().HasForeignKey(p => p.EmployeeId);
            //builder.HasMany(p => p.Permissions).WithOne().HasForeignKey(p => p.UserId);
            base.Configure(builder);
        }

    }
}
