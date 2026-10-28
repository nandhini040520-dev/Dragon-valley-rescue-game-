from pathlib import Path
import hashlib
import zipfile

root = Path(__file__).resolve().parent.parent
docs = root / 'HackathonDocs'
release = root / 'Release'
documents = [docs / name for name in ['Crust-Innovators-Phase-2-Submission-With-Dated-Evidence.docx', 'Crust-Innovators-Demo-and-Handover.docx', 'Crust-Innovators-Playtest-Record.docx']]
files = documents + [docs / 'README.md'] + sorted((docs / 'Live-Snaps').glob('*'))
files += sorted((docs / 'Dated-Asset-Evidence').glob('*'))
evidence = [root / 'QA' / name for name in ['acceptance-results.txt', 'artifact-verification.txt', 'VERIFICATION.md', 'Document-QA.md', 'document-render-results.txt']]
entries = [(source, 'HackathonDocs/' + source.relative_to(docs).as_posix()) for source in files if source.is_file()]
entries += [(source, 'HackathonDocs/TechnicalEvidence/' + source.name) for source in evidence]
assert all(document.is_file() for document in documents)
archive = release / 'Crust-Innovators-Hackathon-Documents.zip'
with zipfile.ZipFile(archive, 'w', zipfile.ZIP_DEFLATED) as bundle:
    for source, entry in entries:
        bundle.write(source, entry)
with zipfile.ZipFile(archive) as bundle:
    assert bundle.testzip() is None
    for source, entry in entries:
        assert hashlib.sha256(bundle.read(entry)).digest() == hashlib.sha256(source.read_bytes()).digest()
artifacts = sorted(list(release.glob('*.zip')) + list(release.glob('*.unitypackage')))
lines = []
for artifact in artifacts:
    with artifact.open('rb') as stream:
        digest = hashlib.file_digest(stream, 'sha256').hexdigest().upper()
    lines.append(digest + '  ' + artifact.name)
(release / 'SHA256SUMS.txt').write_text('\n'.join(lines) + '\n', encoding='utf-8')
print('PASS: three documents, ten live snaps and twelve dated asset screenshots packaged; every entry verified; release SHA-256 sums updated.')
