using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AvatarCodex.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "eras",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_eras", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "timeline_events",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    date_label = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    image_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    image_source_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    image_credit = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    order = table.Column<int>(type: "integer", nullable: false),
                    era_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_timeline_events", x => x.id);
                    table.ForeignKey(
                        name: "fk_timeline_events_eras_era_id",
                        column: x => x.era_id,
                        principalTable: "eras",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "timeline_event_characters",
                columns: table => new
                {
                    character_id = table.Column<int>(type: "integer", nullable: false),
                    timeline_event_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_timeline_event_characters", x => new { x.character_id, x.timeline_event_id });
                    table.ForeignKey(
                        name: "fk_timeline_event_characters_characters_character_id",
                        column: x => x.character_id,
                        principalTable: "characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_timeline_event_characters_timeline_events_timeline_event_id",
                        column: x => x.timeline_event_id,
                        principalTable: "timeline_events",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_eras_name",
                table: "eras",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_eras_order",
                table: "eras",
                column: "order",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_timeline_event_characters_timeline_event_id",
                table: "timeline_event_characters",
                column: "timeline_event_id");

            migrationBuilder.CreateIndex(
                name: "ix_timeline_events_era_id",
                table: "timeline_events",
                column: "era_id");

            migrationBuilder.CreateIndex(
                name: "ix_timeline_events_order",
                table: "timeline_events",
                column: "order",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_timeline_events_title",
                table: "timeline_events",
                column: "title",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "timeline_event_characters");

            migrationBuilder.DropTable(
                name: "timeline_events");

            migrationBuilder.DropTable(
                name: "eras");
        }
    }
}
