using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaintenancePortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotoStorageKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StorageUrl",
                table: "RequestPhotos",
                newName: "StorageKey");

            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "RequestPhotos",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "SizeBytes",
                table: "RequestPhotos",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "RequestPhotos");

            migrationBuilder.DropColumn(
                name: "SizeBytes",
                table: "RequestPhotos");

            migrationBuilder.RenameColumn(
                name: "StorageKey",
                table: "RequestPhotos",
                newName: "StorageUrl");
        }
    }
}
