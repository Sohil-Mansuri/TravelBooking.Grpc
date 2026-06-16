using Grpc.Core;

namespace TravelBooking.GrpcApi.Services;

public class TravelService : global::TravelBooking.GrpcApi.TravelService.TravelServiceBase
{
    private readonly ILogger<TravelService> _logger;

    public TravelService(ILogger<TravelService> logger)
    {
        _logger = logger;
    }

    // ─────────────────────────────────────────
    // Flight Search
    // ─────────────────────────────────────────
    public override Task<FlightSearchResponse> SearchFlights(
        FlightSearchRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Searching flights from {Origin} to {Destination} on {Date}",
            request.Origin, request.Destination, request.DepartureDate);

        // TODO: Replace with real flight provider integration (Amadeus, Sabre, etc.)
        var flights = GenerateMockFlights(request);

        return Task.FromResult(new FlightSearchResponse
        {
            TotalResults = flights.Count,
            Flights = { flights }
        });
    }

    // ─────────────────────────────────────────
    // Flight Pricing
    // ─────────────────────────────────────────
    public override Task<FlightPricingResponse> GetFlightPricing(
        FlightPricingRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Getting pricing for flight {FlightId}", request.FlightId);

        // TODO: Replace with real dynamic pricing engine
        double baseFare  = 4500.00 * request.Passengers;
        double taxes     = baseFare * 0.18;   // 18% GST
        double fees      = 350.00;
        double total     = baseFare + taxes + fees;

        return Task.FromResult(new FlightPricingResponse
        {
            FlightId       = request.FlightId,
            BaseFare       = baseFare,
            Taxes          = taxes,
            Fees           = fees,
            TotalPrice     = total,
            Currency       = "INR",
            PriceValidUntil = DateTime.UtcNow.AddMinutes(15).ToString("o"),
            Breakdown =
            {
                new PriceBreakdown { Label = "Base Fare",   Amount = baseFare },
                new PriceBreakdown { Label = "GST (18%)",   Amount = taxes },
                new PriceBreakdown { Label = "Service Fee", Amount = fees },
            }
        });
    }

    // ─────────────────────────────────────────
    // Hotel Search
    // ─────────────────────────────────────────
    public override Task<HotelSearchResponse> SearchHotels(
        HotelSearchRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Searching hotels in {City} from {CheckIn} to {CheckOut}",
            request.City, request.CheckIn, request.CheckOut);

        var hotels = GenerateMockHotels(request);

        return Task.FromResult(new HotelSearchResponse
        {
            TotalResults = hotels.Count,
            Hotels = { hotels }
        });
    }

    // ─────────────────────────────────────────
    // Hotel Pricing
    // ─────────────────────────────────────────
    public override Task<HotelPricingResponse> GetHotelPricing(
        HotelPricingRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Getting pricing for hotel {HotelId}", request.HotelId);

        // TODO: Replace with real hotel inventory/pricing system
        var checkIn  = DateTime.Parse(request.CheckIn);
        var checkOut = DateTime.Parse(request.CheckOut);
        int nights   = (checkOut - checkIn).Days;

        double pricePerNight = 3200.00 * request.Rooms;
        double subtotal      = pricePerNight * nights;
        double taxes         = subtotal * 0.12;
        double total         = subtotal + taxes;

        return Task.FromResult(new HotelPricingResponse
        {
            HotelId            = request.HotelId,
            Nights             = nights,
            PricePerNight      = pricePerNight,
            Subtotal           = subtotal,
            Taxes              = taxes,
            TotalPrice         = total,
            Currency           = "INR",
            CancellationPolicy = "Free cancellation until 24 hours before check-in"
        });
    }

    // ─────────────────────────────────────────
    // Create Booking
    // ─────────────────────────────────────────
    public override Task<BookingResponse> CreateBooking(
        CreateBookingRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Creating booking for flight {FlightId}", request.FlightId);

        // TODO: Integrate with payment gateway (Razorpay, Stripe, etc.)
        // TODO: Call airline GDS to issue ticket
        // TODO: Call hotel system to confirm reservation

        if (string.IsNullOrEmpty(request.FlightId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "FlightId is required"));

        if (request.PassengerDetails.Count == 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "At least one passenger is required"));

        var bookingId = $"BK{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
        var pnrCode   = GeneratePnr();

        return Task.FromResult(new BookingResponse
        {
            BookingId   = bookingId,
            Status      = BookingStatus.Confirmed,
            FlightId    = request.FlightId,
            HotelId     = request.HotelId,
            TotalAmount = 15850.00,
            Currency    = "INR",
            CreatedAt   = DateTime.UtcNow.ToString("o"),
            PnrCode     = pnrCode
        });
    }

    // ─────────────────────────────────────────
    // Get Booking
    // ─────────────────────────────────────────
    public override Task<BookingResponse> GetBooking(
        GetBookingRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Fetching booking {BookingId}", request.BookingId);

        // TODO: Fetch from database
        if (string.IsNullOrEmpty(request.BookingId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "BookingId is required"));

        return Task.FromResult(new BookingResponse
        {
            BookingId   = request.BookingId,
            Status      = BookingStatus.Confirmed,
            FlightId    = "FL001",
            TotalAmount = 15850.00,
            Currency    = "INR",
            CreatedAt   = DateTime.UtcNow.AddHours(-2).ToString("o"),
            PnrCode     = "XYZ123"
        });
    }

    // ─────────────────────────────────────────
    // Cancel Booking
    // ─────────────────────────────────────────
    public override Task<CancelBookingResponse> CancelBooking(
        CancelBookingRequest request, ServerCallContext context)
    {
        _logger.LogInformation("Cancelling booking {BookingId}", request.BookingId);

        // TODO: Process cancellation with airline and hotel
        // TODO: Calculate refund based on cancellation policy
        // TODO: Trigger refund via payment gateway

        return Task.FromResult(new CancelBookingResponse
        {
            BookingId    = request.BookingId,
            Success      = true,
            RefundAmount = 13000.00,
            Message      = "Booking cancelled successfully. Refund will be processed in 5-7 business days."
        });
    }

    // ─────────────────────────────────────────
    // Mock data generators (replace with real data)
    // ─────────────────────────────────────────
    private static List<FlightOption> GenerateMockFlights(FlightSearchRequest request)
    {
        return new List<FlightOption>
        {
            new FlightOption
            {
                FlightId       = "FL001",
                Airline        = "IndiGo",
                FlightNumber   = "6E-456",
                Origin         = request.Origin,
                Destination    = request.Destination,
                DepartureTime  = "2025-12-01T06:00:00Z",
                ArrivalTime    = "2025-12-01T08:15:00Z",
                DurationMinutes = 135,
                Stops          = 0,
                BasePrice      = 4500.00,
                Currency       = "INR",
                CabinClass     = request.CabinClass == CabinClass.Unspecified
                                    ? CabinClass.Economy : request.CabinClass,
                SeatsAvailable = 24
            },
            new FlightOption
            {
                FlightId       = "FL002",
                Airline        = "Air India",
                FlightNumber   = "AI-302",
                Origin         = request.Origin,
                Destination    = request.Destination,
                DepartureTime  = "2025-12-01T10:30:00Z",
                ArrivalTime    = "2025-12-01T12:55:00Z",
                DurationMinutes = 145,
                Stops          = 0,
                BasePrice      = 5200.00,
                Currency       = "INR",
                CabinClass     = CabinClass.Economy,
                SeatsAvailable = 8
            },
            new FlightOption
            {
                FlightId       = "FL003",
                Airline        = "Vistara",
                FlightNumber   = "UK-820",
                Origin         = request.Origin,
                Destination    = request.Destination,
                DepartureTime  = "2025-12-01T14:00:00Z",
                ArrivalTime    = "2025-12-01T16:20:00Z",
                DurationMinutes = 140,
                Stops          = 0,
                BasePrice      = 8900.00,
                Currency       = "INR",
                CabinClass     = CabinClass.Business,
                SeatsAvailable = 6
            }
        };
    }

    private static List<HotelOption> GenerateMockHotels(HotelSearchRequest request)
    {
        return new List<HotelOption>
        {
            new HotelOption
            {
                HotelId       = "HT001",
                Name          = "The Leela Palace",
                City          = request.City,
                Address       = "Diplomatic Enclave, Chanakyapuri",
                Stars         = 5,
                Rating        = 4.8,
                PricePerNight = 12000.00,
                Currency      = "INR",
                Amenities     = { "Pool", "Spa", "Free WiFi", "Gym", "Restaurant", "Valet Parking" },
                FreeCancellation = true
            },
            new HotelOption
            {
                HotelId       = "HT002",
                Name          = "Radisson Blu",
                City          = request.City,
                Address       = "National Highway 8, Mahipalpur",
                Stars         = 4,
                Rating        = 4.2,
                PricePerNight = 5500.00,
                Currency      = "INR",
                Amenities     = { "Pool", "Free WiFi", "Gym", "Restaurant", "Airport Shuttle" },
                FreeCancellation = true
            },
            new HotelOption
            {
                HotelId       = "HT003",
                Name          = "Ibis Hotel",
                City          = request.City,
                Address       = "Sector 53, Aerocity",
                Stars         = 3,
                Rating        = 3.9,
                PricePerNight = 2800.00,
                Currency      = "INR",
                Amenities     = { "Free WiFi", "Restaurant", "24hr Reception" },
                FreeCancellation = false
            }
        };
    }

    private static string GeneratePnr()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Range(0, 6)
            .Select(_ => chars[Random.Shared.Next(chars.Length)])
            .ToArray());
    }
}
