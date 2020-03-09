using AutoPlanning.Models;
using System;
using System.Collections.Generic;
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

            var request = new PlanningRequest()
            {
                //Make a regular style planning (fill up the planning till there are no more options)
                Mode = PlanningMode.Regular,

                //There can be multiple resources to plan for
                Resources = new List<Resource>
                {
                    //Employee #1
                    new Resource
                    {
                        //Make sure you give every resource a unique Id. (keep a hold of it, you need to use it later to map the planning back to your own structure)
                        Id = Guid.NewGuid(),

                        //Every moment a resource is availible for planning must be added as a Window.
                        Windows = new List<Window>
                        {
                            //This employee is availible on 3 specific dates from 9:00 to 17:00
                            new Window(new DateTime(2000,1,1, 9,0,0), TimeSpan.FromHours(8)),
                            new Window(new DateTime(2000,1,2, 9,0,0), TimeSpan.FromHours(8)),
                            new Window(new DateTime(2000,1,3, 9,0,0), TimeSpan.FromHours(8)),
                        },

                        //Employee is classified to operate a Flux Capacitor 9000
                        Qualifications = new List<ResourceQualification>
                        {
                           FluxCapacitor9000
                        },

                        //Employee starts every day from his home in Haarlem
                        StartLocation = HaarlemCity
                    }
                },

                //Create planning for January (1th to 31th)
                PlanningWindow = new Window(new DateTime(2000, 1, 1, 8, 0, 0), TimeSpan.FromDays(31)),

                //These items need to be planned
                PendingPlanItems = new List<PlanItem>
                {
                    //Something to be planned
                    new PlanItem
                    {
                        Id = Guid.NewGuid(),

                        //For 1 hour
                        Duration = TimeSpan.FromHours(1),

                        //On
                        WindowsConfigs = new List<WindowConfig>
                        {
                            new WindowConfig
                            {
                                //Every monday
                                DayOfWeek = DayOfWeek.Monday,

                                //Between 8:00 and 20:00
                                Start = new TimeSpan(08,00,00),
                                End   = new TimeSpan(20,00,00)
                            }
                        },

                        //At Haarlem location
                        Location = HaarlemCity,

                        //Employees that can be assigned to this item must be able to operate a Flux Capacitor
                        ResourceQualifications = new List<ResourceQualification>
                        {
                            FluxCapacitor9000
                        }
                    }
                }
            };

            PlanningTask planning = await client.Plan(request);

            if (planning.Status == PlanningTaskStatus.Success)
            {
                Console.WriteLine("Planned successfully:");
                foreach (var planned in planning.Planning.PlannedItems)
                {
                    Console.WriteLine($"{planned.Item.Id} on {planned.Window.Start}");
                }

                Console.WriteLine("");

                Console.WriteLine("Not planned:");
                foreach (var notPlanned in planning.Planning.FailedToPlanItems)
                {
                    Console.WriteLine($"{notPlanned.Item.Id} ({notPlanned.FailedReasonText})");
                }
            }
            else
            {
                Console.WriteLine($"Whoops! something went wrong: {planning.Status}");
                Console.WriteLine(planning.Exception.Message);
            }
        }

        static Location HaarlemCity { get; } = new Location()
        {
            Name = "Haarlem", Latitude = 52.377639, Longitude = 4.642735
        };
        static ResourceQualification FluxCapacitor9000 { get; } = new ResourceQualification() 
        {
            Id = Guid.NewGuid()
        };
    }
}
 