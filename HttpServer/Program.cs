using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TcpServer;

var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(JsonConfigBuilder.GetConnectionString()).Options;


using HttpListener listener = new HttpListener();
listener.Prefixes.Add("http://localhost:39801/");
listener.Start();

while (true)
{
    using AppDbContext dbContext = new(options);

    var context = listener.GetContext();
    var request = context.Request;
    var response = context.Response;

    switch (request.HttpMethod)
    {
        case "GET":
        {
            var cars = dbContext.Cars.ToList();
            var body = JsonSerializer.Serialize(cars);
            
            await RespondAsync(response, body);
    
            break;
        }

        case "POST":
        {
            Car car = null; 
            
            await using var writer = new StreamWriter(response.OutputStream);
            
            try
            {
                car = await ReadBodyAsync<Car>(request);
            }
            catch (Exception ex)
            {
                await RespondAsync(response, $"Internal server error: {ex.Message}", 500);
                break;
            }
            
            dbContext.Cars.Add(car);
            await dbContext.SaveChangesAsync();
            
            await RespondAsync(response, $"Car {car} has been added", 201);
            
            break;   
        }

        case "PUT":
        {
            Car car = null; 
            
            await using var writer = new StreamWriter(response.OutputStream);
            
            try
            {
                car = await ReadBodyAsync<Car>(request);
            }
            catch (Exception ex)
            {
                await RespondAsync(response, $"Internal server error: {ex.Message}", 500);
                break;
            }
            
            var carToUpdate = dbContext.Cars.FirstOrDefault(c => c.Id == car.Id);
            
            if (carToUpdate is null)
            {
                await RespondAsync(response, $"Car {car} not found", 404);
                break;
            }
            
            carToUpdate.Brand = car.Brand;
            carToUpdate.Model = car.Model;
            carToUpdate.Year = car.Year;
            
            await dbContext.SaveChangesAsync();
            await RespondAsync(response, $"Car {car} has been edited");
            
            break;
        }

        case "DELETE":
        {
            await using var writer = new StreamWriter(response.OutputStream);

            if (!int.TryParse(request.Url!.Segments[3], out int id))
            {
                await RespondAsync(response, "Invalid id", 400);
                break;
            }

            var carToRemove = dbContext.Cars.FirstOrDefault(c => c.Id == id);

            if (carToRemove is null)
            {
                await RespondAsync(response, $"Car {carToRemove} not found", 404);
                break;
            }
            
            dbContext.Cars.Remove(carToRemove);
            await dbContext.SaveChangesAsync();
            
            await RespondAsync(response, $"Car {carToRemove} has been deleted");
            
            break;
        }
    }
    response.Close();
}
async Task<T> ReadBodyAsync<T>(HttpListenerRequest request)
{
    using var reader = new StreamReader(request.InputStream);
    string body = await reader.ReadToEndAsync();
        
    return JsonSerializer.Deserialize<T>(body);
}

async Task RespondAsync(HttpListenerResponse response, string body, int statusCode = 200, string contentType = "text/plain")
{
    response.StatusCode = statusCode;
    response.ContentType = contentType;
    
    await using var writer = new StreamWriter(response.OutputStream);
    await writer.WriteLineAsync(body);
}

