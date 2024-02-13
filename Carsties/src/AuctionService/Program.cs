using AuctionService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<AuctionDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
//var conString = builder.Configuration.GetConnectionString("DefaultConnection");

//builder.Services.AddDbContextPool<AuctionDbContext>(
//    options => options.UseNpgsql(conString, options =>
//    {
//        options.EnableRetryOnFailure();
//        //options.UseNetTopologySuite();
//    }));

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

try
{
    DbInitializer.InitDb(app);
}
catch (Exception e)
{
    Console.WriteLine(e);
}

app.Run();
