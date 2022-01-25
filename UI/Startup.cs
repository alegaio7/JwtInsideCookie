using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using UI.Authorization;
using UI.Helpers;

namespace UI
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
            var dpp = DataProtectionProvider.Create("JwtInsideCookie");

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "JwtInsideCookie UI", Version = "v1" });
            });

            services.
                AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).
                AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
                    options.SlidingExpiration = true;
                    options.TicketDataFormat = new JwtAuthTicketFormat(
                            JwtOnCookieHelper.TokenValidationParameters,
                            TicketSerializer.Default, 
                            dpp.CreateProtector("CookieEncryption")
                        );
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = options.LoginPath;
                    options.ReturnUrlParameter = "returnUrl";
                });

            services.Configure<ApiOptions>(Configuration.GetSection("ApiEndpoint"));

            services.AddHttpClient("ApiClient", c =>
            {
                var apiEP = new ApiOptions();
                Configuration.GetSection("ApiEndpoint").Bind(apiEP);
                c.BaseAddress = new Uri(apiEP.Url);
                c.DefaultRequestHeaders.Add("Accept", "application/json");
                if (apiEP.Timeout < 30) apiEP.Timeout = 30;
                if (apiEP.Timeout > 600) apiEP.Timeout = 600;
                c.Timeout = TimeSpan.FromSeconds(apiEP.Timeout);
            });

            //services.AddMemoryCache(); // not needed since AddRazorPages uses it implicitly (it's already added)

            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
            });

            services.AddRazorPages().
                AddRazorRuntimeCompilation().
                AddRazorPagesOptions(options => {
                    options.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
                }).
                AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = null);

            services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = null);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "JwtInsideCookie UI v1"));
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // extracts the UserState claim from the authenticated user and puts it in the httpContext as a ApiClaimsPrincipal
            app.UseMiddleware<UserStateExtractorMiddleware>();

            // this middleware checks if a request to the API generated a 401 (Unauthorized) response,
            // if so, performs a sign out and redirect the user to the login page
            app.UseMiddleware<UnauthorizedJwtRedirectMiddleware>();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "api/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllers();
                endpoints.MapRazorPages();
            });
        }
    }
}