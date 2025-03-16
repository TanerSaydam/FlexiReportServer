using FlexiReportServer.Context;
using FlexiReportServer.Endpoints;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors();

builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

builder.Services.AddHttpClient();

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseCors(x => x
.AllowAnyMethod()
.AllowAnyHeader()
.AllowAnyOrigin()
.SetPreflightMaxAge(TimeSpan.FromMinutes(10)));

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapCategoryRoutes();
app.MapProductRoutes();
app.MapSalesmanRoutes();
app.MapInvoiceRoutes();

app.Run();