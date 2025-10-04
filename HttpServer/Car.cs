namespace TcpServer;

public class Car
{
    public int Id { get; set; }
    public required string Brand { get; set; }
    public required string Model { get; set; }
    public required int Year { get; set; }

    public override string ToString()
    {
        return $"Id[{Id}]: {Brand} — {Model} — {Year}";
    }
}