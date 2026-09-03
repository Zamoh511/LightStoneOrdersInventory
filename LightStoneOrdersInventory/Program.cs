using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
// Add Razor Pages to host the Sales Report UI
builder.Services.AddRazorPages();

//Services 
builder.Services.AddScoped<LightStoneOrdersInventory.Services.Interfaces.IProductService, LightStoneOrdersInventory.Services.ProductsService>();
builder.Services.AddScoped<LightStoneOrdersInventory.Services.Interfaces.IOrdersService, LightStoneOrdersInventory.Services.OrdersService>();

//Repositories
builder.Services.AddScoped<LightStoneOrdersInventory.Repositories.IProductRepository, LightStoneOrdersInventory.Repositories.ProductRepository>();
builder.Services.AddScoped<LightStoneOrdersInventory.Repositories.IOrdersRepository, LightStoneOrdersInventory.Repositories.OrdersRepository>();
builder.Services.AddScoped<LightStoneOrdersInventory.Repositories.IReportsRepository, LightStoneOrdersInventory.Repositories.ReportsRepository>();
builder.Services.AddScoped<LightStoneOrdersInventory.Repositories.IReportsRepository, LightStoneOrdersInventory.Repositories.ReportsRepository>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
// Diagnostic endpoint to inspect mapped endpoints and whether Razor Pages appear to be active
app.MapGet("/__diagnostics", (EndpointDataSource endpointDataSource) =>
{
    var endpoints = endpointDataSource.Endpoints.Select(e =>
    {
        var route = e is RouteEndpoint re && re.RoutePattern != null ? re.RoutePattern.RawText : null;
        return new { DisplayName = e.DisplayName, Route = route };
    }).ToList();

    // Heuristic: consider Razor Pages active if any endpoint DisplayName or Route contains "Pages" or "Page"
    var razorActive = endpoints.Any(ep =>
        (!string.IsNullOrEmpty(ep.DisplayName) && (ep.DisplayName.Contains("Page") || ep.DisplayName.Contains("Pages") || ep.DisplayName.Contains("Razor")))
        || (!string.IsNullOrEmpty(ep.Route) && ep.Route.Contains("Pages", StringComparison.OrdinalIgnoreCase)));

    return Results.Json(new { RazorPagesActive = razorActive, Endpoints = endpoints });
});
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.Run();
