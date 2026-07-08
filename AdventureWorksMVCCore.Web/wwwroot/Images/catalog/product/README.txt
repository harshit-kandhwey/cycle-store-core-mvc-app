Per-product photos for the pivoted demo catalog.

One image per product, named with the product number in lowercase (hyphens
kept), e.g. bx-mtb-01.avif. Any of these formats is accepted and resolved
automatically: .jpg .jpeg .png .webp .avif  (see Models/CatalogImages.cs).
If a product has no file here, the app falls back to the subcategory photo
(../sub/{slug}.jpg), then the category photo.

The storefront catalog was pivoted to a hand-built set of real cycling
products (CYCLE_STORE ProductID 1000-1072). Product numbers use these prefixes:
  BX-MTB / BX-RB / BX-TB   bikes (mountain / road / touring+kids)
  PX-*                     components (saddles, chains, derailleurs, brakes, handlebars, wheels)
  CX-*                     clothing (gloves, jerseys, shorts, caps, vests, tights)
  AX-*                     accessories (helmets, lights, locks, pumps, bottles, fenders, panniers, racks, stands, packs, cleaners)

The authoritative product list lives in Models/CatalogCuration.cs.
Optional extra gallery angles: {product-number}-2 / -3 / -4 (any accepted format).
