// <auto-generated />
using System;
using ASC.Webhooks.Core.EF.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ASC.Migrations.MySql.Migrations.WebhooksDb
{
    [DbContext(typeof(WebhooksDbContext))]
    [Migration("20230607172539_WebhooksDbContext_Upgrade1")]
    partial class WebhooksDbContextUpgrade1
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("ASC.Webhooks.Core.EF.Model.DbWebhook", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<string>("Method")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(10)
                        .HasColumnType("varchar(10)")
                        .HasColumnName("method")
                        .HasDefaultValueSql("''");

                    b.Property<string>("Route")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)")
                        .HasColumnName("route")
                        .HasDefaultValueSql("''");

                    b.HasKey("Id")
                        .HasName("PRIMARY");

                    b.ToTable("webhooks", (string)null);

                    b.HasAnnotation("MySql:CharSet", "utf8");
                });

            modelBuilder.Entity("ASC.Webhooks.Core.EF.Model.WebhooksConfig", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<bool>("Enabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("enabled")
                        .HasDefaultValueSql("'1'");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)")
                        .HasColumnName("name");

                    b.Property<bool>("SSL")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("ssl")
                        .HasDefaultValueSql("'1'");

                    b.Property<string>("SecretKey")
                        .ValueGeneratedOnAdd()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)")
                        .HasColumnName("secret_key")
                        .HasDefaultValueSql("''");

                    b.Property<uint>("TenantId")
                        .HasColumnType("int unsigned")
                        .HasColumnName("tenant_id");

                    b.Property<string>("Uri")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("text")
                        .HasColumnName("uri")
                        .HasDefaultValueSql("''")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.HasKey("Id")
                        .HasName("PRIMARY");

                    b.HasIndex("TenantId")
                        .HasDatabaseName("tenant_id");

                    b.ToTable("webhooks_config", (string)null);

                    b.HasAnnotation("MySql:CharSet", "utf8");
                });

            modelBuilder.Entity("ASC.Webhooks.Core.EF.Model.WebhooksLog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("id");

                    b.Property<int>("ConfigId")
                        .HasColumnType("int")
                        .HasColumnName("config_id");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("datetime")
                        .HasColumnName("creation_time");

                    b.Property<DateTime?>("Delivery")
                        .HasColumnType("datetime")
                        .HasColumnName("delivery");

                    b.Property<string>("RequestHeaders")
                        .HasColumnType("json")
                        .HasColumnName("request_headers");

                    b.Property<string>("RequestPayload")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("request_payload")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.Property<string>("ResponseHeaders")
                        .HasColumnType("json")
                        .HasColumnName("response_headers");

                    b.Property<string>("ResponsePayload")
                        .HasColumnType("text")
                        .HasColumnName("response_payload")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.Property<int>("Status")
                        .HasColumnType("int")
                        .HasColumnName("status");

                    b.Property<uint>("TenantId")
                        .HasColumnType("int unsigned")
                        .HasColumnName("tenant_id");

                    b.Property<string>("Uid")
                        .IsRequired()
                        .HasColumnType("varchar(36)")
                        .HasColumnName("uid")
                        .UseCollation("utf8_general_ci")
                        .HasAnnotation("MySql:CharSet", "utf8");

                    b.Property<int>("WebhookId")
                        .HasColumnType("int")
                        .HasColumnName("webhook_id");

                    b.HasKey("Id")
                        .HasName("PRIMARY");

                    b.HasIndex("ConfigId");

                    b.HasIndex("TenantId")
                        .HasDatabaseName("tenant_id");

                    b.ToTable("webhooks_logs", (string)null);

                    b.HasAnnotation("MySql:CharSet", "utf8");
                });

            modelBuilder.Entity("ASC.Webhooks.Core.EF.Model.WebhooksLog", b =>
                {
                    b.HasOne("ASC.Webhooks.Core.EF.Model.WebhooksConfig", "Config")
                        .WithMany()
                        .HasForeignKey("ConfigId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Config");
                });
#pragma warning restore 612, 618
        }
    }
}
