using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AvatarCodex.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterComparison : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "comparison_attributes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_comparison_attributes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "character_attribute_values",
                columns: table => new
                {
                    character_id = table.Column<int>(type: "integer", nullable: false),
                    comparison_attribute_id = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_character_attribute_values", x => new { x.character_id, x.comparison_attribute_id });
                    table.CheckConstraint("ck_character_attribute_values_value_range", "value >= 0 AND value <= 100");
                    table.ForeignKey(
                        name: "fk_character_attribute_values_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_character_attribute_values_comparison_attributes_comparison",
                        column: x => x.comparison_attribute_id,
                        principalTable: "comparison_attributes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_character_attribute_values_comparison_attribute_id",
                table: "character_attribute_values",
                column: "comparison_attribute_id");

            migrationBuilder.CreateIndex(
                name: "ix_comparison_attributes_name",
                table: "comparison_attributes",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "character_attribute_values");

            migrationBuilder.DropTable(
                name: "comparison_attributes");
        }
    }
}
