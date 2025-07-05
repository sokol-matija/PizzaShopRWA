---
description: Git commit message formatting rules
globs: 
alwaysApply: false
---

# Git Usage Rules

## Commit Message Format

Use the following **prefixes** for commit messages, followed by a **colon** and a **space**:

- `fix` — for bug fixes
- `feat` — for new features
- `perf` — for performance improvements
- `docs` — for documentation changes
- `style` — for formatting changes
- `refactor` — for code refactoring
- `test` — for adding missing tests
- `chore` — for routine tasks

## Guidelines

- When determining the commit message prefix, pick the most **relevant** option from the list above
- Use **lowercase** for all commit messages
- If the change is not self-explanatory, include a **bullet list of changes** after a blank line below the summary

## Examples

```
feat: add image optimization with lazy loading

- implemented OptimizedImage.razor component
- added HTML extensions for responsive images
- integrated compression and srcset generation
- updated trips and destinations pages
```

```
fix: resolve build error in unsplash service

- removed ImageSize enum dependency
- updated component to use string-based parameters
- fixed compilation issues
```

```
perf: optimize image loading performance

- reduced image sizes by 80% using responsive sizing
- added lazy loading to prevent unnecessary downloads
- implemented browser caching headers
``` 