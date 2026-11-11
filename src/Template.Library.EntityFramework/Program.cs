// Copyright © Korshun Ivan. All rights reserved.

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Template.Library.Common;
using Template.Library.EntityFramework;

IHostBuilder builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    string connectionString = context.Configuration["Library:ConnectionString"].EnsureNotEmpty();

    services.AddDbContext<LibraryDbContext>(
        o => o.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(Program).Assembly.FullName)));
});

IHost host = builder.Build();
host.Run();
