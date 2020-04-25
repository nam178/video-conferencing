import Logger from '../logging/logger.js';

export default class Queue
{
    _processItemAsync = null;

    _items = [];

    _isProcessingQueueItems = false;

    /**
     * @var {Logger}
     */
    _logger = null;

    constructor(queueName, processItemAsync) {
        this._logger = new Logger(`queue-${queueName}`);
        if(!processItemAsync) {
            throw new 'Must provide a function to process items';
        }
        this._processItemAsync  = processItemAsync;
    }

    enqueue(item) {
        this._items.push(item);
        this._processQueueItems();
    }

    async _processQueueItems() {
        if (this._isProcessingQueueItems) {
            return;
        }
        this._isProcessingQueueItems = true;
        try {
            while (this._items.length > 0) {
                var dequeuedJob = this._items[0];
                this._items = this._items.splice(1);
                try
                {
                    await this._processItemAsync(dequeuedJob);
                }
                catch(err) {
                    this._logger.error(`Failed processing item: ${err}`, dequeuedJob);
                }
            }
        }
        finally {
            this._isProcessingQueueItems = false;
        }
    }
}