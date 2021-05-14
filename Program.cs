using System;
using System.Net;
using System.Threading;
using Newtonsoft.Json;


namespace GasNotifier
{
    class Program
    {
        protected static int origRow;
        protected static int origCol;
        protected static void WriteAt(string s, int x, int y)
        {
            try
            {
                Console.SetCursorPosition(origCol + x, origRow + y);
                Console.Write(s);
            }
            catch(Exception e)
            {
                Console.WriteLine("\n \n \n");
                Console.WriteLine("Encountered exception while updating values, press enter to exit...");
                Console.ReadLine();
                System.Environment.Exit(1);
            }
        }

        protected static string getAPIData(string p)
        {
            try
            {
                var myClient = new WebClient();
                string response = myClient.DownloadString(p);
                return response;
            } catch (Exception e)
            {
                if (p == "https://api.coinbase.com/v2/prices/ETH-EUR/spot")
                {
                    return "{\"data\":{\"base\":\"ETH\",\"currency\":\"EUR\",\"amount\":\"-1\"}}";
                } else if(p == "https://www.ethgasstation.info/json/ethgasAPI.json")
                {
                    return "{\"fast\": -10, \"fastest\": -10, \"safeLow\": -10, \"average\": -10}";
                } else
                {
                    return "{}";
                }
            }

        }
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing GasNotifier...");

            //Some variables
            string response;
            string etherPriceAPI;
            int averagePrice;
            int highPrice;
            int lowPrice;
            double etherPrice;
            double oldEtherPrice;
            int apiCooldown = 0;

            string[] cui = {"Low Price:", "Mid Price:", "Top Price:\n", "Buy?:\n", "ETH Value:", "0.05 ETH Value:\n",};

            //Testing whether a connection works, if not, it exits
            try
            {
                Console.WriteLine("Trying connection to gas station API...");
                response = getAPIData("https://www.ethgasstation.info/json/ethgasAPI.json");
                Console.WriteLine("Got response : " + response);

                Console.WriteLine("Trying connection to coinbase API...");
                etherPriceAPI = getAPIData("https://api.coinbase.com/v2/prices/ETH-EUR/spot");
                Console.WriteLine("Got response : " + etherPriceAPI);
            } catch(Exception e)
            {
                Console.WriteLine("Error while testing connection, exiting...");
                System.Environment.Exit(0);
            }

            Console.WriteLine("Ready! Starting in 5 secs...");
            Thread.Sleep(5000);
            Console.Clear();

            //Print everything
            foreach(string s in cui)
            {
                Console.WriteLine(s);
            }

            //Getting base price at startup
            etherPriceAPI = getAPIData("https://api.coinbase.com/v2/prices/ETH-EUR/spot");
            dynamic etherPriceJson = JsonConvert.DeserializeObject(etherPriceAPI);
            oldEtherPrice = etherPriceJson.data.amount;

            //
            //  MAIN LOOP 
            //
            while (true)
            {

                response = getAPIData("https://www.ethgasstation.info/json/ethgasAPI.json");
                if(apiCooldown == 7)
                {
                    etherPriceAPI = getAPIData("https://api.coinbase.com/v2/prices/ETH-EUR/spot");
                    apiCooldown = 0;
                }
                
                dynamic responseJson = JsonConvert.DeserializeObject(response);
                etherPriceJson = JsonConvert.DeserializeObject(etherPriceAPI);
                etherPrice = etherPriceJson.data.amount;

                //Converting to gwei.
                averagePrice = responseJson.fast / 10;
                highPrice = responseJson.fastest / 10;
                lowPrice = responseJson.safeLow / 10;

                WriteAt(lowPrice.ToString() + " gwei  ", 11, 0);
                WriteAt(averagePrice.ToString() + " gwei  ", 11, 1);
                WriteAt(highPrice.ToString() + " gwei  ", 11, 2);

                WriteAt(etherPrice.ToString() + "E       ", 11, 6);
                WriteAt("(" + Math.Round((etherPrice - oldEtherPrice)).ToString() + "E)   ", 20, 6);
                WriteAt(Math.Round((etherPrice * 0.05)).ToString() + "E   ", 16, 7);

                if(highPrice <= 100 || averagePrice <= 75 || lowPrice <= 65)
                {
                    WriteAt("YES", 6, 4);
                } else
                {
                    WriteAt("NO   ", 6, 4);
                }

                apiCooldown++;
                Thread.Sleep(8000);
            }
        }
    }
}