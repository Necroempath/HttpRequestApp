using System.Net;
using System.Net.Sockets;
using TcpClientApp;
using System.Text.Json;

IPEndPoint endPoint = new(IPAddress.Loopback, 39801);

using TcpClient? client = default;

while (true)
{
    Console.Write(@"    
    1. Get all cars (GET)
    2. Add car (POST)
    3. Edit car (PUT)
    4. Delete car (DELETE)
    5. Exit

Select option: ");

    string report = string.Empty;

    switch (GetOption())
    {
        case MenuOption.Get:

            Command commandGet = new(Request.Get);

            string response = null;

            try
            {
                response = SendRequest(endPoint, commandGet);
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
                break;
            }

            var cars = JsonSerializer.Deserialize<List<Car>>(response);
            cars!.ForEach(Console.WriteLine);

            break;

        case MenuOption.Post:

            var postCar = new CarCreationService().CreateCar(out List<string> postErrors);

            if (postCar is null)
            {
                postErrors.ForEach(Console.WriteLine);
                break;
            }

            Command commandPost = new(Request.Post, postCar);

            try
            {
                report = SendRequest(endPoint, commandPost);
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }

            break;

        case MenuOption.Put:

            var putCar = new CarCreationService().CreateCar(out List<string> putErrors);

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

            Command commandPut = new(Request.Put, putCar, putId);

            report = SendRequest(endPoint, commandPut);

            break;

        case MenuOption.Delete:
            int? deleteId = GetId();

            if (deleteId is null)
            {
                Console.WriteLine("Invalid id");
                break;
            }

            Command deleteCommand = new(Request.Delete, null, deleteId);

            report = SendRequest(endPoint, deleteCommand);

            break;

        case MenuOption.Exit:
            return;
        default:
            Console.WriteLine("Unknown option");
            break;
    }

    Console.WriteLine(report);
    client?.Dispose();
    ContinuationMessage();
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

string SendRequest(IPEndPoint endPoint, Command command)
{
    var client = new TcpClient();

    client.Connect(endPoint);

    using var stream = client.GetStream();

    using var writer = new BinaryWriter(stream);
    using var reader = new BinaryReader(stream);

    writer.Write(JsonSerializer.Serialize(command));

    return reader.ReadString();
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
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();

    if (clear) Console.Clear();
}