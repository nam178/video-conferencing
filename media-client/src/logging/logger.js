export default class Logger
{
    constructor(name)
    {
        this._name = name;
    }

    log()
    {
        if(arguments.length > 1)
            console.log(`[${this._name}]`, arguments);
        else
            console.log(`[${this._name}] ${arguments[0]}`);
    }

    warn()
    {
        if(arguments.length > 1)
            console.warn(`[${this._name}]`, arguments);
        else
            console.warn(`[${this._name}] ${arguments[0]}`);
    }
    
    error()
    {
        if(arguments.length > 1)
            console.error(`[${this._name}]`, arguments);
        else
            console.error(`[${this._name}] ${arguments[0]}`);
    }

    info()
    {
        if(arguments.length > 1)
            console.info(`[${this._name}]`, arguments);
        else
            console.info(`[${this._name}] ${arguments[0]}`);
    }
}