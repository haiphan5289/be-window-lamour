using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Lamour.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CategoriesCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create categories table first — need it to exist before we can backfill from the
            //    old free-text products.category column (EF's auto-scaffold drops that column first,
            //    which would destroy the data before we could read it — reordered here on purpose).
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_categories_name",
                table: "categories",
                column: "name",
                unique: true);

            // 2. Add category_id as nullable for now — populated by the backfill below, then locked
            //    to NOT NULL once every row has a value.
            migrationBuilder.AddColumn<int>(
                name: "category_id",
                table: "products",
                type: "integer",
                nullable: true);

            // 3. One category row per distinct existing product.category value — trimmed and
            //    deduped case-insensitively so "Son môi" / "son môi " don't become 2 categories.
            migrationBuilder.Sql(@"
                INSERT INTO categories (name)
                SELECT DISTINCT ON (LOWER(TRIM(category))) TRIM(category)
                FROM products
                WHERE category IS NOT NULL AND TRIM(category) <> ''
                ORDER BY LOWER(TRIM(category)), id;
            ");

            // 4. Fallback category for any product whose category was blank/whitespace-only.
            migrationBuilder.Sql(@"
                INSERT INTO categories (name)
                SELECT 'Chưa phân loại'
                WHERE EXISTS (SELECT 1 FROM products WHERE category IS NULL OR TRIM(category) = '')
                  AND NOT EXISTS (SELECT 1 FROM categories WHERE LOWER(name) = LOWER('Chưa phân loại'));
            ");

            // 5. Point every product at its matching category (case-insensitive, trimmed).
            migrationBuilder.Sql(@"
                UPDATE products p
                SET category_id = c.id
                FROM categories c
                WHERE LOWER(TRIM(p.category)) = LOWER(c.name);
            ");
            migrationBuilder.Sql(@"
                UPDATE products p
                SET category_id = c.id
                FROM categories c
                WHERE p.category_id IS NULL AND LOWER(c.name) = LOWER('Chưa phân loại');
            ");

            // 6. Every row now has a category_id — lock the column down and add the FK.
            migrationBuilder.AlterColumn<int>(
                name: "category_id",
                table: "products",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_products_category_id",
                table: "products",
                column: "category_id");

            migrationBuilder.AddForeignKey(
                name: "FK_products_categories_category_id",
                table: "products",
                column: "category_id",
                principalTable: "categories",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            // 7. Drop the old free-text column last, now that its data has been migrated out.
            migrationBuilder.DropColumn(
                name: "category",
                table: "products");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "products",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            // Best-effort restore: copy the category name back onto products via the FK before
            // the categories table/relationship is dropped.
            migrationBuilder.Sql(@"
                UPDATE products p
                SET category = c.name
                FROM categories c
                WHERE p.category_id = c.id;
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_products_categories_category_id",
                table: "products");

            migrationBuilder.DropIndex(
                name: "IX_products_category_id",
                table: "products");

            migrationBuilder.DropColumn(
                name: "category_id",
                table: "products");

            migrationBuilder.DropTable(
                name: "categories");
        }
    }
}
