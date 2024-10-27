import Keycloak from 'keycloak-js';

const keycloak = new Keycloak({
    url: 'https://localhost:8443',
    realm: 'app',
    clientId: 'app-user',
});

export default keycloak;