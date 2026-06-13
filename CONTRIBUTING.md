# Contributing to Passbolt.Api

Thank you for contributing.

## Workflow

1. Fork the repository.
2. Create a feature branch from `main`.
3. Make focused changes with tests.
4. Ensure the build passes with zero diagnostics.
5. Open a pull request against `main`.

## Coding Standards

- Follow `.editorconfig`.
- Use file-scoped namespaces.
- Add XML documentation for public APIs.
- Prefer `System.Text.Json` over `Newtonsoft.Json`.
- Use Refit for HTTP client interfaces.
- Keep `TreatWarningsAsErrors` enabled.

## Testing

- Use xUnit v3.
- Use AwesomeAssertions for assertions.
- Keep tests deterministic and isolated.
- Ensure all tests pass before opening a PR.

## Pull Request Checklist

- [ ] Build passes in Release mode.
- [ ] Tests pass in Release mode.
- [ ] New public APIs have XML docs.
- [ ] No secrets were introduced.
