# Hamiltonian Graph

![Nuget](https://img.shields.io/nuget/v/hamiltoniangraph.svg)
[![Build Status](https://travis-ci.org/kofon95/HamiltonianGraph.svg?branch=master)](https://travis-ci.org/kofon95/HamiltonianGraph)

## What is that?
A Hamiltonian graph, also called a Hamilton graph, is a graph possessing a Hamiltonian cycle (or circuit).

## Travelling salesman problem [(or TSP)](https://en.wikipedia.org/wiki/Travelling_salesman_problem)
**Travelling salesman problem** asks the following question: _"Given a list of cities and the distances between each pair of cities, what is the shortest possible route that visits each city exactly once and returns to the origin city?"_  
The problem belongs to the class of NP-complete problems.

## Project
There are two algorithms presented here:
* LatinComposition
* [Branch and bound (BnB)](https://en.wikipedia.org/wiki/Branch_and_bound)

## Usage
```cs
string input =
@"5
- 24 17 8 -
1 - 3 - 11
17 8 - - 6
8 - 9 - 12
17 11 6 - -
";

int?[,] weights = GraphUtil.FromMatrixFormat(input);
int[] cycle = new BranchAndBound(weights).GetShortestHamiltonianCycle();


// output: "0 -> 3 -> 4 -> 2 -> 1 -> 0"
Console.WriteLine(string.Join(" -> ", cycle));

// or: "A -> D -> E -> C -> B -> A"
string cycleSymbols = string.Join(" -> ", cycle.Select(vertex => (char)(vertex + 'A')));
Console.WriteLine(cycleSymbols);
```
