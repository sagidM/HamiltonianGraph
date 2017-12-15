The first line represents two values: matrix dimension - `n`, and shortest path index - `k`  
Then `n` lines go with `n+1` cells in those, separating with space (`' '`).  
Each `n[i]` line represents Hamiltonian cycle.  
The first and the last number in paths are the same.

For example:
```
4 0
0 3 2 4 1 0
0 3 4 2 1 0
0 3 4 1 2 0
0 3 2 1 4 0
```
That would be a 4 paths.  
And the shortest one is supposed to be `[0 3 2 4 1 0]`