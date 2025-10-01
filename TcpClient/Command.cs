namespace TcpClient;

public record Command(Request Request, Car? Param = null, int? Id = null);