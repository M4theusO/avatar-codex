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
    }
  }
}

const apiUrl = () => Cypress.expose('apiUrl') as string

Cypress.Commands.add('newUser', () => {
  const suffix = `${Date.now().toString(36)}${Cypress._.random(1000, 9999)}`
  return cy.wrap({
    username: `e2e_${suffix}`,
    email: `e2e_${suffix}@example.com`,
    password: 'Senha123',
  })
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
