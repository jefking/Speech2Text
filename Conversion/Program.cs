namespace Conversion
{
    using System;
    using Microsoft.CognitiveServices.SpeechRecognition;

    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Please enter your subscription key: ");
            var key = Console.ReadLine();
            Console.WriteLine("Key provided is: {0}", key);


            Console.Write("Please provide file: ");
            var file = Console.ReadLine();
            Console.WriteLine("File provided is: {0}", file);
        }
    }
}