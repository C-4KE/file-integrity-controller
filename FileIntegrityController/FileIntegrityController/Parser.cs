using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace FileIntegrityController
{
    /**
     * <summary>Класс, считывающий пары (имя_файла : хэш) из Json файла, а также сортирующий пары по дискам.</summary>
     */
    public class Parser
    {
        /**
         * <summary>Метод, который считывает пары (имя_файла : хэш) из JSON'а.</summary>
         * <param name="jsonPath">Путь к JSON файлу.</param>
         * <returns>Словарь с парами (имя_файла : хэш).</returns>
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

        /**
         * <summary>Метод, сортирующий словарь с парами (имя_файла : хэш) по дискам.</summary>
         * <remarks>Если входной словарь не пустой, но содержит пары с несуществующими файлами / некорректными ключами, то онине добавляются в выходной массив.</remarks>
         * <param name="filesHashes">Словарь с парами (имя_файла : хэш).</param>
         * <returns>Возвращает массив объектов FileGroup, каждый из которых хранит информацию о файлах с одного диска. Если входной словарь пустой, возвращает null.</returns>
         */
        public static FileGroup[] SortFilesByDisks(Dictionary<string, string> filesHashes)
        {
            if (filesHashes.Count != 0)
            {
                FileGroup[] fileGroups = null;
                List<string> disks = new List<string>();
                foreach (KeyValuePair<string, string> fileHash in filesHashes)
                {
                    try
                    {
                        DriveInfo driveInfo = GetDriveInfo(fileHash.Key);
                        if (driveInfo != null)
                        {
                            string drive = driveInfo.Name;
                            if (disks.Contains(drive))
                            {
                                foreach (FileGroup fileGroup in fileGroups)
                                {
                                    if (fileGroup.DiskName == drive)
                                    {
                                        fileGroup.FilesHashes.Add(fileHash.Key, fileHash.Value);
                                    }
                                }
                            }
                            else
                            {
                                disks.Add(drive);
                                if (fileGroups == null)
                                {
                                    fileGroups = new FileGroup[1];
                                }
                                else
                                {
                                    Array.Resize<FileGroup>(ref fileGroups, fileGroups.Length + 1);
                                }
                                Dictionary<string, string> newDict = new Dictionary<string, string>();
                                newDict.Add(fileHash.Key, fileHash.Value);
                                fileGroups[fileGroups.Length - 1] = new FileGroup(drive, newDict);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Failed to add pair <" + fileHash.Key + "; " + fileHash.Value + ">: this file does not exist.");
                        }
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine("Failed to add pair <" + fileHash.Key + "; " + fileHash.Value + ">: " + exc.Message);
                    }
                }
                return fileGroups;
            }
            else
            {
                Console.WriteLine("Failed to sort files\' hashes by disks: dictionary is empty.");
                return null;
            }
        }

        /**
         * <summary>Метод, возвращающий информацию о носителе, на котором находится файл.</summary>
         * <param name="path">Путь к файлу</param>
         * <returns>Объект DriveInfo, если файл существует. В обратном случае - null.</returns>
         */
        public static DriveInfo GetDriveInfo(string path)
        {
            return File.Exists(path) ? new DriveInfo((new FileInfo(path)).Directory.Root.FullName) : null;
        }
    }
}