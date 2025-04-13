﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyCinebook.ScheduleData.Migrations
{
    [DbContext(typeof(ScheduleDbContext))]
    partial class ScheduleDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("MyCinebook.ScheduleData.SeatModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<char>("Line")
                        .HasColumnType("character(1)");

                    b.Property<int>("Number")
                        .HasColumnType("integer");

                    b.Property<int?>("ShowModelId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ShowModelId");

                    b.ToTable("Seats");
                });

            modelBuilder.Entity("MyCinebook.ScheduleData.ShowModel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Shows");
                });

            modelBuilder.Entity("MyCinebook.ScheduleData.SeatModel", b =>
                {
                    b.HasOne("MyCinebook.ScheduleData.ShowModel", null)
                        .WithMany("Seats")
                        .HasForeignKey("ShowModelId");
                });

            modelBuilder.Entity("MyCinebook.ScheduleData.ShowModel", b =>
                {
                    b.Navigation("Seats");
                });
#pragma warning restore 612, 618
        }
    }
}
