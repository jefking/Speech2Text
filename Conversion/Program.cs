namespace Conversion
{
    using System;
    using Microsoft.CognitiveServices.SpeechRecognition;
    using System.IO;
    using System.Threading;

    class Program
    {
        private static bool done;

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
                // Event handlers for speech recognition results
                dataClient.OnResponseReceived += OnDataDictationResponseReceivedHandler;
                dataClient.OnPartialResponseReceived += OnPartialResponseReceivedHandler;
                dataClient.OnConversationError += OnConversationErrorHandler;

                using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    Console.Write("Processing File");

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
                            Console.Write(".");

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

                    Console.WriteLine();

                    Console.Write("Waiting for response");
                    while (!done)
                    {
                        Console.Write(".");

                        Thread.Sleep(250);
                    }
                }
            }
        }


        /// <summary>
        /// Called when a final response is received;
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SpeechResponseEventArgs"/> instance containing the event data.</param>
        private static void OnDataDictationResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            Console.WriteLine("--- OnDataDictationResponseReceivedHandler ---");
            if (e.PhraseResponse.RecognitionStatus == RecognitionStatus.EndOfDictation ||
                e.PhraseResponse.RecognitionStatus == RecognitionStatus.DictationEndSilenceTimeout)
            {
                Console.WriteLine("Completed");
            }

            WriteResponseResult(e);
        }

        /// <summary>
        /// Writes the response result.
        /// </summary>
        /// <param name="e">The <see cref="SpeechResponseEventArgs"/> instance containing the event data.</param>
        private static void WriteResponseResult(SpeechResponseEventArgs e)
        {
            if (e.PhraseResponse.Results.Length == 0)
            {
                Console.WriteLine("No phrase response is available.");
            }
            else
            {
                Console.WriteLine("********* Final n-BEST Results *********");
                for (int i = 0; i < e.PhraseResponse.Results.Length; i++)
                {
                    Console.WriteLine(
                        "[{0}] Confidence={1}, Text=\"{2}\"",
                        i,
                        e.PhraseResponse.Results[i].Confidence,
                        e.PhraseResponse.Results[i].DisplayText);
                }

                Console.WriteLine();
            }

            done = true;
        }

        /// <summary>
        /// Called when a final response is received and its intent is parsed
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SpeechIntentEventArgs"/> instance containing the event data.</param>
        private static void OnIntentHandler(object sender, SpeechIntentEventArgs e)
        {
            Console.WriteLine("--- Intent received by OnIntentHandler() ---");
            Console.WriteLine("{0}", e.Payload);
            Console.WriteLine();
        }

        /// <summary>
        /// Called when a partial response is received.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PartialSpeechResponseEventArgs"/> instance containing the event data.</param>
        private static void OnPartialResponseReceivedHandler(object sender, PartialSpeechResponseEventArgs e)
        {
            Console.WriteLine("--- Partial result received by OnPartialResponseReceivedHandler() ---");
            Console.WriteLine("{0}", e.PartialResult);
            Console.WriteLine();
            done = true;
        }

        /// <summary>
        /// Called when an error is received.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SpeechErrorEventArgs"/> instance containing the event data.</param>
        private static void OnConversationErrorHandler(object sender, SpeechErrorEventArgs e)
        {
            Console.WriteLine("--- Error received by OnConversationErrorHandler() ---");
            Console.WriteLine("Error code: {0}", e.SpeechErrorCode.ToString());
            Console.WriteLine("Error text: {0}", e.SpeechErrorText);
            Console.WriteLine();
            done = true;
        }
    }
}