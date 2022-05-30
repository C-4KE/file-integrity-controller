using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace FileIntegrityController
{
    /**
     * <summary>Класс, содержащий информацию о проверяемом файле, необходимую для запуска заданий на проверку и выдачи результата.</summary>
     */
    class IntegrityCheckInfo
    {
        private KeyValuePair<string, string> _fileHash; // Пара (путь_к_файлу : хэш)
        private FileStream _fileStream;                 // IO-поток
        private MD5 _md5;                               // Объект для подсчёта и хранения хэша
        private Task _previousTask;                     // Предыдущее задание
        private Task _nextTask;                         // Следующее задание
        private Exception _initializationException;     // Исключение при инициализации объекта

        /**
         * <summary>Конструктор</summary>
         * <param name="fileHash">Пара (путь_к_файлу : хэш)</param>
         */
        public IntegrityCheckInfo(KeyValuePair<string, string> fileHash)
        {
            _fileHash = new KeyValuePair<string, string>(fileHash.Key, fileHash.Value);
            _initializationException = null;
            try
            {
                _fileStream = new FileStream(fileHash.Key, FileMode.Open);
                _md5 = MD5.Create();
                _previousTask = null;
                _nextTask = null;
            }
            catch (Exception exc)
            {
                _initializationException = exc;
            }
        }

        /**
         * <value>Исключение при создании объекта. Если создание произошло успешно, то исключение равно null.</value>
         */
        public Exception InitializationException
        {
            get
            {
                return _initializationException;
            }
        }

        /**
         * <value>Пара (путь_к_файлу : хэш), с которой ассоциирован данный объект IntegrityCheckInfo.</value>
         */
        public KeyValuePair<string, string> FileHash
        {
            get
            {
                return _fileHash;
            }
        }

        /**
         * <value>Поток для чтения файла.</value>
         */
        public FileStream Stream
        {
            get
            {
                return _fileStream;
            }
        }

        /**
         * <value>Объект, использующийся для вычисления хэша файла.</value>
         */
        public MD5 MD5Object
        {
            get
            {
                return _md5;
            }
        }

        /**
         * <value>Задание на вычисление предыдущего куска файла.</value>
         */
        public Task PreviousTask
        {
            get
            {
                return _previousTask;
            }
        }

        /**
         * <value>Задание на вычисление следующего куска файла.</value>
         */
        public Task NextTask
        {
            get
            {
                return _nextTask;
            }
        }

        /**
         * <summary>Метод, добавляющий следующее задание и обновляющий предыдущее.</summary>
         * <param name="nextTask">Следующее задание</param>
         */
        public void SetNextTask(Task nextTask)
        {
            _previousTask = _nextTask;
            _nextTask = nextTask;
        }

        /**
         * <summary>Метод, закрывающий файловый поток.</summary>
         */
        public void EndIO()
        {
            if (_fileStream != null)
                _fileStream.Close();
        }

        /**
         * <summary>Метод, закрывающий объект MD5.</summary>
         */
        public void EndMD5()
        {
            if (_md5 != null)
                _md5.Dispose();
        }
    }
}
