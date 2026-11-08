import '@testing-library/cypress/add-commands'
import './commands'

// Cada teste apaga as contas que criou (docs/testing.md, "E2E").
afterEach(() => {
  cy.deleteCreatedUsers()
})
