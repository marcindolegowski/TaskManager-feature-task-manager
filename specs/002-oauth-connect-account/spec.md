# Feature Spec: Connect Claude Account via OAuth ("Authorize" button)

**Feature ID**: 002-oauth-connect-account
**Status**: Draft — **RESEARCH REQUIRED before planning/implementation**
**Created**: 2026-06-15
**Constitution**: `.specify/memory/constitution.md` · **Strategy**: `docs/cli-deployment-strategy.md`
**Related**: builds on the per-user/per-team credential layer (commit `feb3b4c`).

> This document exists so the OAuth flow can be **researched in a separate session**
> before any implementation. The "Open research questions" section is the point of
> this doc — do not implement until those are answered.

## Summary

Let a developer connect their own Claude account to TaskManager by clicking
**"Connect Claude account → Authorize"** (a browser OAuth consent), instead of pasting
a long-lived token. Connected accounts are used to run that developer's agent tasks
under their own subscription/quota (see the per-user/per-team credential feature).

## Why

The current connect-account UX (`POST /api/credentials/me`) requires the developer to
run `claude setup-token` in a terminal and paste the resulting `sk-ant-oat01-…` token.
That works and is the **documented** path, but it is clunky and unfamiliar. The product
goal is a one-click "Authorize" flow, exactly like `claude login` and like
"Sign in with GitHub".

## Current state (already implemented — the fallback)

- `POST /api/credentials/me` and `/api/credentials/team` accept `{ oauthToken | apiKey }`
  and store them in **Azure Key Vault** (`KeyVaultCredentialStore`), keyed by user/team.
- `ICredentialResolver` (user-first, team-fallback) resolves the credential per run; the
  sidecar runs `query()` under it via `options.env`. Token never travels on the bus.
- This token-paste path stays as-is; the OAuth flow is an **additive, nicer front door**
  that ends up writing the same Key Vault secret.

## Target UX & proposed flow (to be validated by research)

OAuth 2.0/2.1 **authorization-code + PKCE** (interactive; `client_credentials` is not an
option and is not supported by Claude for this):

```
1. Dev clicks "Connect Claude"  → GET /api/credentials/connect/start
      └─ generate PKCE verifier+challenge + state; redirect to Anthropic authorize URL
2. Dev authenticates (their login = your SSO/SAML for Team/Enterprise) and approves
3. Anthropic redirects → GET /api/credentials/connect/callback?code&state
      └─ validate state; exchange code + PKCE verifier at the token endpoint
      └─ receive access token (+ refresh token); store in Key Vault for this dev
4. Later runs resolve and use that credential (existing resolver + sidecar path)
```

## Open research questions (DO THIS IN THE RESEARCH SESSION)

1. **Is third-party minting of inference tokens permitted?** Claude Code uses its own
   OAuth client; there is **no documented turnkey "Login with Claude" partner product**
   for obtaining an Agent-SDK/subscription inference token. Confirm whether a third-party
   app may do this at all, and under what **Terms of Service**. This gates everything.
2. **Client registration model**: which of **DCR** (Dynamic Client Registration), **CIMD**
   (Client ID Metadata Documents), or **Anthropic-held credentials** applies for this use?
   How do we obtain/register a `client_id`? Is a public client (PKCE, no secret) allowed?
3. **Endpoints & params**: exact **authorize** and **token** endpoint URLs, required
   `scope`(s), `redirect_uri` registration/exact-match rules, and the token response shape.
4. **Token type & lifetime**: does this yield an `sk-ant-oat01-…` subscription token (and
   thus draw on the **monthly Agent SDK credit**, effective 2026-06-15), or something else?
   Access-token lifetime and **refresh-token** semantics (we must refresh before use).
5. **Scopes/permissions**: minimum scope needed for headless Agent SDK inference; confirm
   it is "inference only" and cannot do more than intended.
6. **Revocation**: how a dev disconnects / revokes, and how we detect a revoked token.
7. **Bedrock/Vertex/Foundry alternative**: for an Azure-deployed product, is **Foundry +
   Entra ID** (federated, no per-user token at all) a better fit than per-user OAuth for
   some/all users? Compare against the per-user model.
8. **Connector OAuth vs account OAuth**: the documented Claude *connector* OAuth is a
   different surface — confirm it is NOT the mechanism we need (we need an account/inference
   credential, not an MCP connector authorization).

## Architecture fit

- New endpoints on `CredentialsController` (or a dedicated `OAuthController`):
  `GET /connect/start`, `GET /connect/callback`.
- Reuse `ICredentialStore` (Key Vault) to persist the resulting token (+ refresh token).
  May need to extend `AgentCredential` with a refresh token + expiry.
- All OAuth client values (authorize/token URLs, `client_id`, `scope`, `redirect_uri`)
  come from **`IConfiguration`** (`Agent:OAuth:*`) — never hardcoded.
- PKCE verifier + state stored short-term (e.g., distributed cache / Redis already present)
  keyed by state, with a short TTL.

## Security considerations

- **Identity binding**: `/connect/start` and `/callback` must run as the authenticated
  developer (not a body-supplied `userId`); fixes the PoC `TODO(security)`.
- `state` (CSRF) + PKCE (code interception) are mandatory; exact `redirect_uri` match.
- Tokens (access + refresh) stored only in Key Vault; never logged, never on the bus.
- TLS everywhere; the API↔sidecar hop already carries the resolved token.

## Out of scope (this feature)

- Changing the resolver policy (stays user-first, team-fallback).
- The sidecar (already credential-agnostic via `options.env`).
- Team-account connection (admin still sets the team credential; OAuth is per-user first).

## Done when

- A developer connects via a browser "Authorize" click; a token lands in Key Vault for
  them; a subsequent task runs under their account — with no token pasting.
- Refresh works unattended; revocation removes access.

## References (starting points for the research session)

- Claude OAuth 2.0 + PKCE (authorization-code): https://deepwiki.com/anomalyco/opencode-anthropic-auth/3.2-oauth-2.0-with-pkce
- Community OAuth+PKCE implementation: https://github.com/querymt/anthropic-auth
- Claude connector authentication (DCR/CIMD) — different surface, for comparison: https://claude.com/docs/connectors/building/authentication
- Claude Code authentication (setup-token, precedence): https://code.claude.com/docs/en/authentication
- Agent SDK credit change (2026-06-15): https://support.claude.com/en/articles/15036540-use-the-claude-agent-sdk-with-your-claude-plan
