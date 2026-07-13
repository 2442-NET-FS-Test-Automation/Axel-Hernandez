using AutoSupply.Data.Entities;
using AutoSupply.Data.Enums;

namespace AutoSupply.Api.Fulfillment;



public class BurstPlanner
{
    public IReadOnlyList<int> OrderByPriority(IEnumerable<Order> orders)
    {
        PriorityQueue<int, int> priorityQueue = new PriorityQueue<int, int>();

        foreach(Order order in orders)
        {
            priorityQueue.Enqueue(order.Id, order.Priority == Priority.Expedited ? 0 : 1);
        }


        var orderByPriority = new List<int>();

        while(priorityQueue.TryDequeue(out int id, out _))
        {
            orderByPriority.Add(id);
        }


        return orderByPriority;
    }
}