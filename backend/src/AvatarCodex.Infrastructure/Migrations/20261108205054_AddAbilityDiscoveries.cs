using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AvatarCodex.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAbilityDiscoveries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ability_discoveries",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ability_id = table.Column<int>(type: "integer", nullable: false),
                    discovered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ability_discoveries", x => new { x.user_id, x.ability_id });
                    table.ForeignKey(
                        name: "fk_ability_discoveries_abilities_ability_id",
                        column: x => x.ability_id,
                        principalTable: "abilities",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_ability_discoveries_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ability_discoveries_ability_id",
                table: "ability_discoveries",
                column: "ability_id");

            migrationBuilder.CreateIndex(
                name: "ix_ability_discoveries_user_id_discovered_at",
                table: "ability_discoveries",
                columns: new[] { "user_id", "discovered_at" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ability_discoveries");
        }
    }
}
