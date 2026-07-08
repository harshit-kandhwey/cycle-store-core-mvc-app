# Features

## Browsing
- **Home** — category cards; only curated, non-empty categories shown.
- **Category page** (`/Products/Category/{name}`) — subcategory tiles + products.
- **Subcategory page** (`/Products/Subcategory/{id}`) — the filterable product grid.
- **Details** (`/Products/Details/{id}`) — gallery, price, description, specs,
  ratings & reviews, related products.
- **Search** (`/Products/Search?q=`) — name/number match; input trimmed and length-capped.
- **Breadcrumb**, active-nav highlight, and a "you're viewing" tag.

## Product presentation
- **Image gallery** — main photo + real angle shots (`-2/-3/-4`); no stock padding.
- **Quick view** — modal fetched from `/Products/QuickView/{id}` (no full navigation).
- **Ratings & reviews** — star rating + review count on cards; a reviews block on
  Details. Deterministic per product (stable, no DB).
- **Sale pricing** — current `ListPrice` shown as the sale price with a higher
  struck-through "was" price and a **−N%** badge.
- **Brand** — detected from the product name (Rockrider, Triban, Van Rysel, …).
- **Descriptions** — generated per product from brand + subcategory + attributes.

## Filtering & sorting (Subcategory)
Multi-**colour**, **brand**, **price range**, and **in-stock** facets; sort by name
or price; removable active-filter chips; rich empty state.

## Cart & checkout
- **Guest cart** in the session (no login) — `CartStore` / `CartController`.
- **Add to cart** from cards, quick-view, and Details via `fetch` with a CSRF
  token; the header badge updates live.
- **Cart page** — quantity update, remove, subtotal.
- **Checkout** — validated contact/address form.
- **Confirmation** — generated order number (checkout is a mock; the schema has
  no order tables). All POSTs are `[ValidateAntiForgeryToken]`.

## UX
- **Dark / light theme** — no-flash inline switch, follows OS, persisted in
  `localStorage`, toggle in the header.
- **Loading skeletons** during infinite-scroll reveal and quick-view fetch.
- **Infinite scroll** on product grids.
- **Empty & error states** with icons across search, filters, and cart.
- **Lazy-loaded** product images.

## Security
- Nonce-based **Content-Security-Policy** + `X-Frame-Options`,
  `X-Content-Type-Options`, `Referrer-Policy`.
- **Host-header allow-list** (`AllowedHosts`, from config).
- **Search input** trimmed and length-bounded.
- **No secrets in source** — connection string comes only from configuration.
- CSRF tokens on all state-changing requests.
