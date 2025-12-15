using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Profiling;

public class AlgorithmPerformanceTester : MonoBehaviour
{
    [Header("Test Config")]
    [Tooltip("Số lần lặp để lấy trung bình (nên >= 10 cho kết quả ổn định)")]
    public int runsPerAlgorithm = 10;

    [Header("Pool Options")]
    [Tooltip("If true, disable all pooled objects before each algorithm run to reduce scene load/lag.")]
    public bool disablePoolDuringTest = true;

    private ResourceAllocationGA ga;
    private ResourceAllocationDE de;
    private ResourceAllocationCMAES cmaes;

    [ContextMenu("Testing Algorithm")]
    public void StartTestingAlgorithm()
    {
        UnityEngine.Debug.Log("=== 🔍 Bắt đầu kiểm tra hiệu suất thuật toán ===");
        ga = GetComponent<ResourceAllocationGA>();
        de = GetComponent<ResourceAllocationDE>();
        cmaes = GetComponent<ResourceAllocationCMAES>();

        // Start coroutine that runs tests sequentially and waits for completion
        StartCoroutine(RunAllTests());
    }

    IEnumerator RunAllTests()
    {
        if (ga != null)
            yield return StartCoroutine(TestAlgorithmRoutine("GA", () => ga.RunGA(), () => ga.isOptimized, () => ga.isOptimized = false, runsPerAlgorithm));
        else
            UnityEngine.Debug.LogWarning("GA component not found on GameObject.");

        if (de != null)
            yield return StartCoroutine(TestAlgorithmRoutine("DE", () => de.RunDE(), () => de.isOptimized, () => de.isOptimized = false, runsPerAlgorithm));
        else
            UnityEngine.Debug.LogWarning("DE component not found on GameObject.");

        if (cmaes != null)
            yield return StartCoroutine(TestAlgorithmRoutine("CMA-ES", () => cmaes.RunCMAES(), () => cmaes.isOptimized, () => cmaes.isOptimized = false, runsPerAlgorithm));
        else
            UnityEngine.Debug.LogWarning("CMA-ES component not found on GameObject.");

        UnityEngine.Debug.Log("=== ✅ Kiểm tra hoàn tất ===");
    }

    IEnumerator TestAlgorithmRoutine(string name, Action startAction, Func<bool> finishedPredicate, Action resetAction, int runs)
    {
        List<long> times = new List<long>();
        List<long> managedMemories = new List<long>();
        List<long> unityAllocated = new List<long>();

        // Warm-up run (and wait for it to finish)
        if (startAction == null || finishedPredicate == null)
        {
            UnityEngine.Debug.LogError($"[{name}] Invalid parameters passed to TestAlgorithmRoutine");
            yield break;
        }

        // Optionally disable pooled objects to reduce scene overhead
        bool poolWasModified = false;
        if (disablePoolDuringTest && ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.SetAllPooledActive(false);
            poolWasModified = true;
            // wait a frame for Unity to process deactivations
            yield return null;
        }

        // Ensure component is not considered finished from previous runs
        resetAction?.Invoke();

        //UnityEngine.Debug.Log($"[{name}] Warm-up run starting...");
        startAction.Invoke();
        yield return StartCoroutine(WaitUntilFinishedWithTimeout(finishedPredicate, 600f));
        resetAction?.Invoke();
        yield return null;

        for (int i = 0; i < runs; i++)
        {
            GC.Collect();
            long beforeManaged = GC.GetTotalMemory(true);
            long beforeUnity = Profiler.GetTotalAllocatedMemoryLong();

            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
            startAction.Invoke();
            // Wait until the algorithm sets its isOptimized flag, with timeout
            yield return StartCoroutine(WaitUntilFinishedWithTimeout(finishedPredicate, 600f));
            sw.Stop();

            long afterManaged = GC.GetTotalMemory(false);
            long afterUnity = Profiler.GetTotalAllocatedMemoryLong();

            long managedUsed = Math.Max(0, afterManaged - beforeManaged);
            long unityUsed = Math.Max(0, afterUnity - beforeUnity);

            times.Add(sw.ElapsedMilliseconds);
            managedMemories.Add(managedUsed);
            unityAllocated.Add(unityUsed);

            // Reset finished flag so next run will wait again
            resetAction?.Invoke();

            // Give the engine a frame to breathe
            yield return null;
        }

        // Restore pool active state if we modified it
        if (poolWasModified && ObjectPooler.Instance != null)
        {
            ObjectPooler.Instance.SetAllPooledActive(true);
            // allow a frame for re-activation
            yield return null;
        }

        float avgTime = (float)times.Average();
        float avgManagedMB = (float)managedMemories.Average() / 1024f / 1024f;
        float avgUnityMB = (float)unityAllocated.Average() / 1024f / 1024f;
        long minTime = times.Min();
        long maxTime = times.Max();

        UnityEngine.Debug.Log(
            $"[{name}] Avg Time: {avgTime:F2} ms | Min: {minTime} ms | Max: {maxTime} ms | " +
            $"Avg Managed: {avgManagedMB:F3} MB | Avg Unity Alloc: {avgUnityMB:F3} MB | Runs: {runs}"
        );

        yield break;
    }

    IEnumerator WaitUntilFinishedWithTimeout(Func<bool> finishedPredicate, float timeoutSeconds)
    {
        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();
        while (!finishedPredicate())
        {
            if (sw.Elapsed.TotalSeconds > timeoutSeconds)
            {
                UnityEngine.Debug.LogWarning($"WaitUntilFinishedWithTimeout: timed out after {timeoutSeconds} seconds.");
                yield break;
            }
            yield return null;
        }
    }
}