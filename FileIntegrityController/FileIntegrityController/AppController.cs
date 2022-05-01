using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;

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
        async public static void ManageApp()
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

                    // Подготовка к запуску проверки
                    StorageInfo storageInfo = new StorageInfo();
                    List<string>[] invalidFiles = new List<string>[fileGroups.Length];
                    Task<List<string>>[] tasks = new Task<List<string>>[fileGroups.Length];

                    Console.WriteLine("Starting integrity verifying...");
                    // Запуск проверки
                    for (int i = 0; i < fileGroups.Length; i++)
                    {
                        tasks[i] = Distributor.DistributeAsync(fileGroups[i], storageInfo);
                    }
                    for (int i = 0; i < fileGroups.Length; i++)
                    {
                        invalidFiles[i] = await tasks[i];
                    }

                    // Вывод результата проверки на экран
                    PrintResult(fileGroups, invalidFiles);
                }
                else
                {
                    Console.WriteLine("There are no files to check.");
                }
            }
        }

        /**
         * <summary>Метод, выводящий результат проверки на целостность.</summary>
         * <param name="fileGroups">Массив проверявшихся групп файлов.</param>
         * <param name="invalidFiles">Массив списков изменённых файлов, в котором каждый список по индексу соответствует индексу в fileGroups.</param>
         */
        public static void PrintResult(FileGroup[] fileGroups, List<string>[] invalidFiles)
        {
            if (fileGroups.Length == invalidFiles.Length)
            {
                Console.WriteLine("");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Results: ");
                Console.ResetColor();
                Console.WriteLine("");
                for (int i = 0; i < fileGroups.Length; i++)
                {
                    Console.WriteLine($"Disk: {fileGroups[i].DiskName}");
                    Console.WriteLine($"{fileGroups[i].FilesHashes.Count - invalidFiles[i].Count} of {fileGroups[i].FilesHashes.Count} files valid.");
                    if (invalidFiles[i].Count != 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid files: ");
                        Console.ResetColor();
                        foreach (string file in invalidFiles[i])
                        {
                            Console.WriteLine(file);
                        }
                    }
                    Console.WriteLine("");
                }
            }
        }

        /**
         * <summary>Метод, считывающий путь к JSON файлу из config файла.</summary>
         * <returns>Путь к JSON файлу, если чтение произошло успешно и JSON файл существует. Иначе - возвращает null.</returns>
         */
        public static string ReadConfig()
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
    }
}
