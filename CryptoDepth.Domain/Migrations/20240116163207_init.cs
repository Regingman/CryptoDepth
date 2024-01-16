using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CryptoDepth.Domain.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TopCoinsInfos",
                columns: table => new
                {
                    Uq_Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Symbol = table.Column<string>(type: "text", nullable: true),
                    Id = table.Column<string>(type: "text", nullable: true),
                    Name1 = table.Column<string>(type: "text", nullable: true),
                    CostToMoveUpUsd1 = table.Column<decimal>(type: "numeric", nullable: false),
                    CostToMoveDownUsd1 = table.Column<decimal>(type: "numeric", nullable: false),
                    Name2 = table.Column<string>(type: "text", nullable: true),
                    CostToMoveUpUsd2 = table.Column<decimal>(type: "numeric", nullable: false),
                    CostToMoveDownUsd2 = table.Column<decimal>(type: "numeric", nullable: false),
                    Name3 = table.Column<string>(type: "text", nullable: true),
                    CostToMoveUpUsd3 = table.Column<decimal>(type: "numeric", nullable: false),
                    CostToMoveDownUsd3 = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopCoinsInfos", x => x.Uq_Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TopCoinsInfos");
        }
    }
}
