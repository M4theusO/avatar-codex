using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AvatarCodex.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddElementTest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "element_test_questions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    text = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_element_test_questions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "element_test_options",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    question_id = table.Column<int>(type: "integer", nullable: false),
                    text = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    element_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_element_test_options", x => x.id);
                    table.ForeignKey(
                        name: "fk_element_test_options_element_test_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "element_test_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_element_test_options_elements_element_id",
                        column: x => x.element_id,
                        principalTable: "elements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_element_test_options_element_id",
                table: "element_test_options",
                column: "element_id");

            migrationBuilder.CreateIndex(
                name: "ix_element_test_options_question_id_element_id",
                table: "element_test_options",
                columns: new[] { "question_id", "element_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_element_test_options_question_id_order",
                table: "element_test_options",
                columns: new[] { "question_id", "order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_element_test_questions_order",
                table: "element_test_questions",
                column: "order",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "element_test_options");

            migrationBuilder.DropTable(
                name: "element_test_questions");
        }
    }
}
