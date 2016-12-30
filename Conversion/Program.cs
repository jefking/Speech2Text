namespace Conversion
{
    using System;
    using Microsoft.CognitiveServices.SpeechRecognition;
    using System.IO;

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

            var defaultLocale = "en-US";
            var mode = SpeechRecognitionMode.ShortPhrase;

            using (var dataClient = SpeechRecognitionServiceFactory.CreateDataClient(mode, defaultLocale, key))
            {
                using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    // Note for wave files, we can just send data from the file right to the server.
                    // In the case you are not an audio file in wave format, and instead you have just
                    // raw data (for example audio coming over bluetooth), then before sending up any 
                    // audio data, you must first send up an SpeechAudioFormat descriptor to describe 
                    // the layout and format of your raw audio data via DataRecognitionClient's sendAudioFormat() method.
                    int bytesRead = 0;
                    byte[] buffer = new byte[1024];

                    try
                    {
                        do
                        {
                            // Get more Audio data to send into byte buffer.
                            bytesRead = fileStream.Read(buffer, 0, buffer.Length);

                            // Send of audio data to service. 
                            dataClient.SendAudio(buffer, bytesRead);
                        }
                        while (bytesRead > 0);
                    }
                    finally
                    {
                        // We are done sending audio.  Final recognition results will arrive in OnResponseReceived event call.
                        dataClient.EndAudio();
                    }
                }
            }
        }
    }
}