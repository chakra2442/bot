using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using R3DD17;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebAppCoreMVC.Models;

namespace WebAppCoreMVC.Controllers
{
    public partial class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly IConfiguration config;
        private IAzureTableStorage<Stats> storageStats;
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.config = configuration;
        }

        public IActionResult Index(bool useVader = true)
        {
            //var x = new List<Person>();
            //var rand = new Random();
            //for (int i = 0; i < 100; i++)
            //{
            //    x.Add(new Person() { Name = rand.Next().ToString(), Age = rand.Next() });
            //}


            var srcTableName = "AggTableName";
            if (useVader)
            {
                srcTableName = "AggTableNameVader";
            }
            var settingsStats = new AzureTableSettings(this.config["ConnectionString"], this.config[srcTableName]);
            this.storageStats = new AzureTableStorage<Stats>(settingsStats);
            ViewBag.DataSource = this.storageStats.GetList().Result.OrderByDescending(x => x.Score);
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
