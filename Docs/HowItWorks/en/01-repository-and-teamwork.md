# 01 · Repository and teamwork

**In one sentence:** the project lives in a git repository on GitHub, where every change arrives on its own branch and is merged into `main` through a reviewed pull request.

## Why we need it

Four people change the same Unity project. Without version control we would overwrite each other's work, lose history, and have no way to undo a broken change. GitHub also gives us a shared task board and a record of who did what.

## How it works

1. **The repository** stores every file needed to open the project: `Assets/`, `Packages/`, `ProjectSettings/` and our documents. Files Unity regenerates on each computer (`Library/`, `Temp/`, `Logs/`, `UserSettings/`, IDE project files) are listed in `.gitignore` and never stored.
2. **Branches.** `main` always holds the latest working version. For each task you create a branch from `main` (e.g. `art/silo-model`), commit your work there, and push it to GitHub.
3. **Pull requests.** On GitHub you open a pull request from your branch to `main`. Tymoteusz reviews it and merges it with **Squash and merge**, which turns the whole branch into one commit on `main`, titled after the pull request.
4. **Protected `main`.** A GitHub ruleset blocks direct pushes, force pushes and deletion of `main`, and requires a linear history. Even the repository owner goes through pull requests, so nothing reaches `main` unreviewed.
5. **Git LFS.** Git is built for text. Binary files (`.png`, `.fbx`, `.wav`, `.psd`, fonts) are large and change completely with every edit, which would bloat the history. `.gitattributes` sends them to Git LFS: the repository stores a small pointer, and the real file is stored separately and downloaded when needed.
6. **Locking.** Binary source files cannot be merged, so two people editing the same `.psd` would lose one person's work. Those types are marked `lockable`: `git lfs lock <file>` reserves a file until it is unlocked.
7. **Meta files.** Unity gives every asset a `.meta` file containing its unique ID (GUID). Scenes and prefabs refer to assets by that ID, so a `.meta` file must always be committed together with its asset; losing it breaks every reference to the asset.
8. **Text serialization.** Unity is set to save scenes, prefabs and assets as text (YAML), so git can show and merge changes line by line. `UnityYAMLMerge`, Unity's merge tool, is registered for these files in `.gitattributes` because it understands their structure better than a plain text merge.
9. **Commit messages** follow Conventional Commits (`feat(economy): add global storage`), so the history reads as a changelog and can be filtered by type and module.

## Why this approach

| Option | Why not (or why) |
| --- | --- |
| Shared drive or zip files | No history, no merging, constant overwrites. |
| Unity Version Control (Plastic) | Handles binaries well, but the team already knows GitHub, and GitHub's free tier, pull requests and project board cover our needs. |
| Git without LFS | Repository size would grow with every texture change and cloning would become slow. |
| Pushing straight to `main` | Fast, but one mistake breaks the project for everyone. Pull requests add a review step at almost no cost. |
| Merge commits instead of squash | Keeps every small work-in-progress commit on `main`; squash keeps `main` readable, one commit per finished task. |

## Where it lives

- `.gitignore`: files git never stores.
- `.gitattributes`: line endings, LFS types, lockable types, merge tool.
- `README.md`: setup steps and team rules.
- GitHub settings: the ruleset "Protect main", squash-only merging, labels, project board.

## How we know it works

- `git lfs ls-files` lists the files stored in LFS.
- Trying to push to `main` directly is rejected by GitHub.
- Cloning the repository on a fresh computer and opening it in the exact Unity version reproduces the project.

## Likely questions

**Why do some files show as "pointers" on GitHub?** They are stored with Git LFS; GitHub shows the real file when you open it, the repository itself stores only a reference.

**What happens if two people edit the same scene?** Git tries to merge; with UnityYAMLMerge simple changes merge automatically. Complex conflicts are avoided by agreeing that only one person edits a scene at a time and by putting reusable objects in prefabs.

**Why is `Library/` not in the repository?** It is a cache Unity rebuilds from the assets on every computer. Storing it would add gigabytes and cause constant conflicts.

**Why must everyone use exactly the same Unity version?** Different versions re-save many files in slightly different formats, which creates noise in every commit and can break assets.

**How do you know who changed what?** Every commit records its author and date, and every change to `main` is linked to a pull request with a description.
