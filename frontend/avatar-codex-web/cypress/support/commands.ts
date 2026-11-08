export interface TestUser {
  username: string
  email: string
  password: string
}

declare global {
  namespace Cypress {
    interface Chainable {
      /** Dados de um usuário que ainda não existe (cada teste usa o seu). */
      newUser(): Chainable<TestUser>
      /** Cria a conta pela API; o cookie de refresh fica no navegador, então a sessão já vale no app. */
      registerViaApi(user: TestUser): Chainable<void>
      /** Id de um personagem do seed, pelo nome. */
      characterId(name: string): Chainable<number>
      /** Fecha o modal de novo nível, se estiver aberto (ele bloqueia o resto da página). */
      dismissLevelUp(): Chainable<void>
      /**
       * Exclui pela API as contas geradas por `newUser` neste teste (roda no afterEach global).
       * Quem não chegou a ser criado, ou já foi excluído pelo próprio teste, é ignorado.
       */
      deleteCreatedUsers(): Chainable<void>
    }
  }
}

const apiUrl = () => Cypress.expose('apiUrl') as string

// Contas geradas no teste atual; o afterEach (support/e2e.ts) as exclui para não sobrar
// usuário de teste no banco, inclusive quando os E2E rodam contra a produção.
let createdUsers: TestUser[] = []

Cypress.Commands.add('newUser', () => {
  const suffix = `${Date.now().toString(36)}${Cypress._.random(1000, 9999)}`
  const user = {
    username: `e2e_${suffix}`,
    email: `e2e_${suffix}@example.com`,
    password: 'Senha123',
  }
  createdUsers.push(user)
  return cy.wrap(user)
})

Cypress.Commands.add('registerViaApi', (user: TestUser) => {
  cy.request('POST', `${apiUrl()}/api/auth/register`, {
    username: user.username,
    email: user.email,
    password: user.password,
    confirmPassword: user.password,
  })
})

Cypress.Commands.add('characterId', (name: string) =>
  cy
    .request(`${apiUrl()}/api/characters?search=${encodeURIComponent(name)}`)
    .then(
      ({ body }) => (body.items as { id: number; name: string }[]).find((c) => c.name === name)!.id,
    ),
)

Cypress.Commands.add('dismissLevelUp', () => {
  cy.get('body').then(($body) => {
    if ($body.find('dialog[open]').length > 0)
      cy.findByRole('button', { name: 'Continuar' }).click()
  })
})

Cypress.Commands.add('deleteCreatedUsers', () => {
  const users = createdUsers
  createdUsers = []
  for (const user of users) {
    cy.request({
      method: 'POST',
      url: `${apiUrl()}/api/auth/login`,
      body: { email: user.email, password: user.password },
      failOnStatusCode: false,
    }).then((login) => {
      if (login.status !== 200) return
      cy.request({
        method: 'DELETE',
        url: `${apiUrl()}/api/users/me`,
        headers: { Authorization: `Bearer ${login.body.accessToken}` },
        body: { password: user.password },
      })
        .its('status')
        .should('eq', 204)
    })
  }
})
