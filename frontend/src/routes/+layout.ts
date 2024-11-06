import keycloak from '$lib/keycloak'
import { updateUser } from '$lib/stores/currentUserStore'
import type { KeycloakTokenParsed } from 'keycloak-js'

export const ssr = false

updateUser({userId:"fc30317e-d774-4863-bbbd-3e4eb8287c77", username:"Admin"})

// try {
//     await keycloak.init({
//       onLoad: 'login-required'
//     })
//     if (keycloak.authenticated != null && keycloak.authenticated)
//       updateUser({
//         userId: keycloak.subject!,
//         username:
//           (
//             keycloak.tokenParsed as
//               | (KeycloakTokenParsed & { preferred_username?: string })
//               | undefined
//           )?.preferred_username ?? ''
//       })
//   } catch (error) {
//     console.error('Ошибка авторизации:', error)
//   }