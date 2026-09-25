// views/messages.js — sohbet listesi + seçilen konuşma paneli (SignalR ile canlı)

import { esc, initials, fullName, formatTime, formatDate, timeAgo, icons, toast } from '../utils.js';

export async function renderMessages(ctx, { params }) {
    const { renderView, api, signalr, auth } = ctx;
    const activeId = params.conversationId || null;
    const me = auth.getUser();

    renderView('Mesajlar', `<div class="chat-shell">
        <div class="chat-list ${activeId ? 'mobile-hidden' : ''}" id="chat-list">
            <div class="chat-list-header">Sohbetler</div>
            <div id="chat-list-items"><div class="loading-box"><div class="spinner"></div></div></div>
        </div>
        <div class="chat-panel ${activeId ? '' : 'mobile-hidden'}" id="chat-panel">
            ${activeId
            ? '<div class="loading-box"><div class="spinner"></div></div>'
            : '<div class="card-empty" style="flex:1;display:flex;flex-direction:column;justify-content:center;"><div class="empty-title">Sohbet seç</div><div>Sol taraftan bir sohbet aç.</div></div>'}
        </div>
    </div>`);

    const conversationsEl = document.getElementById('chat-list-items');
    let conversations = [];
    try {
        const r = await api.getConversations();
        conversations = r.data?.items || [];
    } catch (e) {
        conversationsEl.innerHTML = `<div class="card-empty text-danger">${esc(e?.message || '')}</div>`;
        return;
    }

    const renderConvList = () => {
        if (conversations.length === 0) {
            conversationsEl.innerHTML = `<div class="card-empty"><div class="empty-title">Sohbet yok</div><div>Bir freelancer'a veya işverene mesaj göndererek başla.</div></div>`;
            return;
        }
        conversationsEl.innerHTML = conversations.map(c => `
            <div class="chat-list-item ${c.id === activeId ? 'active' : ''}" data-id="${c.id}">
                <div class="avatar avatar-lg">${esc(initials(c.otherFirstName, c.otherLastName))}</div>
                <div class="grow" style="min-width:0">
                    <div class="flex items-center gap-2">
                        <div class="font-medium truncate grow">${esc(fullName(c.otherFirstName, c.otherLastName))}</div>
                        ${c.lastMessageAt ? `<div class="text-sm text-soft shrink-0">${timeAgo(c.lastMessageAt)}</div>` : ''}
                    </div>
                    <div class="text-sm text-muted truncate">${esc(c.lastMessagePreview || 'Mesaj yok')}</div>
                </div>
                ${c.unreadCount ? `<div class="unread-dot">${c.unreadCount}</div>` : ''}
            </div>
        `).join('');
        conversationsEl.querySelectorAll('.chat-list-item').forEach(it => {
            it.addEventListener('click', () => {
                location.hash = '#/messages/' + it.dataset.id;
            });
        });
    };
    renderConvList();

    // Aktif sohbet yükleniyorsa panel'i hazırla
    if (!activeId) {
        // canlı: yeni bir mesaj gelirse liste güncellemesi
        const unsubMsg = signalr.on('ReceiveMessage', async () => {
            try {
                const r = await api.getConversations();
                conversations = r.data?.items || [];
                renderConvList();
            } catch { }
        });
        return () => unsubMsg();
    }

    const conv = conversations.find(c => c.id === activeId);
    const panel = document.getElementById('chat-panel');
    panel.innerHTML = `
        <div class="chat-header">
            <button class="btn btn-icon btn-ghost desktop-only mobile-menu-btn" id="back-btn" style="display:none">${icons.back}</button>
            <div class="avatar">${esc(initials(conv?.otherFirstName, conv?.otherLastName))}</div>
            <div class="grow">
                <div class="font-semibold">${esc(fullName(conv?.otherFirstName, conv?.otherLastName)) || 'Sohbet'}</div>
                <div class="text-sm text-muted">Canlı sohbet</div>
            </div>
        </div>
        <div class="chat-messages" id="chat-msgs"></div>
        <form class="chat-input-row" id="msg-form">
            <input class="input grow" id="msg-input" placeholder="Mesaj yaz..." autocomplete="off" />
            <button class="btn btn-primary" type="submit">${icons.send} Gönder</button>
        </form>
    `;

    // Mobil back butonu (mobil view sadece panel gösterildiğinde back'e izin ver)
    document.querySelector('#back-btn')?.addEventListener('click', () => {
        location.hash = '#/messages';
    });

    const msgsEl = document.getElementById('chat-msgs');
    let messages = [];
    try {
        const r = await api.getConversationMessages(activeId, 1, 50);
        messages = (r.data?.items || []).slice().reverse(); // yeni altta
    } catch (e) {
        msgsEl.innerHTML = `<div class="card-empty text-danger">${esc(e?.message || '')}</div>`;
        return;
    }

    const renderMsgs = () => {
        msgsEl.innerHTML = messages.map(m => {
            const mine = m.senderId === me.userId;
            return `
                <div class="msg ${mine ? 'msg-out' : 'msg-in'}">${esc(m.content).replace(/\n/g, '<br>')}
                    <div class="msg-meta">${formatTime(m.createdAt)}${mine && m.isRead ? ' · Okundu' : ''}</div>
                </div>
            `;
        }).join('');
        msgsEl.scrollTop = msgsEl.scrollHeight;
    };
    renderMsgs();

    // Read olarak işaretle
    try { await api.markConversationRead(activeId); ctx.refreshBadges(); } catch { }

    // Send
    const form = document.getElementById('msg-form');
    const input = document.getElementById('msg-input');
    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        const content = input.value.trim();
        if (!content) return;
        input.value = '';
        try {
            const r = await api.sendMessage(activeId, content);
            if (r.data) {
                messages.push(r.data);
                renderMsgs();
            }
        } catch (ex) {
            toast(ex?.message || 'Mesaj gönderilemedi.', 'error');
            input.value = content;
        }
    });

    // Canlı: yeni mesaj gelirse (kendi gönderdiklerimiz zaten yukarıda append edilir)
    const unsubIncoming = signalr.on('ReceiveMessage', (m) => {
        if (!m) return;
        if (m.conversationId === activeId) {
            messages.push(m);
            renderMsgs();
            api.markConversationRead(activeId).then(() => ctx.refreshBadges()).catch(() => { });
        } else {
            // Farklı sohbete gelen — üstteki listeyi tazele
            api.getConversations().then(r => { conversations = r.data?.items || []; renderConvList(); }).catch(() => { });
        }
    });
    const unsubRead = signalr.on('ConversationRead', (p) => {
        if (p?.conversationId === activeId) {
            messages.forEach(m => { if (m.senderId === me.userId) m.isRead = true; });
            renderMsgs();
        }
    });

    input.focus();

    return () => { unsubIncoming(); unsubRead(); };
}