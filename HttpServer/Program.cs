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
            var result = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cars));

            response.ContentLength64 = result.Length;

            await response.OutputStream.WriteAsync(result, 0, result.Length);

            break;
        }

        case "POST":
        {
            Car car = null; 
            
            await using var writer = new StreamWriter(response.OutputStream);
            
            try
            {
                car = await ReadBodyAsync<Car>();
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                await writer.WriteAsync($"Internal server error: {ex.Message}");
                break;
            }
            
            response.StatusCode = 201;
            
            dbContext.Cars.Add(car);
            await dbContext.SaveChangesAsync();
            
            
            await writer.WriteAsync($"Car {car} has been added");
            
            break;   
        }

        case "PUT":
        {
            Car car = null; 
            
            await using var writer = new StreamWriter(response.OutputStream);
            
            try
            {
                car = await ReadBodyAsync<Car>();
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                await writer.WriteAsync($"Internal server error: {ex.Message}");
                break;
            }
            
            var carToUpdate = dbContext.Cars.FirstOrDefault(c => c.Id == car.Id);
            
            if (carToUpdate is null)
            {
                await writer.WriteAsync($"Car {car} not found");
                response.StatusCode = 404;
                break;
            }
            
            carToUpdate.Brand = car.Brand;
            carToUpdate.Model = car.Model;
            carToUpdate.Year = car.Year;
            
            await dbContext.SaveChangesAsync();
            await writer.WriteAsync($"Car {car} has been edited");
            
            break;
        }

        case "DELETE":
        {
            await using var writer = new StreamWriter(response.OutputStream);

            if (!int.TryParse(request.Url!.Segments[3], out int id))
            {
                await writer.WriteAsync("Invalid id");
                response.StatusCode = 400;
                break;
            }

            var carToRemove = dbContext.Cars.FirstOrDefault(c => c.Id == id);

            if (carToRemove is null)
            {
                await writer.WriteAsync($"Car {carToRemove} not found");
                break;
            }
            
            dbContext.Cars.Remove(carToRemove);
            await dbContext.SaveChangesAsync();
            
            await writer.WriteAsync($"Car {carToRemove} has been deleted");
            
            break;
        }
    }
    response.Close();
    async Task<T> ReadBodyAsync<T>()
    {
        using var reader = new StreamReader(request.InputStream);
        string body = await reader.ReadToEndAsync();
        
        return JsonSerializer.Deserialize<T>(body);
    }
}

