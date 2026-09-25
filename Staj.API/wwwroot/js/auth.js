// auth.js — JWT / kullanıcı bilgisi localStorage yönetimi

const KEY = 'staj_auth';

export function setAuth(loginResponse) {
    // { userId, email, firstName, lastName, roles, accessToken, refreshToken, expiresAt }
    localStorage.setItem(KEY, JSON.stringify(loginResponse));
}

export function clearAuth() {
    localStorage.removeItem(KEY);
}

export function getAuth() {
    try { return JSON.parse(localStorage.getItem(KEY) || 'null'); }
    catch { return null; }
}

export function getAccessToken() {
    const a = getAuth();
    return a && a.accessToken;
}
export function getRefreshToken() {
    const a = getAuth();
    return a && a.refreshToken;
}
export function isAuthenticated() {
    return !!getAccessToken();
}
export function getRoles() {
    const a = getAuth();
    return (a && a.roles) || [];
}
export function hasRole(role) {
    return getRoles().includes(role);
}
export function getUser() {
    const a = getAuth();
    if (!a) return null;
    return {
        userId: a.userId,
        email: a.email,
        firstName: a.firstName,
        lastName: a.lastName,
        roles: a.roles || []
    };
}
export function isAdmin() { return hasRole('Admin'); }
export function isFreelancer() { return hasRole('Freelancer'); }
export function isClient() { return hasRole('Client'); }