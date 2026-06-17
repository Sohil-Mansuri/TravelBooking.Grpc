# Travel Booking gRPC API

A dual-protocol API for flight and hotel search, pricing, and booking built on **.NET 8**. One `.proto` file powers both a native gRPC API and a full REST/JSON API simultaneously — no separate gateway needed.

> **Author:** Sohil Mansuri &nbsp;|&nbsp; **Version:** 1.0 &nbsp;|&nbsp; **Framework:** .NET 8

---

## How It Works

```
Client (Postman / Mobile / Web)
        │
        ├── REST/JSON  ──▶  :5001  ──▶  JSON Transcoding  ──┐
        │                                                     ▼
        └── gRPC binary ──▶  :5000  ──────────────────▶  TravelService.cs
                                                              │
                                                    LoggingInterceptor
                                                    ExceptionInterceptor
```

| Protocol | Port | Format | Best for |
|----------|------|--------|----------|
| gRPC (native) | 5000 | Protobuf binary | Service-to-service |
| REST / JSON | 5001 | JSON (transcoded) | Postman, mobile, web |

---

## Project Structure

```
TravelBooking.GrpcApi/
├── Protos/
│   └── travel.proto              # Service + message definitions
├── Services/
│   └── TravelService.cs          # Business logic implementation
├── Interceptors/
│   ├── LoggingInterceptor.cs     # Logs all calls with timing
│   └── ExceptionInterceptor.cs   # Maps exceptions → HTTP codes
├── google/api/
│   ├── annotations.proto         # Required for HTTP annotations
│   └── http.proto                # HttpRule definitions
├── Program.cs                    # Startup, DI, middleware
└── appsettings.json
```

---

## Quick Start

```bash
# 1. Restore packages
dotnet restore

# 2. Run the API
dotnet run

# 3. Open Swagger UI
open https://localhost:5001/swagger

# 4. Health check
curl https://localhost:5001/health
```

> **SSL Note:** When testing in Postman, disable SSL certificate verification under  
> Settings → General → SSL certificate verification → **OFF**

---

## API Endpoints

### 🛫 Flights

| Method | URL | gRPC Method |
|--------|-----|-------------|
| `POST` | `/v1/flights/search` | `SearchFlights` |
| `GET` | `/v1/flights/{flight_id}/pricing` | `GetFlightPricing` |

### 🏨 Hotels

| Method | URL | gRPC Method |
|--------|-----|-------------|
| `POST` | `/v1/hotels/search` | `SearchHotels` |
| `GET` | `/v1/hotels/{hotel_id}/pricing` | `GetHotelPricing` |

### 📋 Bookings

| Method | URL | gRPC Method |
|--------|-----|-------------|
| `POST` | `/v1/bookings` | `CreateBooking` |
| `GET` | `/v1/bookings/{booking_id}` | `GetBooking` |
| `DELETE` | `/v1/bookings/{booking_id}` | `CancelBooking` |

---

## How URL Mapping Works

The `google.api.http` annotation in `travel.proto` defines how each gRPC method maps to a REST endpoint. Three rules cover everything:

**Rule 1 — HTTP verb:** The keyword (`get:`, `post:`, `delete:`) becomes the HTTP method.

**Rule 2 — Path params:** `{field_name}` in the URL binds to the matching proto message field. Remaining fields become query string params.

**Rule 3 — `body: "*"`:** Used on POST requests — maps the entire JSON body to the proto message.

### Example

```proto
rpc GetFlightPricing (FlightPricingRequest)
    returns (FlightPricingResponse) {
  option (google.api.http) = {
    get: "/v1/flights/{flight_id}/pricing"
  };
}
```

Produces this REST call:

```
GET /v1/flights/FL001/pricing?passengers=2&cabinClass=ECONOMY
```

- `flight_id` → extracted from the URL path
- `passengers`, `cabinClass` → become query string params
- Transcoding auto-converts `snake_case` ↔ `camelCase` and enum integers ↔ readable strings

---

## Sample Requests

### Search Flights

```bash
curl -X POST https://localhost:5001/v1/flights/search \
  -H "Content-Type: application/json" \
  -d '{
    "origin": "BOM",
    "destination": "DEL",
    "departureDate": "2025-12-01",
    "passengers": 2,
    "cabinClass": "ECONOMY"
  }'
```

**Response:**

```json
{
  "flights": [
    {
      "flightId": "FL001",
      "airline": "IndiGo",
      "flightNumber": "6E-456",
      "origin": "BOM",
      "destination": "DEL",
      "departureTime": "2025-12-01T06:00:00Z",
      "arrivalTime": "2025-12-01T08:15:00Z",
      "durationMinutes": 135,
      "stops": 0,
      "basePrice": 4500.00,
      "currency": "INR",
      "cabinClass": "ECONOMY",
      "seatsAvailable": 24
    }
  ],
  "totalResults": 3
}
```

### Get Flight Pricing

```bash
curl "https://localhost:5001/v1/flights/FL001/pricing?passengers=2&cabinClass=ECONOMY"
```

**Response:**

```json
{
  "flightId": "FL001",
  "baseFare": 9000.00,
  "taxes": 1620.00,
  "fees": 350.00,
  "totalPrice": 10970.00,
  "currency": "INR",
  "breakdown": [
    { "label": "Base Fare",   "amount": 9000.00 },
    { "label": "GST (18%)",   "amount": 1620.00 },
    { "label": "Service Fee", "amount": 350.00 }
  ],
  "priceValidUntil": "2025-12-01T10:30:00Z"
}
```

### Search Hotels

```bash
curl -X POST https://localhost:5001/v1/hotels/search \
  -H "Content-Type: application/json" \
  -d '{
    "city": "Delhi",
    "checkIn": "2025-12-01",
    "checkOut": "2025-12-05",
    "guests": 2,
    "rooms": 1,
    "minStars": 4,
    "maxPricePerNight": 15000
  }'
```

### Create Booking

```bash
curl -X POST https://localhost:5001/v1/bookings \
  -H "Content-Type: application/json" \
  -d '{
    "flightId": "FL001",
    "hotelId": "HT001",
    "passengers": 1,
    "cabinClass": "ECONOMY",
    "passengerDetails": [
      {
        "firstName": "Rahul",
        "lastName": "Sharma",
        "dateOfBirth": "1990-05-15",
        "passportNumber": "A1234567",
        "nationality": "IN"
      }
    ],
    "contact": {
      "email": "rahul@example.com",
      "phone": "+91-9999999999"
    },
    "paymentToken": "pay_test_token_xyz"
  }'
```

**Response:**

```json
{
  "bookingId": "BK20251201103045_1234",
  "status": "CONFIRMED",
  "flightId": "FL001",
  "hotelId": "HT001",
  "totalAmount": 15850.00,
  "currency": "INR",
  "createdAt": "2025-12-01T10:30:45Z",
  "pnrCode": "XYZ123"
}
```

### Get Booking

```bash
curl https://localhost:5001/v1/bookings/BK20251201103045_1234
```

### Cancel Booking

```bash
curl -X DELETE "https://localhost:5001/v1/bookings/BK20251201103045_1234?reason=Change%20of%20plans"
```

---

## gRPC Native (grpcurl)

```bash
# Install grpcurl
brew install grpcurl

# Search flights via native gRPC
grpcurl -plaintext -d '{
  "origin": "BOM",
  "destination": "DEL",
  "departure_date": "2025-12-01",
  "passengers": 2
}' localhost:5000 travel.TravelService/SearchFlights
```

---

## Status Code Mapping

| gRPC Status | HTTP Code | Triggered by |
|-------------|-----------|--------------|
| `OK` | 200 | Successful response |
| `INVALID_ARGUMENT` | 400 | Missing `flightId`, no passengers |
| `NOT_FOUND` | 404 | Booking / flight not found |
| `ALREADY_EXISTS` | 409 | Duplicate booking |
| `INTERNAL` | 500 | Unhandled exception |

---

## Interceptors

Every call passes through two interceptors before reaching `TravelService`:

- **`LoggingInterceptor`** — logs the gRPC method name and elapsed milliseconds for every call.
- **`ExceptionInterceptor`** — catches unhandled exceptions and maps them to the correct gRPC status code so the client always receives a well-formed error response.

---

## NuGet Packages

| Package | Version | Purpose |
|---------|---------|---------|
| `Grpc.AspNetCore` | 2.62.0 | Core gRPC server support |
| `Microsoft.AspNetCore.Grpc.JsonTranscoding` | 8.0.0 | REST/JSON endpoint mapping |
| `Microsoft.AspNetCore.Grpc.Swagger` | 0.3.0 | Swagger UI for JSON endpoints |
| `Swashbuckle.AspNetCore` | 6.5.0 | OpenAPI docs generation |

---

## Cabin Class Values

| Value | Description |
|-------|-------------|
| `ECONOMY` | Economy class |
| `PREMIUM_ECONOMY` | Premium economy |
| `BUSINESS` | Business class |
| `FIRST` | First class |

---

## Next Steps

- [ ] Replace mock flight data with real GDS (Amadeus, Sabre)
- [ ] Add hotel inventory API (Expedia, Booking.com)
- [ ] Integrate payment gateway (Razorpay, Stripe)
- [ ] Add JWT authentication via `Authorization` header / gRPC metadata
- [ ] Add database layer (EF Core + PostgreSQL) for booking persistence
- [ ] Add Redis caching for flight search results
- [ ] Add OpenTelemetry distributed tracing
- [ ] Add rate limiting middleware
