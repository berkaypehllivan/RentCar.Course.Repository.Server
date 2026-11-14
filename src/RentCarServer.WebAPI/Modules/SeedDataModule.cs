using GenericRepository;
using Microsoft.EntityFrameworkCore;
using RentCarServer.Domain.Abstractions;
using RentCarServer.Domain.Categories;
using RentCarServer.Domain.Customers;
using RentCarServer.Domain.Customers.ValueObjects;
using RentCarServer.Domain.Extras;
using RentCarServer.Domain.ProtectionPackages;
using RentCarServer.Domain.Reservations;
using RentCarServer.Domain.Reservations.Forms;
using RentCarServer.Domain.Reservations.Forms.ValueObjects;
using RentCarServer.Domain.Reservations.ValueObjects;
using RentCarServer.Domain.Shared;
using RentCarServer.Domain.Vehicles;
using RentCarServer.Domain.Vehicles.ValueObjects;
using TS.Result;
// Shared.Address ile Customers.ValueObjects.Address çakıştığı için alias kullanıyoruz.
using CustomerAddress = RentCarServer.Domain.Customers.ValueObjects.Address;

namespace RentCarServer.WebAPI.Modules;

public static class SeedDataModule
{
    public static void MapSeedData(this IEndpointRouteBuilder builder)
    {
        var app = builder.MapGroup("seed-data").RequireAuthorization().WithTags("SeedData");

        //customers
        app.MapGet("customers",
            async (ICustomerRepository repository, IUnitOfWork unitOfWork, CancellationToken cancellationToken) =>
            {
                var existingIdentities = await repository.GetAll().Select(c => c.IdentityNumber.Value).ToListAsync(cancellationToken);

                var customers = CustomerSeeds
                    .Select((c, i) => new Customer(
                        new FirstName(c.FirstName),
                        new LastName(c.LastName),
                        new IdentityNumber(GenerateTcKn(i)),
                        new DateOfBirth(c.BirthDate),
                        new Email(c.Email),
                        new PhoneNumber(c.Phone),
                        new CustomerAddress(c.Address),
                        new DriverLicenceIssueDate(c.LicenseDate),
                        true))
                    .Where(c => !existingIdentities.Contains(c.IdentityNumber.Value))
                    .ToList();

                repository.AddRange(customers);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Results.Ok(Result<string>.Succeed("Müşteri seed data başarıyla tamamlandı"));
            })
            .Produces<Result<string>>();

        //vehicles
        app.MapGet("vehicles",
            async (
                IVehicleRepository vehicleRepository,
                ICategoryRepository categoryRepository,
                IUnitOfWork unitOfWork,
                CancellationToken cancellationToken) =>
            {
                var branchId = Guid.Parse("0197de0b-7613-7846-af49-b5a2cc121576");
                var existingPlates = await vehicleRepository.GetAll().Select(v => v.Plate.Value).ToListAsync(cancellationToken);
                var categories = await categoryRepository.GetAll().ToListAsync(cancellationToken);

                var baseFeatures = new[] { "Airbag", "ABS", "ESP", "Klima", "Bluetooth", "USB Bağlantısı" };
                var midFeatures = new[] { "Park Sensörü", "Geri Görüş Kamerası", "Cruise Control", "Dokunmatik Ekran", "Apple CarPlay" };
                var premiumFeatures = new[] { "GPS Navigasyon", "Isıtmalı Koltuk", "Sunroof", "Premium Ses Sistemi", "Alarm Sistemi" };

                var vehicles = new List<Vehicle>();

                for (int i = 0; i < VehicleSeeds.Length; i++)
                {
                    var v = VehicleSeeds[i];
                    if (existingPlates.Contains(v.Plate)) continue;

                    var category = categories.FirstOrDefault(c => c.Name.Value == v.Category);
                    if (category is null) continue;

                    var features = baseFeatures.AsEnumerable();
                    if (v.DailyPrice >= 1800) features = features.Concat(midFeatures);
                    if (v.DailyPrice >= 2800) features = features.Concat(premiumFeatures);

                    var lastMaintenanceKm = v.Kilometer / 15000 * 15000;
                    var (weekly, monthly) = v.DailyPrice >= 3000 ? (7, 12) : (10, 15);

                    vehicles.Add(new Vehicle(
                        new Brand(v.Brand),
                        new Model(v.Model),
                        new ModelYear(v.Year),
                        new Color(v.Color),
                        new Plate(v.Plate),
                        new IdentityId(category.Id),
                        new IdentityId(branchId),
                        new VinNumber($"WVW{Guid.NewGuid():N}".ToUpper()[..17]),
                        new EngineNumber($"ENG{Guid.NewGuid():N}".ToUpper()[..12]),
                        new Description($"{v.Year} model {v.Brand} {v.Model}, {v.Fuel.ToLower()} yakıtlı, {v.Transmission.ToLower()} vites."),
                        new ImageUrl(""), // görselleri sonradan ekle
                        new FuelType(v.Fuel),
                        new Transmission(v.Transmission),
                        new EngineVolume(v.EngineVolume),
                        new EnginePower(v.EnginePower),
                        new TractionType(v.Traction),
                        new FuelConsumption(v.FuelConsumption),
                        new SeatCount(v.Seats),
                        new Kilometer(v.Kilometer),
                        new DailyPrice(v.DailyPrice),
                        new WeeklyDiscountRate(weekly),
                        new MonthlyDiscountRate(monthly),
                        new InsuranceType("Kasko & Sigorta"),
                        new LastMaintanceDate(DateOnly.FromDateTime(DateTime.Now.AddMonths(-(2 + i % 5)))),
                        new LastMaintanceKm(lastMaintenanceKm),
                        new NextMaintanceKm(lastMaintenanceKm + 15000),
                        new InspectionDate(DateOnly.FromDateTime(DateTime.Now.AddMonths(3 + (i * 2) % 18))),
                        new InsuranceEndDate(DateOnly.FromDateTime(DateTime.Now.AddMonths(4 + (i * 3) % 9))),
                        new CascoEndDate(DateOnly.FromDateTime(DateTime.Now.AddMonths(4 + (i * 3) % 9))),
                        new TireStatus(v.Kilometer > 35000 ? "Orta" : "İyi"),
                        new GeneralStatus(v.Kilometer > 35000 ? "İyi" : "Çok İyi"),
                        features.Select(f => new Feature(f)).ToList(),
                        true));
                }

                vehicleRepository.AddRange(vehicles);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Results.Ok(Result<string>.Succeed("Araç seed data başarıyla tamamlandı"));
            })
            .Produces<Result<string>>();
        // ================= RESERVATIONS =================
        app.MapGet("reservations",
            async (
                IReservationRepository reservationRepository,
                ICustomerRepository customerRepository,
                IVehicleRepository vehicleRepository,
                IProtectionPackageRepository protectionPackageRepository,
                IExtraRepository extraRepository,
                IUnitOfWork unitOfWork,
                CancellationToken cancellationToken) =>
            {
                var existingReservationsCount = await reservationRepository.GetAll().CountAsync(cancellationToken);
                if (existingReservationsCount > 0)
                    return Results.Ok(Result<string>.Succeed("Rezervasyon verileri zaten mevcut."));

                var customers = await customerRepository.GetAll().ToListAsync(cancellationToken);
                var vehicles = await vehicleRepository.GetAll().ToListAsync(cancellationToken);
                var protectionPackages = await protectionPackageRepository.GetAll().ToListAsync(cancellationToken);
                var extras = await extraRepository.GetAll().ToListAsync(cancellationToken);

                if (!customers.Any() || !vehicles.Any() || !protectionPackages.Any())
                    return Results.BadRequest(Result<string>.Failure("Önce müşteri, araç ve paket seed datalarını yükleyin."));

                var random = new Random();
                var reservations = new List<Reservation>();

                for (int i = 0; i < 30; i++)
                {
                    var customer = customers[random.Next(customers.Count)];
                    var vehicle = vehicles[random.Next(vehicles.Count)];

                    // DİKKAT: DTO'daki INNER JOIN nedeniyle veri kaybı yaşamamak için 
                    // %100 ihtimalle bir koruma paketi seçtiriyoruz.
                    var package = protectionPackages[random.Next(protectionPackages.Count)];

                    // Rastgele geçmiş/gelecek tarih üretimi
                    var pickUpDateVal = DateOnly.FromDateTime(DateTime.Now.AddDays(random.Next(-60, 30)));
                    var pickUpTimeVal = new TimeOnly(random.Next(8, 17), 0); // 08:00 - 17:00 arası

                    var days = random.Next(1, 14); // 1 ile 14 gün arası kiralama
                    var deliveryDateVal = pickUpDateVal.AddDays(days);
                    var deliveryTimeVal = pickUpTimeVal;

                    // Rastgele ekstralar (0 ile 3 adet arası)
                    var selectedExtras = extras.OrderBy(x => random.Next()).Take(random.Next(0, 4)).ToList();

                    var reservationExtras = selectedExtras
                        .Select(e => new ReservationExtra(e.Id, e.Price.Value))
                        .ToList();

                    // Fiyat hesaplamaları
                    decimal vehiclePrice = vehicle.DailyPrice.Value;
                    decimal packagePrice = package.Price.Value;
                    decimal extrasTotal = selectedExtras.Sum(e => e.Price.Value);

                    decimal totalAmount = ((vehiclePrice + packagePrice) * days) + extrasTotal;

                    // Formları doldurma (Kilometreyi güne göre artırıyoruz)
                    int currentKm = vehicle.Kilometer.Value;
                    var pickUpForm = new Form(
                        new Kilometer(currentKm),
                        new List<Supplies>(),
                        new List<ImageUrl>(),
                        new List<Damage>(),
                        new Note("Araç temiz teslim edildi.")
                    );

                    var deliveryForm = new Form(
                        new Kilometer(currentKm + (days * random.Next(30, 150))),
                        new List<Supplies>(),
                        new List<ImageUrl>(),
                        new List<Damage>(),
                        new Note("Araç sorunsuz iade alındı.")
                    );

                    // History nesnesi (Rezervasyondan 1-5 gün öncesi)
                    var historyDate = new DateTime(pickUpDateVal, pickUpTimeVal).AddDays(-random.Next(1, 5));
                    var history = new ReservationHistory(
                        "Rezervasyon Oluşturuldu",
                        "Online Olarak Rezervasyon Oluşturuldu",
                        new DateTimeOffset(historyDate, TimeSpan.FromHours(3))
                    );

                    var paymentInfo = new PaymentInformation(
                        $"{random.Next(1000, 9999)}", // Kart son 4 hanesi
                        $"{customer.FirstName.Value} {customer.LastName.Value}"
                    );

                    // Entity oluşturuluyor
                    var reservation = Reservation.Create(
                        new IdentityId(customer.Id),
                        vehicle.BranchId, // Araç hangi şubedeyse PickUp oradan yapılıyor
                        new PickUpDate(pickUpDateVal),
                        new PickUpTime(pickUpTimeVal),
                        new DeliveryDate(deliveryDateVal),
                        new DeliveryTime(deliveryTimeVal),
                        new IdentityId(vehicle.Id),
                        new Price(vehiclePrice),
                        new IdentityId(package.Id),
                        new Price(packagePrice),
                        reservationExtras,
                        new Note(i % 5 == 0 ? "Bebek koltuğu önceden takılsın." : ""),
                        paymentInfo,
                        new Status("Bekliyor"),
                        new Total(totalAmount),
                        new TotalDay(days),
                        history,
                        pickUpForm,
                        deliveryForm
                    );

                    reservations.Add(reservation);
                }

                reservationRepository.AddRange(reservations);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Results.Ok(Result<string>.Succeed($"Başarıyla {reservations.Count} adet rastgele rezervasyon oluşturuldu."));
            })
            .Produces<Result<string>>();

    }

    // Geçerli (algoritmaya uygun) TC Kimlik No üretir
    private static string GenerateTcKn(int index)
    {
        var d = (100000000 + index * 7919 + 123456).ToString().Select(c => c - '0').ToArray();
        var d10 = (((d[0] + d[2] + d[4] + d[6] + d[8]) * 7) - (d[1] + d[3] + d[5] + d[7])) % 10;
        if (d10 < 0) d10 += 10;
        var d11 = (d.Sum() + d10) % 10;
        return string.Concat(d) + d10 + d11;
    }

    private sealed record CustomerSeed(
        string FirstName, string LastName, DateOnly BirthDate,
        string Email, string Phone, string Address, DateOnly LicenseDate);

    private static readonly CustomerSeed[] CustomerSeeds =
    {
        new("Ahmet", "Yılmaz", new(1988, 3, 14), "ahmet.yilmaz@gmail.com", "5321234501", "Alsancak Mah. 1471 Sk. No:12 D:3 Konak/İzmir", new(2008, 6, 20)),
        new("Elif", "Demir", new(1992, 7, 2), "elif.demir@hotmail.com", "5331234502", "Karşıyaka Mah. Cemal Gürsel Cd. No:45 D:7 Karşıyaka/İzmir", new(2012, 9, 11)),
        new("Mehmet", "Kaya", new(1985, 11, 23), "mehmet.kaya@outlook.com", "5421234503", "Bornova Mah. Ankara Cd. No:210 D:5 Bornova/İzmir", new(2005, 2, 8)),
        new("Zeynep", "Çelik", new(1995, 1, 30), "zeynep.celik@gmail.com", "5551234504", "Buca Mah. Kuruçeşme Sk. No:8 D:2 Buca/İzmir", new(2015, 5, 17)),
        new("Mustafa", "Şahin", new(1979, 5, 9), "mustafa.sahin@gmail.com", "5061234505", "Çankaya Mah. Atatürk Cd. No:33 D:4 Çankaya/Ankara", new(1999, 8, 25)),
        new("Ayşe", "Yıldız", new(1990, 12, 18), "ayse.yildiz@hotmail.com", "5071234506", "Gaziemir Mah. Akçay Cd. No:19 D:1 Gaziemir/İzmir", new(2010, 4, 3)),
        new("Hasan", "Öztürk", new(1983, 8, 6), "hasan.ozturk@gmail.com", "5301234507", "Mecidiyeköy Mah. Büyükdere Cd. No:101 D:12 Şişli/İstanbul", new(2003, 10, 14)),
        new("Fatma", "Aydın", new(1998, 4, 21), "fatma.aydin@outlook.com", "5341234508", "Bayraklı Mah. Manas Blv. No:39 D:9 Bayraklı/İzmir", new(2018, 7, 29)),
        new("İbrahim", "Arslan", new(1976, 9, 12), "ibrahim.arslan@gmail.com", "5351234509", "Menemen Mah. Cumhuriyet Cd. No:14 Menemen/İzmir", new(1996, 3, 6)),
        new("Hatice", "Doğan", new(1987, 2, 27), "hatice.dogan@hotmail.com", "5361234510", "Urla Mah. İskele Cd. No:22 D:3 Urla/İzmir", new(2007, 11, 19)),
        new("Emre", "Kılıç", new(1996, 6, 15), "emre.kilic@gmail.com", "5371234511", "Çiğli Mah. 8222 Sk. No:6 D:8 Çiğli/İzmir", new(2016, 1, 22)),
        new("Büşra", "Aslan", new(2000, 10, 3), "busra.aslan@gmail.com", "5381234512", "Kadıköy Mah. Bağdat Cd. No:288 D:6 Kadıköy/İstanbul", new(2019, 3, 12)),
        new("Burak", "Çetin", new(1991, 3, 8), "burak.cetin@outlook.com", "5391234513", "Narlıdere Mah. Sahil Cd. No:54 D:2 Narlıdere/İzmir", new(2011, 6, 30)),
        new("Merve", "Koç", new(1993, 5, 25), "merve.koc@gmail.com", "5411234514", "Balçova Mah. Mithatpaşa Cd. No:77 D:5 Balçova/İzmir", new(2013, 8, 9)),
        new("Oğuzhan", "Kurt", new(1989, 7, 19), "oguzhan.kurt@hotmail.com", "5431234515", "Seferihisar Mah. Sığacık Cd. No:3 Seferihisar/İzmir", new(2009, 12, 4)),
        new("Selin", "Özdemir", new(1997, 9, 1), "selin.ozdemir@gmail.com", "5441234516", "Alsancak Mah. 1453 Sk. No:27 D:4 Konak/İzmir", new(2017, 2, 15)),
        new("Can", "Erdoğan", new(1984, 12, 11), "can.erdogan@gmail.com", "5451234517", "Nilüfer Mah. 224. Sk. No:18 D:3 Nilüfer/Bursa", new(2004, 5, 27)),
        new("Deniz", "Polat", new(1994, 8, 29), "deniz.polat@outlook.com", "5461234518", "Muratpaşa Mah. Lara Cd. No:92 D:11 Muratpaşa/Antalya", new(2014, 10, 8)),
        new("Gamze", "Yavuz", new(1986, 1, 16), "gamze.yavuz@hotmail.com", "5471234519", "Karabağlar Mah. Gazi Blv. No:150 D:6 Karabağlar/İzmir", new(2006, 4, 21)),
        new("Serkan", "Tekin", new(1981, 6, 4), "serkan.tekin@gmail.com", "5481234520", "Torbalı Mah. İzmir Cd. No:61 Torbalı/İzmir", new(2001, 9, 13)),
        new("Pınar", "Güneş", new(1999, 11, 20), "pinar.gunes@gmail.com", "5491234521", "Karşıyaka Mah. Girne Blv. No:112 D:10 Karşıyaka/İzmir", new(2018, 1, 5)),
        new("Kerem", "Aksoy", new(1990, 2, 9), "kerem.aksoy@outlook.com", "5501234522", "Çeşme Mah. Cumhuriyet Mey. No:5 D:1 Çeşme/İzmir", new(2010, 7, 16)),
        new("Seda", "Bulut", new(1992, 4, 13), "seda.bulut@hotmail.com", "5511234523", "Bornova Mah. Kazımdirik Cd. No:48 D:7 Bornova/İzmir", new(2012, 3, 28)),
        new("Tolga", "Karaca", new(1978, 10, 27), "tolga.karaca@gmail.com", "5521234524", "Keçiören Mah. Fatih Cd. No:35 D:2 Keçiören/Ankara", new(1998, 6, 10)),
        new("Ece", "Türk", new(2001, 3, 5), "ece.turk@gmail.com", "5531234525", "Konak Mah. Gazi Blv. No:9 D:5 Konak/İzmir", new(2020, 5, 2)),
        new("Barış", "Şen", new(1987, 7, 31), "baris.sen@outlook.com", "5541234526", "Ataşehir Mah. Barbaros Blv. No:67 D:14 Ataşehir/İstanbul", new(2007, 1, 18)),
        new("Neslihan", "Erdem", new(1993, 9, 22), "neslihan.erdem@gmail.com", "5551234527", "Bayraklı Mah. Şair Eşref Blv. No:81 D:3 Bayraklı/İzmir", new(2013, 11, 7)),
        new("Yusuf", "Taş", new(1982, 5, 17), "yusuf.tas@hotmail.com", "5561234528", "Menderes Mah. Gaziemir Cd. No:26 Menderes/İzmir", new(2002, 8, 23)),
        new("Cansu", "Uçar", new(1996, 12, 8), "cansu.ucar@gmail.com", "5571234529", "Foça Mah. Atatürk Cd. No:13 D:2 Foça/İzmir", new(2016, 4, 19)),
        new("Onur", "Çakır", new(1989, 1, 24), "onur.cakir@outlook.com", "5581234530", "Buca Mah. Şirinyer Cd. No:102 D:8 Buca/İzmir", new(2009, 2, 11)),
    };

    private sealed record VehicleSeed(
        string Brand, string Model, int Year, string Color, string Plate, string Category,
        string Fuel, string Transmission, decimal EngineVolume, int EnginePower,
        string Traction, decimal FuelConsumption, int Seats, int Kilometer, int DailyPrice);

    private static readonly VehicleSeed[] VehicleSeeds =
    {
        // Binek
        new("Renault", "Clio", 2023, "Beyaz", "35ACL101", "Binek", "Benzin", "Manuel", 1.0m, 90, "Önden Çekiş", 5.4m, 5, 18500, 1650),
        new("Fiat", "Egea", 2023, "Gri", "35AEG102", "Binek", "Dizel", "Manuel", 1.6m, 130, "Önden Çekiş", 4.8m, 5, 24000, 1600),
        new("Toyota", "Corolla", 2024, "Beyaz", "35TCR103", "Binek", "Hibrit", "Otomatik", 1.8m, 140, "Önden Çekiş", 4.5m, 5, 9000, 2300),
        new("Volkswagen", "Polo", 2023, "Kırmızı", "35VPL104", "Binek", "Benzin", "Otomatik", 1.0m, 95, "Önden Çekiş", 5.6m, 5, 15000, 1900),
        new("Hyundai", "i20", 2024, "Mavi", "35HIT105", "Binek", "Benzin", "Otomatik", 1.4m, 100, "Önden Çekiş", 5.7m, 5, 7500, 1850),
        new("Peugeot", "208", 2023, "Siyah", "35PGT106", "Binek", "Benzin", "Otomatik", 1.2m, 100, "Önden Çekiş", 5.5m, 5, 21000, 1950),
        new("Honda", "Civic", 2022, "Gri", "35HCV107", "Binek", "Benzin", "Otomatik", 1.5m, 182, "Önden Çekiş", 6.7m, 5, 38000, 2600),
        new("Opel", "Corsa", 2023, "Beyaz", "35OCR108", "Binek", "Dizel", "Manuel", 1.5m, 102, "Önden Çekiş", 4.3m, 5, 27000, 1700),
        new("Renault", "Megane Sedan", 2023, "Gri", "35RMG109", "Binek", "Dizel", "Otomatik", 1.5m, 115, "Önden Çekiş", 4.6m, 5, 31000, 2100),
        new("Skoda", "Octavia", 2024, "Siyah", "35SOC110", "Binek", "Benzin", "Otomatik", 1.5m, 150, "Önden Çekiş", 5.9m, 5, 12000, 2700),

        // Station Wagon
        new("Skoda", "Octavia Combi", 2023, "Gri", "35SOK201", "Station Wagon", "Dizel", "Otomatik", 2.0m, 150, "Önden Çekiş", 4.9m, 5, 29000, 2800),
        new("Volkswagen", "Passat Variant", 2022, "Siyah", "35VPV202", "Station Wagon", "Dizel", "Otomatik", 2.0m, 150, "Önden Çekiş", 5.0m, 5, 45000, 3200),
        new("Ford", "Focus Station Wagon", 2022, "Beyaz", "35FFS203", "Station Wagon", "Dizel", "Otomatik", 1.5m, 120, "Önden Çekiş", 4.4m, 5, 42000, 2200),
        new("Opel", "Astra Sports Tourer", 2023, "Gri", "35OAS204", "Station Wagon", "Dizel", "Otomatik", 1.5m, 130, "Önden Çekiş", 4.6m, 5, 26000, 2300),

        // Minibüs
        new("Volkswagen", "Transporter", 2023, "Beyaz", "35VTR301", "Minibüs", "Dizel", "Manuel", 2.0m, 110, "Önden Çekiş", 7.5m, 9, 36000, 3800),
        new("Ford", "Tourneo Custom", 2022, "Gri", "35FTC302", "Minibüs", "Dizel", "Otomatik", 2.0m, 170, "Önden Çekiş", 7.2m, 9, 52000, 4200),
        new("Mercedes-Benz", "Vito Tourer", 2023, "Siyah", "35MVT303", "Minibüs", "Dizel", "Otomatik", 2.0m, 163, "Arkadan İtiş", 7.0m, 8, 34000, 4600),

        // SUV
        new("Dacia", "Duster", 2023, "Haki Yeşil", "35DDS401", "SUV", "Benzin", "Manuel", 1.3m, 130, "Önden Çekiş", 6.2m, 5, 22000, 1900),
        new("Nissan", "Qashqai", 2023, "Beyaz", "35NQS402", "SUV", "Benzin", "Otomatik", 1.3m, 140, "Önden Çekiş", 6.3m, 5, 19000, 2500),
        new("Hyundai", "Tucson", 2024, "Gri", "35HTC403", "SUV", "Hibrit", "Otomatik", 1.6m, 215, "4 Çeker", 5.8m, 5, 8000, 3100),
        new("Toyota", "C-HR", 2023, "Mavi", "35TCH404", "SUV", "Hibrit", "Otomatik", 1.8m, 122, "Önden Çekiş", 4.9m, 5, 16000, 2600),
        new("Renault", "Captur", 2023, "Turuncu", "35RCP405", "SUV", "Benzin", "Otomatik", 1.3m, 140, "Önden Çekiş", 6.0m, 5, 23000, 2100),
        new("Peugeot", "3008", 2023, "Siyah", "35PGE406", "SUV", "Dizel", "Otomatik", 1.5m, 130, "Önden Çekiş", 5.0m, 5, 28000, 2900),
        new("Suzuki", "Vitara", 2022, "Kırmızı", "35SZV407", "SUV", "Benzin", "Otomatik", 1.4m, 129, "4 Çeker", 6.2m, 5, 41000, 2200),
        new("BMW", "X1", 2023, "Beyaz", "35BX1408", "SUV", "Dizel", "Otomatik", 2.0m, 150, "Önden Çekiş", 5.2m, 5, 21000, 4200),

        // Üstü Açık
        new("Mini", "Cooper Cabrio", 2022, "Kırmızı", "35MCC501", "Üstü Açık", "Benzin", "Otomatik", 1.5m, 136, "Önden Çekiş", 6.3m, 4, 27000, 3900),
        new("Mazda", "MX-5", 2022, "Beyaz", "35MMX502", "Üstü Açık", "Benzin", "Manuel", 2.0m, 184, "Arkadan İtiş", 7.0m, 2, 19000, 4300),

        // MPV
        new("Fiat", "Doblo", 2023, "Beyaz", "35FDB601", "MPV", "Dizel", "Manuel", 1.5m, 100, "Önden Çekiş", 4.9m, 5, 33000, 1900),
        new("Citroen", "Berlingo", 2023, "Gri", "35CBL602", "MPV", "Dizel", "Otomatik", 1.5m, 130, "Önden Çekiş", 5.0m, 5, 25000, 2000),
        new("Volkswagen", "Caddy", 2023, "Siyah", "35VCD603", "MPV", "Dizel", "Otomatik", 2.0m, 122, "Önden Çekiş", 5.4m, 5, 30000, 2200),
    };
}