using TaskPlanning.Client;
using TaskPlanning.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TaskPlanning.Client.Sample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("--------------------------------------");
            Console.WriteLine("| TaskPlanning.Client.Sample Console |");
            Console.WriteLine("--------------------------------------");
            Console.WriteLine("");
            Console.WriteLine("0. Regular (plan until full)");
            Console.WriteLine("1. Options (show all availible options for a single planitem)");
            Console.WriteLine("");

            Console.Write(">");
            var key = Console.ReadKey();
            Console.WriteLine("");

            var mode = (PlanningMode)int.Parse(key.KeyChar.ToString());

            //Create your client using your private access key
            var accessKey = string.Empty;
            if (File.Exists("AccessKey.txt"))
                accessKey = File.ReadAllText("AccessKey.txt");
            
            TaskPlanningClient client;
            try
            {
                client = await TaskPlanningClient.Create(accessKey);
                client.PlanningTaskUpdated += Client_PlanningTaskUpdated;
            }
            catch (TaskPlanning.Client.InvalidAccessKeyException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            PlanningRequest request = GetPlanningRequest(mode);

            if (mode == PlanningMode.Options)
            {
                //When requesting options only one planitem is alowed.
                request.PendingPlanItems = new List<PlanItem> {
                    request.PendingPlanItems.First()
                };
                
                //In order to retrieve a list of possibilities in an existing planning you must provide
                //existing planned items per resource using the PrePlannedItems list
                var employee = request.Resources.First();
                employee.PrePlannedItems.Add(new PlanItemResult
                {
                    Item = new PlanItem { Duration = TimeSpan.FromHours(1), Location = HaarlemCity },
                    Window = new Window(new DateTime(2000, 1, 1, 9, 0, 0), TimeSpan.FromHours(1))
                });
            }

            PlanningTask planning = await client.Plan(request, TimeSpan.FromMilliseconds(500));

            if (planning.Status == PlanningTaskStatus.Success)
            {
                if (mode == PlanningMode.Regular)
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
                else if(mode == PlanningMode.Options)
                {
                    Console.WriteLine($"Possible options for {planning.Planning.PlannedItems.FirstOrDefault()?.Item.Id}");
                    foreach (var planned in planning.Planning.PlannedItems)
                    {
                        Console.WriteLine($"{planned.Window.Start}");
                    }
                }
            }
            else
            {
                Console.WriteLine($"Whoops! something went wrong: {planning.Status}");
                Console.WriteLine(planning.Exception.Message);
            }
        }

        private static void Client_PlanningTaskUpdated(object sender, TaskPlanningUpdateEventArgs e)
        {
            var progressString = e.PlanningTask.Progress.ToString().PadLeft(3, ' ');
            Console.WriteLine($"UPDATE\t Progress: {progressString}%\t Status: {e.PlanningTask.Status}");

            if(e.PlanningTask.Status == PlanningTaskStatus.Error)
                Console.WriteLine($"ERROR\t Exception:\n{e.PlanningTask.Exception}");
        }

        private static PlanningRequest GetPlanningRequest(PlanningMode mode)
        {
            var request = new PlanningRequest()
            {
                Mode = mode,

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

                //These items need to be planned (for Options mode only one plan item is allowed)
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
            return request;
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
 