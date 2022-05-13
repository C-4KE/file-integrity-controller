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
         * <remarks>Если входной словарь не пустой, но содержит пары с несуществующими файлами / некорректными ключами, то они не добавляются в выходной массив.</remarks>
         * <param name="filesHashes">Словарь с парами (имя_файла : хэш).</param>
         * <returns>Возвращает массив объектов FileGroup, каждый из которых хранит информацию о файлах с одного диска. Если входной словарь пустой, возвращает null.</returns>
         */
        public static FileGroup[] SortFilesByDisks(Dictionary<string, string> filesHashes)
        {
            if (filesHashes.Count != 0)
            {
                FileGroup[] fileGroups = null;
                Dictionary<string, FileGroup> volumeGroup = new Dictionary<string, FileGroup>();
                foreach (KeyValuePair<string, string> fileHash in filesHashes)
                {
                    try
                    {
                        DriveInfo driveInfo = GetDriveInfo(fileHash.Key);
                        if (driveInfo != null)
                        {
                            string volume = driveInfo.Name;
                            if (volumeGroup.ContainsKey(volume))    // Уже встречали файл на этом разделе
                            {
                                FileGroup fileGroup;
                                volumeGroup.TryGetValue(volume, out fileGroup);
                                fileGroup.FilesHashes.Add(fileHash.Key, fileHash.Value);
                            }
                            else   // Ещё не встречали файл на этом разделе
                            {
                                string serialNumber = (new StorageInfo()).GetDiskSerialNumber(driveInfo);
                                bool isGroupExists = false;
                                foreach (FileGroup fileGroup in fileGroups)
                                {
                                    if (fileGroup.DiskSerialNumber == serialNumber)     // Группа файлов, у которой серийный номер диска совпадает с серийным номер диска, на котором определён раздел, существует
                                    {
                                        fileGroup.FilesHashes.Add(fileHash.Key, fileHash.Value);
                                        volumeGroup.Add(volume, fileGroup);
                                        isGroupExists = true;
                                        break;
                                    }
                                }
                                if (!isGroupExists)     // Нет группы с тем же серийным номером, что и серийный номер диска, на котором находится раздел
                                {
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
                                    fileGroups[fileGroups.Length - 1] = new FileGroup(serialNumber, newDict);
                                    volumeGroup.Add(volume, fileGroups[fileGroups.Length - 1]);
                                }
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