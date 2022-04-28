using System.Collections.Generic;
using System.Linq;

namespace FileIntegrityController
{
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
    }
}
