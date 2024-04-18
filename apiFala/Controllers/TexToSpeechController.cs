using System.Reflection.Metadata;
using apiFala.Domain;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.Transcription;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace apiFala.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TexToSpeechController : ControllerBase
    {

        //setx SPEECH_KEY your-key
        //setx SPEECH_REGION your-region

        private readonly IWebHostEnvironment _webHostEnvironment;

        public TexToSpeechController(IWebHostEnvironment env)
        {
            _webHostEnvironment = env;
        }

        


        [HttpPost("TextToSpeech")]
        public async Task<IActionResult> Post(TextToSpeech mensagem)
        {
            // Obtem a chave que está na variavel de ambiente do sistema.
            string speechKey = Environment.GetEnvironmentVariable("SPEECH_KEY");

            // Obtem a regiao que está na variavel de ambiente do sistema.
            string speechRegion = Environment.GetEnvironmentVariable("SPEECH_REGION");
            // Acessa a raiz da api
            string pastaRaiz = _webHostEnvironment.ContentRootPath;
            // Combina a pasta raiz com a pasta Data e Audios.
            string pastaAudios = Path.Combine(pastaRaiz, "Data\\Audios");
            
            string arquivoWav = Path.Combine(pastaAudios, "file.wav");
            
            //  Configuração para selecionar o idioma e a voz
            var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);                        
            speechConfig.SpeechSynthesisLanguage = "pt-BR";

            // Caminho da pasta mais o nome do arquvio

            // Criado uma instancia de AudioConfig para gravar automaticamente a saída em um arquivo .wav usando a função FromWavFileOutput()
            using var audioConfig = AudioConfig.FromWavFileOutput(arquivoWav);

            //Crie uma instância SpeechSynthesizer com outra instrução using. Passe os objetos speechConfig e audioConfig como parâmetros. Para sintetizar a fala e gravar em um arquivo, execute SpeakTextAsync() com uma cadeia de caracteres de texto.
            using var speechSynthesizer = new SpeechSynthesizer(speechConfig, audioConfig);
            await speechSynthesizer.SpeakTextAsync(mensagem.Message);

            // Retorna o caminho do arquvio
            return Ok(arquivoWav);
            
        }
        
        [HttpPost("SpeechToText")]
        public async Task<IActionResult> Post(string textoeeeeeeeeeeeeeeeeeeee)
        {
            string speechKey = Environment.GetEnvironmentVariable("SPEECH_KEY");
            string speechRegion = Environment.GetEnvironmentVariable("SPEECH_REGION");
            //  Configuração para selecionar o idioma e a voz
            var speechConfig = SpeechConfig.FromSubscription(speechKey, speechRegion);
            speechConfig.SpeechRecognitionLanguage = "pt-BR";

            string arquivoAudio = "C:\\Users\\allan\\OneDrive\\Documentos\\Projetos\\apiFala\\apiFala\\Data\\Audios\\file.wav";
            string arquivoAudio2 = "C:\\Users\\allan\\OneDrive\\Documentos\\Gravações de som\\Gravando.wav";

            using var audioConfig = AudioConfig.FromWavFileInput(arquivoAudio2);

            using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

            var stopRecognition = new TaskCompletionSource<int>(); //



            // Make the following call at some point to stop recognition:
            // await speechRecognizer.StopContinuousRecognitionAsync();

            speechRecognizer.Recognizing += (s, e) =>
            {
                Console.WriteLine($"RECOGNIZING: Text={e.Result.Text}");
            };

            string mensagem = "";

            speechRecognizer.Recognized += (s, e) =>
            {

                if (e.Result.Reason == ResultReason.RecognizedSpeech)
                {
                    Console.WriteLine($"RECOGNIZED: Text={mensagem +=e.Result.Text}");
                }
                else if (e.Result.Reason == ResultReason.NoMatch)
                {
                    Console.WriteLine($"NOMATCH: Speech could not be recognized.");
                }
            };

            speechRecognizer.Canceled += (s, e) =>
            {
                Console.WriteLine($"CANCELED: Reason={e.Reason}");

                if (e.Reason == CancellationReason.Error)
                {
                    Console.WriteLine($"CANCELED: ErrorCode={e.ErrorCode}");
                    Console.WriteLine($"CANCELED: ErrorDetails={e.ErrorDetails}");
                    Console.WriteLine($"CANCELED: Did you set the speech resource key and region values?");
                }

                stopRecognition.TrySetResult(0);
            };

            speechRecognizer.SessionStopped += (s, e) =>
            {
                Console.WriteLine("\n    Session stopped event.");
                stopRecognition.TrySetResult(0);
            };


            await speechRecognizer.StartContinuousRecognitionAsync();

            // Waits for completion. Use Task.WaitAny to keep the task rooted.
            Task.WaitAny(new[] { stopRecognition.Task });
            return Ok(mensagem);
        }
    }
 }
