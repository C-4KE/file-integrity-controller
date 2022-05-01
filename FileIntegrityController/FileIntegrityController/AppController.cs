using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileIntegrityController
{
    /**
     * <summary>Класс, управляющий ходом выполнения программы.</summary>
     */
    public class AppController
    {
        static private string _jsonPath = "";

        /**
         * <summary>Метод, управляющий ходом работы программы.</summary>
         */
        async public static void ManageApp()
        {
            // Получение данных из JSON файла
            Console.WriteLine("Program has started.");
            Console.WriteLine("Parsing JSON...");
            Dictionary<string, string> filesHashes = Parser.ParseJSON(_jsonPath);
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
    }
}
