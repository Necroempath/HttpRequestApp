using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TcpServer;

var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer(JsonConfigBuilder.GetConnectionString()).Options;


TcpListener listener = new TcpListener(IPAddress.Loopback, 39801);
listener.Start();

while (true)
{
    var client = listener.AcceptTcpClient();
    var stream = client.GetStream();
    
    using AppDbContext context = new(options);
    string report = string.Empty;
    
    using BinaryReader br = new BinaryReader(stream);
    using BinaryWriter bw = new BinaryWriter(stream);


    var command = JsonSerializer.Deserialize<Command>(br.ReadString());

    switch (command!.Request)
    {
        case Request.Get:
            var cars = context.Cars.ToList();
            report = JsonSerializer.Serialize(cars);

            break;

        case Request.Post:
            context.Cars.Add(command.Param!);
            context.SaveChanges();
            
            report = $"Car {command.Param!} added successfully";
            break;

        case Request.Put:
            var carUpdate = context.Cars.FirstOrDefault(c => c.Id == command.Id);
            
            if (carUpdate == null)
            {
                report = $"Car by given id [{command.Id}] not found. Operation denied.";
                break;
            }

            carUpdate.Brand = command.Param!.Brand;
            carUpdate.Model = command.Param!.Model;
            carUpdate.Year = command.Param!.Year;
            
            context.SaveChanges();
            
            report = $"Car {command.Param!} edited successfully";
            break;

        case Request.Delete:
            var carDelete = context.Cars.FirstOrDefault(c => c.Id == command.Id);
            
            if (carDelete == null)
            {
                report = $"Car by given id [{command.Id}] not found. Operation denied.";
                break;
            }
            context.Cars.Remove(carDelete);
            context.SaveChanges();
            
            report = $"Car {carDelete} deleted successfully";
            break;

        default:
            report = "Invalid request";
            break;
    }

    bw.Write(report);
    client.Dispose();
}