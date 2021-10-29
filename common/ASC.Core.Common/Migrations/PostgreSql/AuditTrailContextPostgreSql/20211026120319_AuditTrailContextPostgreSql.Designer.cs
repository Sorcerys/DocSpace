// <auto-generated />
using System;
using ASC.Core.Common.EF.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ASC.Core.Common.Migrations.PostgreSql.AuditTrailContextPostgreSql
{
    [DbContext(typeof(PostgreSqlAuditTrailContext))]
    [Migration("20211026120319_AuditTrailContextPostgreSql")]
    partial class AuditTrailContextPostgreSql
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.10");

            modelBuilder.Entity("ASC.Core.Common.EF.Model.AuditEvent", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<int>("Action")
                        .HasColumnType("int")
                        .HasColumnName("action");

                    b.Property<string>("Browser")
                        .HasColumnType("varchar(200)")
                        .HasColumnName("browser")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime")
                        .HasColumnName("date");

                    b.Property<string>("Description")
                        .HasColumnType("varchar(20000)")
                        .HasColumnName("description")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.Property<string>("Initiator")
                        .HasColumnType("varchar(200)")
                        .HasColumnName("initiator")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.Property<string>("Ip")
                        .HasColumnType("varchar(50)")
                        .HasColumnName("ip")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.Property<string>("Page")
                        .HasColumnType("varchar(300)")
                        .HasColumnName("page")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.Property<string>("Platform")
                        .HasColumnType("varchar(200)")
                        .HasColumnName("platform")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.Property<string>("Target")
                        .HasColumnType("text")
                        .HasColumnName("target")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.Property<int>("TenantId")
                        .HasColumnType("int")
                        .HasColumnName("tenant_id");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("char(38)")
                        .HasColumnName("user_id")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.HasKey("Id");

                    b.HasIndex("TenantId", "Date")
                        .HasDatabaseName("date");

                    b.ToTable("audit_events");
                });
#pragma warning restore 612, 618
        }
    }
}
