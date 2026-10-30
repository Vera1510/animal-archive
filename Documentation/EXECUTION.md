# Execution

## PUB-001 — Public GitHub publication

Status: Completed, 2026-10-30.

Repository: https://github.com/Vera1510/animal-archive (public).

Scope: Publish the editable project, required assets and concise documentation; retain the existing Web address. Exclude personal coursework, temporary media, caches, secrets and internal history. Use a generic project commit identity.

Acceptance: Review staged files for personal data and credentials; make a local source commit; push to a public repository; verify remote branch and anonymous access. Do not launch the game or Unity.

Checks: Repository content, privacy filtering, Git integrity and remote publication. Earlier game build/gameplay evidence predates this publication and does not represent a fresh gameplay run.

Results: 300 tracked files, approximately 62 MB of working files, largest file below 6 MB; no LFS dependency. The privacy/credential scan found no specified personal identifiers, private email addresses or credential patterns in published files. Required asset `.meta` pairs are included. Anonymous GitHub API access confirmed public visibility, matching commit identity and all 300 file paths after the first push. The final documentation update is checked against the remote branch before handoff.

Maintenance adjustments: The Web builder now creates its output-report directory on a fresh checkout; the selected audio synthesis script resolves output paths from its own project location. These adjustments received static review only during publication. No Unity or game process was launched. Generated Unity YAML contains its existing trailing whitespace; no bulk reformat was applied.

Exclusions: Course PPT/video/build bundles and personal submission identity remain local. Historical internal notes, caches, logs, deployment credentials and machine-specific state were not committed. GitHub hosts source; the Web game remains at its existing address. No new hosting or paid service was enabled.
