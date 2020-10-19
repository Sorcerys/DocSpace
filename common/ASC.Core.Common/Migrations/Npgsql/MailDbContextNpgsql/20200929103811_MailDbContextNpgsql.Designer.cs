﻿// <auto-generated />
using System;
using ASC.Core.Common.EF.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ASC.Core.Common.Migrations.Npgsql.MailDbContextNpgsql
{
    [DbContext(typeof(PostgreSqlMailDbContext))]
    [Migration("20200929103811_MailDbContextNpgsql")]
    partial class MailDbContextNpgsql
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:Enum:onlyoffice.enum_dbip_location", "ipv4,ipv6")
                .HasAnnotation("Npgsql:Enum:onlyoffice.enum_mail_mailbox_server", "pop3,imap,smtp")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("ASC.Core.Common.Models.MailMailbox", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnName("address")
                        .HasColumnType("character varying(255)")
                        .HasMaxLength(255);

                    b.Property<DateTime>("BeginDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("begin_date")
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("'1975-01-01 00:00:00'::timestamp without time zone");

                    b.Property<DateTime?>("DateAuthError")
                        .HasColumnName("date_auth_error")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime?>("DateChecked")
                        .HasColumnName("date_checked")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime?>("DateCreated")
                        .HasColumnName("date_created")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime>("DateLoginDelayExpires")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("date_login_delay_expires")
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("'1975-01-01 00:00:00'::timestamp without time zone");

                    b.Property<DateTime>("DateModified")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("date_modified")
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<DateTime?>("DateUserChecked")
                        .HasColumnName("date_user_checked")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("EmailInFolder")
                        .HasColumnName("email_in_folder")
                        .HasColumnType("text");

                    b.Property<short>("Enabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("enabled")
                        .HasColumnType("smallint")
                        .HasDefaultValueSql("'1'::smallint");

                    b.Property<int>("IdInServer")
                        .HasColumnName("id_in_server")
                        .HasColumnType("integer");

                    b.Property<int>("IdSmtpServer")
                        .HasColumnName("id_smtp_server")
                        .HasColumnType("integer");

                    b.Property<string>("IdUser")
                        .IsRequired()
                        .HasColumnName("id_user")
                        .HasColumnType("character varying(38)")
                        .HasMaxLength(38);

                    b.Property<short>("Imap")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("imap")
                        .HasColumnType("smallint")
                        .HasDefaultValueSql("'0'::smallint");

                    b.Property<string>("ImapIntervals")
                        .HasColumnName("imap_intervals")
                        .HasColumnType("text");

                    b.Property<short>("IsDefault")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("is_default")
                        .HasColumnType("smallint")
                        .HasDefaultValueSql("'0'::smallint");

                    b.Property<short>("IsProcessed")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("is_processed")
                        .HasColumnType("smallint")
                        .HasDefaultValueSql("'0'::smallint");

                    b.Property<short>("IsRemoved")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("is_removed")
                        .HasColumnType("smallint")
                        .HasDefaultValueSql("'0'::smallint");

                    b.Property<short>("IsServerMailbox")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("is_server_mailbox")
                        .HasColumnType("smallint")
                        .HasDefaultValueSql("'0'::smallint");

                    b.Property<long>("LoginDelay")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("login_delay")
                        .HasColumnType("bigint")
                        .HasDefaultValueSql("'30'::bigint");

                    b.Property<int>("MsgCountLast")
                        .HasColumnName("msg_count_last")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("name")
                        .HasColumnType("character varying(255)")
                        .HasDefaultValueSql("NULL::character varying")
                        .HasMaxLength(255);

                    b.Property<string>("Pop3Password")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("pop3_password")
                        .HasColumnType("character varying(255)")
                        .HasDefaultValueSql("NULL::character varying")
                        .HasMaxLength(255);

                    b.Property<short>("QuotaError")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("quota_error")
                        .HasColumnType("smallint")
                        .HasDefaultValueSql("'0'::smallint");

                    b.Property<int>("SizeLast")
                        .HasColumnName("size_last")
                        .HasColumnType("integer");

                    b.Property<string>("SmtpPassword")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("smtp_password")
                        .HasColumnType("character varying(255)")
                        .HasDefaultValueSql("NULL::character varying")
                        .HasMaxLength(255);

                    b.Property<int>("Tenant")
                        .HasColumnName("tenant")
                        .HasColumnType("integer");

                    b.Property<string>("Token")
                        .HasColumnName("token")
                        .HasColumnType("text");

                    b.Property<short>("TokenType")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("token_type")
                        .HasColumnType("smallint")
                        .HasDefaultValueSql("'0'::smallint");

                    b.Property<short>("UserOnline")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("user_online")
                        .HasColumnType("smallint")
                        .HasDefaultValueSql("'0'::smallint");

                    b.HasKey("Id");

                    b.HasIndex("Address")
                        .HasName("address_index");

                    b.HasIndex("IdInServer")
                        .HasName("main_mailbox_id_in_server_mail_mailbox_server_id");

                    b.HasIndex("IdSmtpServer")
                        .HasName("main_mailbox_id_smtp_server_mail_mailbox_server_id");

                    b.HasIndex("DateChecked", "DateLoginDelayExpires")
                        .HasName("date_login_delay_expires");

                    b.HasIndex("Tenant", "IdUser")
                        .HasName("user_id_index");

                    b.ToTable("mail_mailbox","onlyoffice");
                });

            modelBuilder.Entity("ASC.Core.Common.Models.MailMailboxProvider", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("DisplayName")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("display_name")
                        .HasColumnType("character varying(255)")
                        .HasDefaultValueSql("NULL::character varying")
                        .HasMaxLength(255);

                    b.Property<string>("DisplayShortName")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("display_short_name")
                        .HasColumnType("character varying(255)")
                        .HasDefaultValueSql("NULL::character varying")
                        .HasMaxLength(255);

                    b.Property<string>("Documentation")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("documentation")
                        .HasColumnType("character varying(255)")
                        .HasDefaultValueSql("NULL::character varying")
                        .HasMaxLength(255);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasColumnType("character varying(255)")
                        .HasMaxLength(255);

                    b.HasKey("Id");

                    b.ToTable("mail_mailbox_provider","onlyoffice");
                });

            modelBuilder.Entity("ASC.Core.Common.Models.MailMailboxServer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Authentication")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("authentication")
                        .HasColumnType("character varying(255)")
                        .HasDefaultValueSql("NULL::character varying")
                        .HasMaxLength(255);

                    b.Property<string>("Hostname")
                        .IsRequired()
                        .HasColumnName("hostname")
                        .HasColumnType("character varying");

                    b.Property<int>("IdProvider")
                        .HasColumnName("id_provider")
                        .HasColumnType("integer");

                    b.Property<short>("IsUserData")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("is_user_data")
                        .HasColumnType("smallint")
                        .HasDefaultValueSql("'0'::smallint");

                    b.Property<int>("Port")
                        .HasColumnName("port")
                        .HasColumnType("integer");

                    b.Property<string>("SocketType")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnName("socket_type")
                        .HasColumnType("character varying")
                        .HasDefaultValueSql("'plain'::character varying");

                    b.Property<string>("Username")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("username")
                        .HasColumnType("character varying(255)")
                        .HasDefaultValueSql("NULL::character varying")
                        .HasMaxLength(255);

                    b.HasKey("Id");

                    b.HasIndex("IdProvider")
                        .HasName("id_provider_mail_mailbox_server");

                    b.ToTable("mail_mailbox_server","onlyoffice");
                });

            modelBuilder.Entity("ASC.Core.Common.Models.MailServerServer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("ConnectionString")
                        .IsRequired()
                        .HasColumnName("connection_string")
                        .HasColumnType("text");

                    b.Property<int>("ImapSettingsId")
                        .HasColumnName("imap_settings_id")
                        .HasColumnType("integer");

                    b.Property<string>("MxRecord")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnName("mx_record")
                        .HasColumnType("character varying(128)")
                        .HasDefaultValueSql("' '::character varying")
                        .HasMaxLength(128);

                    b.Property<int>("ServerType")
                        .HasColumnName("server_type")
                        .HasColumnType("integer");

                    b.Property<int>("SmtpSettingsId")
                        .HasColumnName("smtp_settings_id")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("ServerType")
                        .HasName("mail_server_server_type_server_type_fk_id");

                    b.ToTable("mail_server_server","onlyoffice");
                });
#pragma warning restore 612, 618
        }
    }
}
