# Catalog & data

## How the catalog is composed

1. **Database** holds the products, subcategories, and categories
   (`Production.Product` / `ProductSubcategory` / `ProductCategory`).
2. **`CatalogCuration`** (app-side allow-list) decides which subcategories and
   product numbers the storefront actually shows. The DB may contain more.
3. **`CatalogImages`** resolves each product's photo from
   `wwwroot/Images/catalog/product/{product-number}.{jpg|jpeg|png|webp|avif}`,
   falling back to a subcategory then category image if none exists.
4. **`CatalogContent`** adds brand, generated description, deterministic
   ratings/reviews, and a compare-at ("was") sale price — all computed, no DB columns.

## The seeded catalog

`deploy/db/catalog_pivot.sql` creates the store's **114 curated products**
(ProductID 1000–1113) and 6 extra subcategories, moves **Helmets** under Clothing,
and removes duplicate size-variants from the original AdventureWorks rows. It is
idempotent — safe to re-run — and is applied after the base schema
(`CYCLE_STORE_Schema_data.sql`).

Product-number prefixes:
| Prefix | Category |
|--------|----------|
| `BX-*` | Bikes (mountain / road / touring + kids) |
| `PX-*` | Components (cranksets, chains, brakes, forks, pedals, saddles, wheels, …) |
| `CX-*` | Clothing (jerseys, gloves, shoes, socks, sunglasses, …) |
| `AX-*` | Accessories (helmets, lights, locks, pumps, bottles, bags, …) |

## Product images

- Live under `wwwroot/Images/catalog/product/`, named by product number in
  lowercase (e.g. `bx-mtb-01.avif`, `cx-sho-03.jpg`).
- Any of `.jpg .jpeg .png .webp .avif` is accepted and resolved automatically.
- Optional extra gallery angles: `{product-number}-2`, `-3`, `-4`.
- Only real product photos appear in the gallery; there is no stock-image padding.

## Adding a product

1. Insert a row into `Production.Product` with a new `ProductID` (≥ 1114 to avoid
   clashes), a unique `ProductNumber`, `ListPrice`, and an existing
   `ProductSubcategoryID`. (See the `IF NOT EXISTS … INSERT` pattern in
   `catalog_pivot.sql`.)
2. Drop its photo in `wwwroot/Images/catalog/product/{product-number}.<ext>`.
3. Add the product number (and its subcategory name, if new) to
   `Models/CatalogCuration.cs`.
4. Rebuild/redeploy the app tier (`git pull` + `deploy/setup-app.sh`).

## Adding a subcategory

Insert into `Production.ProductSubcategory` (pick an unused ID; not an identity
column) under the right `ProductCategoryID` (1 Bikes, 2 Components, 3 Clothing,
4 Accessories), then add its name to `CatalogCuration.Subcategories`.

## Database facts

- `ProductID` is **not** an identity column — supply it explicitly.
- Schema is `Production` (not `dbo`); database name is `CYCLE_STORE`.
- `cycleapp` has `SELECT/INSERT/UPDATE/DELETE` on the `Production` schema.
