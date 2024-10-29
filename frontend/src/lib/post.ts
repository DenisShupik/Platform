import keycloak from './keycloak';

export const post = async (url: string, body: any = {}, options: RequestInit = {}) => {
    const fullUrl = `https://localhost:8000/api${url}`;
    options.method = 'POST'
    options.headers = {
        'Content-Type': 'application/json',
        ...(options.headers ?? {}),
    };
    options.body = JSON.stringify(body)
    if (keycloak.authenticated) {
        await keycloak.updateToken(30);
        options.headers = {
            ...options.headers,
            Authorization: `Bearer ${keycloak.token}`,
        };
    }
    return fetch(fullUrl, options);
};