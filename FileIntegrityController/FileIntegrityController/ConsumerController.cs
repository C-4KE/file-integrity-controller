using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace FileIntegrityController
{
    /**
     * <summary>Класс, контролирующий работу Consumer'ов.</summary>
     */
    public class ConsumerController
    {
        private ActionBlock<(Task, Task)> _consumers;

        /**
         * <summary>Конструктор</summary>
         * <param name="producerBuffers">Буферы</param>
         */
        public ConsumerController(List<BufferBlock<(Task, Task)>> producerBuffers)
        {
            // Создание ActionBlock, запускающего задания по числу логических процессоров
            _consumers = new ActionBlock<(Task, Task)>(((Task, Task) tasks) =>
            {
                if (tasks.Item2 != null)
                    tasks.Item2.Wait();
                tasks.Item1.RunSynchronously();
            }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount, BoundedCapacity = Environment.ProcessorCount });

            // Подключение producr'ов к Consumer'ам. Условие завершения ActionBlock - все Producer'ы завершили чтение.
            Task[] completionTasks = new Task[producerBuffers.Count];
            int count = 0;
            foreach (var buffer in producerBuffers)
            {
                buffer.LinkTo(_consumers);
                completionTasks[count] = buffer.Completion;
                count++;
            }
            Task.WhenAll(completionTasks).ContinueWith(_ => _consumers.Complete());
        }

        public ConsumerController(List<BufferBlock<(Task, Task)>> producerBuffers, int threadsNumber)
        {
            threadsNumber = threadsNumber > 0 ? threadsNumber : Environment.ProcessorCount;
            // Создание ActionBlock, запускающего задания по числу, заданному в качестве параметра конструктора
            _consumers = new ActionBlock<(Task, Task)>(((Task, Task) tasks) =>
            {
                if (tasks.Item2 != null)
                    tasks.Item2.Wait();
                tasks.Item1.RunSynchronously();
            }, new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = threadsNumber, BoundedCapacity = threadsNumber });

            // Подключение producr'ов к Consumer'ам. Условие завершения ActionBlock - все Producer'ы завершили чтение.
            Task[] completionTasks = new Task[producerBuffers.Count];
            int count = 0;
            foreach (var buffer in producerBuffers)
            {
                buffer.LinkTo(_consumers);
                completionTasks[count] = buffer.Completion;
                count++;
            }
            Task.WhenAll(completionTasks).ContinueWith(_ => _consumers.Complete());
        }
    }
}
