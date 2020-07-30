using Accounts;
using Accounts.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Projects.Data;
using Steeltoe.CloudFoundry.Connector.MySql.EFCore;
using Steeltoe.Management.CloudFoundry;
using Users.Data;
using Steeltoe.Discovery.Client;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Steeltoe.Security.Authentication.CloudFoundry;

namespace RegistrationServer
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
            services.AddCloudFoundryActuators(Configuration);

            services.AddControllers(mvcOptions =>
 {
     if (!Configuration.GetValue("DISABLE_AUTH", false))
     {
         // Set Authorized as default policy
         var policy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
  .RequireAuthenticatedUser()
  .RequireClaim("scope", "uaa.resource")
  .Build();

         mvcOptions.Filters.Add(new AuthorizeFilter(policy));
     }
 });

            services.AddScoped<IAccountDataGateway, AccountDataGateway>();
            services.AddDbContext<AccountContext>(options => options.UseMySql(Configuration));

            services.AddScoped<IUserDataGateway, UserDataGateway>();
            services.AddDbContext<UserContext>(options => options.UseMySql(Configuration));

            services.AddScoped<IProjectDataGateway, ProjectDataGateway>();
            services.AddDbContext<ProjectContext>(options => options.UseMySql(Configuration));

            services.AddScoped<IRegistrationService, RegistrationService>();
            services.AddDiscoveryClient(Configuration);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
   .AddCloudFoundryJwtBearer(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCloudFoundryActuators();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            app.UseDiscoveryClient();
        }
    }
}
