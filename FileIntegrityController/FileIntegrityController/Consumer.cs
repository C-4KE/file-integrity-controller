using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace FileIntegrityController
{
    public class Consumer
    {
        private BufferBlock<Task> _consumerBuffer;
        private List<BufferBlock<Task>> _producerBuffers;

        /**
         * <summary>Конструктор</summary>
         * <param name="consumerBuffer">Буфер потребителя, откуда будут считываться задания.</param>
         * <param name="producerBuffers">Буферы производителей, которые соединены с буфером потребителя.</param>
         */
        public Consumer(BufferBlock<Task> consumerBuffer, List<BufferBlock<Task>> producerBuffers)
        {
            _consumerBuffer = consumerBuffer;
            _producerBuffers = producerBuffers;
            Task[] completionTasks = new Task[_producerBuffers.Count];
            int count = 0;
            foreach (var buffer in _producerBuffers)
            {
                completionTasks[count] = buffer.Completion;
                count++;
            }
            Task.WhenAll(completionTasks).ContinueWith(_ => _consumerBuffer.Complete());
        }

        /**
         * <summary>Метод, принимающий задания на проверку от производителей и запускающий их.</summary>
         */
        async public void Execute()
        {
            while (await _consumerBuffer.OutputAvailableAsync())
            {
                while (_consumerBuffer.TryReceive(out Task task))
                {
                    task.Start();
                }
            }
        }
    }
}
