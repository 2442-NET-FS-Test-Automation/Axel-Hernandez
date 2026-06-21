using System.Text.Json;
using Serilog;

namespace WeekOne.Domain;

public class OpenLibraryClient
{

    private static readonly HttpClient client = new();

    public async Task<CarRental?> FetchByIdAsync(int id = 440)
    {
        string url = $"https://vpic.nhtsa.dot.gov/api/vehicles/GetModelsForMakeId/{id}?format=json";
        Console.WriteLine("Fetch By Id Async .........................");

        try
        {
            string jsonResponse = await client.GetStringAsync(url);

            return Parse(jsonResponse);
        }
        catch (HttpRequestException ex)
        {
            Log.Warning($"Network fetch failed for {id}: {ex.Message}");
            return null;
        }catch(Exception ex)
        {
            Log.Warning($"FetchByIsbnAsync failed: {ex.Message}");
            return null;
        }
    }

    public static CarRental? Parse(string json)
    {
        Console.WriteLine(json);
        Console.WriteLine("Parse .........................");

        Dictionary<string, JsonElement>? resp = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

        if (resp is null ||  !resp.TryGetValue("Results", out JsonElement results) || results.GetArrayLength() == 0)
        {
            return null; // no docs array somehow, docs array is empty, or the json itself was empty - return a null
        }

        Console.WriteLine("Diccionario logrado");

        JsonElement foundCar = results[0]; 
        //string brand, string model, int dayCost, int rentalPeriod, bool isAvailable

        string brand = foundCar.GetProperty("Make_Name").GetString() ?? "No Brand";
        string model = foundCar.GetProperty("Model_Name").GetString() ?? "No Model";

        int dayCost = 10;
        int rentalPeriod = 1;
        bool isAvailable = true;

        CarRental car = new CarRental(brand, model, dayCost, rentalPeriod, isAvailable);

        Console.WriteLine($"Rental car : {car} ");
        return car; 
    }
}