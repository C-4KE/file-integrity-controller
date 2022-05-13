using System.Collections.Generic;
using System.Linq;
using System;

namespace FileIntegrityController
{
    /**
     * <summary>Класс, хранящий коллекцию пар (путь_к_файлу : хэш) для файлов, лежащих на одном диске.</summary>
     */
    public class FileGroup
    {
        private string _diskSerialNumber;
        private Dictionary<string, string> _filesHashes;

        public FileGroup(string diskName, Dictionary<string, string> filesHashes)
        {
            _diskSerialNumber = diskName;
            _filesHashes = new Dictionary<string, string>(filesHashes);
        }

        /**
         * <value>Серийный номер диска, на котором находятся файлы, чьи пути и хэши хранятся в этом объекте.</value>
         */
        public string DiskSerialNumber
        {
            get
            {
                return _diskSerialNumber;
            }
        }

        /**
         * <value>Словарь с парами (имя_файла : хэш), хранящиеся на одном диске.</value>
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
            bool answer = (_diskSerialNumber == ((FileGroup)obj)._diskSerialNumber) && (_filesHashes.Count == ((FileGroup)obj)._filesHashes.Count) && (!_filesHashes.Except(((FileGroup)obj)._filesHashes).Any());
            Console.WriteLine(answer);
            return answer;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            hash += _diskSerialNumber.GetHashCode();
            foreach (KeyValuePair<string, string> fileHash in _filesHashes)
            {
                hash += fileHash.Key.GetHashCode();
                hash += fileHash.Value.GetHashCode();
            }
            return hash;
        }
    }
}