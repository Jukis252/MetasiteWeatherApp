using Metasite_v2.Classes; //usage of classes
using Newtonsoft.Json;  //Package for Deserialazation
using RestSharp;    // Package for accessing API , receiving data from API
using System;       // Built in
using System.Threading; // Library for enabeling Thread class
using Serilog; // Package for logging

namespace Metasite_v2
{
    public class Program // class for sending and receiving data
    {
        public string token; // to save received bearer key
        public string[] cities; // to save received cities
        public void PostAuthentication() // function to send username and password to get access
        {
            string PostUrl = "https://metasite-weather-api.herokuapp.com/api/authorize"; // URL of authorization
            Login body = new Login { Username = "meta", Password = "site" }; // created new object with login credentials
            RestClient restClient = new RestClient(); // created new object for specifying URL
            restClient.BaseUrl = new Uri(PostUrl); // Specifying clients URL
            RestRequest restRequest = new RestRequest(Method.POST); //created new object for sending request(Method.POST)
            restRequest.AddJsonBody(body);// added JsonBody to be able to sent login credentials for confirmation
            IRestResponse restResponse = restClient.Execute(restRequest); // new object to get response from API
            //Console.WriteLine("Status code: " + restResponse.StatusCode); //Testing received status code
            //Console.WriteLine("Status message " + restResponse.Content); // Testing received content
            string result = restResponse.Content; // assigning received content to more "work-comfortable" form
            Reply reply = JsonConvert.DeserializeObject<Reply>(result); // iniciating deserialazation
            token = reply.Bearer.ToString(); //assigning key to public variable
        }
        public string[] GetCities() // function to receive available cities
        {
            string GetCitiesUrl = "https://metasite-weather-api.herokuapp.com/api/Cities"; // URL of cities
            RestClient restClient = new RestClient(); // created new object for specifying URL
            restClient.BaseUrl = new Uri(GetCitiesUrl); // Specifying clients URL
            RestRequest restRequest = new RestRequest(Method.GET); //created new object for sending request(Method.GET)
            restRequest.AddHeader("Authorization", "bearer " + token); //sending token to get authorized
            //Console.WriteLine("bearer " + token); // Testing sent token
            restRequest.AddHeader("accept", "text/plain"); // specifying format of received content
            IRestResponse restResponse = restClient.Execute(restRequest); // new object to get response from API
            //Console.WriteLine("Status code: " + restResponse.StatusCode); //Testing received status code
            //Console.WriteLine("Status message " + restResponse.Content); // Testing received content
            cities = restResponse.Content.Split(",");  // spliting  received content to strings and assigning them to list
            return cities; //receiving list of string cities
        }
        public void GetCitiesWeather(string city) // function to receive weather of specific city
        {
            string GetCitiesWeatherUrl = "https://metasite-weather-api.herokuapp.com/api/Weather";// URL of weather
            RestClient restClient = new RestClient(); // created new object for specifying URL
            restClient.BaseUrl = new Uri(GetCitiesWeatherUrl+"/"+city);// Specifying clients URL
            RestRequest restRequest = new RestRequest(Method.GET);//created new object for sending request(Method.GET)
            restRequest.AddHeader("Authorization", "bearer " + token);//sending token to get authorized
            restRequest.AddHeader("city", city);// sending city name which weather want to receive
            IRestResponse restResponse = restClient.Execute(restRequest); //new object to get response from API
            //Console.WriteLine("Status code: " + restResponse.StatusCode); //Testing received status code
            //Console.WriteLine("Status message " + restResponse.Content); // Testing received content
            string result = restResponse.Content; // assigning received content to more "work-comfortable" form
            Weather weather = JsonConvert.DeserializeObject<Weather>(result); // iniciating deserialazation
            Log.Information("City name: " + weather.city); //output to console and file of city's name
            Log.Information("Temperature: "+ weather.temperature); //output to console and file of city's temperature
            Log.Information("Precipitation: "+ weather.precipitation); //output to console and file of city's precipitation
            Log.Information("Weather: "+ weather.weather); //output to console and file of city's weather
        }
        static void Main(string[] args) // main function
        {
            Log.Logger = new LoggerConfiguration() // Serilog initialization
                .MinimumLevel.Debug() // ability to debug
                .WriteTo.Console() // output to console
                .WriteTo.File("C:/Users/JustinasPlauškaTechn/source/repos/Metasite_v2/Metasite_v2/Logs/myapp.txt", rollingInterval: RollingInterval.Day) // output to file
                .CreateLogger(); // creates a logger

            Log.Information("Logging started"); // Start of task
            Log.Information(" "); // empty line for visual purposes

            bool engine = true; // bool for infinite cycle
            while(engine == true) // start of cycle
            {
                int counter = 2; // counter for invalid arguments
                Program program = new Program(); // creating object of main class Program
                program.PostAuthentication(); // initializing key receiving function
                string[] cities = program.GetCities(); // assigning available cities to public array of strings.
                for (int i = 0; i < cities.Length; i++) // cycle for cleaning strings of unwanted symbols
                {
                    cities[i] = cities[i].Trim('"', '[', ']');
                }
                if (args[0] == "weather") //checking which task is called
                {
                    for(int i=2;i<args.Length;i++) // cycle for completing task
                    {
                        args[i] = args[i].Trim(' ', ',', '.'); // cleaning arguments of unwanted symbols
                        if (string.Concat(cities).Contains(args[i])) // searching if array of cities contains requested city
                        {
                            program.GetCitiesWeather(args[i]); //output the weather
                            Log.Information(" "); // empty line for visual purposes
                        }
                        else // if city is not found
                        {
                            Log.Information("City " + args[i] + " is not available");  // letting user to know that city is unavailable
                            Log.Information(" "); // empty line for visual purposes
                            counter++; // adding not available city
                        }
                        if(counter>=args.Length) //if no city is available to show weather forecast
                        {
                            Log.Information("No available arguments. Program will exit now");
                            Thread.Sleep(5000);
                            System.Environment.Exit(0);
                        }
                    }

                }
                else // if not correct task is called 
                {
                    Log.Information("Task is invalid. Program will exit now");
                    Thread.Sleep(5000);
                    System.Environment.Exit(0);
                }
                Log.Information("To stop program press CTRL + C");
                Log.Information(" ");
                Thread.Sleep(30000); // 30 sec delay, before new cycle.
            }
            Log.CloseAndFlush();
        }
    }
}
