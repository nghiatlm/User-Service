using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserService.Repository.Migrations
{
    /// <inheritdoc />
    public partial class initCreateDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    user_name = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    first_name = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(100)", nullable: false),
                    date_of_birth = table.Column<DateTime>(type: "date", nullable: true),
                    phone = table.Column<string>(type: "nvarchar(15)", nullable: false),
                    password = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    avatar = table.Column<string>(type: "nvarchar(255)", nullable: false),
                    role_name = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
