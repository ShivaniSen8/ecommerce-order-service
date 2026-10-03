using System.Diagnostics;

using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Repositories;
using OrderService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("OrderDatabase")));

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService.Services.OrderService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("authenticated", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});
var app = builder.Build();

app.Use(async (context, next) =>
{
    var activity = Activity.Current;
    var traceId = activity?.TraceId.ToString() ?? context.TraceIdentifier;
    var spanId = activity?.SpanId.ToString() ?? context.TraceIdentifier;

    context.Response.Headers["X-Trace-ID"] = traceId;
    context.Response.Headers["X-Span-ID"] = spanId;

    using (app.Logger.BeginScope(new Dictionary<string, object>
    {
        ["TraceId"] = traceId,
        ["SpanId"] = spanId
    }))
    {
        app.Logger.LogInformation("Request started: {Method} {Path} TraceId={TraceId} SpanId={SpanId}", context.Request.Method, context.Request.Path, traceId, spanId);
        await next();
        app.Logger.LogInformation("Request finished: {StatusCode} {Method} {Path} TraceId={TraceId} SpanId={SpanId}", context.Response.StatusCode, context.Request.Method, context.Request.Path, traceId, spanId);
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();
app.MapControllers();

app.Run();
