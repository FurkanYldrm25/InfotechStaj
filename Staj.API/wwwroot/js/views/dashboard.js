// views/dashboard.js — ana sayfa özet

import { esc, initials, fullName, timeAgo, formatDate, formatBudget } from '../utils.js';
import { JobPostStatus, JobPostStatusBadge, ProposalStatus, ProposalStatusBadge, NotificationType } from '../utils.js';

export async function renderDashboard(ctx) {
    const { renderView, api, auth } = ctx;
    const user = auth.getUser();
    const roles = user?.roles || [];
    const isFreelancer = roles.includes('Freelancer');
    const isClient = roles.includes('Client');

    renderView('Ana sayfa', `
        <div class="page-header">
            <div>
                <h1>Merhaba, ${esc(user?.firstName || '')} 👋</h1>
                <div class="page-sub">Bugün platformda neler olduğuna göz at.</div>
            </div>
        </div>
        <div id="dash-content"><div class="loading-box"><div class="spinner"></div></div></div>
    `);

    const container = document.getElementById('dash-content');

    try {
        const [unreadMsg, unreadNotif, notif, proposals, jobs] = await Promise.all([
            api.getUnreadMessageCount().catch(() => ({ data: { totalUnread: 0 } })),
            api.getUnreadNotificationCount().catch(() => ({ data: { unreadCount: 0 } })),
            api.getNotifications({ page: 1, pageSize: 5 }).catch(() => ({ data: { items: [] } })),
            isFreelancer || isClient
                ? api.getMyProposals({ direction: isFreelancer ? 1 : 2, page: 1, pageSize: 5 }).catch(() => ({ data: { items: [] } }))
                : Promise.resolve({ data: { items: [] } }),
            isClient
                ? api.getMyJobs({ page: 1, pageSize: 5 }).catch(() => ({ data: { items: [] } }))
                : api.searchJobs({ page: 1, pageSize: 5 }).catch(() => ({ data: { items: [] } }))
        ]);

        const notifItems = notif.data?.items || [];
        const propItems = proposals.data?.items || [];
        const jobItems = jobs.data?.items || [];

        container.innerHTML = `
            <div class="grid grid-4 mb-4">
                <div class="card stat">
                    <div class="stat-label">Okunmamış mesaj</div>
                    <div class="stat-value">${unreadMsg.data?.totalUnread ?? 0}</div>
                    <div class="stat-sub"><a href="#/messages">Sohbetlere git →</a></div>
                </div>
                <div class="card stat">
                    <div class="stat-label">Yeni bildirim</div>
                    <div class="stat-value">${unreadNotif.data?.unreadCount ?? 0}</div>
                    <div class="stat-sub"><a href="#/notifications">Tümünü gör →</a></div>
                </div>
                <div class="card stat">
                    <div class="stat-label">${isClient ? 'İlanlarım' : 'Aktif ilanlar'}</div>
                    <div class="stat-value">${jobs.data?.totalCount ?? 0}</div>
                    <div class="stat-sub"><a href="${isClient ? '#/my-jobs' : '#/jobs'}">Detay →</a></div>
                </div>
                <div class="card stat">
                    <div class="stat-label">${isFreelancer ? 'Başvurularım' : 'Alınan teklifler'}</div>
                    <div class="stat-value">${proposals.data?.totalCount ?? 0}</div>
                    <div class="stat-sub"><a href="#/proposals">Detay →</a></div>
                </div>
            </div>

            <div class="grid grid-2">
                <div class="card tight">
                    <div class="card-header">
                        <div class="card-title">Son bildirimler</div>
                        <a class="text-sm" href="#/notifications">Tümü</a>
                    </div>
                    <div>
                        ${notifItems.length ? notifItems.map(n => `
                            <div class="list-row static">
                                <div class="avatar" style="background:var(--bg);color:var(--text-muted)">${NotificationType[n.type]?.icon || '🔔'}</div>
                                <div class="grow">
                                    <div class="font-medium">${esc(n.title)}</div>
                                    <div class="text-sm text-muted line-clamp-2">${esc(n.body)}</div>
                                </div>
                                <div class="text-sm text-soft shrink-0">${timeAgo(n.createdAt)}</div>
                            </div>
                        `).join('') : '<div class="card-empty"><div class="empty-title">Bildirim yok</div><div>Yeni etkinlikler burada görünecek.</div></div>'}
                    </div>
                </div>

                <div class="card tight">
                    <div class="card-header">
                        <div class="card-title">${isFreelancer ? 'Son başvurularım' : 'Gelen teklifler'}</div>
                        <a class="text-sm" href="#/proposals">Tümü</a>
                    </div>
                    <div>
                        ${propItems.length ? propItems.map(p => `
                            <div class="list-row" data-nav="/proposals">
                                <div class="grow">
                                    <div class="font-medium truncate">${esc(p.jobPostTitle || 'Doğrudan teklif')}</div>
                                    <div class="text-sm text-muted">${esc(fullName(isFreelancer ? p.clientFirstName : p.freelancerFirstName, isFreelancer ? p.clientLastName : p.freelancerLastName))}</div>
                                </div>
                                <span class="badge badge-${ProposalStatusBadge[p.status]}">${esc(ProposalStatus[p.status])}</span>
                            </div>
                        `).join('') : '<div class="card-empty"><div class="empty-title">Henüz teklif yok</div><div>İlanlara başvurmaya başlayabilirsin.</div></div>'}
                    </div>
                </div>
            </div>

            <div class="card tight mt-4">
                <div class="card-header">
                    <div class="card-title">${isClient ? 'Son ilanlarım' : 'Yeni ilanlar'}</div>
                    <a class="text-sm" href="${isClient ? '#/my-jobs' : '#/jobs'}">Tümü</a>
                </div>
                <div>
                    ${jobItems.length ? jobItems.map(j => `
                        <div class="list-row" data-nav="/jobs/${j.id}">
                            <div class="grow">
                                <div class="font-medium">${esc(j.title)}</div>
                                <div class="text-sm text-muted flex items-center gap-2 mt-1">
                                    <span class="badge badge-muted">${esc(j.categoryName)}</span>
                                    <span>${esc(formatBudget(j.budgetMin, j.budgetMax, j.currency))}</span>
                                    ${j.city ? `<span>· ${esc(j.city)}</span>` : ''}
                                </div>
                            </div>
                            <span class="badge badge-${JobPostStatusBadge[j.status]}">${esc(JobPostStatus[j.status])}</span>
                        </div>
                    `).join('') : `<div class="card-empty"><div class="empty-title">Henüz ilan yok</div><div>${isClient ? 'İlk ilanını oluştur.' : 'Yeni ilanlar burada görünecek.'}</div></div>`}
                </div>
            </div>
        `;
    } catch (err) {
        container.innerHTML = `<div class="card"><div class="text-danger">Yüklenirken bir hata oluştu: ${esc(err?.message || '')}</div></div>`;
    }
}