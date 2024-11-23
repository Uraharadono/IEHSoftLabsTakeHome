﻿// <auto-generated />
using System;
using FoodTester.DbContext.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace FoodTester.DbContext.Migrations
{
    [DbContext(typeof(FoodQualityContext))]
    [Migration("20241122225746_InitMigration")]
    partial class InitMigration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("FoodTester.DbContext.Entities.AnalysisRequest", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<long>("AnalysisTypeId")
                        .HasColumnType("bigint");

                    b.Property<long>("BatchId")
                        .HasColumnType("bigint");

                    b.Property<DateTime?>("CompletedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("VARCHAR(50)");

                    b.HasKey("Id");

                    b.HasIndex("AnalysisTypeId");

                    b.HasIndex("BatchId");

                    b.ToTable("AnalysisRequests");
                });

            modelBuilder.Entity("FoodTester.DbContext.Entities.AnalysisResult", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<DateTime?>("CompletedAt")
                        .HasColumnType("datetime2");

                    b.Property<long>("RequestId")
                        .HasColumnType("bigint");

                    b.Property<string>("ResultData")
                        .IsRequired()
                        .HasColumnType("VARCHAR(MAX)");

                    b.HasKey("Id");

                    b.HasIndex("RequestId")
                        .IsUnique();

                    b.ToTable("AnalysisResults");
                });

            modelBuilder.Entity("FoodTester.DbContext.Entities.AnalysisType", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("ModifiedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("TypeName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("VARCHAR");

                    b.HasKey("Id");

                    b.ToTable("AnalysisTypes");
                });

            modelBuilder.Entity("FoodTester.DbContext.Entities.FoodBatch", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("ModifiedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("VARCHAR");

                    b.Property<string>("SerialNumber")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("VARCHAR");

                    b.HasKey("Id");

                    b.ToTable("FoodBatches");
                });

            modelBuilder.Entity("FoodTester.DbContext.Entities.Sample", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<long>("BlogId")
                        .HasColumnType("bigint");

                    b.Property<string>("Content")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("ModifiedAt")
                        .HasColumnType("datetime2");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Samples");
                });

            modelBuilder.Entity("FoodTester.DbContext.Entities.AnalysisRequest", b =>
                {
                    b.HasOne("FoodTester.DbContext.Entities.AnalysisType", "AnalysisType")
                        .WithMany()
                        .HasForeignKey("AnalysisTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FoodTester.DbContext.Entities.FoodBatch", "FoodBatch")
                        .WithMany("AnalysisRequests")
                        .HasForeignKey("BatchId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AnalysisType");

                    b.Navigation("FoodBatch");
                });

            modelBuilder.Entity("FoodTester.DbContext.Entities.AnalysisResult", b =>
                {
                    b.HasOne("FoodTester.DbContext.Entities.AnalysisRequest", "AnalysisRequest")
                        .WithOne("AnalysisResult")
                        .HasForeignKey("FoodTester.DbContext.Entities.AnalysisResult", "RequestId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AnalysisRequest");
                });

            modelBuilder.Entity("FoodTester.DbContext.Entities.AnalysisRequest", b =>
                {
                    b.Navigation("AnalysisResult");
                });

            modelBuilder.Entity("FoodTester.DbContext.Entities.FoodBatch", b =>
                {
                    b.Navigation("AnalysisRequests");
                });
#pragma warning restore 612, 618
        }
    }
}
