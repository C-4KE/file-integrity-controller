using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks.Dataflow;
using System.Threading.Tasks;

namespace FileIntegrityController
{
    /**
     * <summary>Класс, управляющий ходом выполнения программы.</summary>
     */
    public class AppController
    {
        /**
         * <summary>Метод, управляющий ходом работы программы.</summary>
         */
        public static void ManageApp()
        {
            Console.WriteLine("Program has started.");
            // Получение адреса JSON файла из config файла.
            Console.WriteLine("Reading a path to JSON file from config...");
            string jsonPath = ReadConfig();
            if (jsonPath != null)
            {
                // Получение данных из JSON файла
                Console.WriteLine("Parsing JSON...");
                Dictionary<string, string> filesHashes = Parser.ParseJSON(jsonPath);
                if (filesHashes.Count != 0)
                {
                    Console.WriteLine("Sorting files by disks...");
                    FileGroup[] fileGroups = Parser.SortFilesByDisks(filesHashes);

                    // Создание блока Dataflow, который будет читать потребитель, и к которому будут подключены блоки производителей
                    BufferBlock<Task> consumerBuffer = new BufferBlock<Task>();

                    // Создание производителей
                    Producer[] producers = new Producer[fileGroups.Length];
                    for (int i = 0; i < producers.Length; i++)
                    {
                        producers[i] = new Producer(fileGroups[i], consumerBuffer);
                    }

                    // Создание потребителя
                    List<BufferBlock<Task>> producerBuffers = new List<BufferBlock<Task>>();
                    for (int i = 0; i < producers.Length; i++)
                    {
                        producerBuffers.Add(producers[i].ProducerBuffer);
                    }
                    Consumer consumer = new Consumer(consumerBuffer, producerBuffers);

                    // Запуск потребителя
                    Task<Dictionary<string, bool>>[] producerTasks = new Task<Dictionary<string, bool>>[producers.Length];
                    Task consumerTask = Task.Run(() => consumer.Execute());

                    // Запуск производителей
                    for (int i = 0; i < producerTasks.Length; i++)
                    {
                        var localProducer = producers[i];
                        producerTasks[i] = Task<Dictionary<string, bool>>.Run(() => localProducer.Execute());
                    }

                    // Ожидание результата
                    consumerTask.Wait();
                    Task.WaitAll(producerTasks);

                    // Вывод результата
                    Dictionary<string, bool> result = GetResultFromTasks(producerTasks);
                    PrintResult(result);
                }
            }
            else
            {
                Console.WriteLine("There are no files to check.");
            }
        }

        /**
         * <summary>Метод, считывающий путь к JSON файлу из config файла.</summary>
         * <returns>Путь к JSON файлу, если чтение произошло успешно и JSON файл существует. Иначе - возвращает null.</returns>
         */
        private static string ReadConfig()
        {
            string jsonPath = ConfigurationManager.AppSettings.Get("JSONPath");
            if (jsonPath == null)
            {
                Console.WriteLine("Program has failed to read a path to JSON file.");
            }
            else
            {
                if (!File.Exists(jsonPath))
                {
                    Console.WriteLine("File \"" + jsonPath + "\" does not exist.");
                    jsonPath = null;
                }
            }
            return jsonPath;
        }

        /**
         * <summary>Метод, достающий результаты проверки (пары (путь_к_файлу : хэш)) из заданий.</summary>
         * <param name="tasks">Массив заданий, запускавших производителей.</param>
         * <returns>Словарь пар (путь_к_файлу : хэш) всех проверенных файлов.</returns>
         */
        private static Dictionary<string, bool> GetResultFromTasks(Task[] tasks)
        {
            Dictionary<string, bool> result = new Dictionary<string, bool>();
            for (int i = 0; i < tasks.Length; i++)
            {
                foreach (KeyValuePair<string, bool> pair in ((Task<Dictionary<string, bool>>)tasks[i]).Result)
                {
                    result.Add(pair.Key, pair.Value);
                }
            }
            return result;
        }

        /**
         * <summary>Метод, выводящий результат работы программы.</summary>
         * <param name="checkResult">Словарь всех проверенных файлов.</param>
         */
        private static void PrintResult(Dictionary<string, bool> checkResult)
        {
            if (checkResult.Count != 0)
            {
                int correctCount = 0;
                foreach (KeyValuePair<string, bool> pair in checkResult)
                {
                    if (pair.Value)
                        correctCount++;
                }
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Results: ");
                Console.ResetColor();
                Console.WriteLine($"{checkResult.Count} files were checked.");
                Console.WriteLine($"{correctCount} out of {checkResult.Count} files are not changed.");
                if (correctCount != checkResult.Count)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid files: ");
                    Console.ResetColor();
                    foreach (KeyValuePair<string, bool> pair in checkResult)
                    {
                        if (!pair.Value)
                        {
                            Console.WriteLine(pair.Key);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("No one file wasn\'t checked.");
            }
        }
    }
}
