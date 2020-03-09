using AutoPlanning.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TaskPlanning.Client.Sample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var accessKey = File.ReadAllText("AccessKey.txt");

            var client = TaskPlanningClient.Create(accessKey);

            var request = new PlanningRequest();

            PlanningTask task = await client.Plan(request);
        }
    }
}
