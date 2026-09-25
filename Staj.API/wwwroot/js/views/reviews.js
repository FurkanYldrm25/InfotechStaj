// views/reviews.js — verdiğim ve aldığım değerlendirmeler

import { esc, initials, fullName, stars, formatDate, toast, openModal, confirmModal, icons } from '../utils.js';

export async function renderMyReviews(ctx, { query }) {
    const { renderView, api } = ctx;
    const tab = query.tab || 'received';

    renderView('Değerlendirmeler', `
        <div class="page-header">
            <div>
                <h1>Değerlendirmeler</h1>
                <div class="page-sub">Sana yazılan ve senin yazdığın değerlendirmeler.</div>
            </div>
        </div>
        <div class="tabs">
            <button class="tab ${tab === 'received' ? 'active' : ''}" data-tab="received">Aldığım</button>
            <button class="tab ${tab === 'given' ? 'active' : ''}" data-tab="given">Verdiğim</button>
        </div>
        <div id="rev-body"><div class="loading-box"><div class="spinner"></div></div></div>
    `);
    document.querySelectorAll('.tab[data-tab]').forEach(t => {
        t.addEventListener('click', () => { location.hash = '#/reviews?tab=' + t.dataset.tab; });
    });

    const body = document.getElementById('rev-body');
    try {
        const r = tab === 'received' ? await api.getMyReceivedReviews() : await api.getMyGivenReviews();
        const items = r.data?.items || [];
        if (items.length === 0) {
            body.innerHTML = `<div class="card"><div class="card-empty"><div class="empty-title">Değerlendirme yok</div><div>${tab === 'received' ? 'Sana henüz kimse yorum yazmadı.' : 'Henüz kimseyi değerlendirmedin.'}</div></div></div>`;
            return;
        }
        body.innerHTML = `
            <div class="card tight">
                ${items.map(rv => reviewRow(rv, tab, ctx.auth)).join('')}
            </div>
        `;
        body.querySelectorAll('.edit-btn').forEach(b => b.addEventListener('click', (e) => {
            e.stopPropagation();
            const id = e.target.closest('[data-id]').dataset.id;
            const item = items.find(x => x.id === id);
            showEditReviewModal(ctx, item, () => renderMyReviews(ctx, { query }));
        }));
        body.querySelectorAll('.del-btn').forEach(b => b.addEventListener('click', async (e) => {
            e.stopPropagation();
            const id = e.target.closest('[data-id]').dataset.id;
            if (!await confirmModal('Değerlendirmeyi silmek istediğinden emin misin?', { okKind: 'danger', okLabel: 'Sil' })) return;
            try { await api.deleteReview(id); toast('Silindi.', 'success'); renderMyReviews(ctx, { query }); }
            catch (ex) { toast(ex?.message || 'Hata', 'error'); }
        }));
    } catch (e) {
        body.innerHTML = `<div class="card text-danger">${esc(e?.message || 'Yüklenemedi')}</div>`;
    }
}

function reviewRow(r, tab, auth) {
    const me = auth.getUser();
    const isAuthor = me && me.userId === r.authorUserId;
    const isTarget = me && me.userId === r.targetUserId;
    const showEdit = tab === 'given' || isAuthor;
    const person = tab === 'given'
        ? { first: r.targetFirstName, last: r.targetLastName }
        : { first: r.authorFirstName, last: r.authorLastName };
    return `
        <div class="list-row static" style="align-items:flex-start" data-id="${r.id}">
            <div class="avatar avatar-lg">${esc(initials(person.first, person.last))}</div>
            <div class="grow">
                <div class="flex items-center gap-2 mb-1">
                    <div class="font-medium">${esc(fullName(person.first, person.last))}</div>
                    <span class="stars">${stars(r.rating)}</span>
                    <span class="text-muted text-sm">${formatDate(r.createdAt)}</span>
                </div>
                ${r.jobPostTitle ? `<div class="text-muted text-sm mb-1">İş: ${esc(r.jobPostTitle)}</div>` : ''}
                <div style="white-space:pre-wrap">${esc(r.comment)}</div>
            </div>
            ${showEdit ? `
                <div class="flex gap-2 shrink-0">
                    <button class="btn btn-ghost btn-sm edit-btn">${icons.edit}</button>
                    <button class="btn btn-ghost btn-sm del-btn">${icons.trash}</button>
                </div>
            ` : ''}
        </div>
    `;
}

function showEditReviewModal(ctx, item, onDone) {
    const { api } = ctx;
    const modal = openModal({
        title: 'Değerlendirmeyi düzenle',
        body: `
            <form id="rev-form">
                <div class="form-group">
                    <label class="form-label required">Puan</label>
                    <div class="star-picker" id="star-picker">
                        ${[1, 2, 3, 4, 5].map(i => `<span class="${i <= item.rating ? 'filled' : ''}" data-v="${i}">★</span>`).join('')}
                    </div>
                    <input type="hidden" name="rating" value="${item.rating}" required />
                </div>
                <div class="form-group">
                    <label class="form-label required">Yorum</label>
                    <textarea class="textarea" name="comment" rows="5" minlength="10" maxlength="2000" required>${esc(item.comment)}</textarea>
                </div>
            </form>
        `,
        footer: `<button class="btn btn-secondary" data-act="cancel">Vazgeç</button><button class="btn btn-primary" data-act="submit">Kaydet</button>`
    });
    const stars = modal.body.querySelectorAll('#star-picker span');
    const ratingInput = modal.body.querySelector('input[name=rating]');
    stars.forEach(s => s.addEventListener('click', () => {
        const v = parseInt(s.dataset.v);
        ratingInput.value = v;
        stars.forEach(x => x.classList.toggle('filled', parseInt(x.dataset.v) <= v));
    }));
    modal.footer.querySelector('[data-act=cancel]').onclick = modal.close;
    modal.footer.querySelector('[data-act=submit]').onclick = async () => {
        const fd = new FormData(modal.body.querySelector('form'));
        try {
            await api.updateReview(item.id, {
                rating: parseInt(fd.get('rating')),
                comment: fd.get('comment')
            });
            modal.close();
            toast('Güncellendi.', 'success');
            onDone && onDone();
        } catch (ex) { toast(ex?.message || 'Hata', 'error'); }
    };
}