# Postprocess — ct-git-diff

After the analysis completes, present these options to the user based on findings. **Never auto-invoke any of them.**

---

## 1. Deep Code Review

If the checklist flagged async, EF Core, DTO, or architecture violations:

```
/review-code <highest-risk changed file>
```

---

## 2. Unit Test Generation

If test gaps were found (changed files with no corresponding test changes):

```
/ct-unittest <untested UseCase or Repository>
```

---

## 3. Bug Fix

If the checklist flagged runtime-risk issues (missing AsNoTracking, .Result/.Wait(), entity leakage):

```
/ct-bugfix-skill
```

---

## 4. Full Diff Inspection

If the user wants to dive deeper into specific changes (only shown in summary mode):

```
/ct-git-diff <target> --full --path <narrowed path>
```

---

## 5. PR Description

If the narrative from Layer 4 is ready, the user can copy it directly into their PR description on GitHub/Bitbucket.
