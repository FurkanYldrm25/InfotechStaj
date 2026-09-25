// views/jobs.js — ilan arama, detay, kendi ilanlarım, editor (create/update)

import {
    esc, formatDate, formatBudget, JobPostStatus, JobPostStatusBadge, WorkMode, WorkModeOptions,
    renderPagination, bindPagination, openModal, confirmModal, toast, icons, fullName
} from '../utils.js';

// ==================== İlan arama listesi ====================
export async function renderJobsSearch(ctx, { query }) {
    const { renderView, api, router, auth } = ctx;

    renderView('İlanlar', `
        <div class="page-header">
            <div>
                <h1>İlanlar</h1>
                <div class="page-sub">Yayınlanan tüm ilanları ara ve filtrele.</div>
            </div>
            ${auth.isClient() ? `<a class="btn btn-primary" href="#/jobs/new">${icons.plus} Yeni ilan</a>` : ''}
        </div>
        <div class="filter-bar">
            <input class="input" id="q" placeholder="🔍 Anahtar kelime..." value="${esc(query.keyword || '')}" style="min-width:220px" />
            <select class="select" id="cat"><option value="">Tüm kategoriler</option></select>
            <select class="select" id="wm">
                <option value="">Çalışma tipi</option>
                ${WorkModeOptions.map(o => `<option value="${o.value}" ${String(query.workMode) === String(o.value) ? 'selected' : ''}>${o.label}</option>`).join('')}
            </select>
            <input class="input" id="minB" type="number" placeholder="Min bütçe" value="${esc(query.minBudget || '')}" style="max-width:130px" />
            <input class="input" id="maxB" type="number" placeholder="Max bütçe" value="${esc(query.maxBudget || '')}" style="max-width:130px" />
            <input class="input" id="city" placeholder="Şehir" value="${esc(query.city || '')}" style="max-width:140px" />
            <select class="select" id="sort" style="max-width:170px">
                <option value="newest">En yeni</option>
                <option value="oldest" ${query.sortBy === 'oldest' ? 'selected' : ''}>En eski</option>
                <option value="budget_asc" ${query.sortBy === 'budget_asc' ? 'selected' : ''}>Bütçe ↑</option>
                <option value="budget_desc" ${query.sortBy === 'budget_desc' ? 'selected' : ''}>Bütçe ↓</option>
            </select>
            <button class="btn btn-primary btn-sm" id="apply-filter">Uygula</button>
            <button class="btn btn-ghost btn-sm" id="clear-filter">Temizle</button>
        </div>
        <div id="jobs-list"><div class="loading-box"><div class="spinner"></div></div></div>
    `);

    // Kategorileri yükle
    try {
        const cats = await api.getCategories();
        const catSel = document.getElementById('cat');
        (cats.data || []).forEach(c => {
            const opt = document.createElement('option');
            opt.value = c.id; opt.textContent = c.name;
            if (query.categoryId === c.id) opt.selected = true;
            catSel.appendChild(opt);
        });
    } catch { }

    const applyFilters = () => {
        const q = {};
        const kv = document.getElementById('q').value.trim();
        const cat = document.getElementById('cat').value;
        const wm = document.getElementById('wm').value;
        const minB = document.getElementById('minB').value;
        const maxB = document.getElementById('maxB').value;
        const city = document.getElementById('city').value.trim();
        const sort = document.getElementById('sort').value;
        if (kv) q.keyword = kv;
        if (cat) q.categoryId = cat;
        if (wm) q.workMode = wm;
        if (minB) q.minBudget = minB;
        if (maxB) q.maxBudget = maxB;
        if (city) q.city = city;
        if (sort && sort !== 'newest') q.sortBy = sort;
        const qs = new URLSearchParams(q).toString();
        location.hash = '#/jobs' + (qs ? '?' + qs : '');
    };
    document.getElementById('apply-filter').onclick = applyFilters;
    document.getElementById('q').addEventListener('keydown', e => { if (e.key === 'Enter') applyFilters(); });
    document.getElementById('clear-filter').onclick = () => { location.hash = '#/jobs'; };

    // İlanları yükle
    const list = document.getElementById('jobs-list');
    try {
        const params = {
            page: parseInt(query.page || '1'),
            pageSize: 10,
            keyword: query.keyword,
            categoryId: query.categoryId,
            workMode: query.workMode,
            minBudget: query.minBudget,
            maxBudget: query.maxBudget,
            city: query.city,
            sortBy: query.sortBy
        };
        const r = await api.searchJobs(params);
        const p = r.data;
        if (!p || !p.items || p.items.length === 0) {
            list.innerHTML = `<div class="card"><div class="card-empty"><div class="empty-title">İlan bulunamadı</div><div>Filtreleri değiştirmeyi dene.</div></div></div>`;
            return;
        }
        list.innerHTML = `
            <div class="grid" style="gap:12px">
                ${p.items.map(j => jobCard(j)).join('')}
            </div>
            ${renderPagination(p)}
        `;
        list.querySelectorAll('.job-card').forEach(c => {
            c.addEventListener('click', () => router.go('/jobs/' + c.dataset.id));
        });
        bindPagination(list, (page) => {
            const q = { ...query, page };
            const qs = new URLSearchParams(q).toString();
            location.hash = '#/jobs?' + qs;
        });
    } catch (ex) {
        list.innerHTML = `<div class="card"><div class="text-danger">Yüklenemedi: ${esc(ex?.message || '')}</div></div>`;
    }
}

function jobCard(j) {
    return `
        <div class="job-card" data-id="${j.id}">
            <div class="flex items-start justify-between gap-3">
                <div class="grow">
                    <div class="flex items-center gap-2 mb-2">
                        <span class="badge badge-primary">${esc(j.categoryName)}</span>
                        <span class="badge badge-muted">${esc(WorkMode[j.workMode] || '')}</span>
                        ${j.durationDays ? `<span class="badge badge-muted">${j.durationDays} gün</span>` : ''}
                    </div>
                    <h3 class="mb-2">${esc(j.title)}</h3>
                    <div class="chip-list mb-3">
                        ${(j.skills || []).slice(0, 6).map(s => `<span class="chip">${esc(s)}</span>`).join('')}
                    </div>
                    <div class="text-sm text-muted flex gap-3 items-center">
                        <span>${esc(j.clientCompanyName || 'Bireysel')}</span>
                        ${j.city ? `<span>· ${esc(j.city)}</span>` : ''}
                        <span>· ${formatDate(j.publishedAt || j.createdAt)}</span>
                    </div>
                </div>
                <div class="text-right shrink-0">
                    <div class="font-semibold text-lg">${esc(formatBudget(j.budgetMin, j.budgetMax, j.currency))}</div>
                    <span class="badge badge-${JobPostStatusBadge[j.status]} mt-1">${esc(JobPostStatus[j.status])}</span>
                </div>
            </div>
        </div>
    `;
}

// ==================== İlan detayı ====================
export async function renderJobDetail(ctx, { params }) {
    const { renderView, api, router, auth } = ctx;
    renderView('İlan detayı', `<div class="loading-box"><div class="spinner"></div></div>`);
    const view = document.getElementById('view');

    let job;
    try {
        const r = await api.getJob(params.id);
        job = r.data;
    } catch (ex) {
        view.innerHTML = `<div class="card"><div class="text-danger">İlan bulunamadı: ${esc(ex?.message || '')}</div></div>`;
        return;
    }

    const me = auth.getUser();
    const isOwner = me && me.userId === job.clientUserId;
    const canApply = auth.isFreelancer() && !isOwner && job.status === 2;

    view.innerHTML = `
        <div class="page-header">
            <div>
                <button class="btn btn-ghost btn-sm" data-nav="/jobs">${icons.back} İlanlara dön</button>
                <h1 class="mt-2">${esc(job.title)}</h1>
                <div class="page-sub">${esc(job.categoryName)} · ${esc(WorkMode[job.workMode] || '')} · ${formatDate(job.publishedAt || job.createdAt)}</div>
            </div>
            <div class="flex gap-2">
                ${isOwner ? ownerActions(job) : ''}
                ${canApply ? `<button class="btn btn-primary" id="apply-btn">Başvur</button>` : ''}
            </div>
        </div>

        <div class="grid grid-sidebar">
            <div>
                <div class="card">
                    <div class="section-title">Açıklama</div>
                    <div style="white-space:pre-wrap">${esc(job.description)}</div>
                </div>
                <div class="card mt-4">
                    <div class="section-title">Gerekli yetkinlikler</div>
                    <div class="chip-list">
                        ${(job.skills || []).map(s => `<span class="chip">${esc(s.name)}</span>`).join('') || '<span class="text-muted">Belirtilmemiş</span>'}
                    </div>
                </div>
                ${isOwner ? `
                    <div class="card mt-4">
                        <div class="flex items-center justify-between mb-3">
                            <div class="section-title" style="margin:0">Gelen başvurular</div>
                            <a class="btn btn-secondary btn-sm" href="#/jobs/${job.id}/proposals">Tümünü yönet</a>
                        </div>
                        <div id="proposals-preview" class="text-muted">Yükleniyor...</div>
                    </div>
                ` : ''}
            </div>
            <aside>
                <div class="card">
                    <div class="section-title">Bütçe</div>
                    <div class="text-xl font-semibold">${esc(formatBudget(job.budgetMin, job.budgetMax, job.currency))}</div>
                    ${job.durationDays ? `<div class="text-muted mt-2">Tahmini süre: <b>${job.durationDays} gün</b></div>` : ''}
                    <div class="text-muted mt-1">Çalışma: <b>${esc(WorkMode[job.workMode] || '—')}</b></div>
                    ${(job.city || job.country) ? `<div class="text-muted mt-1">Konum: <b>${esc([job.city, job.country].filter(Boolean).join(', '))}</b></div>` : ''}
                    <div class="mt-3"><span class="badge badge-${JobPostStatusBadge[job.status]}">${esc(JobPostStatus[job.status])}</span></div>
                </div>
                <div class="card mt-4">
                    <div class="section-title">Veren</div>
                    <div class="flex items-center gap-3">
                        <div class="avatar avatar-lg">${esc((job.clientFirstName || '?')[0])}${esc((job.clientLastName || '')[0] || '')}</div>
                        <div>
                            <div class="font-semibold">${esc(fullName(job.clientFirstName, job.clientLastName))}</div>
                            ${job.clientCompanyName ? `<div class="text-muted text-sm">${esc(job.clientCompanyName)}</div>` : ''}
                        </div>
                    </div>
                </div>
            </aside>
        </div>
    `;

    // Owner: proposals preview
    if (isOwner) {
        try {
            const r = await api.getProposalsForJob(job.id);
            const items = r.data || [];
            const el = document.getElementById('proposals-preview');
            if (items.length === 0) {
                el.innerHTML = 'Henüz başvuru yok.';
            } else {
                el.innerHTML = items.slice(0, 3).map(p => `
                    <div class="list-row static" style="padding:10px 0">
                        <div class="avatar">${esc((p.freelancerFirstName || '?')[0])}${esc((p.freelancerLastName || '')[0] || '')}</div>
                        <div class="grow">
                            <div class="font-medium">${esc(fullName(p.freelancerFirstName, p.freelancerLastName))}</div>
                            <div class="text-sm text-muted">${esc(p.freelancerTitle || '')}</div>
                        </div>
                        <span class="badge badge-warning">${p.proposedRate ? p.currency + p.proposedRate : ''}</span>
                    </div>
                `).join('');
            }
        } catch { }
    }

    // Owner actions
    if (isOwner) {
        view.querySelector('#publish-btn')?.addEventListener('click', async () => {
            if (!await confirmModal('İlanı yayına almak istediğinden emin misin?')) return;
            try { await api.publishJob(job.id); toast('İlan yayına alındı.', 'success'); renderJobDetail(ctx, { params }); }
            catch (e) { toast(e?.message || 'Hata', 'error'); }
        });
        view.querySelector('#close-btn')?.addEventListener('click', async () => {
            if (!await confirmModal('İlanı kapatmak istediğinden emin misin?')) return;
            try { await api.closeJob(job.id); toast('İlan kapatıldı.', 'success'); renderJobDetail(ctx, { params }); }
            catch (e) { toast(e?.message || 'Hata', 'error'); }
        });
        view.querySelector('#cancel-btn')?.addEventListener('click', async () => {
            if (!await confirmModal('İlanı iptal etmek istediğinden emin misin?', { okKind: 'danger', okLabel: 'İptal et' })) return;
            try { await api.cancelJob(job.id); toast('İlan iptal edildi.', 'warning'); renderJobDetail(ctx, { params }); }
            catch (e) { toast(e?.message || 'Hata', 'error'); }
        });
        view.querySelector('#delete-btn')?.addEventListener('click', async () => {
            if (!await confirmModal('İlanı kalıcı olarak silmek istediğinden emin misin?', { okKind: 'danger', okLabel: 'Sil' })) return;
            try { await api.deleteJob(job.id); toast('İlan silindi.', 'success'); router.go('/my-jobs'); }
            catch (e) { toast(e?.message || 'Hata', 'error'); }
        });
    }

    // Apply modal
    view.querySelector('#apply-btn')?.addEventListener('click', () => showApplyModal(ctx, job));
}

function ownerActions(job) {
    const parts = [];
    parts.push(`<a class="btn btn-secondary btn-sm" href="#/jobs/${job.id}/edit">${icons.edit} Düzenle</a>`);
    if (job.status === 1) parts.push(`<button class="btn btn-primary btn-sm" id="publish-btn">Yayına al</button>`);
    if (job.status === 2) parts.push(`<button class="btn btn-secondary btn-sm" id="close-btn">Kapat</button>`);
    if (job.status === 1 || job.status === 2) parts.push(`<button class="btn btn-ghost btn-sm" id="cancel-btn">İptal et</button>`);
    if (job.status === 1) parts.push(`<button class="btn btn-danger btn-sm" id="delete-btn">${icons.trash}</button>`);
    return parts.join('');
}

function showApplyModal(ctx, job) {
    const { api, router, toast } = ctx;
    const modal = openModal({
        title: 'İlana başvur',
        wide: true,
        body: `
            <form id="apply-form">
                <div class="text-muted mb-3">"${esc(job.title)}" ilanına başvuruyorsun.</div>
                <div class="form-group">
                    <label class="form-label required">Ön yazı</label>
                    <textarea class="textarea" name="coverMessage" rows="6" required placeholder="Kendini kısaca tanıt, neden bu iş için uygun olduğunu anlat..."></textarea>
                </div>
                <div class="form-row-3">
                    <div class="form-group">
                        <label class="form-label">Önerdiğin ücret</label>
                        <input class="input" name="proposedRate" type="number" step="0.01" placeholder="0" />
                    </div>
                    <div class="form-group">
                        <label class="form-label">Para birimi</label>
                        <input class="input" name="currency" maxlength="3" placeholder="TRY" value="${esc(job.currency || 'TRY')}" />
                    </div>
                    <div class="form-group">
                        <label class="form-label">Tahmini süre (gün)</label>
                        <input class="input" name="proposedDurationDays" type="number" placeholder="30" />
                    </div>
                </div>
            </form>
        `,
        footer: `
            <button class="btn btn-secondary" data-act="cancel">Vazgeç</button>
            <button class="btn btn-primary" data-act="submit">Başvuruyu gönder</button>
        `
    });
    modal.footer.querySelector('[data-act=cancel]').onclick = modal.close;
    modal.footer.querySelector('[data-act=submit]').onclick = async () => {
        const form = modal.body.querySelector('form');
        const fd = new FormData(form);
        const payload = {
            jobPostId: job.id,
            coverMessage: fd.get('coverMessage'),
            proposedRate: fd.get('proposedRate') ? parseFloat(fd.get('proposedRate')) : null,
            proposedDurationDays: fd.get('proposedDurationDays') ? parseInt(fd.get('proposedDurationDays')) : null,
            currency: fd.get('currency') || null
        };
        try {
            await api.submitApplication(payload);
            modal.close();
            toast('Başvurun gönderildi.', 'success');
            router.go('/proposals');
        } catch (e) {
            toast(e?.message || 'Başvuru gönderilemedi.', 'error');
        }
    };
}

// ==================== Kendi ilanlarım (Client) ====================
export async function renderMyJobs(ctx, { query }) {
    const { renderView, api, router } = ctx;
    renderView('İlanlarım', `
        <div class="page-header">
            <div>
                <h1>İlanlarım</h1>
                <div class="page-sub">Açtığın tüm ilanları burada yönet.</div>
            </div>
            <a class="btn btn-primary" href="#/jobs/new">${icons.plus} Yeni ilan</a>
        </div>
        <div class="tabs">
            <button class="tab ${!query.status ? 'active' : ''}" data-status="">Tümü</button>
            <button class="tab ${query.status === '1' ? 'active' : ''}" data-status="1">Taslak</button>
            <button class="tab ${query.status === '2' ? 'active' : ''}" data-status="2">Yayında</button>
            <button class="tab ${query.status === '3' ? 'active' : ''}" data-status="3">Kapalı</button>
            <button class="tab ${query.status === '4' ? 'active' : ''}" data-status="4">İptal</button>
        </div>
        <div id="my-jobs-list"><div class="loading-box"><div class="spinner"></div></div></div>
    `);

    document.querySelectorAll('.tab[data-status]').forEach(t => {
        t.addEventListener('click', () => {
            const s = t.dataset.status;
            location.hash = '#/my-jobs' + (s ? '?status=' + s : '');
        });
    });

    const list = document.getElementById('my-jobs-list');
    try {
        const params = { page: parseInt(query.page || '1'), pageSize: 15, status: query.status };
        const r = await api.getMyJobs(params);
        const p = r.data;
        if (!p || !p.items || p.items.length === 0) {
            list.innerHTML = `<div class="card"><div class="card-empty"><div class="empty-title">İlan yok</div><div>Yeni ilan oluşturarak başlayabilirsin.</div></div></div>`;
            return;
        }
        list.innerHTML = `
            <div class="card tight">
                <table class="table hoverable">
                    <thead>
                        <tr><th>Başlık</th><th>Kategori</th><th>Bütçe</th><th>Durum</th><th>Tarih</th></tr>
                    </thead>
                    <tbody>
                        ${p.items.map(j => `
                            <tr data-id="${j.id}">
                                <td class="font-medium">${esc(j.title)}</td>
                                <td>${esc(j.categoryName)}</td>
                                <td>${esc(formatBudget(j.budgetMin, j.budgetMax, j.currency))}</td>
                                <td><span class="badge badge-${JobPostStatusBadge[j.status]}">${esc(JobPostStatus[j.status])}</span></td>
                                <td class="text-muted">${formatDate(j.publishedAt || j.createdAt)}</td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
            </div>
            ${renderPagination(p)}
        `;
        list.querySelectorAll('tr[data-id]').forEach(r => r.addEventListener('click', () => router.go('/jobs/' + r.dataset.id)));
        bindPagination(list, (page) => {
            const q = { ...query, page };
            const qs = new URLSearchParams(q).toString();
            location.hash = '#/my-jobs?' + qs;
        });
    } catch (e) {
        list.innerHTML = `<div class="card text-danger">Yüklenemedi: ${esc(e?.message || '')}</div>`;
    }
}

// ==================== İlan editor (create / edit) ====================
export async function renderJobEditor(ctx, { params }) {
    const { renderView, api, router } = ctx;
    const isEdit = !!params.id;
    renderView(isEdit ? 'İlanı düzenle' : 'Yeni ilan', `<div class="loading-box"><div class="spinner"></div></div>`);
    const view = document.getElementById('view');

    let job = null;
    let categories = [];
    let allSkills = [];
    try {
        const [cats, skills] = await Promise.all([api.getCategories(), api.getSkills()]);
        categories = cats.data || [];
        allSkills = skills.data || [];
        if (isEdit) {
            const r = await api.getJob(params.id);
            job = r.data;
        }
    } catch (e) {
        view.innerHTML = `<div class="card text-danger">Yüklenemedi: ${esc(e?.message || '')}</div>`;
        return;
    }

    view.innerHTML = `
        <div class="page-header">
            <div>
                <button class="btn btn-ghost btn-sm" data-nav="/my-jobs">${icons.back} İlanlarıma dön</button>
                <h1 class="mt-2">${isEdit ? 'İlanı düzenle' : 'Yeni ilan oluştur'}</h1>
            </div>
        </div>
        <form id="job-form" class="card" style="max-width:820px">
            <div class="form-group">
                <label class="form-label required">Başlık</label>
                <input class="input" name="title" required maxlength="200" value="${esc(job?.title || '')}" />
            </div>
            <div class="form-row">
                <div class="form-group">
                    <label class="form-label required">Kategori</label>
                    <select class="select" name="categoryId" required>
                        <option value="">Seç...</option>
                        ${categories.map(c => `<option value="${c.id}" ${job?.categoryId === c.id ? 'selected' : ''}>${esc(c.name)}</option>`).join('')}
                    </select>
                </div>
                <div class="form-group">
                    <label class="form-label required">Çalışma tipi</label>
                    <select class="select" name="workMode" required>
                        ${WorkModeOptions.map(o => `<option value="${o.value}" ${job?.workMode === o.value ? 'selected' : ''}>${o.label}</option>`).join('')}
                    </select>
                </div>
            </div>
            <div class="form-group">
                <label class="form-label required">Açıklama</label>
                <textarea class="textarea" name="description" rows="8" required minlength="20">${esc(job?.description || '')}</textarea>
                <div class="form-hint">İşin kapsamı, beklentiler, deliverable'lar...</div>
            </div>
            <div class="form-row-3">
                <div class="form-group">
                    <label class="form-label">Min bütçe</label>
                    <input class="input" name="budgetMin" type="number" step="0.01" value="${job?.budgetMin ?? ''}" />
                </div>
                <div class="form-group">
                    <label class="form-label">Max bütçe</label>
                    <input class="input" name="budgetMax" type="number" step="0.01" value="${job?.budgetMax ?? ''}" />
                </div>
                <div class="form-group">
                    <label class="form-label">Para birimi</label>
                    <input class="input" name="currency" maxlength="3" placeholder="TRY" value="${esc(job?.currency || 'TRY')}" />
                </div>
            </div>
            <div class="form-row-3">
                <div class="form-group">
                    <label class="form-label">Süre (gün)</label>
                    <input class="input" name="durationDays" type="number" value="${job?.durationDays ?? ''}" />
                </div>
                <div class="form-group">
                    <label class="form-label">Ülke</label>
                    <input class="input" name="country" value="${esc(job?.country || '')}" />
                </div>
                <div class="form-group">
                    <label class="form-label">Şehir</label>
                    <input class="input" name="city" value="${esc(job?.city || '')}" />
                </div>
            </div>
            <div class="form-group">
                <label class="form-label">Gerekli yetkinlikler</label>
                <div class="chip-list" id="skills-picker">
                    ${allSkills.map(s => {
        const selected = job && (job.skills || []).some(x => x.skillId === s.id);
        return `<span class="chip ${selected ? 'selected' : ''}" data-id="${s.id}">${esc(s.name)}</span>`;
    }).join('')}
                </div>
            </div>
            <div class="flex justify-end gap-2 mt-4">
                <button type="button" class="btn btn-secondary" data-nav="/my-jobs">Vazgeç</button>
                <button type="submit" class="btn btn-primary" id="save-btn">${isEdit ? 'Kaydet' : 'Oluştur'}</button>
            </div>
        </form>
    `;

    view.querySelectorAll('#skills-picker .chip').forEach(c => {
        c.addEventListener('click', () => c.classList.toggle('selected'));
    });

    view.querySelector('#job-form').addEventListener('submit', async (e) => {
        e.preventDefault();
        const form = e.target;
        const fd = new FormData(form);
        const skillIds = [...view.querySelectorAll('#skills-picker .chip.selected')].map(c => c.dataset.id);
        const payload = {
            categoryId: fd.get('categoryId'),
            title: fd.get('title'),
            description: fd.get('description'),
            budgetMin: fd.get('budgetMin') ? parseFloat(fd.get('budgetMin')) : null,
            budgetMax: fd.get('budgetMax') ? parseFloat(fd.get('budgetMax')) : null,
            currency: fd.get('currency') || null,
            workMode: parseInt(fd.get('workMode')),
            durationDays: fd.get('durationDays') ? parseInt(fd.get('durationDays')) : null,
            country: fd.get('country') || null,
            city: fd.get('city') || null,
            skillIds
        };
        const btn = form.querySelector('#save-btn');
        btn.disabled = true; btn.textContent = 'Kaydediliyor...';
        try {
            if (isEdit) {
                await api.updateJob(job.id, payload);
                toast('İlan güncellendi.', 'success');
                router.go('/jobs/' + job.id);
            } else {
                const r = await api.createJob(payload);
                toast('İlan taslak olarak oluşturuldu.', 'success');
                router.go('/jobs/' + (r.data));
            }
        } catch (ex) {
            toast(ex?.message || 'Kaydedilemedi.', 'error');
            btn.disabled = false; btn.textContent = isEdit ? 'Kaydet' : 'Oluştur';
        }
    });
}