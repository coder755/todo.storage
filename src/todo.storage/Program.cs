﻿using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using todo.storage.db;
using todo.storage.Services.User;

var builder = WebApplication.CreateBuilder(args);
// Ensure appsettings.json is loaded
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true);
var connectionString = GetConnectionString(builder.Configuration);

builder.Services.AddDbContext<UsersContext>(options =>
{
    var serverVersion = new MySqlServerVersion("8.0");
    options.UseMySql(connectionString, serverVersion, optionsBuilder =>
    {
        optionsBuilder.EnableRetryOnFailure();
    });
});
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddMemoryCache();
builder.Services.AddMvc(options => options.EnableEndpointRouting = false)
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    })
    .AddNewtonsoftJson(options =>
    {
        options.AllowInputFormatterExceptionMessages = false;
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
    });
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
// app.UseAuthentication();
// app.UseAuthorization();
app.MapHealthChecks("/healthcheck");
app.MapControllers();
app.Run();

string GetConnectionString(IConfiguration configuration)
{
    const string dbSection = "Todo.Storage:Db";
    var dbConfig = configuration.GetSection(dbSection);
    var server = dbConfig.GetValue<string>("Server");
    var port = dbConfig.GetValue<string>("Port");
    var database = dbConfig.GetValue<string>("Db");
    var userId = Environment.GetEnvironmentVariable("TODO_DB_ID");
    var password = Environment.GetEnvironmentVariable("TODO_DB_PW");
    var connStr = $"server={server};port={port};user={userId};password={password};database={database};";

    return connStr;
}