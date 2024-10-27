import keycloak from './keycloak';

export const send = async (url: string, options: RequestInit = {}) => {

    if (keycloak.authenticated) {
        await keycloak.updateToken(30);
        options.headers = {
            ...options.headers,
            Authorization: `Bearer ${keycloak.token}`,
        };
    }

    return fetch(url, options);
};