# Todoify

A full-stack task manager — .NET Web API backend, React frontend. Register, create, and manage tasks with due dates, priorities, and descriptions.

`.NET Web API` • `React` • `TailwindCSS` • `Docker` • `OAuth2 / JWT`

---

## Quick start

1. **Clone the repo**
   ```bash
   git clone https://github.com/ghoti143/todoify.git
   ```

2. **Spin up with Docker**
   ```bash
   cd todoify && npm run docker:up
   ```

3. **Open in browser**
   http://localhost:8080

4. **Log in with the demo account**
   ```
   demo@mytodoifyapp.com / demomytodoifyapp123
   ```

---

## Running locally

### Prerequisites
- `npm >= 10.8.2`
- `node >= 20.19.6`
- `dotnet/sdk >= 10.0.202`

1. ```bash
   npm install
   ```

2. ```bash
   npm run setup
   ```

3. **Kick the tires**
   ```bash
   npm run test
   ```

3. **Start client and server together**
   ```bash
   npm run dev
   ```

4. **Open App:** http://localhost:8080 · **Open Swagger:** http://localhost:8081/swagger
 
5. **Log in with the demo account**
   ```
   demo@mytodoifyapp.com / demomytodoifyapp123
   ```

---

## Features

### Server
- DTOs with fluent validation + unit tests
- OAuth2 authentication via Microsoft.IdentityModel
- Integration tests for correct HTTP status codes
- XML doc comments for enriched Swagger docs

### DX
- Docker for single-command local spin-up
- Root npm scripts for install, dev, lint, and test
- `.editorconfig` for consistent linting

### Client
- TailwindCSS for clean, utility-first styling
- React Query for loading and error state management
- `AuthProvider` stores JWT in local storage
- Axios interceptors attach JWT to request headers

---

## Future improvements

### Server
- Multi-tenancy — allow users in the same org to share tasks
- Move JWT signing key out of repo and into Azure Key Vault
- Add refresh tokens

### Client
- Client-side form validation to avoid unnecessary round trips
- URL-based routing for easy state sharing via copy/paste
- Migrate JWT storage from local storage to `httpOnly` cookie

### DX
- GitHub Actions to run tests on pull request builds
- GitHub Actions for automatic deploy on merge to main
- Generate TypeScript types from C# DTOs
- Add Storybook to view component variations