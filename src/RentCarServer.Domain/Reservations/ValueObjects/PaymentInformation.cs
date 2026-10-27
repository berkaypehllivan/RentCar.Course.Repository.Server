namespace RentCarServer.Domain.Reservations.ValueObjects;

public sealed record PaymentInformation(string CardNumber, string CardOwner);
