using System.Net;
using TcpClient;
using System.Text.Json;

IPEndPoint endPoint = new(IPAddress.Loopback, 2048);
IEnumerable<Car> cars;

while (true)
{
    Console.Write(@"    
    1. Get all cars (GET)
    2. Add car (POST)
    3. Edit car (PUT)
    4. Delete car (DELETE)

Select option: ");

    switch (GetOption())
    {
        case MenuOption.Get:
            Command getCommand = new(Request.Get);

            ConnectAndRequest(endPoint, getCommand, out var getBr, out var getBw);

            string response = getBr.ReadString();
            cars = JsonSerializer.Deserialize<IEnumerable<Car>>(response)!;

            foreach (var item in cars)
            {
                Console.WriteLine(item.ToString());
            }

            break;

        case MenuOption.Post:
            Car? postCar = CreateCar(out List<string> postErrors);

            if (postCar is null)
            {
                postErrors.ForEach(Console.WriteLine);
                break;
            }

            Command postCommand = new(Request.Post, postCar);

            ConnectAndRequest(endPoint, postCommand, out var postBr, out var postBw);
            break;

        case MenuOption.Put:
            Car? putCar = CreateCar(out List<string> putErrors);

            if (putCar is null)
            {
                putErrors.ForEach(Console.WriteLine);
                break;
            }

            int? putId = GetId();

            if (putId is null)
            {
                Console.WriteLine("Invalid id");
                break;
            }

            putCar.Id = putId.Value;

            Command putCommand = new(Request.Put, putCar);
            ConnectAndRequest(endPoint, putCommand, out var putBr, out var putBw);

            break;
        case MenuOption.Delete:
            int? deleteId = GetId();

            if (deleteId is null)
            {
                Console.WriteLine("Invalid id");
                break;
            }

            Command deleteCommand = new(Request.Delete, null, deleteId);
            ConnectAndRequest(endPoint, deleteCommand, out var deleteBr, out var deleteBw);

            break;
        case MenuOption.Exit:
            return;
        default:
            Console.WriteLine("Unknown option");
            break;
    }
}


List<string> ValidateCar(CarDto car)
{
    List<string> errors = new();

    if (string.IsNullOrEmpty(car.Brand))
    {
        errors.Add("Empty brand");
    }

    if (string.IsNullOrEmpty(car.Model))
    {
        errors.Add("Empty model");
    }

    if (!int.TryParse(car.Year, out int year))
    {
        errors.Add("Incorrect year");
    }
    else if (year < 1900 || year > DateTime.Now.Year)
    {
        errors.Add("Invalid year");
    }

    return errors;
}

Car? CreateCar(out List<string> errors)
{
    CarDto dto = GetCarDto();
    errors = ValidateCar(dto);

    if (errors.Count != 0)
    {
        return null;
    }

    return new() { Brand = dto.Brand, Model = dto.Model, Year = int.Parse(dto.Year) };
}

int? GetId()
{
    Console.WriteLine("Enter Id:");

    if (int.TryParse(Console.ReadLine(), out int id))
    {
        if (id > 0) return id;
    }

    return null;
}

CarDto GetCarDto()
{
    Console.WriteLine("Enter brand:");
    string brand = Console.ReadLine()!.Trim();

    Console.WriteLine("Enter model:");
    string model = Console.ReadLine().Trim();

    Console.WriteLine("Enter year:");
    string year = Console.ReadLine().Trim();

    return new CarDto(brand, model, year);
}

ContinuationMessage();

void ConnectAndRequest(IPEndPoint endPoint, Command command, out BinaryReader reader, out BinaryWriter writer)
{
    System.Net.Sockets.TcpClient client = new(endPoint);
    client.Connect(endPoint);

    var stream = client.GetStream();
    reader = new BinaryReader(stream);
    writer = new BinaryWriter(stream);

    writer.Write(JsonSerializer.Serialize(command));
}

MenuOption? GetOption()
{
    string input = Console.ReadLine()!.Trim();

    if (Enum.TryParse<MenuOption>(input, out var option) && Enum.IsDefined(typeof(MenuOption), option))
    {
        return option;
    }

    return null;
}

void ContinuationMessage(bool clear = true)
{
    Console.WriteLine("\n\nPress any key to continue...");
    Console.ReadKey();

    if (clear) Console.Clear();
}