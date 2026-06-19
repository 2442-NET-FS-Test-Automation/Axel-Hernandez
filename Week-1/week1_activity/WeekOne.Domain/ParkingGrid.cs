namespace WeekOne.Domain;
// Represents the parking grid (Lot x Row) where each car in the inventory is placed. The size is calculated automatically based
// on the number of cars in stock.
public class ParkingGrid
{
    public int Lots { get; private set; }   
    public int Rows { get; private set; }  

    // Calculates the grid dimensions based on the given car count.
    public static ParkingGrid Calculate(int carCount)
    {
        var grid = new ParkingGrid();

        if (carCount <= 0)
        {
            grid.Lots = 0;
            grid.Rows = 0;
            return grid;
        }

        int lots = (int)Math.Ceiling(Math.Sqrt(carCount));
        int rows = (int)Math.Ceiling((double)carCount / lots);

        grid.Lots = lots;
        grid.Rows = rows;
        return grid;
    }

    // Given a car's Id, this function returns its [row, col] position within the grid.
    public (int Row, int Col) PositionFor(int carId)
    {
        if (Lots == 0)
        {
            throw new InvalidOperationException("The grid is empty, there are no positions to calculate.");
        }

        int index = carId - 1; 
        int row = index / Lots;
        int col = index % Lots;
        return (row, col);
    }
}