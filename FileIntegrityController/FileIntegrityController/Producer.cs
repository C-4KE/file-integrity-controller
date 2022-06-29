using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Linq;

namespace FileIntegrityController
{
    /**
     * <summary>Класс, отвечающий за открытие файла, отправку задания на проверку целостности и возврат результата.</summary>
     */
    public class Producer
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private FileGroup _fileGroup;                           // Группа файлов
        private BufferBlock<(Task, Task)> _producerBuffer;      // Буффер, куда загружаются задания (следующее задание, предыдущее задание)
        private int _queueSize;                                 // Размер очереди буффера
        private int _filesAmount;                               // Количество проверяемых файлов
        private int _filesCounter;                              // Счётчик, указывающий на позицию в массиве ключей для словаря с файлами и хэшами
        private int _checkForNewIOCooldown = 6;                 // Количество итераций, через которое проверяется, не надо ли открыть новый IO-поток
        private int _bufferSize = 4096;                         // Размер порции файла
            
        /**
         * <summary>Конструктор</summary>
         * <param name="consumerBuffer">Буфер потребителя, к которому будет подключен буфер производителя для передачи заданий.</param>
         * <param name="fileGroup">Группа файлов, лежащая на одном диске.</param>
         * <param name="queueSize">Размер очереди для проверяемых порций файлов в Producer'е.</param>
         */
        public Producer(FileGroup fileGroup, int queueSize)
        {
            queueSize = queueSize > 0 ? queueSize : Environment.ProcessorCount;
            _queueSize = queueSize;
            _fileGroup = fileGroup;
            _producerBuffer = new BufferBlock<(Task, Task)>(new DataflowBlockOptions() { BoundedCapacity = queueSize });
            _filesAmount = _fileGroup.FilesHashes.Count;
            _filesCounter = 0;
        }

        public BufferBlock<(Task, Task)> ProducerBuffer
        {
            get
            {
                return _producerBuffer;
            }
        }

        /**
         * <summary>Метод, рассылающий задания на проверку целостности потребителю и возвращающий результат.</summary>
         * <returns>Словарь пар (путь_к_файлу : результат_проверку (true/false))</returns>
         */
        public Dictionary<string, bool> Execute()
        {
            Dictionary<string, bool> results = new Dictionary<string, bool>();
            string[] keys = _fileGroup.FilesHashes.Keys.ToArray();

            // Получение первого объекта, хранящего данные о проверяемом файле
            IntegrityCheckInfo firstCheckInfo = GetNewCheckInfo(keys);
            if (firstCheckInfo != null)
            {
                List<IntegrityCheckInfo> checkList = new List<IntegrityCheckInfo>();
                checkList.Add(firstCheckInfo);
                List<Task> finalizingTasks = new List<Task>();
                List<IntegrityCheckInfo> finishedChecks = new List<IntegrityCheckInfo>();
                int busyIterations = 0; // Счётчик итераций в очередном цикле проверки на открытие нового IO-потока, на которых очередь была заполнена
                int checkIO = 0;

                // Цикл отправки кусков файлов Consumer'ам
                while (_filesAmount > 0)
                {
                    List<int> toRemove = new List<int>();
                    // Отправка кусков файлов уже открытых IO-потоков
                    for (int i = 0; i < checkList.Count; i++)
                    {
                        IntegrityCheckInfo currentCheck = checkList[i];
                        byte[] buffer = new byte[_bufferSize];
                        int readAmount = 0;
                        if ((readAmount = currentCheck.Stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            Task task = new Task(() => IntegrityVerifier.CheckPart(currentCheck.MD5Object, buffer, readAmount));
                            currentCheck.SetNextTask(task);
                            _producerBuffer.SendAsync((currentCheck.NextTask, currentCheck.PreviousTask));
                        }
                        else
                        {
                            // Отсылка завершающего задания
                            Task task = new Task<bool>(() => IntegrityVerifier.VerifyHash(currentCheck.MD5Object, currentCheck.FileHash.Value));
                            currentCheck.SetNextTask(task);
                            finalizingTasks.Add(task);
                            finishedChecks.Add(currentCheck);
                            currentCheck.EndIO();               // Закрытие файлового потока
                            _filesAmount--;
                            _producerBuffer.SendAsync((currentCheck.NextTask, currentCheck.PreviousTask));

                            // Получение нового объекта, хранящего данные о проверяемом файле
                            if (_filesCounter < _fileGroup.FilesHashes.Count)
                            {
                                IntegrityCheckInfo newCheckInfo = GetNewCheckInfo(keys);
                                if (newCheckInfo != null)
                                {
                                    checkList[i] = newCheckInfo;
                                }
                                else
                                {
                                    toRemove.Add(i);
                                }
                            }
                            else
                            {
                                toRemove.Add(i);
                            }
                        }
                    }
                    for (int i = toRemove.Count - 1; i >= 0; i--)
                    {
                        checkList.RemoveAt(toRemove[i]);
                    }
                    toRemove.Clear();

                    // Проверка, не надо ли открыть новый IO-поток
                    checkIO++;
                    if (_producerBuffer.Count == _queueSize)
                    {
                        busyIterations++;
                    }
                    if (checkIO == _checkForNewIOCooldown)
                    {
                        checkIO = 0;
                        if (busyIterations < _checkForNewIOCooldown / 2)
                        {
                            IntegrityCheckInfo newCheckInfo = GetNewCheckInfo(keys);
                            if (newCheckInfo != null)
                            {
                                checkList.Add(newCheckInfo);
                            }
                        }
                        busyIterations = 0;
                    }
                }

                // Ожидание всех завершающих заданий
                Task.WaitAll(finalizingTasks.ToArray());

                // Завершение работы с буфером
                _producerBuffer.Complete();

                // Сборка результата
                foreach (IntegrityCheckInfo checkInfo in finishedChecks)
                {
                    results.Add(checkInfo.FileHash.Key, ((Task<bool>)checkInfo.NextTask).Result);
                    checkInfo.EndMD5();
                }
            }
            return results;
        }

        /**
         * <summary>Метод, возвращающий объект IntegrityCheckInfo для очередного файла из FileGroup.</summary>
         * <param name="keys">Массив ключей словаря, содержащего пары (путь_к_файлу : хэш) в FileGroup.</param>
         * <returns>Объект IntegrityCheckInfo, если всё прошло успешно. Иначе возвращает null.</returns>
         */
        private IntegrityCheckInfo GetNewCheckInfo(string[] keys)
        {
            bool isRead = false;
            IntegrityCheckInfo newCheckInfo = null;
            while ((!isRead) && (_filesCounter < _fileGroup.FilesHashes.Count))
            {
                string hash;
                _fileGroup.FilesHashes.TryGetValue(keys[_filesCounter], out hash);
                newCheckInfo = new IntegrityCheckInfo(new KeyValuePair<string, string>(keys[_filesCounter], hash));
                _filesCounter++;
                if (newCheckInfo.InitializationException == null)
                {
                    isRead = true;
                }
                else
                {
                    logger.Warn($"Error while trying to read {newCheckInfo.FileHash.Key} file: {newCheckInfo.InitializationException.Message}");
                    _filesAmount--;
                    newCheckInfo.EndIO();
                    newCheckInfo.EndMD5();
                }
            }
            if (isRead)
            { 
                return newCheckInfo; 
            }
            else
            {
                return null;
            }
        }
    }
}
