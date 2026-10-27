# wt-lab6-6minimalapi-kleshchenok

Лабораторная работа № 6 «Web-технологии» — **REST API на ASP.NET Core Minimal API** и публикация на Render.
Предметная область: **прокат автомобилей** (основная сущность `Car`, как в WT-3, WT-4, WT-5).
Хранение данных — в памяти приложения 

- GitHub: https://github.com/alishaartemevaa05-netizen/wt-lab6-6minimalapi-kleshchenok
- Render: https://wt-lab6-6minimalapi-kleshchenok-1.onrender.com 

## Запуск

```bash
dotnet run                      # http://localhost:10000
docker build -t wt-lab6-6minimalapi .
docker run -p 10000:10000 wt-lab6-6minimalapi
```

Главная страница: `/` (автопарк, клиенты, аренды) · Swagger UI: `/swagger.html` (для разработчика, на сайте не показывается) · OpenAPI: `/openapi/v1.json`

**Фото автомобилей.** Страница подгружает фото модели из Википедии; для своей фотографии заполните поле `imageUrl` автомобиля. Если фото недоступно — показывается иллюстрация категории.

## Таблица API

| Метод  | URL                              | Назначение                       | Успех            | Ошибки        |
|--------|----------------------------------|----------------------------------|------------------|---------------|
| GET    | `/`                              | проверить работу (текст / HTML)  | 200              | —             |
| GET    | `/api/info`                      | информация о приложении          | 200              | —             |
| GET    | `/api/cars`                      | получить список                  | 200              | —             |
| GET    | `/api/cars?search=ki`            | поиск по марке/модели            | 200              | —             |
| GET    | `/api/cars?category=Suv&available=true` | фильтры                   | 200              | —             |
| GET    | `/api/cars/{id}`                 | получить объект                  | 200              | 404           |
| POST   | `/api/cars`                      | создать объект                   | 201 + `Location` | 400           |
| PUT    | `/api/cars/{id}`                 | изменить объект                  | 200              | 400, 404      |
| DELETE | `/api/cars/{id}`                 | удалить объект                   | 204              | 404, 409      |
| GET    | `/api/customers`, `/api/customers/{id}` | список клиентов / клиент  | 200              | 404           |
| POST   | `/api/customers`                 | создать клиента                  | 201              | 400           |
| PUT    | `/api/customers/{id}`            | изменить клиента                 | 200              | 400, 404      |
| DELETE | `/api/customers/{id}`            | удалить клиента                  | 204              | 404, 409      |
| GET    | `/api/rentals`, `/api/rentals/{id}` | список аренд / аренда         | 200              | 404           |
| DELETE | `/api/rentals/{id}`              | удалить завершённую аренду       | 204              | 404, 409      |
| POST   | `/api/rentals`                   | оформить аренду                  | 201              | 400, 409      |
| POST   | `/api/rentals/{id}/complete`     | завершить аренду                 | 204              | 404           |

## Примеры

**POST /api/cars** — запрос (Id не передаём):

```json
{
  "brand": "Skoda",
  "model": "Octavia",
  "year": 2024,
  "category": "Comfort",
  "transmission": "Automatic",
  "fuelType": "Petrol",
  "seats": 5,
  "dailyPrice": 95,
  "isAvailable": true,
  "description": "Лифтбек для длинных поездок",
  "imageUrl": null
}
```

Ответ `201 Created`, заголовок `Location: /api/cars/7`:

```json
{
  "id": 7, "brand": "Skoda", "model": "Octavia", "year": 2024,
  "category": "Comfort", "transmission": "Automatic", "fuelType": "Petrol",
  "seats": 5, "dailyPrice": 95, "isAvailable": true,
  "description": "Лифтбек для длинных поездок", "fullName": "Skoda Octavia"
}
```

**Ошибка валидации** — `POST /api/cars` с `"year": 1850` → `400 Bad Request`:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": { "Year": ["Год от 1990 до 2030"] }
}
```

**GET /api/cars/999** → `404 Not Found` (пустое тело).

**POST /api/rentals**:

```json
{ "carId": 3, "customerId": 1, "startDate": "2026-10-20", "endDate": "2026-10-23" }
```

→ `201 Created`, стоимость посчитает сервер: 3 суток × 180 = 540.

## curl

```bash
curl https://<имя-сервиса>.onrender.com/api/cars
curl -i -X POST https://<имя-сервиса>.onrender.com/api/cars -H "Content-Type: application/json" \
     -d '{"brand":"Skoda","model":"Octavia","year":2024,"category":"Comfort","transmission":"Automatic","fuelType":"Petrol","seats":5,"dailyPrice":95}'
curl -X PUT  https://<имя-сервиса>.onrender.com/api/cars/1 -H "Content-Type: application/json" -d '{...}'
curl -i -X DELETE https://<имя-сервиса>.onrender.com/api/cars/1
```

## Структура

```
Models/    Car, Customer, Rental, Enums, DTO (RentalListItem, RentalCreateRequest)
Data/      CarStore, CustomerStore, RentalStore  (коллекции в памяти)
Helpers/   ModelValidator (проверка DataAnnotations → 400)
wwwroot/   index.html (главная), swagger.html, img/, video/
Program.cs endpoints, OpenAPI, порт Render
Dockerfile многоэтапная сборка
```
