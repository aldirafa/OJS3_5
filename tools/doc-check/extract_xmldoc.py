"""Extract the VB <code> blocks of the XML documentation into one file so they can be compiled.
usage: python -I extract_xmldoc.py <LibraryDir> <out.vb>"""
import os, re, sys, html

root, outp = sys.argv[1], sys.argv[2]
PRELUDE = {  # free variables the examples rely on
    "ojs": "Dim ojs As OjsClient = Nothing",
    "id": "Dim id As Integer = 6", "key": 'Dim key As String = "k"', "fileId": "Dim fileId As Integer = 5",
    "journalId": "Dim journalId As Integer = 1", "submissionId": "Dim submissionId As Integer = 12",
    "publicationId": "Dim publicationId As Integer = 15", "contributorId": "Dim contributorId As Integer = 31",
    "reviewAssignmentId": "Dim reviewAssignmentId As Integer = 21", "doiId": "Dim doiId As Integer = 9",
    "userId": "Dim userId As Integer = 7", "invitationId": "Dim invitationId As Integer = 6",
    "issueId": "Dim issueId As Integer = 4", "token": 'Dim token As String = "t"',
}
snips = []
for dp, _, files in os.walk(root):
    if any(x in dp for x in (os.sep + "obj", os.sep + "bin", "Docs")): continue
    for f in files:
        if not f.endswith(".vb"): continue
        lines = open(os.path.join(dp, f), encoding="utf-8").read().split("\n")
        i = 0
        while i < len(lines):
            if lines[i].strip() == "''' <code>":
                start = i; body = []
                i += 1
                while lines[i].strip() != "''' </code>":
                    body.append(re.sub(r"^\s*''' ?", "", lines[i])); i += 1
                snips.append((f"{f}:{start + 1}", [html.unescape(b) for b in body]))
            i += 1

out = ["Option Strict On", "Option Infer On", "Imports System.IO", "Imports System.Net.Http", "Imports Library",
       "Imports Library.Models", "Imports Library.Queries", "Imports Library.Resources", "Imports Library.Serialization",
       "Imports Library.Errors", "Imports Library.Http", "Imports Library.Paging", "", "Public Module Snippets", ""]
for n, (loc, body) in enumerate(snips):
    text = "\n".join(body)
    declared = set(re.findall(r"\b(?:Dim|Using|For Each)\s+(\w+)", text))
    used = set(re.findall(r"\b(\w+)\b", text))
    out.append(f"    ' {loc}")
    out.append(f"    Public Async Function Snippet{n}() As Task")
    for name, decl in PRELUDE.items():
        if name in used and name not in declared:
            out.append("        " + decl)
    out += ["        " + b for b in body]
    out += ["    End Function", ""]
out.append("End Module")
open(outp, "w", encoding="utf-8").write("\n".join(out))
print(len(snips), "snippets")
