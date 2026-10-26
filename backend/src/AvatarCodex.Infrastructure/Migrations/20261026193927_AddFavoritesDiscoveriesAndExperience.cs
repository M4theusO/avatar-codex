using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AvatarCodex.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFavoritesDiscoveriesAndExperience : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "character_discoveries",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    character_id = table.Column<int>(type: "integer", nullable: false),
                    discovered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_character_discoveries", x => new { x.user_id, x.character_id });
                    table.ForeignKey(
                        name: "fk_character_discoveries_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_character_discoveries_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "experience_transactions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    source_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_experience_transactions", x => x.id);
                    table.CheckConstraint("ck_experience_transactions_amount_positive", "amount > 0");
                    table.ForeignKey(
                        name: "fk_experience_transactions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "favorite_characters",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    character_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_favorite_characters", x => new { x.user_id, x.character_id });
                    table.ForeignKey(
                        name: "fk_favorite_characters_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_favorite_characters_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nation_discoveries",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nation_id = table.Column<int>(type: "integer", nullable: false),
                    discovered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_nation_discoveries", x => new { x.user_id, x.nation_id });
                    table.ForeignKey(
                        name: "fk_nation_discoveries_nations_nation_id",
                        column: x => x.nation_id,
                        principalTable: "nations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_nation_discoveries_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_character_discoveries_character_id",
                table: "character_discoveries",
                column: "character_id");

            migrationBuilder.CreateIndex(
                name: "ix_character_discoveries_user_id_discovered_at",
                table: "character_discoveries",
                columns: new[] { "user_id", "discovered_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_experience_transactions_user_id_type_source_id",
                table: "experience_transactions",
                columns: new[] { "user_id", "type", "source_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_favorite_characters_character_id",
                table: "favorite_characters",
                column: "character_id");

            migrationBuilder.CreateIndex(
                name: "ix_favorite_characters_user_id_created_at",
                table: "favorite_characters",
                columns: new[] { "user_id", "created_at" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_nation_discoveries_nation_id",
                table: "nation_discoveries",
                column: "nation_id");

            migrationBuilder.CreateIndex(
                name: "ix_nation_discoveries_user_id_discovered_at",
                table: "nation_discoveries",
                columns: new[] { "user_id", "discovered_at" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "character_discoveries");

            migrationBuilder.DropTable(
                name: "experience_transactions");

            migrationBuilder.DropTable(
                name: "favorite_characters");

            migrationBuilder.DropTable(
                name: "nation_discoveries");
        }
    }
}
