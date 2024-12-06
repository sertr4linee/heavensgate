import { NextResponse } from 'next/server';

export async function POST(request: Request) {
    const data = await request.json();
    const { level, message, details } = data;

    // Codes ANSI pour les couleurs
    const colors = {
        info: '\x1b[34m',    // bleu
        warn: '\x1b[33m',    // jaune
        error: '\x1b[31m',   // rouge
        debug: '\x1b[90m'    // gris
    };

    const color = colors[level as keyof typeof colors] || '';
    const reset = '\x1b[0m';
    
    console.log(`${color}[${level.toUpperCase()}] ${message}${reset}`);
    if (details) {
        console.log(JSON.stringify(details, null, 2));
    }

    return NextResponse.json({ success: true });
} 