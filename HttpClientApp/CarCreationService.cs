namespace TcpClientApp;

public class CarCreationService
{
    public Car? CreateCar(out List<string> errors)
    {
        CarDto dto = GetCarDto();
        errors = ValidateCar(dto);

        if (errors.Count != 0)
        {
            return null;
        }

        return new() { Brand = dto.Brand, Model = dto.Model, Year = int.Parse(dto.Year) };
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
}