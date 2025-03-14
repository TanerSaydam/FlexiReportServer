using FlexiReportServer.Context;
using FlexiReportServer.Dtos;
using FlexiReportServer.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors();

builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
    opt.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=eCommerceDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");
});

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

app.MapReportRoutes();
app.MapCategoryRoutes();
app.MapProductRoutes();
app.MapSalesmanRoutes();
app.MapInvoiceRoutes();

app.MapGet("database-schema",
    async (ApplicationDbContext dbContext, CancellationToken cancellationToken) =>
    {
        using var connection = dbContext.Database.GetDbConnection();
        await connection.OpenAsync();

        var query = @"
        SELECT TABLE_NAME, COLUMN_NAME
        FROM INFORMATION_SCHEMA.COLUMNS
        ORDER BY TABLE_NAME, ORDINAL_POSITION";

        var schema = new Dictionary<string, List<string>>();

        using var command = connection.CreateCommand();
        command.CommandText = query;

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            string tableName = reader.GetString(0);
            string columnName = reader.GetString(1);

            if (!schema.ContainsKey(tableName))
                schema[tableName] = new List<string>();

            schema[tableName].Add(columnName);
        }

        return Results.Ok(schema.Where(p => p.Key != "__EFMigrationsHistory").Select(s => new { TableName = s.Key, Columns = s.Value }));
    });

app.MapPost("execute-query",
    async ([FromBody] QueryRequestDto request, ApplicationDbContext dbContext, CancellationToken cancellationToken) =>
    {
        if (string.IsNullOrWhiteSpace(request.SqlQuery))
            return Results.BadRequest("SQL sorgusu bo� olamaz.");

        // Sadece SELECT sorgular�na izin verelim
        if (!request.SqlQuery.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest("Sadece SELECT sorgular�na izin verilmektedir.");

        try
        {
            using var connection = dbContext.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = request.SqlQuery;

            using var reader = await command.ExecuteReaderAsync();
            var result = new List<Dictionary<string, object>>();

            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
                }
                result.Add(row);
            }

            return Results.Ok(result);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { Error = "Sorgu �al��t�r�lamad�.", Details = ex.Message });
        }
    });
app.Run();