"""Merge translated chunks, validate them against English, and write the mod's tr.tsv.

Entries that are missing or fail validation fall back to the English text (never the slot's Polish).
usage: python -I build_tsv.py <en.json> <translation out dir> <tr.tsv> <report.txt>
"""
import collections
import glob
import json
import os
import re
import sys

sys.stdout.reconfigure(encoding="utf-8")
en_path, out_dir, tsv_path, report_path = sys.argv[1:5]
en = json.load(open(en_path, encoding="utf-8"))

tr = collections.defaultdict(dict)
problems = []
for path in sorted(glob.glob(os.path.join(out_dir, "chunk_*.tr*.json"))):
    try:
        data = json.load(open(path, encoding="utf-8-sig"))
    except Exception as e:
        problems.append(f"PARSE {os.path.basename(path)}: {e}")
        continue
    for coll, entries in data.items():
        for key, text in entries.items():
            if coll not in en or key not in en[coll]:
                problems.append(f"EXTRA {coll}/{key} in {os.path.basename(path)}")
                continue
            tr[coll][key] = text

PLACEHOLDER = re.compile(r"\{[^{}]*\}")
TAG = re.compile(r"<[^<>]+>")


def check(src, dst):
    errs = []
    if not isinstance(dst, str) or not dst.strip() and src.strip():
        return ["empty"]
    if collections.Counter(PLACEHOLDER.findall(src)) != collections.Counter(PLACEHOLDER.findall(dst)):
        errs.append(f"placeholders {PLACEHOLDER.findall(src)} -> {PLACEHOLDER.findall(dst)}")
    src_tags = collections.Counter(t.lower().replace(" ", "") for t in TAG.findall(src))
    dst_tags = collections.Counter(t.lower().replace(" ", "") for t in TAG.findall(dst))
    if src_tags != dst_tags:
        errs.append(f"tags {sorted(src_tags.elements())} -> {sorted(dst_tags.elements())}")
    if src.count("\n") != dst.count("\n"):
        errs.append(f"newlines {src.count(chr(10))} -> {dst.count(chr(10))}")
    return errs


def esc(s):
    return s.replace("\\", "\\\\").replace("\t", "\\t").replace("\r", "\\r").replace("\n", "\\n")


ok = missing = invalid = 0
lines = ["# Probably Stolen - Turkce Yama. Format: Collection<TAB>KeyId<TAB>Text"]
for coll, entries in en.items():
    for key, e in entries.items():
        src = e["text"]
        dst = tr.get(coll, {}).get(key)
        if dst is None:
            missing += 1
            problems.append(f"MISSING {coll}/{key}")
            text = src
        else:
            errs = check(src, dst)
            if errs:
                invalid += 1
                problems.append(f"INVALID {coll}/{key}: {'; '.join(errs)}\n    EN: {src!r}\n    TR: {dst!r}")
                text = src
            else:
                ok += 1
                text = dst
        lines.append(f"{coll}\t{e['id']}\t{esc(text)}")

os.makedirs(os.path.dirname(os.path.abspath(tsv_path)), exist_ok=True)
with open(tsv_path, "w", encoding="utf-8", newline="\n") as f:
    f.write("\n".join(lines) + "\n")
with open(report_path, "w", encoding="utf-8") as f:
    f.write("\n".join(problems) + "\n")

total = ok + missing + invalid
print(f"total {total}: translated OK {ok}, missing {missing}, invalid {invalid} (fallback to English)")
print(f"problems: {len(problems)} -> {report_path}")
for p in problems[:25]:
    print(p)
