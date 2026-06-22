using System.Text.Json;
using Serilog;

namespace WeekOne.Domain;

public class OpenLibraryClient
{

    private static readonly HttpClient client = new();

    public async Task<CarRental?> FetchByIdAsync(int id = 440)
    {
        string url = $"https://vpic.nhtsa.dot.gov/api/vehicles/GetModelsForMakeId/{id}?format=json";
        Console.WriteLine("Fetch By Id Async");

        try
        {
            string jsonResponse = await client.GetStringAsync(url);
            return Parse(jsonResponse);

        }
        catch (HttpRequestException ex)
        {
            Log.Error("Network fetch failed for {id}: {ex.Message}", id, ex.Message);
            return null;
        }catch(Exception ex)
        {
            Log.Error("FetchByIsbnAsync failed: {ex.Message}", ex.Message);
            return null;
        }
    }

    public static CarRental? Parse(string json)
    {
        Dictionary<string, JsonElement>? resp = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

        if (resp is null ||  !resp.TryGetValue("Results", out JsonElement results) || results.GetArrayLength() == 0)
        {
            return null; 
        }

        JsonElement foundCar = results[0]; 

        string brand = foundCar.GetProperty("Make_Name").GetString() ?? "No Brand";
        string model = foundCar.GetProperty("Model_Name").GetString() ?? "No Model";

        int dayCost = 10;
        int rentalPeriod = 1;
        bool isAvailable = true;

        if(brand != "No Brand" && model != "No Model")
        {
            CarRental car = new CarRental(brand, model, dayCost, rentalPeriod, CarStatus.Available);
            return car; 
        }
        return null;        
    }
}