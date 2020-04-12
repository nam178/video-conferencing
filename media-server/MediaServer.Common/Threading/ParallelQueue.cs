using NLog;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MediaServer.Common.Threading
{
    public class ParallelQueue : IParallelQueue
    {
        readonly BlockingCollection<(object Target, Action<object> Processor)> _worKItems = new BlockingCollection<(object, Action<object>)>();
        readonly ILogger _logger = NLog.LogManager.GetCurrentClassLogger();

        public void Start()
        {
            Parallel.ForEach(_worKItems, new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            }, ProcessWorkItem);
        }

        void ProcessWorkItem((object Target, Action<object> Processor) workItem) => workItem.Processor.Invoke(workItem.Target);

        public void Enqueue(object item, Action<object> task)
        {
            if(item is null)
                throw new ArgumentNullException(nameof(item));
            if(task is null)
                throw new ArgumentNullException(nameof(task));
            _worKItems.Add((item, task));
        }
    }

}
