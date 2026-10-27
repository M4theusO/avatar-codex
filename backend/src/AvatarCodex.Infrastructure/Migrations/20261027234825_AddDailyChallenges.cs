using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AvatarCodex.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "challenge_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    challenge_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    target_value = table.Column<int>(type: "integer", nullable: false),
                    experience_reward = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_challenge_templates", x => x.id);
                    table.CheckConstraint("ck_challenge_templates_experience_reward_positive", "experience_reward > 0");
                    table.CheckConstraint("ck_challenge_templates_target_value_positive", "target_value > 0");
                });

            migrationBuilder.CreateTable(
                name: "daily_challenges",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    challenge_template_id = table.Column<int>(type: "integer", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_daily_challenges", x => x.id);
                    table.ForeignKey(
                        name: "fk_daily_challenges_challenge_templates_challenge_template_id",
                        column: x => x.challenge_template_id,
                        principalTable: "challenge_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_daily_challenges",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    daily_challenge_id = table.Column<int>(type: "integer", nullable: false),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_daily_challenges", x => new { x.user_id, x.daily_challenge_id });
                    table.ForeignKey(
                        name: "fk_user_daily_challenges_daily_challenges_daily_challenge_id",
                        column: x => x.daily_challenge_id,
                        principalTable: "daily_challenges",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_user_daily_challenges_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_challenge_templates_code",
                table: "challenge_templates",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_daily_challenges_challenge_template_id_date",
                table: "daily_challenges",
                columns: new[] { "challenge_template_id", "date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_daily_challenges_date",
                table: "daily_challenges",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "ix_user_daily_challenges_daily_challenge_id",
                table: "user_daily_challenges",
                column: "daily_challenge_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_daily_challenges_user_id_completed_at",
                table: "user_daily_challenges",
                columns: new[] { "user_id", "completed_at" },
                descending: new[] { false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_daily_challenges");

            migrationBuilder.DropTable(
                name: "daily_challenges");

            migrationBuilder.DropTable(
                name: "challenge_templates");
        }
    }
}
