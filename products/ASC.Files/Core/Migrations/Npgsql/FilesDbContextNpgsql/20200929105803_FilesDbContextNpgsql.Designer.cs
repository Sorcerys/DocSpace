﻿// <auto-generated />
using System;
using ASC.Files.Core.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ASC.Files.Core.Migrations.Npgsql.FilesDbContextNpgsql
{
    [DbContext(typeof(FilesDbContext))]
    [Migration("20200929105803_FilesDbContextNpgsql")]
    partial class FilesDbContextNpgsql
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("ASC.Core.Common.EF.Model.DbTenant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Alias")
                        .IsRequired()
                        .HasColumnName("alias")
                        .HasColumnType("character varying(100)")
                        .HasMaxLength(100);

                    b.Property<bool>("Calls")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("calls")
                        .HasColumnType("boolean")
                        .HasDefaultValueSql("1");

                    b.Property<DateTime>("CreationDateTime")
                        .HasColumnName("creationdatetime")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int?>("Industry")
                        .HasColumnName("industry")
                        .HasColumnType("integer");

                    b.Property<string>("Language")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnName("language")
                        .HasColumnType("character(10)")
                        .HasDefaultValueSql("'en-US'")
                        .IsFixedLength(true)
                        .HasMaxLength(10);

                    b.Property<DateTime>("LastModified")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("last_modified")
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("MappedDomain")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("mappeddomain")
                        .HasColumnType("character varying(100)")
                        .HasDefaultValueSql("NULL")
                        .HasMaxLength(100);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasColumnType("character varying(255)")
                        .HasMaxLength(255);

                    b.Property<Guid>("OwnerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("owner_id")
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("NULL")
                        .HasMaxLength(38);

                    b.Property<string>("PaymentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("payment_id")
                        .HasColumnType("character varying(38)")
                        .HasDefaultValueSql("NULL")
                        .HasMaxLength(38);

                    b.Property<bool>("Public")
                        .HasColumnName("public")
                        .HasColumnType("boolean");

                    b.Property<string>("PublicVisibleProducts")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("publicvisibleproducts")
                        .HasColumnType("character varying(1024)")
                        .HasDefaultValueSql("NULL")
                        .HasMaxLength(1024);

                    b.Property<bool>("Spam")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("spam")
                        .HasColumnType("boolean")
                        .HasDefaultValueSql("1");

                    b.Property<int>("Status")
                        .HasColumnName("status")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("StatusChanged")
                        .HasColumnName("statuschanged")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("TimeZone")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("timezone")
                        .HasColumnType("character varying(50)")
                        .HasDefaultValueSql("NULL")
                        .HasMaxLength(50);

                    b.Property<string>("TrustedDomains")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("trusteddomains")
                        .HasColumnType("character varying(1024)")
                        .HasDefaultValueSql("NULL")
                        .HasMaxLength(1024);

                    b.Property<int>("TrustedDomainsEnabled")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("trusteddomainsenabled")
                        .HasColumnType("integer")
                        .HasDefaultValueSql("1");

                    b.Property<int>("Version")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("version")
                        .HasColumnType("integer")
                        .HasDefaultValueSql("2");

                    b.Property<DateTime>("VersionChanged")
                        .HasColumnName("version_changed")
                        .HasColumnType("timestamp without time zone");

                    b.Property<DateTime?>("Version_Changed")
                        .HasColumnType("timestamp without time zone");

                    b.HasKey("Id");

                    b.HasIndex("Alias")
                        .IsUnique()
                        .HasName("alias");

                    b.HasIndex("LastModified")
                        .HasName("last_modified_tenants_tenants");

                    b.HasIndex("MappedDomain")
                        .HasName("mappeddomain");

                    b.HasIndex("Version")
                        .HasName("version");

                    b.ToTable("tenants_tenants","onlyoffice");
                });

            modelBuilder.Entity("ASC.Core.Common.EF.Model.DbTenantPartner", b =>
                {
                    b.Property<int>("TenantId")
                        .HasColumnName("tenant_id")
                        .HasColumnType("integer");

                    b.Property<string>("AffiliateId")
                        .HasColumnName("affiliate_id")
                        .HasColumnType("text");

                    b.Property<string>("Campaign")
                        .HasColumnName("campaign")
                        .HasColumnType("text");

                    b.Property<string>("PartnerId")
                        .HasColumnName("partner_id")
                        .HasColumnType("text");

                    b.HasKey("TenantId");

                    b.ToTable("tenants_partners");
                });

            modelBuilder.Entity("ASC.Files.Core.EF.DbEncryptedData", b =>
                {
                    b.Property<string>("PublicKey")
                        .HasColumnName("public_key")
                        .HasColumnType("character(64)")
                        .IsFixedLength(true)
                        .HasMaxLength(64);

                    b.Property<string>("FileHash")
                        .HasColumnName("file_hash")
                        .HasColumnType("character(66)")
                        .IsFixedLength(true)
                        .HasMaxLength(66);

                    b.Property<DateTime>("CreateOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("create_on")
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnName("data")
                        .HasColumnType("character(112)")
                        .IsFixedLength(true)
                        .HasMaxLength(112);

                    b.Property<int>("TenantId")
                        .HasColumnName("tenant_id")
                        .HasColumnType("integer");

                    b.HasKey("PublicKey", "FileHash")
                        .HasName("encrypted_data_pkey");

                    b.HasIndex("TenantId")
                        .HasName("tenant_id_encrypted_data");

                    b.ToTable("encrypted_data","onlyoffice");
                });

            modelBuilder.Entity("ASC.Files.Core.EF.DbFile", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnName("id")
                        .HasColumnType("integer");

                    b.Property<int>("TenantId")
                        .HasColumnName("tenant_id")
                        .HasColumnType("integer");

                    b.Property<int>("Version")
                        .HasColumnName("version")
                        .HasColumnType("integer");

                    b.Property<int>("Category")
                        .HasColumnName("category")
                        .HasColumnType("integer");

                    b.Property<string>("Changes")
                        .HasColumnName("changes")
                        .HasColumnType("text");

                    b.Property<string>("Comment")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("comment")
                        .HasColumnType("character varying(255)")
                        .HasDefaultValueSql("NULL::character varying")
                        .HasMaxLength(255);

                    b.Property<long>("ContentLength")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("content_length")
                        .HasColumnType("bigint")
                        .HasDefaultValueSql("'0'::bigint");

                    b.Property<string>("ConvertedType")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("converted_type")
                        .HasColumnType("character varying(10)")
                        .HasDefaultValueSql("NULL::character varying")
                        .HasMaxLength(10);

                    b.Property<Guid>("CreateBy")
                        .HasColumnName("create_by")
                        .HasColumnType("uuid")
                        .IsFixedLength(true)
                        .HasMaxLength(38);

                    b.Property<DateTime>("CreateOn")
                        .HasColumnName("create_on")
                        .HasColumnType("timestamp without time zone");

                    b.Property<bool>("CurrentVersion")
                        .HasColumnName("current_version")
                        .HasColumnType("boolean");

                    b.Property<bool>("Encrypted")
                        .HasColumnName("encrypted")
                        .HasColumnType("boolean");

                    b.Property<int>("FileStatus")
                        .HasColumnName("file_status")
                        .HasColumnType("integer");

                    b.Property<int>("FolderId")
                        .HasColumnName("folder_id")
                        .HasColumnType("integer");

                    b.Property<int>("Forcesave")
                        .HasColumnName("forcesave")
                        .HasColumnType("integer");

                    b.Property<Guid>("ModifiedBy")
                        .HasColumnName("modified_by")
                        .HasColumnType("uuid")
                        .IsFixedLength(true)
                        .HasMaxLength(38);

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnName("modified_on")
                        .HasColumnType("timestamp without time zone");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnName("title")
                        .HasColumnType("character varying(400)")
                        .HasMaxLength(400);

                    b.Property<int>("VersionGroup")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("version_group")
                        .HasColumnType("integer")
                        .HasDefaultValueSql("1");

                    b.HasKey("Id", "TenantId", "Version")
                        .HasName("files_file_pkey");

                    b.HasIndex("FolderId")
                        .HasName("folder_id");

                    b.HasIndex("Id")
                        .HasName("id");

                    b.HasIndex("ModifiedOn")
                        .HasName("modified_on_files_file");

                    b.ToTable("files_file","onlyoffice");
                });

            modelBuilder.Entity("ASC.Files.Core.EF.DbFilesBunchObjects", b =>
                {
                    b.Property<int>("TenantId")
                        .HasColumnName("tenant_id")
                        .HasColumnType("integer");

                    b.Property<string>("RightNode")
                        .HasColumnName("right_node")
                        .HasColumnType("character varying(255)")
                        .HasMaxLength(255);

                    b.Property<string>("LeftNode")
                        .IsRequired()
                        .HasColumnName("left_node")
                        .HasColumnType("character varying(255)")
                        .HasMaxLength(255);

                    b.HasKey("TenantId", "RightNode")
                        .HasName("files_bunch_objects_pkey");

                    b.HasIndex("LeftNode")
                        .HasName("left_node");

                    b.ToTable("files_bunch_objects","onlyoffice");
                });

            modelBuilder.Entity("ASC.Files.Core.EF.DbFilesSecurity", b =>
                {
                    b.Property<int>("TenantId")
                        .HasColumnName("tenant_id")
                        .HasColumnType("integer");

                    b.Property<string>("EntryId")
                        .HasColumnName("entry_id")
                        .HasColumnType("character varying(50)")
                        .HasMaxLength(50);

                    b.Property<int>("EntryType")
                        .HasColumnName("entry_type")
                        .HasColumnType("integer");

                    b.Property<Guid>("Subject")
                        .HasColumnName("subject")
                        .HasColumnType("uuid")
                        .IsFixedLength(true)
                        .HasMaxLength(38);

                    b.Property<Guid>("Owner")
                        .HasColumnName("owner")
                        .HasColumnType("uuid")
                        .IsFixedLength(true)
                        .HasMaxLength(38);

                    b.Property<int>("Security")
                        .HasColumnName("security")
                        .HasColumnType("integer");

                    b.Property<DateTime>("TimeStamp")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("timestamp")
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.HasKey("TenantId", "EntryId", "EntryType", "Subject")
                        .HasName("files_security_pkey");

                    b.HasIndex("Owner")
                        .HasName("owner");

                    b.HasIndex("EntryId", "TenantId", "EntryType", "Owner")
                        .HasName("tenant_id_files_security");

                    b.ToTable("files_security","onlyoffice");
                });

            modelBuilder.Entity("ASC.Files.Core.EF.DbFilesTag", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("Flag")
                        .HasColumnName("flag")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasColumnType("character varying(255)")
                        .HasMaxLength(255);

                    b.Property<Guid>("Owner")
                        .HasColumnName("owner")
                        .HasColumnType("uuid")
                        .HasMaxLength(38);

                    b.Property<int>("TenantId")
                        .HasColumnName("tenant_id")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("TenantId", "Owner", "Name", "Flag")
                        .HasName("name_files_tag");

                    b.ToTable("files_tag","onlyoffice");
                });

            modelBuilder.Entity("ASC.Files.Core.EF.DbFilesTagLink", b =>
                {
                    b.Property<int>("TenantId")
                        .HasColumnName("tenant_id")
                        .HasColumnType("integer");

                    b.Property<int>("TagId")
                        .HasColumnName("tag_id")
                        .HasColumnType("integer");

                    b.Property<int>("EntryType")
                        .HasColumnName("entry_type")
                        .HasColumnType("integer");

                    b.Property<string>("EntryId")
                        .HasColumnName("entry_id")
                        .HasColumnType("character varying(32)")
                        .HasMaxLength(32);

                    b.Property<Guid>("CreateBy")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("create_by")
                        .HasColumnType("uuid")
                        .HasDefaultValueSql("NULL::bpchar")
                        .IsFixedLength(true)
                        .HasMaxLength(38);

                    b.Property<DateTime>("CreateOn")
                        .HasColumnName("create_on")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("TagCount")
                        .HasColumnName("tag_count")
                        .HasColumnType("integer");

                    b.HasKey("TenantId", "TagId", "EntryType", "EntryId")
                        .HasName("files_tag_link_pkey");

                    b.HasIndex("CreateOn")
                        .HasName("create_on_files_tag_link");

                    b.HasIndex("TenantId", "EntryType", "EntryId")
                        .HasName("entry_id");

                    b.ToTable("files_tag_link","onlyoffice");
                });

            modelBuilder.Entity("ASC.Files.Core.EF.DbFilesThirdpartyAccount", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<DateTime>("CreateOn")
                        .HasColumnName("create_on")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("FolderType")
                        .HasColumnName("folder_type")
                        .HasColumnType("integer");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnName("password")
                        .HasColumnType("character varying(100)")
                        .HasMaxLength(100);

                    b.Property<string>("Provider")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnName("provider")
                        .HasColumnType("character varying(50)")
                        .HasDefaultValueSql("'0'::character varying")
                        .HasMaxLength(50);

                    b.Property<int>("TenantId")
                        .HasColumnName("tenant_id")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnName("customer_title")
                        .HasColumnType("character varying(400)")
                        .HasMaxLength(400);

                    b.Property<string>("Token")
                        .HasColumnName("token")
                        .HasColumnType("text");

                    b.Property<string>("Url")
                        .HasColumnName("url")
                        .HasColumnType("text");

                    b.Property<Guid>("UserId")
                        .HasColumnName("user_id")
                        .HasColumnType("uuid")
                        .HasMaxLength(38);

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnName("user_name")
                        .HasColumnType("character varying(100)")
                        .HasMaxLength(100);

                    b.HasKey("Id");

                    b.ToTable("files_thirdparty_account","onlyoffice");
                });

            modelBuilder.Entity("ASC.Files.Core.EF.DbFilesThirdpartyApp", b =>
                {
                    b.Property<Guid>("UserId")
                        .HasColumnName("user_id")
                        .HasColumnType("uuid")
                        .HasMaxLength(38);

                    b.Property<string>("App")
                        .HasColumnName("app")
                        .HasColumnType("character varying(50)")
                        .HasMaxLength(50);

                    b.Property<DateTime>("ModifiedOn")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("modified_on")
                        .HasColumnType("timestamp without time zone")
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    b.Property<int>("TenantId")
                        .HasColumnName("tenant_id")
                        .HasColumnType("integer");

                    b.Property<string>("Token")
                        .HasColumnName("token")
                        .HasColumnType("text");

                    b.HasKey("UserId", "App")
                        .HasName("files_thirdparty_app_pkey");

                    b.ToTable("files_thirdparty_app","onlyoffice");
                });

            modelBuilder.Entity("ASC.Files.Core.EF.DbFilesThirdpartyIdMapping", b =>
                {
                    b.Property<string>("HashId")
                        .HasColumnName("hash_id")
                        .HasColumnType("character(32)")
                        .IsFixedLength(true)
                        .HasMaxLength(32);

                    b.Property<string>("Id")
                        .IsRequired()
                        .HasColumnName("id")
                        .HasColumnType("text");

                    b.Property<int>("TenantId")
                        .HasColumnName("tenant_id")
                        .HasColumnType("integer");

                    b.HasKey("HashId")
                        .HasName("files_thirdparty_id_mapping_pkey");

                    b.HasIndex("TenantId", "HashId")
                        .HasName("index_1");

                    b.ToTable("files_thirdparty_id_mapping","onlyoffice");
                });

            modelBuilder.Entity("ASC.Files.Core.EF.DbFolder", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<Guid>("CreateBy")
                        .HasColumnName("create_by")
                        .HasColumnType("uuid")
                        .IsFixedLength(true)
                        .HasMaxLength(38);

                    b.Property<DateTime>("CreateOn")
                        .HasColumnName("create_on")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("FilesCount")
                        .HasColumnName("filesCount")
                        .HasColumnType("integer");

                    b.Property<int>("FolderType")
                        .HasColumnName("folder_type")
                        .HasColumnType("integer");

                    b.Property<int>("FoldersCount")
                        .HasColumnName("foldersCount")
                        .HasColumnType("integer");

                    b.Property<Guid>("ModifiedBy")
                        .HasColumnName("modified_by")
                        .HasColumnType("uuid")
                        .IsFixedLength(true)
                        .HasMaxLength(38);

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnName("modified_on")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("ParentId")
                        .HasColumnName("parent_id")
                        .HasColumnType("integer");

                    b.Property<int>("TenantId")
                        .HasColumnName("tenant_id")
                        .HasColumnType("integer");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnName("title")
                        .HasColumnType("character varying(400)")
                        .HasMaxLength(400);

                    b.HasKey("Id");

                    b.HasIndex("ModifiedOn")
                        .HasName("modified_on_files_folder");

                    b.HasIndex("TenantId", "ParentId")
                        .HasName("parent_id");

                    b.ToTable("files_folder","onlyoffice");
                });

            modelBuilder.Entity("ASC.Files.Core.EF.DbFolderTree", b =>
                {
                    b.Property<int>("ParentId")
                        .HasColumnName("parent_id")
                        .HasColumnType("integer");

                    b.Property<int>("FolderId")
                        .HasColumnName("folder_id")
                        .HasColumnType("integer");

                    b.Property<int>("Level")
                        .HasColumnName("level")
                        .HasColumnType("integer");

                    b.HasKey("ParentId", "FolderId")
                        .HasName("files_folder_tree_pkey");

                    b.HasIndex("FolderId")
                        .HasName("folder_id_files_folder_tree");

                    b.ToTable("files_folder_tree","onlyoffice");
                });

            modelBuilder.Entity("ASC.Core.Common.EF.Model.DbTenantPartner", b =>
                {
                    b.HasOne("ASC.Core.Common.EF.Model.DbTenant", "Tenant")
                        .WithOne("Partner")
                        .HasForeignKey("ASC.Core.Common.EF.Model.DbTenantPartner", "TenantId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
