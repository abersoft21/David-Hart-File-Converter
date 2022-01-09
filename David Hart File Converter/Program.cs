using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Text.RegularExpressions;

namespace David_Hart_File_Converter
{
    class Program
    {
        static void Main(string[] args)
        {
            ShowDialog();

            CommandHandler();

            System.Threading.Thread.Sleep(4000);
        }

        private static void CommandHandler()
        {
            throw new NotImplementedException();
        }

        private static void ShowDialog()
        {
            string version = ConfigurationManager.AppSettings["Version"];
            bool running = true;

            // set up prompts to get the file, get help menu and exit application
            //Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Convert CSV files to JSON and XML formats version {version}.");
            Console.WriteLine("Enter /h or help for further options, /c or close to close.");
            Console.WriteLine();
            Console.WriteLine("Enter the full filename path to convert");


            while (running)
            {
                string line = Console.ReadLine();

                switch (line.ToLower())
                {
                    case "/h": case "help/":
                        HelpDialog();
                    break;

                    case "/c" : case "close":
                        System.Environment.Exit(0);
                    break;
                    // other commands can be watched here

                    default:
                        string msg = Convert.PushCommand(line);
                        Console.WriteLine(msg);

                    break;
                }
            }

        }

        

        private static void HelpDialog()
        {
            Console.WriteLine();
            Console.WriteLine("You can enter a full path e.g. \"c:/Users/tech/desktop/AddressBookExport.csv\".");
            Console.WriteLine("");
        }
    }
}
