# Archive documentation (session notes)

Les fichiers `ANALYSE_*`, `RESUME_*`, `RECAP_*`, `CORRECTION_*`, `GUIDE_TEST_*`,
et autres journaux de session (`AJOUT_*`, `MODIFICATION_*`, etc.) sont archivés ici
(`docs/archive/md/`), pas la documentation produit.

Les scripts SQL one-shot sont dans `docs/archive/sql/`.

**À conserver à la racine / dans `docs/` :**

- `README.md`
- `ARCHITECTURE.md`
- `FRONTEND_DEVELOPER_GUIDE.md`
- `API_DOCUMENTATION*.md`
- `DOCUMENTATION_*.md`
- `docs/SCHEMA_SOURCE_OF_TRUTH.md`
- `docs/PLAN_UPGRADE_NET8_ET_SCHEMA.md`
- `appsettings.template.json`

**Source de vérité schéma :** migrations EF (`Migrations/*.cs`), dont `AddReportingViews`.

Ne pas committer de secrets dans ces archives ; utiliser des placeholders.
