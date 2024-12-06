type LogLevel = 'debug' | 'error';

class Logger {
    private isDev = process.env.NODE_ENV === 'development';

    private log(level: LogLevel, message: string, data?: any) {
        if (!this.isDev) return;

        const timestamp = new Date().toISOString();
        const prefix = `[${timestamp}] [${level.toUpperCase()}]`;

        if (level === 'error') {
            console.error(prefix, message, data || '');
        } else {
            console.log(prefix, message, data || '');
        }
    }

    debug(message: string, data?: any) {
        if (this.isDev) {
            this.log('debug', message, data);
        }
    }

    error(message: string, error?: any) {
        const errorData = error ? {
            message: error.message,
            response: error.response?.data,
            status: error.response?.status
        } : undefined;

        this.log('error', message, errorData);
    }
}

export const logger = new Logger(); 