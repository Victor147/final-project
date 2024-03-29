﻿using final_project.Data.Initialization;
using final_project.Data.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace final_project.Extensions
{   
    public static class DataExtensions
    {
        public static void AddPersistence(this WebApplicationBuilder builder)
        {
            // var connectionStringBuilder = new SqlConnectionStringBuilder(builder.Configuration.GetConnectionString("DefaultConnection"));
            var connectionStringBuilder = new SqlConnectionStringBuilder("Data Source=SQL6030.site4now.net;Initial Catalog=db_a99ca8_ecommercedb;User Id=db_a99ca8_ecommercedb_admin;Password=MyRootPass1234");
            // connectionStringBuilder.UserID = builder.Configuration["DbUser"];
            // connectionStringBuilder.Password = builder.Configuration["DbPassword"];

            builder.Services.AddDbContext<EcommerceDbContext>(opt =>
            opt.UseSqlServer(connectionStringBuilder.ConnectionString));
        }

        public static void EnsureDbCreated(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetService<EcommerceDbContext>();
            db!.Database.EnsureCreated();

            var dataInitializer = scope.ServiceProvider.GetService<DataInitializer>();
            dataInitializer!.Seed().Wait();
        }
    }
}
