using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace FileIntegrityController
{
    /**
     * <summary>Класс, считывающий пары (имя_файла : хеш) из Json файла, а также сортирующий пары по дискам.</summary>
     */
    public class Parser
    {
        /**
         * <summary>Метод, который считывает пары (имя_файла : хеш) из JSON'а.</summary>
         * <param name="jsonPath">Путь к JSON файлу.</param>
         * <returns>Словарь с парами (имя_файла : хеш).</returns>
         */
        public static Dictionary<string, string> ParseJSON(string jsonPath)
        {
            Dictionary<string, string> fileHash;
            if (File.Exists(jsonPath))
            {
                string jsonString = File.ReadAllText(jsonPath);
                if (jsonString.Length != 0)
                {
                    try
                    {
                        fileHash = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine("Failed to deserialize Json string: " + exc.Message);
                        fileHash = null;
                    }
                }
                else
                {
                    Console.WriteLine("There are no <file_name : hash> pairs in Json file.");
                    fileHash = null;
                }
            }
            else
            {
                Console.WriteLine("File \"" + jsonPath + "\" does not exist.");
                fileHash = null;
            }
            return fileHash;
        }
    }
}
