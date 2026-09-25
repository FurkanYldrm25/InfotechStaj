// views/proposals.js — kendi tekliflerim + ilana gelen başvurular

import {
    esc, initials, fullName, formatDate, formatDateTime,
    ProposalStatus, ProposalStatusBadge, ProposalKind,
    renderPagination, bindPagination, openModal, confirmModal, toast, icons
} from '../utils.js';

// ==================== Kendi tekliflerim ====================
export async function renderMyProposals(ctx, { query }) {
    const { renderView, api, router, auth } = ctx;
    const isFreelancer = auth.isFreelancer();
    const isClient = auth.isClient();

    // Direction: 1=Sent, 2=Received
    // Kullanıcı freelancer ise varsayılan: Sent (gönderdiği başvurular + aldığı direct offer'lar)
    // Client ise varsayılan: Received (kendi ilanlarına gelen başvurular + gönderdiği direct offer'lar)
    const direction = parseInt(query.direction || (isFreelancer ? '1' : '2'));

    renderView('Tekliflerim', `
        <div class="page-header">
            <div>
                <h1>Tekliflerim</h1>
                <div class="page-sub">Gönderdiğin ve aldığın tüm teklifleri buradan yönet.</div>
            </div>
        </div>
        <div class="tabs">
            <button class="tab ${direction === 1 ? 'active' : ''}" data-dir="1">${isFreelancer ? 'Gönderdiklerim' : 'Gönderdiğim doğrudan teklifler'}</button>
            <button class="tab ${direction === 2 ? 'active' : ''}" data-dir="2">${isFreelancer ? 'Aldığım doğrudan teklifler' : 'İlanlarıma gelenler'}</button>
        </div>
        <div class="filter-bar">
            <select class="select" id="kind" style="max-width:180px">
                <option value="">Tüm türler</option>
                <option value="1" ${query.kind === '1' ? 'selected' : ''}>Başvuru</option>
                <option value="2" ${query.kind === '2' ? 'selected' : ''}>Doğrudan teklif</option>
            </select>
            <select class="select" id="status" style="max-width:180px">
                <option value="">Tüm durumlar</option>
                <option value="1" ${query.status === '1' ? 'selected' : ''}>Beklemede</option>
                <option value="2" ${query.status === '2' ? 'selected' : ''}>Kabul edildi</option>
                <option value="3" ${query.status === '3' ? 'selected' : ''}>Reddedildi</option>
                <option value="4" ${query.status === '4' ? 'selected' : ''}>Geri çekildi</option>
            </select>
            <button class="btn btn-primary btn-sm" id="apply">Uygula</button>
        </div>
        <div id="prop-list"><div class="loading-box"><div class="spinner"></div></div></div>
    `);

    document.querySelectorAll('.tab[data-dir]').forEach(t => {
        t.addEventListener('click', () => {
            const d = t.dataset.dir;
            location.hash = '#/proposals?direction=' + d;
        });
    });
    document.getElementById('apply').onclick = () => {
        const q = { direction };
        const k = document.getElementById('kind').value;
        const s = document.getElementById('status').value;
        if (k) q.kind = k;
        if (s) q.status = s;
        location.hash = '#/proposals?' + new URLSearchParams(q).toString();
    };

    const list = document.getElementById('prop-list');
    try {
        const params = {
            direction,
            kind: query.kind || null,
            status: query.status || null,
            page: parseInt(query.page || '1'),
            pageSize: 15
        };
        const r = await api.getMyProposals(params);
        const p = r.data;
        if (!p || !p.items || p.items.length === 0) {
            list.innerHTML = `<div class="card"><div class="card-empty"><div class="empty-title">Teklif yok</div><div>Henüz eşleşen bir teklif bulunmuyor.</div></div></div>`;
            return;
        }
        list.innerHTML = `
            <div class="card tight">
                ${p.items.map(pp => proposalRow(pp, direction, isFreelancer)).join('')}
            </div>
            ${renderPagination(p)}
        `;
        list.querySelectorAll('.list-row[data-id]').forEach(r => {
            r.addEventListener('click', () => openProposalModal(ctx, r.dataset.id, direction));
        });
        bindPagination(list, page => {
            const q = { ...query, page, direction };
            location.hash = '#/proposals?' + new URLSearchParams(q).toString();
        });
    } catch (e) {
        list.innerHTML = `<div class="card text-danger">Yüklenemedi: ${esc(e?.message || '')}</div>`;
    }
}

function proposalRow(p, direction, isFreelancer) {
    // Karşı taraf kim?
    const otherName = direction === 1
        ? (p.kind === 1 ? fullName(p.clientFirstName, p.clientLastName) : fullName(p.freelancerFirstName, p.freelancerLastName))
        : (p.kind === 1 ? fullName(p.freelancerFirstName, p.freelancerLastName) : fullName(p.clientFirstName, p.clientLastName));
    return `
        <div class="list-row" data-id="${p.id}">
            <div class="avatar">${esc(otherName[0] || '?')}</div>
            <div class="grow">
                <div class="font-medium">${esc(p.jobPostTitle || (p.kind === 2 ? 'Doğrudan teklif' : ''))}</div>
                <div class="text-sm text-muted flex items-center gap-2 mt-1">
                    <span class="badge badge-muted">${esc(ProposalKind[p.kind])}</span>
                    <span>${esc(otherName)}</span>
                    ${p.proposedRate ? `<span>· ${p.currency || ''}${p.proposedRate}</span>` : ''}
                    ${p.proposedDurationDays ? `<span>· ${p.proposedDurationDays} gün</span>` : ''}
                    <span>· ${formatDate(p.createdAt)}</span>
                </div>
            </div>
            <span class="badge badge-${ProposalStatusBadge[p.status]}">${esc(ProposalStatus[p.status])}</span>
        </div>
    `;
}

async function openProposalModal(ctx, id, direction) {
    const { api } = ctx;
    let p;
    try {
        const r = await api.getProposal(id);
        p = r.data;
    } catch (e) { toast(e?.message || 'Yüklenemedi', 'error'); return; }

    const me = ctx.auth.getUser();
    const iAmFreelancer = me.userId === p.freelancerUserId;
    const iAmClient = me.userId === p.clientUserId;

    // Kim aksiyon alabilir?
    // Application → JobPost sahibi Client onaylar (yani karşı taraf Client)
    // DirectOffer → hedef Freelancer onaylar
    const canAccept = p.status === 1 && (
        (p.kind === 1 && iAmClient) ||
        (p.kind === 2 && iAmFreelancer)
    );
    const canReject = canAccept;
    // Gönderen taraf geri çekebilir
    const canWithdraw = p.status === 1 && (
        (p.kind === 1 && iAmFreelancer) ||
        (p.kind === 2 && iAmClient)
    );
    // Kabul edildiyse ve karşı tarafa yorum yazma hakkı varsa "Değerlendir" butonu
    const canReview = p.status === 2;

    const modal = openModal({
        title: p.jobPostTitle || (p.kind === 2 ? 'Doğrudan teklif' : 'Teklif'),
        wide: true,
        body: `
            <div class="flex items-center gap-3 mb-3">
                <span class="badge badge-muted">${esc(ProposalKind[p.kind])}</span>
                <span class="badge badge-${ProposalStatusBadge[p.status]}">${esc(ProposalStatus[p.status])}</span>
                <span class="text-muted">${formatDateTime(p.createdAt)}</span>
            </div>

            <div class="grid grid-2 mb-4">
                <div>
                    <div class="section-title">Freelancer</div>
                    <div class="flex items-center gap-2 mt-1">
                        <div class="avatar">${esc(initials(p.freelancerFirstName, p.freelancerLastName))}</div>
                        <div>
                            <div class="font-medium">${esc(fullName(p.freelancerFirstName, p.freelancerLastName))}</div>
                            <div class="text-muted text-sm">${esc(p.freelancerTitle || '')}</div>
                        </div>
                    </div>
                </div>
                <div>
                    <div class="section-title">Client</div>
                    <div class="flex items-center gap-2 mt-1">
                        <div class="avatar">${esc(initials(p.clientFirstName, p.clientLastName))}</div>
                        <div>
                            <div class="font-medium">${esc(fullName(p.clientFirstName, p.clientLastName))}</div>
                            <div class="text-muted text-sm">${esc(p.clientCompanyName || '')}</div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="section-title">Teklif metni</div>
            <div class="mb-3" style="white-space:pre-wrap">${esc(p.coverMessage)}</div>

            <div class="grid grid-3 mb-3">
                <div><div class="section-title">Önerilen ücret</div><div class="font-medium mt-1">${p.proposedRate ? esc((p.currency || '') + p.proposedRate) : '—'}</div></div>
                <div><div class="section-title">Tahmini süre</div><div class="font-medium mt-1">${p.proposedDurationDays ? p.proposedDurationDays + ' gün' : '—'}</div></div>
                <div><div class="section-title">Yanıt tarihi</div><div class="font-medium mt-1">${p.respondedAt ? formatDate(p.respondedAt) : '—'}</div></div>
            </div>

            ${p.responseNote ? `<div class="section-title">Yanıt notu</div><div class="text-muted mb-3" style="white-space:pre-wrap">${esc(p.responseNote)}</div>` : ''}
        `,
        footer: `
            ${p.jobPostId ? `<button class="btn btn-ghost btn-sm" data-nav="/jobs/${p.jobPostId}">İlanı aç</button>` : ''}
            <div class="grow"></div>
            ${canWithdraw ? `<button class="btn btn-ghost" data-act="withdraw">Geri çek</button>` : ''}
            ${canReject ? `<button class="btn btn-danger" data-act="reject">Reddet</button>` : ''}
            ${canAccept ? `<button class="btn btn-success" data-act="accept">Kabul et</button>` : ''}
            ${canReview ? `<button class="btn btn-primary" data-act="review">Değerlendir</button>` : ''}
        `
    });

    modal.footer.querySelector('[data-act=accept]')?.addEventListener('click', async () => {
        try { await api.acceptProposal(p.id, null); toast('Teklif kabul edildi.', 'success'); modal.close(); location.reload(); }
        catch (e) { toast(e?.message || 'Hata', 'error'); }
    });
    modal.footer.querySelector('[data-act=reject]')?.addEventListener('click', async () => {
        if (!await confirmModal('Teklifi reddetmek istediğinden emin misin?', { okKind: 'danger', okLabel: 'Reddet' })) return;
        try { await api.rejectProposal(p.id, null); toast('Teklif reddedildi.', 'warning'); modal.close(); location.reload(); }
        catch (e) { toast(e?.message || 'Hata', 'error'); }
    });
    modal.footer.querySelector('[data-act=withdraw]')?.addEventListener('click', async () => {
        if (!await confirmModal('Teklifi geri çekmek istediğinden emin misin?')) return;
        try { await api.withdrawProposal(p.id); toast('Teklif geri çekildi.', 'success'); modal.close(); location.reload(); }
        catch (e) { toast(e?.message || 'Hata', 'error'); }
    });
    modal.footer.querySelector('[data-act=review]')?.addEventListener('click', () => {
        modal.close();
        showReviewModal(ctx, p);
    });
}

function showReviewModal(ctx, p) {
    const { api } = ctx;
    const modal = openModal({
        title: 'Değerlendirme yaz',
        body: `
            <div class="text-muted mb-3">"${esc(p.jobPostTitle || 'Doğrudan teklif')}" için karşı tarafa değerlendirme.</div>
            <form id="rev-form">
                <div class="form-group">
                    <label class="form-label required">Puan</label>
                    <div class="star-picker" id="star-picker">
                        ${[1, 2, 3, 4, 5].map(i => `<span data-v="${i}">★</span>`).join('')}
                    </div>
                    <input type="hidden" name="rating" required />
                </div>
                <div class="form-group">
                    <label class="form-label required">Yorum</label>
                    <textarea class="textarea" name="comment" rows="5" minlength="10" maxlength="2000" required></textarea>
                    <div class="form-hint">10-2000 karakter arası.</div>
                </div>
            </form>
        `,
        footer: `
            <button class="btn btn-secondary" data-act="cancel">Vazgeç</button>
            <button class="btn btn-primary" data-act="submit">Gönder</button>
        `
    });
    const stars = modal.body.querySelectorAll('#star-picker span');
    const ratingInput = modal.body.querySelector('input[name=rating]');
    stars.forEach(s => {
        s.addEventListener('click', () => {
            const v = parseInt(s.dataset.v);
            ratingInput.value = v;
            stars.forEach(x => x.classList.toggle('filled', parseInt(x.dataset.v) <= v));
        });
    });
    modal.footer.querySelector('[data-act=cancel]').onclick = modal.close;
    modal.footer.querySelector('[data-act=submit]').onclick = async () => {
        const form = modal.body.querySelector('form');
        const fd = new FormData(form);
        if (!fd.get('rating')) { toast('Puan seç.', 'warning'); return; }
        try {
            await api.createReview({
                proposalId: p.id,
                rating: parseInt(fd.get('rating')),
                comment: fd.get('comment')
            });
            modal.close();
            toast('Değerlendirmen kaydedildi.', 'success');
        } catch (e) { toast(e?.message || 'Değerlendirme gönderilemedi.', 'error'); }
    };
}

// ==================== İlana gelen başvurular (Client) ====================
export async function renderProposalsForJob(ctx, { params }) {
    const { renderView, api } = ctx;
    renderView('İlan başvuruları', `<div class="loading-box"><div class="spinner"></div></div>`);
    const view = document.getElementById('view');

    let job = null;
    let proposals = [];
    try {
        const [j, r] = await Promise.all([
            api.getJob(params.id),
            api.getProposalsForJob(params.id)
        ]);
        job = j.data;
        proposals = r.data || [];
    } catch (e) {
        view.innerHTML = `<div class="card text-danger">Yüklenemedi: ${esc(e?.message || '')}</div>`;
        return;
    }

    view.innerHTML = `
        <div class="page-header">
            <div>
                <button class="btn btn-ghost btn-sm" data-nav="/jobs/${job.id}">${icons.back} İlana dön</button>
                <h1 class="mt-2">${esc(job.title)}</h1>
                <div class="page-sub">${proposals.length} başvuru</div>
            </div>
        </div>
        ${proposals.length ? `
            <div class="card tight">
                ${proposals.map(p => `
                    <div class="list-row" data-id="${p.id}">
                        <div class="avatar avatar-lg">${esc(initials(p.freelancerFirstName, p.freelancerLastName))}</div>
                        <div class="grow">
                            <div class="font-medium">${esc(fullName(p.freelancerFirstName, p.freelancerLastName))}</div>
                            <div class="text-muted text-sm">${esc(p.freelancerTitle || '')}</div>
                            <div class="text-sm mt-1">
                                ${p.proposedRate ? `<span class="text-muted">Ücret: <b>${esc((p.currency || '') + p.proposedRate)}</b></span>` : ''}
                                ${p.proposedDurationDays ? `<span class="text-muted ml-2">· Süre: <b>${p.proposedDurationDays} gün</b></span>` : ''}
                                <span class="text-muted ml-2">· ${formatDate(p.createdAt)}</span>
                            </div>
                        </div>
                        <span class="badge badge-${ProposalStatusBadge[p.status]}">${esc(ProposalStatus[p.status])}</span>
                    </div>
                `).join('')}
            </div>
        ` : `<div class="card"><div class="card-empty"><div class="empty-title">Henüz başvuru yok</div><div>İlana başvurular geldikçe burada görünecek.</div></div></div>`}
    `;

    view.querySelectorAll('.list-row[data-id]').forEach(r => {
        r.addEventListener('click', () => openProposalModal(ctx, r.dataset.id, 2));
    });
}