using RentCarServer.Domain.Abstractions;
using RentCarServer.Domain.Customers.ValueObjects;
using RentCarServer.Domain.Users.ValueObjects;
using Email = RentCarServer.Domain.Customers.ValueObjects.Email;
using FirstName = RentCarServer.Domain.Customers.ValueObjects.FirstName;
using LastName = RentCarServer.Domain.Customers.ValueObjects.LastName;

namespace RentCarServer.Domain.Customers;

public sealed class Customer : Entity
{
    private Customer() { }

    public Customer(
        FirstName firstName,
        LastName lastName,
        IdentityNumber identityNumber,
        DateOfBirth dateOfBirth,
        Email email,
        PhoneNumber phoneNumber,
        Address address,
        DriverLicenceIssueDate driverLicenceIssueDate,
        Password password,
        bool isActive)
    {
        SetFirstName(firstName);
        SetLastName(lastName);
        SetIdentityNumber(identityNumber);
        SetDateOfBirth(dateOfBirth);
        SetEmail(email);
        SetPhoneNumber(phoneNumber);
        SetAddress(address);
        SetDriverLicenceIssueDate(driverLicenceIssueDate);
        SetStatus(isActive);
        SetPassword(password);
    }

    #region Behaviors
    public FirstName FirstName { get; private set; } = default!;
    public LastName LastName { get; private set; } = default!;
    public IdentityNumber IdentityNumber { get; private set; } = default!;
    public DateOfBirth DateOfBirth { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public PhoneNumber PhoneNumber { get; private set; } = default!;
    public DriverLicenceIssueDate DriverLicenceIssueDate { get; private set; } = default!;
    public Address Address { get; private set; } = default!;
    public Password Password { get; private set; } = default!;
    public void SetFirstName(FirstName firstName) => FirstName = firstName;
    public void SetLastName(LastName lastName) => LastName = lastName;
    public void SetIdentityNumber(IdentityNumber identityNumber) => IdentityNumber = identityNumber;
    public void SetDateOfBirth(DateOfBirth dateOfBirth) => DateOfBirth = dateOfBirth;
    public void SetEmail(Email email) => Email = email;
    public void SetPhoneNumber(PhoneNumber phoneNumber) => PhoneNumber = phoneNumber;
    public void SetDriverLicenceIssueDate(DriverLicenceIssueDate driverLicenceIssueDate) => DriverLicenceIssueDate = driverLicenceIssueDate;
    public void SetAddress(Address address) => Address = address;
    public void SetPassword(Password password) => Password = password;

    public bool VerifyPasswordHash(string password)
    {
        using var hmac = new System.Security.Cryptography.HMACSHA512(Password.PasswordSalt);
        var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        return computedHash.SequenceEqual(Password.PasswordHash);
    }
    #endregion


}