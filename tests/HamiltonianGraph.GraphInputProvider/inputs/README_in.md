The first line represents matrix dimension: `n`  
Then `n` lines go with `n` cells in those, separating with space (`' '`).  
Each `n[i]` cell must be either:
* integer number, which represents weight from `row number` to `column number`, or
* hyphen (`'-'`), which says: there is no direct path between row and number

For example:
```
3
- 2 -
4 - 7
1 - -
```
That would be a **3x3 matrix**, with paths _(the first number)_ and weights _(the seconds number)_ in it
```
[0-1, 2],
[1-0, 4],
[1-2, 7],
[2-0, 1]
```