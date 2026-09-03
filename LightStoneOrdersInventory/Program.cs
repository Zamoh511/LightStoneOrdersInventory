var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

//Services 
builder.Services.AddScoped<LightStoneOrdersInventory.Services.Interfaces.IProductService, LightStoneOrdersInventory.Services.ProductsService>();
builder.Services.AddScoped<LightStoneOrdersInventory.Services.Interfaces.IOrdersService, LightStoneOrdersInventory.Services.OrdersService>();

//Repositories
builder.Services.AddScoped<LightStoneOrdersInventory.Repositories.IProductRepository, LightStoneOrdersInventory.Repositories.ProductRepository>();
builder.Services.AddScoped<LightStoneOrdersInventory.Repositories.IOrdersRepository, LightStoneOrdersInventory.Repositories.OrdersRepository>();
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

app.MapControllers();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.Run();
