using System.Text;
using FlexiReportServer.Context;
using FlexiReportServer.Dtos;
using FlexiReportServer.Endpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors();

builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
    opt.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=eCommerceDb;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");
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

app.MapPost("prompt",
    async (AIRequestDto request, HttpClient httpClient, CancellationToken cancellationToken) =>
    {
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {request.ApiKey}");

        var requestBody = new
        {
            model = request.Model,
            messages = new[]
            {
                new { role = "system", content = $"""
            You are a SQL expert specialized in Microsoft SQL Server (MSSQL).  
            Find the most suitable tables from the given database schema and generate an optimized, valid, and modern SQL query for MSSQL.  
            Return the SQL query.
            Do not include additional explanations or unnecessary characters.  
            Do not wrap the query in triple quotes or code blocks.  
            Use `TOP` instead of `LIMIT` when selecting a specific number of rows.  
            Here is the database schema: {request.Schema}
        """ },
                new { role = "user", content = request.Prompt}
            }
        };

        var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
        var responseString = await response.Content.ReadAsStringAsync();

        dynamic? jsonResponse = JsonConvert.DeserializeObject(responseString);
        return Results.Ok(jsonResponse?.choices[0].message.content.ToString());
    });
app.Run();