using System.Collections.Generic;
using System.Linq;

namespace FileIntegrityController
{
    /**
     * <summary>Класс, хранящий коллекцию пар (путь_к_файлу : хеш) для файлов, лежащих на одном диске.</summary>
     */
    public class FileGroup
    {
        private string _diskName;
        private Dictionary<string, string> _filesHashes;

        public FileGroup(string diskName, Dictionary<string, string> filesHashes)
        {
            _diskName = diskName;
            _filesHashes = new Dictionary<string, string>(filesHashes);
        }

        /**
         * <value>Имя диска, на котором находятся файлы, чьи пути и хеши хранятся в этом объекте.</value>
         */
        public string DiskName
        {
            get
            {
                return _diskName;
            }
        }

        /**
         * <value>Словарь с парами (имя_файла : хеш), хранящиеся на одном диске.</value>
         */
        public Dictionary<string, string> FilesHashes
        {
            get
            {
                return _filesHashes;
            }
        }

        public override bool Equals(object obj)
        {
            return (_diskName == ((FileGroup)obj)._diskName) && (_filesHashes.Count == ((FileGroup)obj)._filesHashes.Count) && (!_filesHashes.Except(((FileGroup)obj)._filesHashes).Any());
        }

        /**
         * <summary>Метод, разделяющий группу на части.</summary>
         * <param name="parts">Количество частей, на которые необходимо разбить группу.</param>
         * <returns>Возвращает список из объектов FileGroup, являющиеся частями вызывающего метод объекта.</returns>
         */
        public List<FileGroup> Split(uint parts)
        {
            List<FileGroup> fileGroups = new List<FileGroup>();
            if (parts > 1)
            {
                long elementsPerPart = _filesHashes.Count % parts == 0 ? _filesHashes.Count / parts : _filesHashes.Count / parts + 1;
                FileGroup tempGroup = new FileGroup(_diskName, new Dictionary<string, string>());
                long counter = 0;
                foreach (KeyValuePair<string, string> fileHash in _filesHashes)
                {
                    tempGroup._filesHashes.Add(fileHash.Key, fileHash.Value);
                    counter++;
                    if (counter == elementsPerPart)
                    {
                        fileGroups.Add(tempGroup);
                        tempGroup = new FileGroup(_diskName, new Dictionary<string, string>());
                        counter = 0;
                    }
                }
            }
            else
            {
                fileGroups.Add(this);
            }
            return fileGroups;
        }
    }
}
