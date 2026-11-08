# Testing and Contributing

## Building

```
dotnet build OJS3_5.slnx
dotnet test OJS3_5.slnx
```

The solution has three projects:

| Project | Contents |
|---|---|
| `Library` | The library itself (VB.NET, `Option Strict On`). |
| `Library.Tests` | MSTest tests. |
| `Samples/Library.Sample` | A console sample. |

## Unit tests

The unit tests use a fake `HttpMessageHandler`, so they never touch the network. They check:

* URLs and query strings;
* HTTP methods and request bodies;
* error mapping;
* the PHP JSON quirks.

They run by default.

## Live tests

Some tests run against a real OJS 3.5 journal. They are **skipped** (reported as inconclusive) unless you set
environment variables:

| Variable | Purpose |
|---|---|
| `OJS_BASE_URL` | e.g. `https://journals.example.com` |
| `OJS_JOURNAL` | the journal path, e.g. `jpk` |
| `OJS_USE_INDEX_PHP` | `1` if the URLs contain `index.php` |
| `OJS_TOKEN` | an API token of a journal manager or site administrator |
| `OJS_AUTHOR_TOKEN`, `OJS_EDITOR_TOKEN`, `OJS_REVIEWER_TOKEN`, `OJS_REVIEWER2_TOKEN` | optional tokens for the permission tests |
| `OJS_ALLOW_WRITES` | `1` to run the tests that **change data** (category `LiveWrite`) |

```powershell
$env:OJS_BASE_URL = "https://ojs.example.com"; $env:OJS_JOURNAL = "test"; $env:OJS_TOKEN = "..."
dotnet test --filter TestCategory=Live
```

> **Run `LiveWrite` tests only against a test journal.** They create and delete announcements, institutions,
> templates and invitations. They also enable DOIs and assign them, and accept review requests.
> Set `[email] default = log` on that installation so no real emails go out.

The test journal needs some data:

* at least 3 submissions;
* one published article in a published issue;
* one submission in review with a completed review and an unanswered review request;
* users with the author, section editor and reviewer roles.

Live test classes do not run in parallel, so a small server is not flooded.

## Contributing

Please follow these conventions:

* **Code style.** `Option Strict On`, no warnings, and XML documentation with an `<example>` on every public member.
* **Models.** Read models have `<JsonExtensionData> AdditionalProperties`. Edit models have only writable, nullable properties.
* **Spec versus server.** When the live server and the OpenAPI document disagree, follow the server, add a test, and
  note it on [OJS 3.5 Quirks](OJS-3.5-Quirks).
* **Secrets.** Never commit API tokens or responses captured from real journals; they contain personal data. Build
  fixtures by hand.
