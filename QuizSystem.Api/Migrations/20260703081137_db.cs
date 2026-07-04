using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizSystem.Api.Migrations
{
    /// <inheritdoc />
    public partial class db : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePublicId",
                schema: "quiz_schema",
                table: "Questions",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                schema: "quiz_schema",
                table: "Questions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ImageHeight",
                schema: "quiz_schema",
                table: "QuestionOptions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePublicId",
                schema: "quiz_schema",
                table: "QuestionOptions",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                schema: "quiz_schema",
                table: "QuestionOptions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ImageWidth",
                schema: "quiz_schema",
                table: "QuestionOptions",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePublicId",
                schema: "quiz_schema",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                schema: "quiz_schema",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "ImageHeight",
                schema: "quiz_schema",
                table: "QuestionOptions");

            migrationBuilder.DropColumn(
                name: "ImagePublicId",
                schema: "quiz_schema",
                table: "QuestionOptions");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                schema: "quiz_schema",
                table: "QuestionOptions");

            migrationBuilder.DropColumn(
                name: "ImageWidth",
                schema: "quiz_schema",
                table: "QuestionOptions");
        }
    }
}
