import json
from pathlib import Path
import tempfile
import unittest
from install import install, PACKAGE, DEFAULT_URL


class InstallTests(unittest.TestCase):
    def setUp(self):
        self.directory = tempfile.TemporaryDirectory(prefix="editorcraft-test-")
        self.addCleanup(self.directory.cleanup)
        self.root = Path(self.directory.name)
        (self.root / "Packages").mkdir()
        (self.root / "ProjectSettings").mkdir()
        (self.root / "ProjectSettings/ProjectVersion.txt").write_text("m_EditorVersion: 6000.6.5f1\n")
        self.manifest = self.root / "Packages/manifest.json"
        self.original = b'{"dependencies":{"com.example.existing":"2.0.0"},"scopedRegistries":[]}\n'
        self.manifest.write_bytes(self.original)

    def test_preserve_backup_and_idempotence(self):
        install(self.root)
        data = json.loads(self.manifest.read_text())
        self.assertEqual(data['dependencies'][PACKAGE], DEFAULT_URL)
        self.assertEqual(data['dependencies']['com.example.existing'], '2.0.0')
        self.assertEqual(data['scopedRegistries'], [])
        self.assertEqual(self.manifest.with_name('manifest.json.editorcraft-backup').read_bytes(), self.original)
        first = self.manifest.read_bytes()
        install(self.root)
        self.assertEqual(self.manifest.read_bytes(), first)

    def test_refuse_conflicting_dependency(self):
        self.manifest.write_text(json.dumps({'dependencies': {PACKAGE: 'file:../local'}}))
        before = self.manifest.read_bytes()
        with self.assertRaises(ValueError): install(self.root)
        self.assertEqual(self.manifest.read_bytes(), before)

    def test_invalid_json_unchanged(self):
        for source in ('{"dependencies":{},"dependencies":{}}', '{broken', '{"dependencies":[]}'):
            self.manifest.write_text(source)
            with self.assertRaises(ValueError): install(self.root)
            self.assertEqual(self.manifest.read_text(), source)

    def test_refuse_embedded_package(self):
        (self.root / 'Packages' / PACKAGE).mkdir()
        with self.assertRaises(ValueError): install(self.root)
        self.assertEqual(self.manifest.read_bytes(), self.original)

    def test_refuse_non_project(self):
        with self.assertRaises(ValueError): install(self.root / 'Packages')

    def test_never_overwrite_backup(self):
        backup = self.manifest.with_name('manifest.json.editorcraft-backup')
        backup.write_text('previous backup')
        with self.assertRaises(FileExistsError): install(self.root)
        self.assertEqual(backup.read_text(), 'previous backup')
        self.assertEqual(self.manifest.read_bytes(), self.original)

    def test_custom_ssh_alias(self):
        url = DEFAULT_URL.replace('github.com', 'github-dghn')
        install(self.root, url)
        self.assertEqual(json.loads(self.manifest.read_text())['dependencies'][PACKAGE], url)


if __name__ == '__main__':
    unittest.main()
