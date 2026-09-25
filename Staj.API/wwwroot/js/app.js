// app.js — uygulama girişi: shell render, router bootstrap, SignalR bağlantısı

import * as auth from './auth.js';
import * as router from './router.js';
import * as signalr from './signalr-client.js';
import { api } from './api.js';
import { icons, esc, initials, toast, on as delegate } from './utils.js';

import { renderLogin, renderRegister } from './views/auth.js';
import { renderDashboard } from './views/dashboard.js';
import { renderJobsSearch, renderJobDetail, renderMyJobs, renderJobEditor } from './views/jobs.js';
import { renderFreelancers, renderFreelancerDetail } from './views/freelancers.js';
import { renderMyProposals, renderProposalsForJob } from './views/proposals.js';
import { renderMessages } from './views/messages.js';
import { renderNotifications } from './views/notifications.js';
import { renderMyProfile } from './views/profile.js';
import { renderAdminCategories, renderAdminSkills } from './views/admin.js';
import { renderMyReviews } from './views/reviews.js';

// ============ Layout render ============

let shellState = { unread: 0, notifCount: 0 };
let signalrUnsubs = [];

function renderAppShell() {
    const user = auth.getUser();
    const roles = user?.roles || [];
    const isAdmin = roles.includes('Admin');
    const isFreelancer = roles.includes('Freelancer');
    const isClient = roles.includes('Client');

    const links = [];
    links.push({ href: '#/dashboard', label: 'Ana sayfa', icon: icons.home });
    if (isFreelancer || isClient || isAdmin) {
        links.push({ href: '#/jobs', label: 'İlanlar', icon: icons.briefcase });
    }
    if (isClient) {
        links.push({ href: '#/my-jobs', label: 'İlanlarım', icon: icons.tag });
    }
    if (isFreelancer || isClient) {
        links.push({ href: '#/freelancers', label: 'Freelancerlar', icon: icons.users });
    }
    if (isFreelancer || isClient) {
        links.push({ href: '#/proposals', label: 'Tekliflerim', icon: icons.inbox });
    }
    links.push({ href: '#/messages', label: 'Mesajlar', icon: icons.message, badgeKey: 'unread' });
    links.push({ href: '#/notifications', label: 'Bildirimler', icon: icons.bell, badgeKey: 'notifCount' });
    links.push({ href: '#/reviews', label: 'Değerlendirmeler', icon: icons.star });

    const adminLinks = isAdmin ? [
        { href: '#/admin/categories', label: 'Kategoriler', icon: icons.tag },
        { href: '#/admin/skills', label: 'Yetkinlikler', icon: icons.settings },
    ] : [];

    const app = document.getElementById('app');
    app.innerHTML = `
        <div class="app">
            <div class="sidebar-backdrop" id="sidebar-backdrop"></div>
            <aside class="sidebar" id="sidebar">
                <div class="sidebar-brand">
                    <div class="logo-mark">S</div>
                    <div>Staj</div>
                </div>
                <div>
                    ${links.map(l => sidebarLink(l)).join('')}
                </div>
                ${adminLinks.length ? `
                    <div class="sidebar-section">Yönetim</div>
                    ${adminLinks.map(l => sidebarLink(l)).join('')}
                ` : ''}
                <div class="sidebar-section">Hesap</div>
                <a class="sidebar-link" href="#/profile">${icons.user}<span>Profilim</span></a>
                <a class="sidebar-link" id="logout-btn">${icons.logout}<span>Çıkış yap</span></a>
            </aside>
            <div class="main">
                <header class="topbar">
                    <div class="topbar-left">
                        <button class="icon-btn mobile-menu-btn" id="mobile-menu">${icons.menu}</button>
                        <div class="topbar-title" id="topbar-title">Staj</div>
                    </div>
                    <div class="topbar-right">
                        <button class="icon-btn" title="Bildirimler" id="topbar-notif">
                            ${icons.bell}
                            <span class="badge-dot ${shellState.notifCount ? '' : 'hidden'}" id="notif-dot">${shellState.notifCount}</span>
                        </button>
                        <button class="icon-btn" title="Mesajlar" id="topbar-msg">
                            ${icons.message}
                            <span class="badge-dot ${shellState.unread ? '' : 'hidden'}" id="msg-dot">${shellState.unread}</span>
                        </button>
                        <div class="avatar" title="${esc(user?.email || '')}">${esc(initials(user?.firstName, user?.lastName))}</div>
                    </div>
                </header>
                <main class="content" id="view"></main>
            </div>
        </div>
    `;

    document.getElementById('logout-btn').addEventListener('click', (e) => {
        e.preventDefault();
        signalr.stop();
        auth.clearAuth();
        router.go('/login');
    });
    document.getElementById('mobile-menu').addEventListener('click', () => {
        document.getElementById('sidebar').classList.toggle('open');
        document.getElementById('sidebar-backdrop').classList.toggle('open');
    });
    document.getElementById('sidebar-backdrop').addEventListener('click', () => {
        document.getElementById('sidebar').classList.remove('open');
        document.getElementById('sidebar-backdrop').classList.remove('open');
    });
    document.getElementById('topbar-notif').addEventListener('click', () => router.go('/notifications'));
    document.getElementById('topbar-msg').addEventListener('click', () => router.go('/messages'));

    // Sidebar link tıklamalarında mobile sidebar'ı kapat
    document.querySelectorAll('.sidebar-link').forEach(a => {
        a.addEventListener('click', () => {
            document.getElementById('sidebar').classList.remove('open');
            document.getElementById('sidebar-backdrop').classList.remove('open');
        });
    });

    highlightActive();
}

function sidebarLink(l) {
    return `<a class="sidebar-link" href="${l.href}" data-badge="${l.badgeKey || ''}">${l.icon}<span>${esc(l.label)}</span>${l.badgeKey && shellState[l.badgeKey] ? `<span class="badge-dot-inline">${shellState[l.badgeKey]}</span>` : ''}</a>`;
}

function highlightActive() {
    const hash = location.hash || '#/dashboard';
    document.querySelectorAll('.sidebar-link[href]').forEach(a => {
        const href = a.getAttribute('href');
        // startsWith kontrolü — alt path'ler için
        const active = hash === href || (href !== '#/' && hash.startsWith(href + '/')) || (href === '#/jobs' && hash.startsWith('#/jobs') && !hash.startsWith('#/my-jobs'));
        a.classList.toggle('active', active);
    });
}

function setTitle(title) {
    const el = document.getElementById('topbar-title');
    if (el) el.textContent = title;
}

function updateBadges() {
    const notifDot = document.getElementById('notif-dot');
    const msgDot = document.getElementById('msg-dot');
    if (notifDot) {
        notifDot.textContent = shellState.notifCount || '';
        notifDot.classList.toggle('hidden', !shellState.notifCount);
    }
    if (msgDot) {
        msgDot.textContent = shellState.unread || '';
        msgDot.classList.toggle('hidden', !shellState.unread);
    }
    // sidebar badge'leri
    document.querySelectorAll('.sidebar-link[data-badge]').forEach(a => {
        const key = a.dataset.badge;
        if (!key) return;
        let badge = a.querySelector('.badge-dot-inline');
        const count = shellState[key];
        if (count) {
            if (!badge) {
                badge = document.createElement('span');
                badge.className = 'badge-dot-inline';
                a.appendChild(badge);
            }
            badge.textContent = count;
        } else if (badge) {
            badge.remove();
        }
    });
}

async function refreshBadges() {
    if (!auth.isAuthenticated()) return;
    try {
        const [mc, nc] = await Promise.all([
            api.getUnreadMessageCount().catch(() => null),
            api.getUnreadNotificationCount().catch(() => null)
        ]);
        shellState.unread = mc?.data?.totalUnread ?? 0;
        shellState.notifCount = nc?.data?.unreadCount ?? 0;
        updateBadges();
    } catch { }
}

// ============ SignalR canlı olaylar ============

function bindSignalRBadges() {
    signalrUnsubs.forEach(u => u());
    signalrUnsubs = [];
    signalrUnsubs.push(signalr.on('ReceiveNotification', (n) => {
        shellState.notifCount = (shellState.notifCount || 0) + 1;
        updateBadges();
        toast(n?.title || 'Yeni bildirim', 'info');
    }));
    signalrUnsubs.push(signalr.on('ReceiveMessage', (m) => {
        // Mesajlar sayfasında değilse okunmamış say
        if (!location.hash.startsWith('#/messages')) {
            shellState.unread = (shellState.unread || 0) + 1;
            updateBadges();
        }
    }));
}

// ============ Router setup ============

function requireAuth(r) {
    if (!auth.isAuthenticated()) return { redirect: '#/login' };
    if (r.roles && r.roles.length > 0) {
        const has = r.roles.some(role => auth.hasRole(role));
        if (!has) return { redirect: '#/dashboard' };
    }
    return null;
}

function mountShellIfNeeded() {
    if (!document.querySelector('.app .sidebar')) {
        renderAppShell();
    } else {
        highlightActive();
    }
}

function renderView(title, htmlOrEl) {
    mountShellIfNeeded();
    setTitle(title);
    const view = document.getElementById('view');
    view.innerHTML = '';
    if (typeof htmlOrEl === 'string') view.innerHTML = htmlOrEl;
    else if (htmlOrEl instanceof Node) view.appendChild(htmlOrEl);
    highlightActive();
}

const ctx = { renderView, refreshBadges, api, auth, router, signalr, toast };

router.setAuthGate((r) => r.auth ? requireAuth(r) : null);

// ===== Public routes =====
router.register('login', () => renderLogin(document.getElementById('app'), { auth, router, toast, api }));
router.register('register', () => renderRegister(document.getElementById('app'), { auth, router, toast, api }));

// ===== Auth-only routes =====
router.register('dashboard', (arg) => renderDashboard(ctx, arg), { auth: true });
router.register('jobs', (arg) => renderJobsSearch(ctx, arg), { auth: true });
router.register('jobs/new', (arg) => renderJobEditor(ctx, arg), { auth: true, roles: ['Client'] });
router.register('jobs/:id/edit', (arg) => renderJobEditor(ctx, arg), { auth: true, roles: ['Client'] });
router.register('jobs/:id', (arg) => renderJobDetail(ctx, arg), { auth: true });
router.register('jobs/:id/proposals', (arg) => renderProposalsForJob(ctx, arg), { auth: true, roles: ['Client'] });
router.register('my-jobs', (arg) => renderMyJobs(ctx, arg), { auth: true, roles: ['Client'] });
router.register('freelancers', (arg) => renderFreelancers(ctx, arg), { auth: true });
router.register('freelancers/:id', (arg) => renderFreelancerDetail(ctx, arg), { auth: true });
router.register('proposals', (arg) => renderMyProposals(ctx, arg), { auth: true });
router.register('messages', (arg) => renderMessages(ctx, arg), { auth: true });
router.register('messages/:conversationId', (arg) => renderMessages(ctx, arg), { auth: true });
router.register('notifications', (arg) => renderNotifications(ctx, arg), { auth: true });
router.register('profile', (arg) => renderMyProfile(ctx, arg), { auth: true });
router.register('reviews', (arg) => renderMyReviews(ctx, arg), { auth: true });
router.register('admin/categories', (arg) => renderAdminCategories(ctx, arg), { auth: true, roles: ['Admin'] });
router.register('admin/skills', (arg) => renderAdminSkills(ctx, arg), { auth: true, roles: ['Admin'] });

// ============ Bootstrap ============

async function boot() {
    if (auth.isAuthenticated()) {
        await signalr.start();
        bindSignalRBadges();
        await refreshBadges();
        // Badge'leri periyodik yenile (SignalR düşerse fallback)
        setInterval(refreshBadges, 60000);
    } else if (!['#/login', '#/register'].some(p => location.hash.startsWith(p))) {
        location.hash = '#/login';
    }
    router.start();
}

// Login sonrası shell'i yeniden oluşturmak için event
window.addEventListener('staj-auth-changed', async () => {
    if (auth.isAuthenticated()) {
        await signalr.start();
        bindSignalRBadges();
        await refreshBadges();
    }
});

boot();

// Global click delegation — data-nav attribute'lu her şey router.go tetikler
document.addEventListener('click', (e) => {
    const nav = e.target.closest('[data-nav]');
    if (nav) {
        e.preventDefault();
        router.go(nav.dataset.nav);
    }
});