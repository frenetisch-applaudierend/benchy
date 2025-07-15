# Hashing example

Git repository with 3 commits.

| Commit                                                                    | Message                        |
| ------------------------------------------------------------------------- | ------------------------------ |
| edf4650bb217844db9d264a420a2a92a63a91173                                  | Add Empty Project              |
| 1ca2092ef75fe1f54f8308842c58fa856268a789 (tag: algs/md5)                  | Add Benchmark for MD5          |
| 4545cc41c540f3f5274d6b2fb15a510c9a2548f0 (tag: algs/sha256, branch: main) | Update Benchmark to use SHA256 |

Example usage:

```bash
benchy compare --repository-path Examples/hashing.git algs/md5 algs/sha256 -b ExampleBenchmark
```
