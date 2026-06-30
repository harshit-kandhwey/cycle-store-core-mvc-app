// Cycle Store — front-end behaviours (mobile nav + infinite scroll)
(function () {
  "use strict";

  // ---- Mobile hamburger nav ----
  var bar = document.querySelector("header.bar");
  var toggle = document.querySelector(".nav-toggle");
  if (bar && toggle) {
    toggle.addEventListener("click", function () {
      var open = bar.classList.toggle("open");
      toggle.setAttribute("aria-expanded", open ? "true" : "false");
      var use = toggle.querySelector("use");
      if (use) { use.setAttribute("href", open ? "#close" : "#menu"); }
    });
  }

  // ---- Infinite scroll: progressively reveal cards in a .prods[data-infinite] grid ----
  var grid = document.querySelector(".prods[data-infinite]");
  var sentinel = document.getElementById("scroll-sentinel");
  if (grid) {
    var cards = Array.prototype.slice.call(grid.querySelectorAll(".pcard"));
    var BATCH = 9;
    var shown = Math.min(BATCH, cards.length);

    // hide everything past the first batch
    cards.forEach(function (c, i) { if (i >= shown) { c.style.display = "none"; } });

    function hideSentinel() { if (sentinel) { sentinel.style.display = "none"; } }

    function revealMore() {
      var end = Math.min(shown + BATCH, cards.length);
      for (var i = shown; i < end; i++) { cards[i].style.display = ""; }
      shown = end;
      if (shown >= cards.length) { hideSentinel(); }
    }

    if (shown >= cards.length) {
      hideSentinel();
    } else if (sentinel && "IntersectionObserver" in window) {
      var io = new IntersectionObserver(function (entries) {
        entries.forEach(function (e) { if (e.isIntersecting) { revealMore(); } });
      }, { rootMargin: "500px 0px" });
      io.observe(sentinel);
    } else {
      // Fallback: no IntersectionObserver — just show everything.
      cards.forEach(function (c) { c.style.display = ""; });
      hideSentinel();
    }
  }
})();
