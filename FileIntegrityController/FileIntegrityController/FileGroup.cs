using System.Collections.Generic;
using System.Linq;

namespace FileIntegrityController
{
    /**
     * <summary>Класс, хранящий коллекцию пар (путь_к_файлу : хэш) для файлов, лежащих на одном диске.</summary>
     */
    public class FileGroup
    {
        private string _diskSerialNumber;
        private Dictionary<string, string> _filesHashes;

        /**
         * <summary>Конструктор</summary>
         * <param name="diskSerialNumber">Серийный номер диска, на котором лежат файлы группы.</param>
         * <param name="filesHashes">Словарь пар (путь_к_файлу : хэш).</param>
         */
        public FileGroup(string diskSerialNumber, Dictionary<string, string> filesHashes)
        {
            _diskSerialNumber = diskSerialNumber;
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
            FileGroup fileGroup = obj as FileGroup;
            if (fileGroup == null)
            {
                return false;
            }
            else
            {
                return (_diskSerialNumber == fileGroup._diskSerialNumber)
                       && (_filesHashes.Count == fileGroup._filesHashes.Count)
                       && (!_filesHashes.Except(fileGroup._filesHashes).Any());
            }
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