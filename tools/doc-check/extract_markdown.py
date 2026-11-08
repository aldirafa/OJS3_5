"""Extract ```vb / ```csharp / ```fsharp blocks from Markdown files into compilable files.
usage: python -I extract_markdown.py <outDir> <file-or-dir>...
Fences with 'nocheck' in the info string are skipped. Each block becomes one async method/function that receives
`ojs` (an OjsClient) unless the block declares it; a few common free variables (id, key, ...) are declared for it."""
import os, re, sys
out = sys.argv[1]
sources = []
for arg in sys.argv[2:]:
    if os.path.isdir(arg):
        sources += [os.path.join(arg, f) for f in sorted(os.listdir(arg)) if f.endswith(".md")]
    else:
        sources.append(arg)
FREE = {  # name -> (vb, cs, fs) declarations for free variables used in examples
    "id": ("Dim id As Integer = 6", "int id = 6;", "let id = 6"),
    "key": ('Dim key As String = "k"', 'string key = "k";', 'let key = "k"'),
    "journalId": ("Dim journalId As Integer = 1", "int journalId = 1;", "let journalId = 1"),
    "token": ('Dim token As String = "t"', 'string token = "t";', 'let token = "t"'),
    "invitationId": ("Dim invitationId As Integer = 6", "int invitationId = 6;", "let invitationId = 6"),
    "cancellationToken": ("Dim cancellationToken As Threading.CancellationToken = Nothing", "System.Threading.CancellationToken cancellationToken = default;", "let cancellationToken = System.Threading.CancellationToken.None"),
}
blocks = {"vb": [], "csharp": [], "fsharp": []}
for path in sources:
    f = os.path.basename(path)
    lines = open(path, encoding="utf-8").read().split("\n")
    i = 0
    while i < len(lines):
        m = re.match(r"^(\s*)```(\w+)(.*)$", lines[i])
        if m:
            lang, info, ind = m.group(2), m.group(3), len(m.group(1))
            body = []; i += 1
            while not lines[i].strip().startswith("```"):
                body.append(lines[i][ind:]); i += 1
            if lang in blocks and "nocheck" not in info:
                blocks[lang].append((f"{f}:{i}", body))
        i += 1

def free_decls(text, idx):
    used = set(re.findall(r"\b(\w+)\b", text))
    declared = set(re.findall(r"\b(?:Dim|Using|For Each|var|let|let!|use|for|foreach \(var)\s+(\w+)", text))
    return [d[idx] for n, d in FREE.items() if n in used and n not in declared]

# VB
vb = ["Option Strict On", "Option Infer On", "Imports System.IO", "Imports OJS3_5.Library", "Imports OJS3_5.Library.Models", "Imports OJS3_5.Library.Queries",
      "Imports OJS3_5.Library.Resources", "Imports OJS3_5.Library.Serialization", "Imports OJS3_5.Library.Errors", "", "Public Module WikiSnippets"]
for n, (loc, body) in enumerate(blocks["vb"]):
    body = [b for b in body if not b.startswith("Imports ")]
    text = "\n".join(body)
    declares_ojs = re.search(r"\b(Dim|Using)\s+ojs\b", text) is not None
    vb += [f"    ' {loc}", f"    Public Async Function S{n}({'' if declares_ojs else 'ojs As OjsClient'}) As Task"]
    vb += ["        " + d for d in free_decls(text, 0)] + ["        " + b for b in body] + ["    End Function"]
vb.append("End Module")
open(os.path.join(out, "vb", "WikiSnippets.vb"), "w", encoding="utf-8").write("\n".join(vb))

# C#
cs = ["using System; using System.IO; using System.Linq; using System.Collections.Generic; using System.Net.Http; using System.Threading; using System.Threading.Tasks;",
      "using OJS3_5.Library; using OJS3_5.Library.Models; using OJS3_5.Library.Queries; using OJS3_5.Library.Resources; using OJS3_5.Library.Serialization; using OJS3_5.Library.Errors;",
      "#pragma warning disable CS1998, CS0168, CS0219, CS8321", "public static class WikiSnippets {"]
for n, (loc, body) in enumerate(blocks["csharp"]):
    body = [b for b in body if not re.match(r"^using [\w.]+;\s*$", b)]
    text = "\n".join(body)
    declares_ojs = re.search(r"\bvar\s+ojs\b", text) is not None
    cs += [f"    // {loc}", f"    public static async Task S{n}({'' if declares_ojs else 'OjsClient ojs'}) {{"]
    cs += ["        " + d for d in free_decls(text, 1)] + ["        " + b for b in body] + ["    }"]
cs.append("}")
open(os.path.join(out, "cs", "WikiSnippets.cs"), "w", encoding="utf-8").write("\n".join(cs))

# F#
fs = ["module WikiSnippets", "#nowarn \"20\"", "open System", "open System.IO", "open OJS3_5.Library", "open OJS3_5.Library.Models",
      "open OJS3_5.Library.Queries", "open OJS3_5.Library.Resources", "open OJS3_5.Library.Serialization", "open OJS3_5.Library.Errors", ""]
for n, (loc, body) in enumerate(blocks["fsharp"]):
    body = [b for b in body if not b.startswith("open ")]
    text = "\n".join(body)
    fs += [f"// {loc}", f"let s{n} (ojs: OjsClient) =", "    task {"]
    fs += ["        " + d for d in free_decls(text, 2)] + ["        " + b if b.strip() else "" for b in body] + ["        return ()", "    }", ""]
open(os.path.join(out, "fs", "WikiSnippets.fs"), "w", encoding="utf-8").write("\n".join(fs))
print({k: len(v) for k, v in blocks.items()})
