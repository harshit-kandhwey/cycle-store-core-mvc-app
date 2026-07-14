using System;
using AdventureWorksMVCCore.Application.Interfaces;
using AdventureWorksMVCCore.Application.Services;
using AdventureWorksMVCCore.Domain.Interfaces;
using AdventureWorksMVCCore.Infrastructure.Data;
using AdventureWorksMVCCore.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AdventureWorksMVCCore.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            // Connection string is supplied entirely by configuration, i.e. the
            // ConnectionStrings__DefaultConnection environment variable set in the
            // systemd unit. No AWS Secrets Manager, no hardcoded host. On the AWS
            // cutover only that environment variable changes.
            string connectionString = Configuration.GetConnectionString("DefaultConnection");

            // Register DbContext with Clean Architecture pattern
            services.AddDbContext<CycleStoreContext>(options =>
                options.UseSqlServer(connectionString));

            // Register repositories (Infrastructure layer)
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();

            // Register application services (Application layer)
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ICategoryService, CategoryService>();

            // Guest cart lives in the session (no DB order tables in CYCLE_STORE).
            services.AddHttpContextAccessor();
            services.AddDistributedMemoryCache();
            services.AddSession(o =>
            {
                o.IdleTimeout = TimeSpan.FromDays(7);
                o.Cookie.HttpOnly = true;
                o.Cookie.IsEssential = true;
                o.Cookie.Name = "cs-cart";
            });
            // Let the add-to-cart fetch() send the CSRF token in a header.
            services.AddAntiforgery(o => o.HeaderName = "RequestVerificationToken");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            // Security response headers. Runs first so every response — including
            // static files — is covered. A per-request CSP nonce authorises the one
            // inline (no-FOUC theme) script in _Layout without opening the policy to
            // 'unsafe-inline', so injected scripts are still blocked.
            app.Use(async (context, next) =>
            {
                var nonce = Convert.ToBase64String(
                    System.Security.Cryptography.RandomNumberGenerator.GetBytes(16));
                context.Items["csp-nonce"] = nonce;

                var headers = context.Response.Headers;
                headers["X-Content-Type-Options"] = "nosniff";
                headers["X-Frame-Options"] = "DENY";
                headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                headers["X-XSS-Protection"] = "0"; // legacy filter off per current guidance; CSP covers this
                headers["Content-Security-Policy"] =
                    "default-src 'self'; " +
                    "img-src 'self' data:; " +
                    "style-src 'self' 'unsafe-inline'; " +
                    "script-src 'self' 'nonce-" + nonce + "'; " +
                    "font-src 'self' data:; " +
                    "object-src 'none'; " +
                    "base-uri 'self'; " +
                    "frame-ancestors 'none'; " +
                    "form-action 'self'";

                await next();
            });

            // Serve modern image formats: ASP.NET Core's default static-file MIME map has
            // no entry for .avif, so those files would 404. Register it (most product
            // photos are .avif) alongside the built-in types.
            var contentTypes = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            contentTypes.Mappings[".avif"] = "image/avif";
            app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = contentTypes });

            app.UseRouting();

            app.UseSession();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
