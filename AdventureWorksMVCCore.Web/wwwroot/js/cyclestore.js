// Cycle Store — front-end behaviours (mobile nav, theme, infinite scroll, quick-view, gallery)
(function () {
  "use strict";

  // ---- Mobile hamburger nav ----
  const bar = document.querySelector("header.bar");
  const toggle = document.querySelector(".nav-toggle");
  if (bar && toggle) {
    toggle.addEventListener("click", function () {
      var open = bar.classList.toggle("open");
      toggle.setAttribute("aria-expanded", open ? "true" : "false");
      var use = toggle.querySelector("use");
      if (use) {
        use.setAttribute("href", open ? "#close" : "#menu");
      }
    });
  }

  // ---- Dark / light theme toggle (initial theme set inline in <head> to avoid FOUC) ----
  const themeBtn = document.querySelector(".theme-toggle");
  if (themeBtn) {
    themeBtn.addEventListener("click", function () {
      const root = document.documentElement;
      let current = root.getAttribute("data-theme");
      if (!current) {
        current =
          window.matchMedia &&
          window.matchMedia("(prefers-color-scheme: dark)").matches
            ? "dark"
            : "light";
      }
      const next = current === "dark" ? "light" : "dark";
      root.setAttribute("data-theme", next);
      try {
        localStorage.setItem("cs-theme", next);
      } catch (e) {}
      themeBtn.setAttribute(
        "aria-label",
        next === "dark" ? "Switch to light theme" : "Switch to dark theme",
      );
    });
  }

  // ---- Product image gallery (Details page + quick-view modal) ----
  function initGallery(root) {
    (root || document).querySelectorAll("[data-gallery]").forEach(function (g) {
      if (g.dataset.wired) return;
      g.dataset.wired = "1";
      const main = g.querySelector(".gmain img");
      const thumbs = Array.prototype.slice.call(g.querySelectorAll(".gthumb"));
      thumbs.forEach(function (t) {
        t.addEventListener("click", function () {
          const src = t.getAttribute("data-src");
          if (main && src) {
            main.src = src;
          }
          thumbs.forEach(function (x) {
            x.classList.remove("on");
          });
          t.classList.add("on");
        });
      });
      // Zoom on hover (pointer devices only)
      const box = g.querySelector(".gmain");
      if (
        box &&
        main &&
        window.matchMedia &&
        window.matchMedia("(hover:hover)").matches
      ) {
        box.addEventListener("mousemove", function (e) {
          var r = box.getBoundingClientRect();
          main.style.transformOrigin =
            ((e.clientX - r.left) / r.width) * 100 +
            "% " +
            ((e.clientY - r.top) / r.height) * 100 +
            "%";
          main.style.transform = "scale(1.9)";
        });
        box.addEventListener("mouseleave", function () {
          main.style.transform = "";
        });
      }
    });
  }
  initGallery(document);

  // ---- Quick-view modal ----
  const modal = document.getElementById("qvModal");
  const qvContent = document.getElementById("qvContent");
  let lastFocus = null;

  function skeletonQV() {
    return (
      '<div class="qv"><div class="qv-vis skel"></div>' +
      '<div class="qv-body">' +
      '<div class="skel l" style="height:11px;width:30%"></div>' +
      '<div class="skel l" style="height:22px;width:75%;margin-top:6px"></div>' +
      '<div class="skel l" style="height:26px;width:40%;margin-top:10px"></div>' +
      '<div class="skel l" style="height:38px;width:100%;margin-top:16px"></div>' +
      "</div></div>"
    );
  }

  function openModal() {
    if (!modal) return;
    lastFocus = document.activeElement;
    modal.classList.add("open");
    modal.setAttribute("aria-hidden", "false");
    document.body.style.overflow = "hidden";
    var closeBtn = modal.querySelector(".modal-close");
    if (closeBtn) {
      closeBtn.focus();
    }
  }
  function closeModal() {
    if (!modal) return;
    modal.classList.remove("open");
    modal.setAttribute("aria-hidden", "true");
    document.body.style.overflow = "";
    if (qvContent) {
      qvContent.innerHTML = "";
    }
    if (lastFocus && lastFocus.focus) {
      lastFocus.focus();
    }
  }

  if (modal) {
    modal.querySelectorAll("[data-close]").forEach(function (el) {
      el.addEventListener("click", closeModal);
    });
    document.addEventListener("keydown", function (e) {
      if (e.key === "Escape" && modal.classList.contains("open")) {
        closeModal();
      }
    });
  }

  document.addEventListener("click", function (e) {
    const btn = e.target.closest ? e.target.closest("[data-qview]") : null;
    if (!btn || !modal || !qvContent) return;
    e.preventDefault();
    const id = btn.getAttribute("data-qview");
    qvContent.innerHTML = skeletonQV();
    openModal();
    fetch("/Products/QuickView/" + encodeURIComponent(id), {
      headers: { "X-Requested-With": "fetch" },
    })
      .then(function (r) {
        if (!r.ok) throw new Error("bad status");
        return r.text();
      })
      .then(function (html) {
        qvContent.innerHTML = "";
        qvContent.insertAdjacentHTML("beforeend", html);
        initGallery(qvContent);
      })
      .catch(function () {
        qvContent.innerHTML = "";
        const errorDiv = document.createElement("div");
        errorDiv.className = "state";
        errorDiv.style.gridColumn = "1 / -1";
        const h3 = document.createElement("h3");
        h3.textContent = "Couldn’t load preview";
        const p = document.createElement("p");
        p.textContent = "Please try opening the full product page instead.";
        errorDiv.appendChild(h3);
        errorDiv.appendChild(p);
        qvContent.appendChild(errorDiv);
      });
  });

  // ---- Infinite scroll with skeleton flash ----
  const grid = document.querySelector(".prods[data-infinite]");
  const sentinel = document.getElementById("scroll-sentinel");
  if (grid) {
    const cards = Array.prototype.slice.call(grid.querySelectorAll(".pcard"));
    const BATCH = 9;
    let shown = Math.min(BATCH, cards.length);
    let busy = false;
    cards.forEach(function (c, i) {
      if (i >= shown) {
        c.style.display = "none";
      }
    });

    function hideSentinel() {
      if (sentinel) {
        sentinel.style.display = "none";
      }
    }

    function skelRow(n) {
      const box = document.createElement("div");
      box.className = "skgrid sk-temp";
      const one =
        '<div class="skcard"><div class="skv skel"></div><div class="skb">' +
        '<div class="skel l w40"></div><div class="skel l w80"></div><div class="skel l w60"></div></div></div>';
      let html = "";
      for (let i = 0; i < n; i++) {
        html += one;
      }
      box.innerHTML = html;
      return box;
    }

    function revealMore() {
      if (busy) return;
      const remaining = cards.length - shown;
      if (remaining <= 0) {
        hideSentinel();
        return;
      }
      busy = true;
      const n = Math.min(BATCH, remaining);
      const sk = skelRow(n);
      if (grid.parentNode) {
        grid.parentNode.insertBefore(sk, sentinel || null);
      }
      setTimeout(function () {
        const end = shown + n;
        for (let i = shown; i < end; i++) {
          cards[i].style.display = "";
        }
        shown = end;
        if (sk.parentNode) {
          sk.parentNode.removeChild(sk);
        }
        busy = false;
        if (shown >= cards.length) {
          hideSentinel();
        }
      }, 420);
    }

    if (shown >= cards.length) {
      hideSentinel();
    } else if (sentinel && "IntersectionObserver" in window) {
      const io = new IntersectionObserver(
        function (entries) {
          entries.forEach(function (e) {
            if (e.isIntersecting) {
              revealMore();
            }
          });
        },
        { rootMargin: "400px 0px" },
      );
      io.observe(sentinel);
    } else {
      cards.forEach(function (c) {
        c.style.display = "";
      });
      hideSentinel();
    }
  }
})();

// Add to cart — delegated click, posts to /Cart/Add with the CSRF token and
// updates the header badge without a full page reload.
(function () {
  const meta = document.querySelector('meta[name="csrf-token"]');
  const csrf = meta ? meta.getAttribute("content") : "";

  document.addEventListener("click", function (e) {
    const btn = e.target.closest ? e.target.closest("[data-add-cart]") : null;
    if (!btn) return;
    e.preventDefault();

    const id = btn.getAttribute("data-add-cart");
    const orig = btn.innerHTML;
    btn.disabled = true;

    const body = "id=" + encodeURIComponent(id) + "&qty=1";
    fetch("/Cart/Add", {
      method: "POST",
      headers: {
        "X-Requested-With": "fetch",
        RequestVerificationToken: csrf,
        "Content-Type": "application/x-www-form-urlencoded",
      },
      body: body,
    })
      .then(function (r) {
        if (!r.ok) throw new Error("bad status");
        return r.json();
      })
      .then(function (d) {
        if (d && typeof d.count === "number") {
          document
            .querySelectorAll("[data-cart-count]")
            .forEach(function (dot) {
              dot.textContent = d.count;
              dot.classList.remove("hide");
            });
        }
        btn.innerHTML = "Added ✓";
        setTimeout(function () {
          btn.innerHTML = orig;
          btn.disabled = false;
        }, 1100);
      })
      .catch(function () {
        btn.innerHTML = orig;
        btn.disabled = false;
      });
  });
})();
