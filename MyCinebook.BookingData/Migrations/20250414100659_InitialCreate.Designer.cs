﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MyCinebook.BookingData;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyCinebook.BookingData.Migrations
{
    [DbContext(typeof(BookingDbContext))]
    [Migration("20250414100659_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("MyCinebook.BookingData.Models.BookedShow", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ID"));

                    b.Property<int>("BookingId")
                        .HasColumnType("integer");

                    b.Property<int>("ShowId")
                        .HasColumnType("integer");

                    b.Property<string>("ShowTitle")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("character varying(250)");

                    b.HasKey("ID");

                    b.HasIndex("BookingId");

                    b.ToTable("BookedShow");
                });

            modelBuilder.Entity("MyCinebook.BookingData.Models.BookedShowSeat", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ID"));

                    b.Property<int>("BookedShowID")
                        .HasColumnType("integer");

                    b.Property<string>("Line")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Number")
                        .HasColumnType("integer");

                    b.HasKey("ID");

                    b.HasIndex("BookedShowID");

                    b.ToTable("BookedShowSeat");
                });

            modelBuilder.Entity("MyCinebook.BookingData.Models.Booking", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("DeletedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Booking");
                });

            modelBuilder.Entity("MyCinebook.BookingData.Models.BookedShow", b =>
                {
                    b.HasOne("MyCinebook.BookingData.Models.Booking", "Booking")
                        .WithMany("Shows")
                        .HasForeignKey("BookingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Booking");
                });

            modelBuilder.Entity("MyCinebook.BookingData.Models.BookedShowSeat", b =>
                {
                    b.HasOne("MyCinebook.BookingData.Models.BookedShow", "BookedShow")
                        .WithMany("Seats")
                        .HasForeignKey("BookedShowID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BookedShow");
                });

            modelBuilder.Entity("MyCinebook.BookingData.Models.BookedShow", b =>
                {
                    b.Navigation("Seats");
                });

            modelBuilder.Entity("MyCinebook.BookingData.Models.Booking", b =>
                {
                    b.Navigation("Shows");
                });
#pragma warning restore 612, 618
        }
    }
}
