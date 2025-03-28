using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace GeradorDeSequencia
{
    class Program
    {
        private static readonly object FileLock = new object();
        private static readonly HttpClient HttpClient = new HttpClient(new HttpClientHandler
        {
            MaxConnectionsPerServer = 100 // requisicoes simultaneas
        });
        private static readonly ThreadLocal<Random> ThreadRandom = new ThreadLocal<Random>(() => new Random());
        private static readonly ConcurrentQueue<string> TentativasQueue = new ConcurrentQueue<string>();
        private static readonly ConcurrentQueue<string> ChavesQueue = new ConcurrentQueue<string>();
        private static readonly ConcurrentDictionary<string, bool> ChavesUsadas = new ConcurrentDictionary<string, bool>();

        static async Task Main(string[] args)
        {
            var pastaTentativas = @""; //<- - Coloque o caminho da pasta onde deseja salvar os arquivos
            var arquivoTentativas = Path.Combine(pastaTentativas, "tentativas.txt"); //<- - Nome do arquivo de tentativas
            var pastaChaves = @""; //<- - Coloque o caminho da pasta onde deseja salvar os arquivos
            var arquivoChaves = Path.Combine(pastaChaves, "chavesUsadas.txt"); //<- - Nome do arquivo de chaves

            // monta a pasta se não existir
            Directory.CreateDirectory(pastaTentativas);
            Directory.CreateDirectory(pastaChaves);

            // Ve as chaves que ja tentou, pra nao repetir
            if (File.Exists(arquivoChaves))
            {
                foreach (var chave in File.ReadLines(arquivoChaves))
                {
                    ChavesUsadas[chave] = true;
                }
            }

            bool sucesso = false;

            // Grava tentativas e chaves em lote, de tempo em tempo
            GravarTentativasPeriodicamente(arquivoTentativas, arquivoChaves);

            while (!sucesso)
            {
                var tasks = new List<Task<(string sequencia, string resultado)>>();

                for (int i = 0; i < 20; i++) // Multitask 20 tarefas paralelas, foi o maximo que eu vi sem perder performance, talvez meu pc que seja fraco, tente mais ou menos, de acordo com seu computador
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        string sequencia;
                        do
                        {
                            sequencia = GerarSequenciaAleatoria();
                        } while (!ChavesUsadas.TryAdd(sequencia, true)); // Thread-safe precaução para não repetir chaves

                        var resultado = await EnviarSequenciaParaApiLocal(sequencia);
                        return resultado;
                    }));
                }

                var resultados = await Task.WhenAll(tasks);

                foreach (var (sequencia, resultado) in resultados)
                {
                    // Enfileira as tentativas e chaves para gravação em lote, assim fica mais facil de ver os registros
                    TentativasQueue.Enqueue($"Tentativa: {sequencia} - Resultado: {resultado}{Environment.NewLine}");
                    ChavesQueue.Enqueue($"{sequencia}{Environment.NewLine}");

                    if (resultado.Contains("Sucesso", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"Sequência correta encontrada: {sequencia} {DateTime.Now}");
                        sucesso = true;
                        break;
                    }
                    else
                    {
                        Console.WriteLine($"Tentativa {sequencia} falhou. Retorno da API: {resultado}. Continuando...");
                    }
                }
            }
        }

        static string GerarSequenciaAleatoria()
        {
            var upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var lowerChars = "abcdefghijklmnopqrstuvwxyz";
            var digits = "0123456789";

            char[] sequencia = new char[4];
            var random = ThreadRandom.Value;

            for (int i = 0; i < 2; i++)
            {
                if (random.Next(2) == 0)
                {
                    sequencia[i] = upperChars[random.Next(upperChars.Length)];
                }
                else
                {
                    sequencia[i] = lowerChars[random.Next(lowerChars.Length)];
                }
            }

            for (int i = 2; i < 4; i++)
            {
                sequencia[i] = digits[random.Next(digits.Length)];
            }

            return new string(sequencia.OrderBy(_ => random.Next()).ToArray());
        }

        static async Task<(string sequencia, string resultado)> EnviarSequenciaParaApiLocal(string sequencia)
        {
            var url = ""; //Bota url da tua api local ou a que os professores disponibilizarem.
            var grupo = ""; // <- - Coloque o seu grupo.

            var dados = new
            {
                Key = sequencia,
                grupo
            };

            var json = JsonConvert.SerializeObject(dados);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await HttpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                var resultado = await response.Content.ReadAsStringAsync();
                return (sequencia, resultado);
            }
            catch (HttpRequestException ex)
            {
                return (sequencia, $"Erro ao enviar sequência: {ex.Message}");
            }
        }

        private static void GravarTentativasPeriodicamente(string arquivoTentativas, string arquivoChaves)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000); // grava a cada 1 segundo

                    if (!TentativasQueue.IsEmpty || !ChavesQueue.IsEmpty)
                    {
                        lock (FileLock)
                        {
                            while (TentativasQueue.TryDequeue(out var tentativa))
                            {
                                File.AppendAllText(arquivoTentativas, tentativa);
                            }

                            while (ChavesQueue.TryDequeue(out var chave))
                            {
                                File.AppendAllText(arquivoChaves, chave);
                            }
                        }
                    }
                }
            });
        }
    }
}
