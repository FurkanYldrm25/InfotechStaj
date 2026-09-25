// signalr-client.js — ChatHub bağlantısı yönetimi

import * as auth from './auth.js';

let connection = null;
const listeners = {
    ReceiveMessage: new Set(),
    ConversationRead: new Set(),
    ReceiveNotification: new Set()
};

export function on(event, cb) {
    if (!listeners[event]) listeners[event] = new Set();
    listeners[event].add(cb);
    return () => listeners[event].delete(cb);
}

function fire(event, payload) {
    (listeners[event] || []).forEach(cb => {
        try { cb(payload); } catch (e) { console.error('SignalR listener hatası:', e); }
    });
}

export async function start() {
    if (!window.signalR) { console.warn('SignalR client CDN yüklenmedi'); return; }
    if (!auth.isAuthenticated()) return;
    if (connection && connection.state === 'Connected') return;

    connection = new signalR.HubConnectionBuilder()
        .withUrl('/hubs/chat', { accessTokenFactory: () => auth.getAccessToken() })
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Warning)
        .build();

    connection.on('ReceiveMessage', (msg) => fire('ReceiveMessage', msg));
    connection.on('ConversationRead', (p) => fire('ConversationRead', p));
    connection.on('ReceiveNotification', (n) => fire('ReceiveNotification', n));

    try {
        await connection.start();
    } catch (err) {
        console.warn('SignalR bağlanamadı:', err);
        setTimeout(start, 4000);
    }
}

export async function stop() {
    if (connection) {
        try { await connection.stop(); } catch { }
        connection = null;
    }
}

export function isConnected() {
    return connection && connection.state === 'Connected';
}