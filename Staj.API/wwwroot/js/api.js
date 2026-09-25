// api.js — fetch wrapper: JWT ekleme, 401'de otomatik refresh, standart Result<T> parse

import * as auth from './auth.js';

const BASE = '/api';

async function request(method, path, body = null, { retryOn401 = true } = {}) {
    const headers = { 'Accept': 'application/json' };
    if (body !== null) headers['Content-Type'] = 'application/json';
    const token = auth.getAccessToken();
    if (token) headers['Authorization'] = `Bearer ${token}`;

    let res;
    try {
        res = await fetch(BASE + path, {
            method,
            headers,
            body: body !== null ? JSON.stringify(body) : null
        });
    } catch (err) {
        throw { success: false, message: 'Sunucuya ulaşılamıyor.', errors: [err.message] };
    }

    // 401 → refresh dene
    if (res.status === 401 && retryOn401 && auth.getRefreshToken() && path !== '/auth/refresh' && path !== '/auth/login') {
        const ok = await tryRefresh();
        if (ok) return request(method, path, body, { retryOn401: false });
        auth.clearAuth();
        location.hash = '#/login';
        throw { success: false, message: 'Oturum süresi doldu. Lütfen tekrar giriş yapın.' };
    }

    let payload = null;
    const text = await res.text();
    if (text) {
        try { payload = JSON.parse(text); } catch { payload = { message: text }; }
    }

    if (!res.ok) {
        // Result<T> zarfı: { success: false, message, errors }
        throw payload && typeof payload === 'object'
            ? { success: false, ...payload, status: res.status }
            : { success: false, message: `HTTP ${res.status}`, status: res.status };
    }

    // 204 No Content
    if (!payload) return { success: true };
    return payload;
}

async function tryRefresh() {
    try {
        const r = await fetch(BASE + '/auth/refresh', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                accessToken: auth.getAccessToken(),
                refreshToken: auth.getRefreshToken()
            })
        });
        if (!r.ok) return false;
        const data = await r.json();
        if (data && data.success && data.data) {
            auth.setAuth(data.data);
            return true;
        }
    } catch { /* yut */ }
    return false;
}

export const api = {
    get: (path) => request('GET', path),
    post: (path, body) => request('POST', path, body ?? {}),
    put: (path, body) => request('PUT', path, body ?? {}),
    del: (path) => request('DELETE', path),

    // ===== Auth =====
    login: (email, password) => request('POST', '/auth/login', { email, password }, { retryOn401: false }),
    register: (payload) => request('POST', '/auth/register', payload, { retryOn401: false }),

    // ===== Users =====
    me: () => request('GET', '/users/me'),
    updateBasicInfo: (firstName, lastName) => request('PUT', '/users/me/basic-info', { firstName, lastName }),
    updateProfileImage: (imageUrl) => request('PUT', '/users/me/profile-image', { imageUrl }),

    // ===== Categories & Skills =====
    getCategories: () => request('GET', '/categories'),
    getCategory: (id) => request('GET', `/categories/${id}`),
    createCategory: (body) => request('POST', '/categories', body),
    updateCategory: (id, body) => request('PUT', `/categories/${id}`, { id, ...body }),
    deleteCategory: (id) => request('DELETE', `/categories/${id}`),
    assignSkillToCategory: (catId, skillId) => request('POST', `/categories/${catId}/skills/${skillId}`),
    removeSkillFromCategory: (catId, skillId) => request('DELETE', `/categories/${catId}/skills/${skillId}`),

    getSkills: () => request('GET', '/skills'),
    getSkillsByCategory: (catId) => request('GET', `/categories/${catId}/skills`),
    createSkill: (body) => request('POST', '/skills', body),
    updateSkill: (id, body) => request('PUT', `/skills/${id}`, { id, ...body }),
    deleteSkill: (id) => request('DELETE', `/skills/${id}`),

    // ===== Freelancers =====
    searchFreelancers: (params) => {
        const q = new URLSearchParams();
        Object.entries(params || {}).forEach(([k, v]) => {
            if (v === null || v === undefined || v === '') return;
            if (Array.isArray(v)) v.forEach(x => q.append(k, x));
            else q.append(k, v);
        });
        return request('GET', `/freelancers?${q.toString()}`);
    },
    getFreelancer: (id) => request('GET', `/freelancers/${id}`),
    getMyFreelancer: () => request('GET', '/freelancers/me'),
    upsertFreelancer: (body) => request('PUT', '/freelancers/me', body),
    setMySkills: (skills) => request('PUT', '/freelancers/me/skills', { skills }),
    addPortfolio: (body) => request('POST', '/freelancers/me/portfolio', body),
    updatePortfolio: (id, body) => request('PUT', `/freelancers/me/portfolio/${id}`, { id, ...body }),
    deletePortfolio: (id) => request('DELETE', `/freelancers/me/portfolio/${id}`),

    // ===== Clients =====
    getClient: (id) => request('GET', `/clients/${id}`),
    getMyClient: () => request('GET', '/clients/me'),
    upsertClient: (body) => request('PUT', '/clients/me', body),

    // ===== Jobs =====
    searchJobs: (params) => {
        const q = new URLSearchParams();
        Object.entries(params || {}).forEach(([k, v]) => {
            if (v === null || v === undefined || v === '') return;
            if (Array.isArray(v)) v.forEach(x => q.append(k, x));
            else q.append(k, v);
        });
        return request('GET', `/jobs?${q.toString()}`);
    },
    getMyJobs: (params) => {
        const q = new URLSearchParams();
        Object.entries(params || {}).forEach(([k, v]) => {
            if (v === null || v === undefined || v === '') return;
            q.append(k, v);
        });
        return request('GET', `/jobs/me?${q.toString()}`);
    },
    getJob: (id) => request('GET', `/jobs/${id}`),
    createJob: (body) => request('POST', '/jobs', body),
    updateJob: (id, body) => request('PUT', `/jobs/${id}`, { id, ...body }),
    publishJob: (id) => request('POST', `/jobs/${id}/publish`),
    closeJob: (id) => request('POST', `/jobs/${id}/close`),
    cancelJob: (id) => request('POST', `/jobs/${id}/cancel`),
    deleteJob: (id) => request('DELETE', `/jobs/${id}`),

    // ===== Proposals =====
    submitApplication: (body) => request('POST', '/proposals/applications', body),
    submitDirectOffer: (body) => request('POST', '/proposals/direct-offers', body),
    getProposal: (id) => request('GET', `/proposals/${id}`),
    getMyProposals: (params) => {
        const q = new URLSearchParams();
        Object.entries(params || {}).forEach(([k, v]) => {
            if (v === null || v === undefined || v === '') return;
            q.append(k, v);
        });
        return request('GET', `/proposals/me?${q.toString()}`);
    },
    getProposalsForJob: (jobId) => request('GET', `/proposals/for-job/${jobId}`),
    acceptProposal: (id, note) => request('POST', `/proposals/${id}/accept`, { id, responseNote: note }),
    rejectProposal: (id, note) => request('POST', `/proposals/${id}/reject`, { id, responseNote: note }),
    withdrawProposal: (id) => request('POST', `/proposals/${id}/withdraw`),

    // ===== Messages =====
    startConversation: (otherUserId) => request('POST', '/conversations', { otherUserId }),
    getConversations: () => request('GET', '/conversations'),
    getConversationMessages: (id, page = 1, pageSize = 50) =>
        request('GET', `/conversations/${id}/messages?page=${page}&pageSize=${pageSize}`),
    markConversationRead: (id) => request('POST', `/conversations/${id}/read`),
    sendMessage: (conversationId, content) => request('POST', '/messages', { conversationId, content }),
    getUnreadMessageCount: () => request('GET', '/messages/unread-count'),

    // ===== Notifications =====
    getNotifications: (params) => {
        const q = new URLSearchParams();
        Object.entries(params || {}).forEach(([k, v]) => {
            if (v === null || v === undefined || v === '') return;
            q.append(k, v);
        });
        return request('GET', `/notifications/me?${q.toString()}`);
    },
    getUnreadNotificationCount: () => request('GET', '/notifications/me/unread-count'),
    markNotificationRead: (id) => request('POST', `/notifications/${id}/read`),
    markAllNotificationsRead: () => request('POST', '/notifications/me/read-all'),
    deleteNotification: (id) => request('DELETE', `/notifications/${id}`),

    // ===== Reviews =====
    getReviewsForUser: (userId) => request('GET', `/reviews/for-user/${userId}`),
    getUserRatingSummary: (userId) => request('GET', `/reviews/for-user/${userId}/summary`),
    getMyGivenReviews: () => request('GET', '/reviews/me/given'),
    getMyReceivedReviews: () => request('GET', '/reviews/me/received'),
    createReview: (body) => request('POST', '/reviews', body),
    updateReview: (id, body) => request('PUT', `/reviews/${id}`, { id, ...body }),
    deleteReview: (id) => request('DELETE', `/reviews/${id}`),
};