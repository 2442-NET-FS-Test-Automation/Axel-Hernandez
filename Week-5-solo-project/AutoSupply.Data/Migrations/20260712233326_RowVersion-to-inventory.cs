using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSupply.Data.Migrations
{
    /// <inheritdoc />
    public partial class RowVersiontoinventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "InventoryItems");
        }
    }
}
