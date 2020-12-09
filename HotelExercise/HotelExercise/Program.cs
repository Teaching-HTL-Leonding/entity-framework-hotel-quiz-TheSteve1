using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

var factory = new HotelContextFactory();
var dbcontext = factory.CreateDbContext();


	Console.Write("query/add eingeben:");
	string qa = Console.ReadLine();
	if (qa == "add")
	{
		await AddData();
		Console.WriteLine("Hotels hinzugefügt");
	}
	else if (qa == "query")
	{
		
		await QueryData();
	}


async Task AddData()
{

	RoomType sroom3x10, droom10x15, sroom10x15, droom25x30, jsuite5x45, honeymoon1x100;
	await dbcontext.RoomTypes.AddRangeAsync(new[]
	{
		sroom3x10 = new RoomType(){Title = "Single Room",Size = 10,AvailiableRooms = 3,},
		droom10x15 = new RoomType(){Title = "Double Room",Size = 15,AvailiableRooms = 10,},
		sroom10x15 = new RoomType(){Title = "Single Room",Size = 15,AvailiableRooms = 10, IsDisabilityacessible = true},
		droom25x30 = new RoomType(){Title = "Double Room",Size = 30,AvailiableRooms = 20, IsDisabilityacessible = true},
		jsuite5x45 = new RoomType(){Title = "Junior Suite",Size = 45,AvailiableRooms = 5, IsDisabilityacessible = true},
		honeymoon1x100 = new RoomType(){Title = "Honeymoon Suite",Size = 100,AvailiableRooms = 1, IsDisabilityacessible = true},
	});
	
	await dbcontext.RoomPrices.AddRangeAsync(new[]
	{
		new RoomPrice(){ PricePerNight = 40, RoomType = sroom3x10},
		new RoomPrice(){ PricePerNight = 60, RoomType = droom10x15},
		new RoomPrice(){ PricePerNight = 70, RoomType = sroom10x15},
		new RoomPrice(){ PricePerNight = 120, RoomType = droom25x30},
		new RoomPrice(){ PricePerNight = 190, RoomType = jsuite5x45},
		new RoomPrice(){ PricePerNight = 300, RoomType = honeymoon1x100},
	});
	
	HotelSpecial spa, sauna, indoor_pool, outdoor_pool, dogfriendly, organic_food;
	await dbcontext.HotelSpecials.AddRangeAsync(new[]
	{
		spa = new HotelSpecial(){EHotelSpecial = EHotelSpecial.Spa},
		sauna = new HotelSpecial(){EHotelSpecial = EHotelSpecial.Sauna},
		indoor_pool = new HotelSpecial(){EHotelSpecial = EHotelSpecial.Indoor_pool},
		outdoor_pool = new HotelSpecial(){EHotelSpecial = EHotelSpecial.Outdoor_pool},
		dogfriendly = new HotelSpecial(){EHotelSpecial = EHotelSpecial.Dog_friendly},
		organic_food = new HotelSpecial(){EHotelSpecial = EHotelSpecial.Organic_food},
	});
	
	await dbcontext.Hotels.AddRangeAsync(new[]
	{
		new Hotel(){Name = "Pension Marianne",Adress = "Am Hausberg 17, 134 Irgendwo",HotelSpecials = new List<HotelSpecial> { dogfriendly, organic_food},RoomTypes = new List<RoomType> { sroom3x10, droom10x15},},
		new Hotel(){Name = "Grand Hotel Goldener Hirsch",Adress = "Im stillen Tal 42, 4711 Schönberg",HotelSpecials = new List<HotelSpecial> { spa, sauna, indoor_pool, outdoor_pool},RoomTypes = new List<RoomType> { sroom10x15, droom25x30, jsuite5x45, honeymoon1x100},},
	});
	await dbcontext.SaveChangesAsync();
}
async Task QueryData()
{
	
	foreach (var hotel in await dbcontext.Hotels.ToArrayAsync())
	{
		Console.WriteLine($"# {hotel.Name}\n");
		Console.WriteLine("## Location\n");
		Console.WriteLine($"{hotel.Adress}\n");
		Console.WriteLine("## Specials\n");
		foreach (var hotelspecial in hotel.HotelSpecials){
			Console.WriteLine($"* {hotelspecial.EHotelSpecial}");
		}
		
		Console.WriteLine("\n## Room Types\n");
		Console.WriteLine($"| {"Room Type",-22} | {"Size",6} | {"Price Valid From",-18} | {"Price Valid To",-14} | {"Price in €",10} |");
		Console.WriteLine($"| {new string('-', 22)} | {new string('-', 5)}: | {new string('-', 18)} | {new string('-', 14)} | {new string('-', 9)}: |");
		var roomTypesPrice = await dbcontext.RoomPrices
			.Where(price => price.RoomType.HotelId == hotel.Id)
			.Include(x => x.RoomType).ToArrayAsync();
		foreach (var rtp in roomTypesPrice){
			Console.WriteLine($"| {rtp.RoomType.Title,-22} | {rtp.RoomType.Size + " m²",6} | {rtp.ValidFrom,-18} | {rtp.ValidUntil,-14} | {rtp.PricePerNight + " €",10} |");
		}

	}
}
#region Model
class Hotel{
	public int Id { get; set; }
	public string Name { get; set; }
	public string Adress { get; set; }
	public List<HotelSpecial> HotelSpecials { get; set; } = new();
	public List<RoomType> RoomTypes { get; set; } = new();
}
enum EHotelSpecial
{
	Spa,
	Sauna,
	Dog_friendly,
	Indoor_pool,
	Outdoor_pool,
	Bikerental,
	ECar_charging_station,
	Vegetarian_cuisine,
	Organic_food
}
class HotelSpecial
{
	public int Id { get; set; }
	public EHotelSpecial EHotelSpecial { get; set; }
	public List<Hotel> Hotels { get; set; }
}
class RoomType
{
	public int Id { get; set; }
	public Hotel Hotel { get; set; }
	public int HotelId { get; set; }
	public string Title { get; set; }
	public string Description { get; set; }
	public int Size { get; set; }
	public bool IsDisabilityacessible { get; set; } = false;
	public int AvailiableRooms { get; set; }
}
class RoomPrice
{
	public int Id { get; set; }
	public RoomType RoomType { get; set; }
	public int RoomId { get; set; }
	public DateTime? ValidFrom { get; set; }
	public DateTime? ValidUntil { get; set; }

	[Column(TypeName = "decimal(8, 2)")]
	public decimal PricePerNight { get; set; }
}
#endregion

#region DataContext
class HotelContext : DbContext
{
	public DbSet<Hotel> Hotels { get; set; }
	public DbSet<HotelSpecial> HotelSpecials { get; set; }
	public DbSet<RoomType> RoomTypes { get; set; }
	public DbSet<RoomPrice> RoomPrices { get; set; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
	public HotelContext(DbContextOptions<HotelContext> options)
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
		: base(options)
	{ }
}
class HotelContextFactory : IDesignTimeDbContextFactory<HotelContext>
{
	public HotelContext CreateDbContext(string[] args = null)
	{
		var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

		var optionsBuilder = new DbContextOptionsBuilder<HotelContext>();
		optionsBuilder
			// Uncomment the following line if you want to print generated
			// SQL statements on the console.
			//.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddConsole()))
			.UseSqlServer(configuration["ConnectionStrings:DefaultConnection"]);

		return new HotelContext(optionsBuilder.Options);
	}
}
#endregion