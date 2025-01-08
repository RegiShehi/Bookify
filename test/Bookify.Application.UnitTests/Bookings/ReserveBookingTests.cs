using Bookify.Application.Abstractions.Clock;
using Bookify.Application.Bookings.ReserveBooking;
using Bookify.Application.Exceptions;
using Bookify.Application.UnitTests.Apartments;
using Bookify.Application.UnitTests.Users;
using Bookify.Domain.Abstractions;
using Bookify.Domain.Apartments;
using Bookify.Domain.Bookings;
using Bookify.Domain.Users;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;

namespace Bookify.Application.UnitTests.Bookings;

public class ReserveBookingTests
{
    private static readonly DateTime UtcNow = DateTime.UtcNow;

    private static readonly ReserveBookingCommand Command = new(Guid.NewGuid(), Guid.NewGuid(),
        new DateOnly(2024, 1, 1), new DateOnly(2024, 1, 10));

    private readonly ReserveBookingCommandHandler _handler;

    private readonly IUserRepository _userRepositoryMock;
    private readonly IApartmentRepository _apartmentRepositoryMock;
    private readonly IBookingRepository _bookingRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    public ReserveBookingTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _apartmentRepositoryMock = Substitute.For<IApartmentRepository>();
        _bookingRepositoryMock = Substitute.For<IBookingRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();

        var dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
        dateTimeProviderMock.UtcNow.Returns(UtcNow);

        _handler = new ReserveBookingCommandHandler(
            _userRepositoryMock,
            _apartmentRepositoryMock,
            _bookingRepositoryMock,
            _unitOfWorkMock,
            new PricingService(),
            dateTimeProviderMock);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUserIsNull()
    {
        // arrange
        _userRepositoryMock
            .GetByIdAsync(Command.UserId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // assert
        result.Error.Should().Be(UserErrors.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenApartmentIsNull()
    {
        // arrange
        var user = UserData.Create();

        _userRepositoryMock
            .GetByIdAsync(Command.UserId, Arg.Any<CancellationToken>())
            .Returns(user);

        _apartmentRepositoryMock
            .GetByIdAsync(Command.ApartmentId, Arg.Any<CancellationToken>())
            .ReturnsNull();

        // act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // assert
        result.Error.Should().Be(ApartmentErrors.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenApartmentIsBooked()
    {
        // arrange
        var user = UserData.Create();
        var apartment = ApartmentData.Create();
        var duration = DateRange.Create(Command.StartDate, Command.EndDate);

        _userRepositoryMock
            .GetByIdAsync(Command.UserId, Arg.Any<CancellationToken>())
            .Returns(user);

        _apartmentRepositoryMock
            .GetByIdAsync(Command.ApartmentId, Arg.Any<CancellationToken>())
            .Returns(apartment);

        _bookingRepositoryMock
            .IsOverlappingAsync(apartment, duration, Arg.Any<CancellationToken>())
            .Returns(true);

        // act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // assert
        result.Error.Should().Be(BookingErrors.Overlap);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenUnitOfWorkThrowsError()
    {
        // arrange
        var user = UserData.Create();
        var apartment = ApartmentData.Create();
        var duration = DateRange.Create(Command.StartDate, Command.EndDate);

        _userRepositoryMock
            .GetByIdAsync(Command.UserId, Arg.Any<CancellationToken>())
            .Returns(user);

        _apartmentRepositoryMock
            .GetByIdAsync(Command.ApartmentId, Arg.Any<CancellationToken>())
            .Returns(apartment);

        _bookingRepositoryMock
            .IsOverlappingAsync(apartment, duration, Arg.Any<CancellationToken>())
            .Returns(false);

        _unitOfWorkMock
            .SaveChangesAsync()
            .ThrowsAsync(new ConcurrencyException("Concurrency", new Exception()));

        // act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // assert
        result.Error.Should().Be(BookingErrors.Overlap);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenBookingIsReserved()
    {
        // arrange
        var user = UserData.Create();
        var apartment = ApartmentData.Create();
        var duration = DateRange.Create(Command.StartDate, Command.EndDate);

        _userRepositoryMock
            .GetByIdAsync(Command.UserId, Arg.Any<CancellationToken>())
            .Returns(user);

        _apartmentRepositoryMock
            .GetByIdAsync(Command.ApartmentId, Arg.Any<CancellationToken>())
            .Returns(apartment);

        _bookingRepositoryMock
            .IsOverlappingAsync(apartment, duration, Arg.Any<CancellationToken>())
            .Returns(false);

        // act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldCallRepository_WhenBookingIsReserved()
    {
        // arrange
        var user = UserData.Create();
        var apartment = ApartmentData.Create();
        var duration = DateRange.Create(Command.StartDate, Command.EndDate);

        _userRepositoryMock
            .GetByIdAsync(Command.UserId, Arg.Any<CancellationToken>())
            .Returns(user);

        _apartmentRepositoryMock
            .GetByIdAsync(Command.ApartmentId, Arg.Any<CancellationToken>())
            .Returns(apartment);
        _bookingRepositoryMock
            .IsOverlappingAsync(apartment, duration, Arg.Any<CancellationToken>())
            .Returns(false);

        // act
        var result = await _handler.Handle(Command, CancellationToken.None);

        // assert
        _bookingRepositoryMock.Received(1).Add(Arg.Is<Booking>(b => b.Id == result.Value));
    }
}