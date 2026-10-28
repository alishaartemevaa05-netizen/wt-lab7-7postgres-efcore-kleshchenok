using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using wt_lab7_7postgres_efcore_kleshchenok.Data;
using wt_lab7_7postgres_efcore_kleshchenok.Models;
using wt_lab7_7postgres_efcore_kleshchenok.Helpers;

CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

// ───────────── Блок 1. Подключение PostgreSQL через Entity Framework Core ─────────────
// Строка подключения: DATABASE_URL (Render) или ConnectionStrings:DefaultConnection
var connectionString = ConnectionStringResolver.Resolve(builder.Configuration);

// AddDbContext регистрирует AppDbContext в контейнере зависимостей
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

var app = builder.Build();

// ───────────── Блок 2. Автоматическое применение миграций при запуске ─────────────

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    app.Logger.LogInformation("Проверка и применение миграций PostgreSQL...");

    // На Render база может подняться чуть позже приложения — делаем до 5 попыток.
    for (var attempt = 1; ; attempt++)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch (Exception ex) when (attempt < 5)
        {
            app.Logger.LogWarning("Попытка {Attempt} не удалась ({Message}). Повтор через 3 с...", attempt, ex.Message);
            Thread.Sleep(3000);
        }
    }
    app.Logger.LogInformation("Миграции применены. База данных готова.");
}

app.UseStaticFiles();
app.MapOpenApi();

// ───────────────────────── Базовые endpoints ─────────────────────────

app.MapGet("/", (HttpRequest request, IWebHostEnvironment env) =>
{
    var wantsHtml = request.Headers.Accept.ToString().Contains("text/html", StringComparison.OrdinalIgnoreCase);
    return wantsHtml
        ? Results.File(Path.Combine(env.WebRootPath, "index.html"), "text/html; charset=utf-8")
        : Results.Text("Minimal API работает!", "text/plain; charset=utf-8");
}).ExcludeFromDescription();

app.MapGet("/ping", () => "Minimal API работает!")
   .WithSummary("Проверка работы приложения (текст)");

var api = app.MapGroup("/api");

api.MapGet("/info", () => new
{
    Application = "Web Technologies",
    Technology = "ASP.NET Core Minimal API + Entity Framework Core",
    Database = "PostgreSQL",
    Version = "2.0",
    Domain = "Прокат автомобилей",
    Author = "А.А. Клещёнок"
}).WithTags("Info").WithSummary("Информация о приложении");

// Статистика считается запросами к базе (COUNT в SQL), а не подсчётом объектов в памяти.
api.MapGet("/stats", async (AppDbContext db) => new
{
    TotalCars = await db.Cars.CountAsync(),
    AvailableCars = await db.Cars.CountAsync(c => c.IsAvailable),
    TotalCustomers = await db.Customers.CountAsync(),
    ActiveRentals = await db.Rentals.CountAsync(r => r.Status == RentalStatus.Active)
}).WithTags("Info").WithSummary("Сводная статистика");

// ───────────────────────── Автомобили: CRUD через EF Core ─────────────────────────

var cars = api.MapGroup("/cars").WithTags("Cars");

// GET /api/cars — список. 
cars.MapGet("", async (string? search, CarCategory? category, bool? available, AppDbContext db) =>
{
    var query = db.Cars.AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
    {
        var s = search.ToLower();
        query = query.Where(c => c.Brand.ToLower().Contains(s) || c.Model.ToLower().Contains(s));
    }
    if (category.HasValue) query = query.Where(c => c.Category == category.Value);
    if (available.HasValue) query = query.Where(c => c.IsAvailable == available.Value);

    return Results.Ok(await query.OrderBy(c => c.Id).ToListAsync());
}).WithSummary("Получить список автомобилей (с фильтрами)");

// ---- LINQ-запросы  ----

// 1) Поиск по названию (раздел 15): GET /api/cars/search?search=ki
cars.MapGet("/search", async (string? search, AppDbContext db) =>
{
    var query = db.Cars.AsQueryable();
    if (!string.IsNullOrWhiteSpace(search))
    {
        var s = search.ToLower();
        query = query.Where(c => c.Brand.ToLower().Contains(s) || c.Model.ToLower().Contains(s));
    }
    return Results.Ok(await query.ToListAsync());
}).WithSummary("LINQ: поиск по марке и модели");

// 2) Фильтрация: автомобили дороже заданной цены.  SQL: WHERE "DailyPrice" > @min
cars.MapGet("/expensive", async (decimal? minPrice, AppDbContext db) =>
{
    var min = minPrice ?? 100;
    var list = await db.Cars.Where(c => c.DailyPrice > min).ToListAsync();
    return Results.Ok(list);
}).WithSummary("LINQ: фильтрация — дороже minPrice (по умолчанию 100)");

// 3) Сортировка: от дешёвых к дорогим.  SQL: ORDER BY "DailyPrice"
cars.MapGet("/by-price", async (AppDbContext db) =>
    Results.Ok(await db.Cars.OrderBy(c => c.DailyPrice).ToListAsync()))
    .WithSummary("LINQ: сортировка по цене");

// 4) Выборка с условием и сортировкой: только свободные, по марке.  SQL: WHERE "IsAvailable" ORDER BY "Brand"
cars.MapGet("/available", async (AppDbContext db) =>
    Results.Ok(await db.Cars.Where(c => c.IsAvailable).OrderBy(c => c.Brand).ThenBy(c => c.Model).ToListAsync()))
    .WithSummary("LINQ: только свободные автомобили");

// 5) Собственный запрос: группировка по категориям.  SQL: GROUP BY "Category" с COUNT и AVG
cars.MapGet("/report", async (AppDbContext db) =>
{
    var report = await db.Cars
        .GroupBy(c => c.Category)
        .Select(g => new
        {
            Category = g.Key,
            Count = g.Count(),
            AveragePrice = g.Average(c => c.DailyPrice)
        })
        .OrderByDescending(x => x.Count)
        .ToListAsync();
    return Results.Ok(report);
}).WithSummary("LINQ: количество и средняя цена по категориям");

// GET /api/cars/{id}
cars.MapGet("/{id:int}", async (int id, AppDbContext db) =>
{
    var car = await db.Cars.FirstOrDefaultAsync(c => c.Id == id);
    return car is null ? Results.NotFound() : Results.Ok(car);
}).WithSummary("Получить автомобиль по Id");

// POST /api/cars — INSERT
cars.MapPost("", async (Car car, AppDbContext db) =>
{
    var errors = ModelValidator.Validate(car);
    if (errors is not null) return Results.ValidationProblem(errors);

    car.Id = 0;                       
    db.Cars.Add(car);
    await db.SaveChangesAsync();     
    return Results.Created($"/api/cars/{car.Id}", car);
}).WithSummary("Создать автомобиль");

// PUT /api/cars/{id} — UPDATE
cars.MapPut("/{id:int}", async (int id, Car updated, AppDbContext db) =>
{
    var errors = ModelValidator.Validate(updated);
    if (errors is not null) return Results.ValidationProblem(errors);

    var car = await db.Cars.FirstOrDefaultAsync(c => c.Id == id);   
    if (car is null) return Results.NotFound();

    car.Brand = updated.Brand;
    car.Model = updated.Model;
    car.Year = updated.Year;
    car.Category = updated.Category;
    car.Transmission = updated.Transmission;
    car.FuelType = updated.FuelType;
    car.Seats = updated.Seats;
    car.DailyPrice = updated.DailyPrice;
    car.IsAvailable = updated.IsAvailable;
    car.Description = updated.Description;
    car.ImageUrl = updated.ImageUrl;

    await db.SaveChangesAsync();      
    return Results.Ok(car);
}).WithSummary("Изменить автомобиль");

// DELETE /api/cars/{id} — DELETE
cars.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
{
    var car = await db.Cars.FirstOrDefaultAsync(c => c.Id == id);
    if (car is null) return Results.NotFound();

     if (await db.Rentals.AnyAsync(r => r.CarId == id))
        return Results.Conflict(new { error = "Нельзя удалить автомобиль: по нему есть аренды." });

    db.Cars.Remove(car);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithSummary("Удалить автомобиль");

// ───────────────────────── Клиенты ─────────────────────────

var customers = api.MapGroup("/customers").WithTags("Customers");

customers.MapGet("", async (AppDbContext db) =>
    Results.Ok(await db.Customers.OrderBy(c => c.Id).ToListAsync()))
    .WithSummary("Список клиентов");

customers.MapGet("/{id:int}", async (int id, AppDbContext db) =>
{
    var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == id);
    return customer is null ? Results.NotFound() : Results.Ok(customer);
}).WithSummary("Клиент по Id");

customers.MapPost("", async (Customer customer, AppDbContext db) =>
{
    var errors = ModelValidator.Validate(customer);
    if (errors is not null) return Results.ValidationProblem(errors);

    customer.Id = 0;
    db.Customers.Add(customer);
    await db.SaveChangesAsync();
    return Results.Created($"/api/customers/{customer.Id}", customer);
}).WithSummary("Создать клиента");

customers.MapPut("/{id:int}", async (int id, Customer updated, AppDbContext db) =>
{
    var errors = ModelValidator.Validate(updated);
    if (errors is not null) return Results.ValidationProblem(errors);

    var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == id);
    if (customer is null) return Results.NotFound();

    customer.FullName = updated.FullName;
    customer.Phone = updated.Phone;
    customer.Email = updated.Email;
    customer.DriverLicense = updated.DriverLicense;

    await db.SaveChangesAsync();
    return Results.Ok(customer);
}).WithSummary("Изменить клиента");

customers.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
{
    var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == id);
    if (customer is null) return Results.NotFound();

    if (await db.Rentals.AnyAsync(r => r.CustomerId == id))
        return Results.Conflict(new { error = "Нельзя удалить клиента: у него есть аренды." });

    db.Customers.Remove(customer);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithSummary("Удалить клиента");

// ───────────────────────── Аренды ─────────────────────────

var rentals = api.MapGroup("/rentals").WithTags("Rentals");
rentals.MapGet("", async (AppDbContext db) =>
{
    var list = await (
        from r in db.Rentals
        join c in db.Cars on r.CarId equals c.Id
        join u in db.Customers on r.CustomerId equals u.Id
        orderby r.Id
        select new RentalListItem
        {
            Id = r.Id,
            CarId = c.Id,
            CarName = c.Brand + " " + c.Model,
            CustomerId = u.Id,
            CustomerName = u.FullName,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            TotalPrice = r.TotalPrice,
            Status = r.Status
        }).ToListAsync();
    return Results.Ok(list);
}).WithSummary("Список аренд");

rentals.MapGet("/{id:int}", async (int id, AppDbContext db) =>
{
    var rental = await db.Rentals.FirstOrDefaultAsync(r => r.Id == id);
    return rental is null ? Results.NotFound() : Results.Ok(rental);
}).WithSummary("Аренда по Id");

rentals.MapPost("", async (RentalCreateRequest req, AppDbContext db) =>
{
    var errors = ModelValidator.Validate(req) ?? new Dictionary<string, string[]>();

    if (req.EndDate.Date <= req.StartDate.Date)
        errors["endDate"] = new[] { "Дата окончания должна быть позже даты начала" };

    var car = await db.Cars.FirstOrDefaultAsync(c => c.Id == req.CarId);
    var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == req.CustomerId);
    if (car is null) errors["carId"] = new[] { "Автомобиль не найден" };
    if (customer is null) errors["customerId"] = new[] { "Клиент не найден" };

    if (errors.Count > 0) return Results.ValidationProblem(errors);
    if (!car!.IsAvailable) return Results.Conflict(new { error = "Автомобиль сейчас недоступен." });

    var days = (req.EndDate.Date - req.StartDate.Date).Days;
    var rental = new Rental
    {
        CarId = car.Id,
        CustomerId = customer!.Id,
        StartDate = req.StartDate.Date,
        EndDate = req.EndDate.Date,
        TotalPrice = days * car.DailyPrice,
        Status = RentalStatus.Active
    };
    db.Rentals.Add(rental);
    car.IsAvailable = false;
    await db.SaveChangesAsync();   // INSERT аренды и UPDATE автомобиля — в одной транзакции
    return Results.Created($"/api/rentals/{rental.Id}", rental);
}).WithSummary("Оформить аренду");

rentals.MapPost("/{id:int}/complete", async (int id, AppDbContext db) =>
{
    var rental = await db.Rentals.FirstOrDefaultAsync(r => r.Id == id);
    if (rental is null) return Results.NotFound();

    rental.Status = RentalStatus.Completed;
    var car = await db.Cars.FirstOrDefaultAsync(c => c.Id == rental.CarId);
    if (car is not null) car.IsAvailable = true;

    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithSummary("Завершить аренду");

rentals.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
{
    var rental = await db.Rentals.FirstOrDefaultAsync(r => r.Id == id);
    if (rental is null) return Results.NotFound();
    if (rental.Status == RentalStatus.Active)
        return Results.Conflict(new { error = "Сначала завершите аренду." });

    db.Rentals.Remove(rental);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithSummary("Удалить завершённую аренду");

// ───────────────────────── Порт для Render ─────────────────────────
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
app.Run($"http://0.0.0.0:{port}");
