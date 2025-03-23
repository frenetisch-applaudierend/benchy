# Hashing example

Git repository with 3 commits.

| Commit                                                                    | Message               |
| ------------------------------------------------------------------------- | --------------------- |
| 62af84848aab2be1add0487cb1492c7285abff46                                  | Add Empty Project     |
| e367902a8c2e87fef7d06396bc1142f9c81e1753 (tag: algs/md5)                  | Add Benchmark for MD5 |
| 7b38acc58384f49497fd5fe4bb4d93686f88f421 (tag: algs/sha256, branch: main) | Update hashing        |

Example usage:

```bash
benchy Examples/hashing.git algs/md5 algs/sha256 -b ExmapleBenchmark
```
