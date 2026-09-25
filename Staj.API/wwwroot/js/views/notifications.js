// views/notifications.js — bildirim listesi + toplu okundu

import { esc, timeAgo, NotificationType, toast, icons, renderPagination, bindPagination } from '../utils.js';

export async function renderNotifications(ctx, { query }) {
    const { renderView, api, router } = ctx;
    const onlyUnread = query.onlyUnread === 'true';

    renderView('Bildirimler', `
        <div class="page-header">
            <div>
                <h1>Bildirimler</h1>
                <div class="page-sub">Platformdaki tüm bildirimleri buradan takip et.</div>
            </div>
            <div class="flex gap-2">
                <label class="checkbox"><input type="checkbox" id="unread-only" ${onlyUnread ? 'checked' : ''} /> Sadece okunmamışlar</label>
                <button class="btn btn-secondary" id="read-all">Tümünü okundu işaretle</button>
            </div>
        </div>
        <div id="notif-list"><div class="loading-box"><div class="spinner"></div></div></div>
    `);

    document.getElementById('unread-only').addEventListener('change', (e) => {
        location.hash = '#/notifications' + (e.target.checked ? '?onlyUnread=true' : '');
    });
    document.getElementById('read-all').addEventListener('click', async () => {
        try {
            await api.markAllNotificationsRead();
            toast('Tüm bildirimler okundu olarak işaretlendi.', 'success');
            ctx.refreshBadges();
            location.reload();
        } catch (e) { toast(e?.message || 'Hata', 'error'); }
    });

    const list = document.getElementById('notif-list');
    try {
        const r = await api.getNotifications({
            page: parseInt(query.page || '1'),
            pageSize: 20,
            onlyUnread: onlyUnread ? 'true' : null
        });
        const p = r.data;
        if (!p || !p.items || p.items.length === 0) {
            list.innerHTML = `<div class="card"><div class="card-empty"><div class="empty-title">Bildirim yok</div><div>${onlyUnread ? 'Okunmamış bildirim kalmadı.' : 'Henüz bildirim yok.'}</div></div></div>`;
            return;
        }
        list.innerHTML = `
            <div class="card tight">
                ${p.items.map(n => notifRow(n)).join('')}
            </div>
            ${renderPagination(p)}
        `;
        list.querySelectorAll('.notif-row').forEach(row => {
            row.addEventListener('click', async () => {
                const id = row.dataset.id;
                if (!row.classList.contains('read-done')) {
                    try {
                        await api.markNotificationRead(id);
                        row.classList.remove('unread');
                        row.querySelector('.notif-dot')?.classList.add('transparent');
                        row.classList.add('read-done');
                        ctx.refreshBadges();
                    } catch { }
                }
                // Referans linkine yönlendir
                const refType = row.dataset.refType;
                const refId = row.dataset.refId;
                if (refType === 'JobPost' && refId) router.go('/jobs/' + refId);
                else if (refType === 'Proposal' && refId) router.go('/proposals');
                else if (refType === 'Conversation' && refId) router.go('/messages/' + refId);
                else if (refType === 'Message' && refId) router.go('/messages');
                else if (refType === 'Review' && refId) router.go('/reviews');
            });
            const delBtn = row.querySelector('.notif-del');
            delBtn?.addEventListener('click', async (e) => {
                e.stopPropagation();
                try {
                    await api.deleteNotification(row.dataset.id);
                    row.remove();
                    ctx.refreshBadges();
                    toast('Bildirim silindi.', 'success');
                } catch (ex) { toast(ex?.message || 'Silinemedi', 'error'); }
            });
        });
        bindPagination(list, page => {
            const q = { ...query, page };
            location.hash = '#/notifications?' + new URLSearchParams(q).toString();
        });
    } catch (e) {
        list.innerHTML = `<div class="card text-danger">Yüklenemedi: ${esc(e?.message || '')}</div>`;
    }
}

function notifRow(n) {
    const t = NotificationType[n.type] || { label: '', icon: '🔔' };
    return `
        <div class="notif-row ${!n.isRead ? 'unread' : ''}" data-id="${n.id}" data-ref-type="${esc(n.referenceType || '')}" data-ref-id="${esc(n.referenceId || '')}">
            <span class="notif-dot ${n.isRead ? 'transparent' : ''}"></span>
            <div class="avatar" style="background:var(--bg);color:var(--text-muted)">${t.icon}</div>
            <div class="grow">
                <div class="flex items-center gap-2">
                    <div class="font-medium">${esc(n.title)}</div>
                    <span class="badge badge-muted">${esc(t.label)}</span>
                </div>
                <div class="text-sm text-muted mt-1" style="white-space:pre-wrap">${esc(n.body)}</div>
                <div class="text-sm text-soft mt-1">${timeAgo(n.createdAt)}</div>
            </div>
            <button class="btn btn-ghost btn-sm notif-del" title="Sil">${icons.trash}</button>
        </div>
    `;
}