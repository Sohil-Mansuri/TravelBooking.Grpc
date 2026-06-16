# Travel Booking gRPC API (.NET 8)

A gRPC service with automatic REST/JSON support via **JSON Transcoding**.  
One `.proto` file gives you both a native gRPC API and a full REST/JSON API — no gateway needed.

---

## Project Structure

```
TravelBooking.GrpcApi/
├── Protos/
│   └── travel.proto              # Service and message definitions
├── Services/
│   └── TravelService.cs          # Business logic implementation
├── Interceptors/
│   ├── LoggingInterceptor.cs     # Logs all calls with timing
│   └── ExceptionInterceptor.cs   # Maps exceptions to gRPC status codes
├── Program.cs                    # App startup, DI, middleware
└── appsettings.json
```

---

## Quick Start

```bash
# 1. Restore packages
dotnet restore

# 2. Run
dotnet run

# 3. Open Swagger UI
open https://localhost:5001/swagger
```

---

## API Endpoints

### gRPC (port 5000, HTTP/2)
Use a gRPC client like grpcurl or Postman.

### REST/JSON (port 5001, HTTPS)
All endpoints return JSON automatically via transcoding.

---

## REST Examples (curl)

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

---

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
  "priceValidUntil": "2025-11-25T10:30:00Z"
}
```

---

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
    "minStars": 4
  }'
```

---

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
    "paymentToken": "pay_token_xyz"
  }'
```

**Response:**
```json
{
  "bookingId": "BK20251125103045_1234",
  "status": "CONFIRMED",
  "flightId": "FL001",
  "hotelId": "HT001",
  "totalAmount": 15850.00,
  "currency": "INR",
  "createdAt": "2025-11-25T10:30:45Z",
  "pnrCode": "XYZ123"
}
```

---

### Get Booking
```bash
curl https://localhost:5001/v1/bookings/BK20251125103045_1234
```

### Cancel Booking
```bash
curl -X DELETE "https://localhost:5001/v1/bookings/BK20251125103045_1234?reason=Change%20of%20plans"
```

---

## gRPC Examples (grpcurl)

```bash
# Install: brew install grpcurl

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

| gRPC Status       | HTTP Status |
|-------------------|-------------|
| OK                | 200         |
| INVALID_ARGUMENT  | 400         |
| NOT_FOUND         | 404         |
| ALREADY_EXISTS    | 409         |
| INTERNAL          | 500         |

---

## Next Steps

- [ ] Replace mock data with real flight GDS (Amadeus, Sabre)
- [ ] Add hotel inventory API (Expedia, Booking.com)
- [ ] Integrate payment gateway (Razorpay, Stripe)
- [ ] Add authentication (JWT via gRPC metadata / Authorization header)
- [ ] Add database (EF Core + PostgreSQL) for booking persistence
- [ ] Add caching (Redis) for flight search results
- [ ] Add OpenTelemetry tracing
