from pathlib import Path
import hashlib
import json
from zipfile import ZipFile
from lxml import etree
from docx import Document
root=Path(__file__).resolve().parent
ns={'w':'http://schemas.openxmlformats.org/wordprocessingml/2006/main'}
required=['Crust Innovators','Devanand S','Karthiga S','Dharsana K','Nandhini S','AVS Engineering College']
for file in root.glob('*.docx'):
    if file.name.startswith('~$'): continue
    with ZipFile(file) as z:
        assert z.testzip() is None
        xml=etree.fromstring(z.read('word/document.xml'))
        text=' '.join(xml.xpath('//w:t/text()',namespaces=ns))
        for name in required: assert name in text,(file.name,name)
        assert 'registration ID' not in text
        for table in xml.xpath('//w:tbl',namespaces=ns):
            widths=[int(n) for n in table.xpath('./w:tblGrid/w:gridCol/@w:w',namespaces=ns)]
            assert sum(widths)==9360,(file.name,widths)
            for row in table.xpath('./w:tr',namespaces=ns):
                actual=[int(n) for n in row.xpath('./w:tc/w:tcPr/w:tcW/@w:w',namespaces=ns)]
                assert widths==actual,(file.name,widths,actual)
        assert not xml.xpath('//w:trHeight[@w:hRule="exact"]',namespaces=ns)
    d=Document(file)
    assert round(d.sections[0].page_width.inches,2)==8.5
    assert round(d.sections[0].left_margin.inches,2)==1
    if 'Phase-2' in file.name:
        for section in ['1. 3D level','2. Complete gameplay','3. Essential UI','4. Technical plan','5. Riskiest mechanic','6. Asset plan','7. Playtest plan','8. Phase 2 self-check']:
            assert section in text,section
        assert 'independent human playtest is still pending' in text
    if 'With-Dated-Evidence' in file.name:
        assert len(d.inline_shapes)==24,len(d.inline_shapes)
        assert 'Appendix A. Dated Unity asset evidence' in text
        manifest=json.loads((root/'Dated-Asset-Evidence/manifest.json').read_text(encoding='utf-8'))
        assert len(manifest)==12
        with ZipFile(file) as z:
            embedded={hashlib.sha256(z.read(name)).hexdigest() for name in z.namelist() if name.startswith('word/media/')}
        for item in manifest:
            assert item['file'] in text,item['file']
            assert item['sha256'] in embedded,('Screenshot changed or missing',item['file'])
            assert item['sha256']==hashlib.sha256((root/'Dated-Asset-Evidence'/item['file']).read_bytes()).hexdigest()
            if item['visible_clock']: assert item['visible_clock'] in text
    print(file.name+': identity, content, table geometry, images and DOCX integrity checked.')
print('Structural verification passed. Visual review remains a separate gate.')
