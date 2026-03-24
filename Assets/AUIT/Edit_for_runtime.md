# AUIT Code Changes for Standalone Runtime Execution (Quest Build)

This document outlines the underlying issues that prevented the `Experiment_1_Final` adaptation UI logic from executing correctly in standalone builds (such as on the Quest 3) and how those issues were resolved. 

The differences between checking mechanics in the Unity Editor and executing them in a standalone runtime fundamentally come down to context boundaries (`#if UNITY_EDITOR`) and missing fallback operations when expected components (like `OnValidate`) are stripped from the runtime environment.

## Changes Made

### 1. Fixing `_asyncSolver` Initialization in `AUIT.cs`
**The Issue:**
In the Editor, Unity dynamically calls `OnValidate()` every time properties are changed in the Inspector or the code is initialized. The original layout system assigned its solver via this method:

```csharp
private void OnValidate()
{
    if (_previousSolver != backendSolver.solver)
        AssignSolver();
}
```

However, `OnValidate()` is strictly an **Editor-only** execution concept and is explicitly stripped out during standard standalone builds. Because no solver was being successfully assigned at runtime, `_asyncSolver` remained `null`. When `Start()` subsequently called `InitializeSolver(...)`, a silent `NullReferenceException` wiped out the object initialization and caused all `ExperimentInstruction` canvas layouts to lock up.

**The Fix:**
I added an explicit fallback during the `Awake()` method. This safely instantiates the solver specifically for runtime using the serialized reference values when `OnValidate()` behavior is stripped:

```csharp
private void Awake()
{
    Instance = this;

    // Ensure _asyncSolver is initialized at runtime properly
    if (_asyncSolver == null)
    {
        if (solverSettings != null)
        {
            _asyncSolver = solverSettings;
        }
        else
        {
            AssignSolver();
        }
    }
}
```
An internal `null` check was also added in `OnDestroy()` to ensure `_asyncSolver.Destroy()` safely terminates over the course of frame lifecycle changes.

---

### 2. Exception Handling for `AsyncIO.ForceDotNet.Force()`

**The Issue:**
At the start of `AUIT.cs`, the line `AsyncIO.ForceDotNet.Force();` immediately attempts to force synchronous loading of .NET behaviors for NetMQ. Unfortunately, Android endpoints (and many standalone AR builds out-of-box contexts) often lack specific pipeline layers and trigger an unhandled `PlatformNotSupportedException`. An unhandled exception in `Start()` terminates the entire startup routine.

**The Fix:**
I safely wrapped this line in a `try-catch` block. It operates correctly where requested, but skips crashing the Adaptation Manager if execution contexts are unavailable.

```csharp
private void Start()
{
    try 
    {
        AsyncIO.ForceDotNet.Force();
    } 
    catch (Exception e) 
    {
        Debug.LogWarning($"[AUIT] AsyncIO.ForceDotNet.Force() threw an exception (likely unsupported on this platform): {e.Message}");
    }
    // ... continues setup
}
```

---

### 3. Removal of Misplaced `UnityEditor` References

**The Issue:**
The directive `using UnityEditor;` was mistakenly requested globally on numerous behavior implementations without being properly safeguarded by `#if UNITY_EDITOR` preprocessor directives. This causes hard stops at compilation time for standalone devices because the `UnityEditor` library actively refuses to package onto deployable standalone machines.

**The Fix:**
I safely guarded and/or scrubbed non-essential references. 
- Removed `using UnityEditor` outright where unused: `LocalObjective.cs`, `AvoidPhysicalOcclusionObjective.cs`, `KDBounds.cs`, `KDQueryNode.cs`
- Handled via `#if UNITY_EDITOR ... #endif` checks for custom inspectors that genuinely request it: `FieldOfViewGridObjective.cs`, `OnRequestOptimizationTrigger.cs`, and property drawers in `AUIT.cs`.

**Example:**
```csharp
#if UNITY_EDITOR
using UnityEditor;
#endif

// ... Component Implementation ...

#if UNITY_EDITOR
[CustomEditor(typeof(FieldOfViewGridObjective))]
public class FieldOfViewGridObjectiveEditor : Editor
{
   // Editor GUI routines
}
#endif
```