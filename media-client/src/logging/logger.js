export default class Logger {
    constructor(name) {
        this._name = name;
    }

    debug() { // alias for log
        this.write('log', arguments, 'gray');
    }

    log() {
        this.write('log', arguments, 'gray');
    }

    warn() {
        this.write('warn', arguments, 'purple');
    }

    error() {
        this.write('error', arguments, 'red');
    }

    info() {
        this.write('info', arguments, 'blue');
    }

    write(method, args, color) {
        if (args.length > 1)
            console[method](`%c[${this._name}] [${method}] ${args[0]}`, `color:${color};`, args[1]);
        else
            console[method](`%c[${this._name}] [${method}] ${args[0]}`, `color:${color};`);
    }
}