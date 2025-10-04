using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Metadata.Ecma335;
using TcpClientApp;
using System.Text.Json;

using var client = new HttpClient();
string uriBase = "http://localhost:39801/";

while (true)
{
    Console.Write(@"    
    1. Get all cars (GET)
    2. Add car (POST)
    3. Edit car (PUT)
    4. Delete car (DELETE)
    5. Exit

Select option: ");

    switch (GetOption())
    {
        case MenuOption.Get:
        {
            using var response = await client.GetAsync(uriBase + "Cars");

            var cars = JsonSerializer.Deserialize<List<Car>>(response.Content.ReadAsStringAsync().Result);

            cars!.ForEach(Console.WriteLine);

            break;
        }

        case MenuOption.Post:
        {
            var car = new CarCreationService().CreateCar(out List<string> postErrors);

            if (car is null)
            {
                postErrors.ForEach(Console.WriteLine);
                break;
            }

            using HttpContent content = new StringContent(JsonSerializer.Serialize(car));
            using var response = await client.PostAsync(uriBase + "Cars", content);

            Console.WriteLine($"Status: {response.StatusCode}\n{response.Content.ReadAsStringAsync().Result}");
            break;
        }

        case MenuOption.Put:
        {
            var car = new CarCreationService().CreateCar(out List<string> putErrors);

            if (car is null)
            {
                putErrors.ForEach(Console.WriteLine);
                break;
            }

            int? id = GetId();

            if (id is null)
            {
                Console.WriteLine("Invalid id");
                break;
            }
            car.Id = id.Value;
            
            using HttpContent content = new StringContent(JsonSerializer.Serialize(car));
            using var response = await client!.PutAsync(uriBase + "Cars", content);

            Console.WriteLine($"Status: {response.StatusCode}\n{response.Content.ReadAsStringAsync().Result}");
            
            break;
        }

        case MenuOption.Delete:
        {
            int? id = GetId();

            if (id is null)
            {
                Console.WriteLine("Invalid id");
                break;
            }

            using var response = await client!.DeleteAsync($"{uriBase}/Cars/{id.Value}");
            var res = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Status: {response.StatusCode}\n{response.Content.ReadAsStringAsync().Result}");
            
            break;
        }
 

        case MenuOption.Exit:
            return;
        default:
            Console.WriteLine("Unknown option");
            break;
    }
    
    Console.WriteLine("\nPress any key to continue...");
    
    Console.ReadKey();
    Console.Clear();
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


MenuOption? GetOption()
{
    string input = Console.ReadLine()!.Trim();

    if (Enum.TryParse<MenuOption>(input, out var option) && Enum.IsDefined(typeof(MenuOption), option))
    {
        return option;
    }

    return null;
}