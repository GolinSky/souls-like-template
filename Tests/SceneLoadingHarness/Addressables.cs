using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
namespace UnityEngine.AddressableAssets;
public static class Addressables
{
    public sealed class Operation<T>
    {
        public string Path;
        public bool Done, Unload, Valid = true;
        public AsyncOperationStatus Status;
        public Exception Error;
        public T Result;
        public Action Complete;
    }

    public static readonly List<Operation<SceneInstance>> Operations = [];
    public static readonly List<string> Loads = [], Unloads = [], Releases = [];
    public static readonly HashSet<string> Loaded = [], FailLoads = [], ThrowLoads = [], FailUnloads = [];
    public static int MaxPendingAdditive;

    public static void Reset()
    {
        Operations.Clear(); Loads.Clear(); Unloads.Clear(); Releases.Clear(); Loaded.Clear();
        FailLoads.Clear(); ThrowLoads.Clear(); FailUnloads.Clear(); MaxPendingAdditive = 0;
        SceneManager.Active = default; SceneManager.FailActivation = false; UnityEngine.Debug.Errors.Clear();
    }

    public static AsyncOperationHandle<SceneInstance> LoadSceneAsync(string path, LoadSceneMode mode)
    {
        if (ThrowLoads.Contains(path)) throw new InvalidOperationException("Start failed: " + path);
        Loads.Add(path);
        var op = new Operation<SceneInstance> { Path = path, Result = new(new Scene(path)) };
        op.Complete = () =>
        {
            op.Done = true;
            op.Status = FailLoads.Contains(path) ? AsyncOperationStatus.Failed : AsyncOperationStatus.Succeeded;
            if (op.Status == AsyncOperationStatus.Failed) op.Error = new Exception("Load failed: " + path);
            else
            {
                if (mode == LoadSceneMode.Single)
                {
                    Loaded.Clear();
                    // Approximate Addressables' scene-unloaded callback releasing old scene owners.
                    foreach (var old in Operations.Where(x => x != op && x.Done && !x.Unload && x.Status == AsyncOperationStatus.Succeeded)) old.Valid = false;
                }
                Loaded.Add(path);
            }
        };
        Operations.Add(op);
        if (mode == LoadSceneMode.Additive)
            MaxPendingAdditive = Math.Max(MaxPendingAdditive, Operations.Count(x => !x.Done && !x.Unload && x.Path != "Loading"));
        return new(op);
    }

    public static AsyncOperationHandle<SceneInstance> UnloadSceneAsync(AsyncOperationHandle<SceneInstance> load, bool autoReleaseHandle)
    {
        if (autoReleaseHandle) throw new Exception("Harness expects explicit unload-operation ownership");
        var original = load.Operation;
        _ = load.Result;
        Unloads.Add(original.Path);
        var op = new Operation<SceneInstance> { Path = original.Path, Unload = true, Result = original.Result };
        op.Complete = () =>
        {
            op.Done = true;
            op.Status = FailUnloads.Contains(op.Path) ? AsyncOperationStatus.Failed : AsyncOperationStatus.Succeeded;
            if (op.Status == AsyncOperationStatus.Failed) op.Error = new Exception("Unload failed: " + op.Path);
            else { Loaded.Remove(op.Path); original.Valid = false; }
        };
        Operations.Add(op);
        return new(op);
    }

    public static void Release(AsyncOperationHandle<SceneInstance> handle)
    {
        if (!handle.IsValid()) throw new Exception("Double release");
        Releases.Add((handle.Operation.Unload ? "unload:" : "load:") + handle.Operation.Path);
        handle.Operation.Valid = false;
    }

    public static void CompletePending()
    {
        foreach (var op in Operations.Where(x => !x.Done).ToArray()) op.Complete();
    }
}
