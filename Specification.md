# Benchmark Comparison Tool Specification

## Overview

This tool compares the performance of a **BenchmarkDotNet** benchmark **provided by the program being tested** between two different commits of a software repository. The user provides the commit hashes via command-line arguments, and the tool runs the benchmark on both commits, interprets the results, reports them to the console, and cleans up any generated files afterward.

---

## Requirements

### 1. Command-Line Arguments

- `repo_path`: The path to the local Git repository (string).
- `commit_hash_1`: The first commit hash (string).
- `commit_hash_2`: The second commit hash (string).

### 2. Benchmark Framework

- Supports only **BenchmarkDotNet**.
- The **benchmark code must be part of the tested program**.
- Assumes the repository contains a valid build process and configuration to run the **BenchmarkDotNet** benchmarks.

### 3. Functionality

- Check out each commit and run the benchmark.
- Capture and parse the benchmark results.
- Compare the results and display them side by side.
- Clean up generated files.

### 4. Executing the Benchmark

- Uses the command:
  ```bash
  dotnet run -c Release --project <path_to_benchmark_project>
  ```

### 5. Capturing and Interpreting Results

- Captures console output from **BenchmarkDotNet**.
- Parses key metrics such as **Mean, Median, Memory Allocations**.

### 6. Reporting Results

- Displays a side-by-side comparison of results.
- Highlights performance regressions or improvements if possible.

### 7. Error Handling

- Reports errors for invalid paths, hashes, or execution failures.
- Aborts execution with meaningful messages.

### 8. Cleanup

- Removes temporary and generated files after execution.

---

## Implementation Steps in .NET 9 using C\#

### Step 1: Create and Configure the Project

- Create a .NET Console Application.
- Add `LibGit2Sharp` for Git operations.
- Add `System.CommandLine` for command-line argument parsing.

### Step 2: Parse Command-Line Arguments

- Use `System.CommandLine` to capture inputs.

### Step 3: Validate the Repository Path

- Use `LibGit2Sharp` to verify if the path is a valid Git repository.

### Step 4: Checkout and Build Each Commit

- Implement `CheckoutCommit` using `LibGit2Sharp`.
- Execute the build with `dotnet build`.

### Step 5: Run the Benchmark and Capture Results

- Run the benchmark and capture output.

### Step 6: Parse BenchmarkDotNet Output

- Extract key metrics using Regex.

### Step 7: Compare and Display Results

- Display results side by side.

### Step 8: Clean Up Generated Files

- Remove build artifacts and temporary files.

### Step 9: Error Handling

- Handle missing files, build failures, and invalid inputs.

### Step 10: Testing and Optimization

- Test with valid and invalid inputs.

---

## Key Libraries

- **LibGit2Sharp:** For Git operations.
- **System.CommandLine:** For command-line handling.
- **Regex:** For parsing BenchmarkDotNet output
