# OJS 3.5 Quirks

These are places where a real OJS 3.5 server behaves differently from its OpenAPI document, or in ways that are easy to
trip over. We found them by running the library against live installations. Where possible the library handles them
for you; the rest are documented in the XML comments of the methods involved.

## Response shapes

| Endpoint | The document says | The server sends | The library |
|---|---|---|---|
| `/issues`, `/submissions/{id}/files`, `/contributors`, `/stats/*` lists | an array | `{itemsMax, items}` | accepts both shapes |
| `/stats/editorial/averages` | an array | an object | returns `Dictionary(Of String, Double)` |
| `/mailables` items | `id` | no `id`; identified by `emailTemplateKey` | `Mailable.EmailTemplateKey` is the ID to use |
| `/invitations/{id}/getMailable` | a Mailable | `{"mailable": {...}}`, a Laravel mailable | returns `InvitationEmailPreview` |
| `/invitations/add/{type}` | an invitation | `{"invitationId": n}` | returns the ID |
| `/users/reviewers` items | review statistics only | full user data plus statistics | `ReviewerSummary` inherits `User` |
| `/reviews/{s}/{r}/confirmReview` | the updated assignment | `{"_data": ...}`, holding the assignment *before* the change | unwraps it; read the submission again for the new state |
| `/reviews/history/...` | comment objects | comments as HTML strings | `List(Of String)` |
| keywords, subjects, ... | arrays of strings | arrays of `{"name": ...}` objects | `LocalizedVocab`, which accepts both |
| `userGroupName` (contributors) | a string | a multilingual object | `LocalizedString`, which accepts both |
| user group `name` (users) | multilingual | a plain string | `LocalizedString` accepts both |
| invitation `status` | lower case | upper case (`PENDING`) | `Invitation.StatusValue` ignores case |

## Request shapes

| Endpoint | What OJS actually needs | The library |
|---|---|---|
| `POST /institutions` | `ipRanges` as **newline-separated text** (an array causes a 500 error) | `InstitutionEdit.IpRanges` is a list and is joined for you |
| `POST /invitations/add/{type}`, `PUT .../populate`, `.../refine` | the data wrapped in `{"invitationData": ...}` | wraps it for you |
| `PUT /contexts/{id}`, `PUT /contexts/{id}/theme` | sent through the **journal's** API; the site-wide API answers 403 | `ContextsResource` looks up the journal's path |
| `PUT /contexts/{id}` with `enableDois: true` | `doiPrefix` in the same request | documented |

## Permissions

* **Wrong role gives 401.** A valid token whose role may not use an endpoint gets **401**
  `user.authorization.roleBasedAccessDenied`, not 403.
* **Confirming reviews is for editors.** `confirmReview` is the editor's "accept on behalf of the reviewer" action. Only
  managers and administrators may call it; reviewers get 401.
* **Review history is for the reviewer.** `reviews/history` looks the review up by the current user, so only the reviewer
  sees it; everyone else gets 404.
* **Single invitations cannot be read.** `GET /invitations/{id}` answers 401 for every invitation we tried. The route
  seems to collide with `GET /invitations/{type}`. Use the list endpoint instead.
* **Some cancels are refused.** Invitations that were never sent cannot be cancelled (401), and cancelled invitations
  disappear from every list.
* **Journal endpoints need a journal.** Users, institutions, email templates and mailables answer 401
  `user.authorization.contextRequired` or `noContext` when called on the site-wide API (`_`).

## Errors

* **Validation errors come in two shapes.** Most endpoints answer `400` with `{"field": ["message"]}`. Invitations
  answer `422` with `{"errors": {"field": ["message"]}}`. Both become `OjsValidationException`.

## PHP JSON in general

The library reads all of these without complaint:

* dates as `"Y-m-d H:i:s"` or `"Y-m-d"`;
* `""` and `"0000-00-00"` for "no date";
* numbers and booleans sent as strings;
* booleans sent as `0` and `1`;
* `[]` for an empty object;
* lists sent as objects with numeric keys (e.g. `enabledDoiTypes: {"0": "publication", "2": "issue"}`);
* `null` for a value that cannot be null.

## Installation and hosting notes

* **The API key needs a secret.** OJS signs API keys with `api_key_secret`, in the `[security]` section of
  `config.inc.php`. Without it, tokens do not validate.
* **`index.php` in URLs.** Installations without URL rewriting need `UseIndexPhp = True`. For example, the API of
  `https://example.com/index.php/jpk` is at `.../index.php/jpk/api/v1`.
* **Statistics lag a day.** They are compiled by a daily scheduled task, which must actually run.
* **Shared hosting.** It may answer an occasional 500 when it receives many requests at once.
