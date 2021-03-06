﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LidarrAPI.Database.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    [Migration("20170304204209_AddTraktEntity")]
    partial class AddTraktEntity
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752");

            modelBuilder.Entity("LidarrAPI.Database.Models.TraktEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("CreatedAt");

                    b.Property<Guid>("State");

                    b.Property<string>("Target");

                    b.HasKey("Id");

                    b.HasIndex("State");

                    b.ToTable("Trakt");
                });

            modelBuilder.Entity("LidarrAPI.Database.Models.UpdateEntity", b =>
                {
                    b.Property<int>("UpdateEntityId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Branch");

                    b.Property<string>("FixedStr")
                        .HasColumnName("Fixed");

                    b.Property<string>("NewStr")
                        .HasColumnName("New");

                    b.Property<DateTime>("ReleaseDate");

                    b.Property<string>("Version");

                    b.HasKey("UpdateEntityId");

                    b.ToTable("Updates");
                });

            modelBuilder.Entity("LidarrAPI.Database.Models.UpdateFileEntity", b =>
                {
                    b.Property<int>("UpdateEntityId");

                    b.Property<int>("OperatingSystem");

                    b.Property<string>("Filename");

                    b.Property<string>("Hash");

                    b.Property<string>("Url");

                    b.HasKey("UpdateEntityId", "OperatingSystem");

                    b.ToTable("UpdateFiles");
                });

            modelBuilder.Entity("LidarrAPI.Database.Models.UpdateFileEntity", b =>
                {
                    b.HasOne("LidarrAPI.Database.Models.UpdateEntity", "Update")
                        .WithMany("UpdateFiles")
                        .HasForeignKey("UpdateEntityId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
