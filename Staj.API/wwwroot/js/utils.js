// utils.js — enum mapleri, formatlayıcılar, DOM yardımcıları

// ===== Enum → Türkçe etiket =====
export const WorkMode = { 1: 'Uzaktan', 2: 'Hibrit', 3: 'Yerinde' };
export const WorkModeOptions = [
    { value: 1, label: 'Uzaktan' },
    { value: 2, label: 'Hibrit' },
    { value: 3, label: 'Yerinde' }
];

export const JobPostStatus = { 1: 'Taslak', 2: 'Yayında', 3: 'Kapalı', 4: 'İptal' };
export const JobPostStatusBadge = { 1: 'muted', 2: 'success', 3: 'warning', 4: 'danger' };

export const ProposalKind = { 1: 'Başvuru', 2: 'Doğrudan Teklif' };
export const ProposalStatus = { 1: 'Beklemede', 2: 'Kabul Edildi', 3: 'Reddedildi', 4: 'Geri Çekildi' };
export const ProposalStatusBadge = { 1: 'warning', 2: 'success', 3: 'danger', 4: 'muted' };

export const ContactPreference = { 1: 'E-posta', 2: 'Telefon', 3: 'Platform mesajı' };
export const ContactPreferenceOptions = [
    { value: 1, label: 'E-posta' },
    { value: 2, label: 'Telefon' },
    { value: 3, label: 'Platform mesajı' }
];

export const NotificationType = {
    1: { label: 'Yeni başvuru', icon: '📩' },
    2: { label: 'Doğrudan teklif', icon: '💼' },
    3: { label: 'Teklif durumu', icon: '📝' },
    4: { label: 'Yeni mesaj', icon: '💬' },
    5: { label: 'Yeni değerlendirme', icon: '⭐' }
};

// ===== Formatlayıcılar =====
export function formatDate(iso) {
    if (!iso) return '—';
    const d = new Date(iso);
    return d.toLocaleDateString('tr-TR', { day: '2-digit', month: 'short', year: 'numeric' });
}
export function formatDateTime(iso) {
    if (!iso) return '—';
    const d = new Date(iso);
    return d.toLocaleString('tr-TR', { day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' });
}
export function formatTime(iso) {
    if (!iso) return '';
    return new Date(iso).toLocaleTimeString('tr-TR', { hour: '2-digit', minute: '2-digit' });
}
export function timeAgo(iso) {
    if (!iso) return '';
    const now = new Date();
    const then = new Date(iso);
    const diffMs = now - then;
    const min = Math.floor(diffMs / 60000);
    if (min < 1) return 'az önce';
    if (min < 60) return `${min} dk önce`;
    const hr = Math.floor(min / 60);
    if (hr < 24) return `${hr} sa önce`;
    const day = Math.floor(hr / 24);
    if (day < 7) return `${day} gün önce`;
    return formatDate(iso);
}
export function formatBudget(min, max, currency) {
    const cur = currency || '₺';
    if (min && max) return `${cur}${min.toLocaleString('tr-TR')} - ${cur}${max.toLocaleString('tr-TR')}`;
    if (min) return `${cur}${min.toLocaleString('tr-TR')}+`;
    if (max) return `≤ ${cur}${max.toLocaleString('tr-TR')}`;
    return 'Görüşülür';
}
export function initials(first, last) {
    return `${(first || '?')[0]}${(last || '')[0] || ''}`.toUpperCase();
}
export function fullName(first, last) {
    return `${first || ''} ${last || ''}`.trim() || '—';
}
export function stars(rating) {
    const r = Math.round(rating || 0);
    return '★'.repeat(r) + '☆'.repeat(5 - r);
}

// ===== DOM yardımcıları =====
export function h(html) {
    const t = document.createElement('template');
    t.innerHTML = html.trim();
    return t.content.firstElementChild;
}
export function esc(s) {
    if (s == null) return '';
    return String(s)
        .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;').replace(/'/g, '&#39;');
}
export function on(el, sel, ev, handler) {
    el.addEventListener(ev, (e) => {
        const target = e.target.closest(sel);
        if (target && el.contains(target)) handler(e, target);
    });
}

// ===== Toast =====
export function toast(message, type = 'info', title = null) {
    const root = document.getElementById('toast-root');
    if (!root) return;
    const el = h(`
        <div class="toast ${type}">
            ${title ? `<div class="toast-title">${esc(title)}</div>` : ''}
            <div class="toast-body">${esc(message)}</div>
        </div>
    `);
    root.appendChild(el);
    setTimeout(() => {
        el.style.opacity = '0';
        el.style.transition = 'opacity .2s';
        setTimeout(() => el.remove(), 220);
    }, 3500);
}

// ===== Modal =====
export function openModal({ title, body, footer, wide = false }) {
    const root = document.getElementById('modal-root');
    const backdrop = h(`
        <div class="modal-backdrop">
            <div class="modal ${wide ? 'wide' : ''}">
                <div class="modal-header">
                    <div class="modal-title">${esc(title || '')}</div>
                    <button class="modal-close" aria-label="Kapat">×</button>
                </div>
                <div class="modal-body"></div>
                ${footer ? '<div class="modal-footer"></div>' : ''}
            </div>
        </div>
    `);
    const bodyEl = backdrop.querySelector('.modal-body');
    const footerEl = backdrop.querySelector('.modal-footer');
    if (typeof body === 'string') bodyEl.innerHTML = body;
    else if (body instanceof Node) bodyEl.appendChild(body);
    if (footer && footerEl) {
        if (typeof footer === 'string') footerEl.innerHTML = footer;
        else if (footer instanceof Node) footerEl.appendChild(footer);
    }
    const close = () => backdrop.remove();
    backdrop.querySelector('.modal-close').addEventListener('click', close);
    backdrop.addEventListener('click', (e) => { if (e.target === backdrop) close(); });
    root.appendChild(backdrop);
    return { el: backdrop, close, body: bodyEl, footer: footerEl };
}

export function confirmModal(message, { title = 'Onay', okLabel = 'Onayla', okKind = 'primary' } = {}) {
    return new Promise((resolve) => {
        const modal = openModal({
            title,
            body: `<p>${esc(message)}</p>`,
            footer: `
                <button class="btn btn-secondary" data-act="cancel">Vazgeç</button>
                <button class="btn btn-${okKind}" data-act="ok">${esc(okLabel)}</button>
            `
        });
        modal.footer.querySelector('[data-act=cancel]').onclick = () => { modal.close(); resolve(false); };
        modal.footer.querySelector('[data-act=ok]').onclick = () => { modal.close(); resolve(true); };
    });
}

// ===== Pagination bileşeni =====
export function renderPagination(paged, onPage) {
    if (!paged || paged.totalPages <= 1) return '';
    return `
        <div class="pagination">
            <button class="btn btn-secondary btn-sm" data-page="${paged.page - 1}" ${paged.hasPrevious ? '' : 'disabled'}>Önceki</button>
            <span class="pagination-info">Sayfa ${paged.page} / ${paged.totalPages} · ${paged.totalCount} kayıt</span>
            <button class="btn btn-secondary btn-sm" data-page="${paged.page + 1}" ${paged.hasNext ? '' : 'disabled'}>Sonraki</button>
        </div>
    `;
}
export function bindPagination(el, onPage) {
    el.querySelectorAll('[data-page]').forEach(b => {
        b.addEventListener('click', () => {
            const p = parseInt(b.dataset.page);
            if (p >= 1) onPage(p);
        });
    });
}

// ===== SVG ikonlar (Lucide-benzeri) =====
export const icons = {
    home: `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m3 9 9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>`,
    briefcase: `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="2" y="7" width="20" height="14" rx="2"/><path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"/></svg>`,
    users: `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>`,
    inbox: `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="22 12 16 12 14 15 10 15 8 12 2 12"/><path d="M5.45 5.11 2 12v6a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2v-6l-3.45-6.89A2 2 0 0 0 16.76 4H7.24a2 2 0 0 0-1.79 1.11z"/></svg>`,
    message: `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg>`,
    bell: `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M6 8a6 6 0 0 1 12 0c0 7 3 9 3 9H3s3-2 3-9"/><path d="M10.3 21a1.94 1.94 0 0 0 3.4 0"/></svg>`,
    star: `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polygon points="12 2 15.09 8.26 22 9.27 17 14.14 18.18 21.02 12 17.77 5.82 21.02 7 14.14 2 9.27 8.91 8.26 12 2"/></svg>`,
    user: `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>`,
    settings: `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12.22 2h-.44a2 2 0 0 0-2 2v.18a2 2 0 0 1-1 1.73l-.43.25a2 2 0 0 1-2 0l-.15-.08a2 2 0 0 0-2.73.73l-.22.38a2 2 0 0 0 .73 2.73l.15.1a2 2 0 0 1 1 1.72v.51a2 2 0 0 1-1 1.74l-.15.09a2 2 0 0 0-.73 2.73l.22.38a2 2 0 0 0 2.73.73l.15-.08a2 2 0 0 1 2 0l.43.25a2 2 0 0 1 1 1.73V20a2 2 0 0 0 2 2h.44a2 2 0 0 0 2-2v-.18a2 2 0 0 1 1-1.73l.43-.25a2 2 0 0 1 2 0l.15.08a2 2 0 0 0 2.73-.73l.22-.39a2 2 0 0 0-.73-2.73l-.15-.08a2 2 0 0 1-1-1.74v-.5a2 2 0 0 1 1-1.74l.15-.09a2 2 0 0 0 .73-2.73l-.22-.38a2 2 0 0 0-2.73-.73l-.15.08a2 2 0 0 1-2 0l-.43-.25a2 2 0 0 1-1-1.73V4a2 2 0 0 0-2-2z"/><circle cx="12" cy="12" r="3"/></svg>`,
    logout: `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/></svg>`,
    plus: `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>`,
    edit: `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.12 2.12 0 0 1 3 3L12 15l-4 1 1-4z"/></svg>`,
    trash: `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="3 6 5 6 21 6"/><path d="M19 6l-2 14a2 2 0 0 1-2 2H9a2 2 0 0 1-2-2L5 6"/><path d="M10 11v6"/><path d="M14 11v6"/></svg>`,
    search: `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><path d="m21 21-4.35-4.35"/></svg>`,
    menu: `<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="3" y1="6" x2="21" y2="6"/><line x1="3" y1="12" x2="21" y2="12"/><line x1="3" y1="18" x2="21" y2="18"/></svg>`,
    send: `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="22" y1="2" x2="11" y2="13"/><polygon points="22 2 15 22 11 13 2 9 22 2"/></svg>`,
    back: `<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="19" y1="12" x2="5" y2="12"/><polyline points="12 19 5 12 12 5"/></svg>`,
    tag: `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20.59 13.41l-7.17 7.17a2 2 0 0 1-2.83 0L2 12V2h10l8.59 8.59a2 2 0 0 1 0 2.82z"/><line x1="7" y1="7" x2="7.01" y2="7"/></svg>`,
};