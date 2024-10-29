import keycloak from './keycloak';

export const get = async <T>(url: string, options: RequestInit = {}): Promise<T> => {
    const fullUrl = `https://localhost:8000/api${url}`;
    options.method = 'GET'
    options.headers = {
        'Content-Type': 'application/json',
        ...(options.headers ?? {}),
    };
    if (keycloak.authenticated) {
        await keycloak.updateToken(30);
        options.headers = {
            ...options.headers,
            Authorization: `Bearer ${keycloak.token}`,
        };
    }
    const response = await fetch(fullUrl, options);

    if (!response.ok) {
        throw new Error(`Error: ${response.status} ${response.statusText}`);
    }

    return response.json() as Promise<T>;
};