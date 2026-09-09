using SoulsLike.Services.Scenes;
using SoulsLike.Services.Scenes.Data;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public static class Program
{
    private static TestContext _context;
    private static int _passed;

    public static int Main()
    {
        try
        {
            Run("shared model owns transition state", () =>
            {
                var model = new SceneModel(new SceneData());
                var firstService = new SceneService(model);
                var secondService = new SceneService(model);
                Task first = firstService.LoadScene(SceneType.DefaultLocation);
                Check(model.IsLoadingScene, "Model reflects loading immediately");
                Task second = secondService.LoadScene(SceneType.DefaultLocation);
                Check(second.IsFaulted && Addressables.Loads.Count == 1, "Shared model rejects overlap");
                Check(model.IsLoadingScene, "Rejected call must not clear accepted ownership");
                Finish(first);
                Check(!model.IsLoadingScene, "Model resets after success");
            });
            Run("concurrent dependencies and target last", () =>
            {
                var service = NewService(); Addressables.HoldLoads.Add("Zone2");
                Task task = service.LoadScene(SceneType.DefaultLocation);
                for (int i = 0; i < 5; i++) _context.Tick();
                Check(Addressables.MaxPendingAdditive == 3, "All dependencies start together");
                Check(Addressables.Loaded.Contains("Zone1") && Addressables.Loaded.Contains("Zone3"), "Other dependencies complete independently");
                Check(!Addressables.Loads.Contains("DefaultLocation"), "Target waits for every dependency");
                Addressables.HoldLoads.Clear(); Finish(task);
                Check(SceneManager.Active.path == "DefaultLocation", "Main scene activated last");
                Check(Addressables.Loaded.SetEquals(["Zone1", "Zone2", "Zone3", "DefaultLocation"]), "Full destination residency");
                Check(Addressables.Unloads.SequenceEqual(["Loading"]), "Normal Loading unload retained");
                Check(Addressables.Releases.SequenceEqual(["unload:Loading"]), "Normal unload handle released");
            });
            Run("failure propagates while another dependency is pending", () =>
            {
                var model = new SceneModel(new SceneData()); var service = new SceneService(model);
                Addressables.FailLoads.Add("Zone1"); Addressables.HoldLoads.Add("Zone2");
                Task task = service.LoadScene(SceneType.DefaultLocation); Fault(task);
                Check(Addressables.Operations.Any(x => x.Path == "Zone2" && !x.Done), "Failure must not wait for other dependencies");
                Check(!Addressables.Loads.Contains("DefaultLocation"), "Failed dependency prevents target start");
                Check(!model.IsLoadingScene, "Model resets after fault");
                NoRollback();
            });
            Run("loading failure propagates without cleanup", () =>
            {
                Addressables.FailLoads.Add("Loading"); Fault(NewService().LoadScene(SceneType.DefaultLocation));
                Check(Addressables.Loads.SequenceEqual(["Loading"]), "No dependencies after Loading failure");
                NoRollback();
            });
            Run("target failure propagates without cleanup", () =>
            {
                Addressables.FailLoads.Add("DefaultLocation"); Fault(NewService().LoadScene(SceneType.DefaultLocation));
                Check(Addressables.Loaded.Contains("Zone1"), "Partial scene residency deliberately retained");
                NoRollback();
            });
            Run("synchronous start failure propagates", () =>
            {
                Addressables.ThrowLoads.Add("Zone2"); Fault(NewService().LoadScene(SceneType.DefaultLocation));
                Check(!Addressables.Loads.Contains("DefaultLocation"), "No target after start failure");
                NoRollback();
            });
            Run("activation failure propagates", () =>
            {
                SceneManager.FailActivation = true; Fault(NewService().LoadScene(SceneType.DefaultLocation)); NoRollback();
            });
            Run("normal unloading failure propagates", () =>
            {
                Addressables.FailUnloads.Add("Loading"); Fault(NewService().LoadScene(SceneType.DefaultLocation));
                Check(Addressables.Unloads.SequenceEqual(["Loading"]), "No destination rollback");
                Check(Addressables.Releases.SequenceEqual(["unload:Loading"]), "Normal unload operation released");
                Check(UnityEngine.Debug.Errors.Count == 0, "No swallowed/logged failure");
            });
            Run("callback exception is preserved", () =>
            {
                var service = NewService(); var failure = new Exception("Progress callback");
                service.OnProgressUpdated += _ => throw failure;
                Task task = service.LoadScene(SceneType.DefaultLocation); Fault(task);
                Check(ReferenceEquals(task.Exception.InnerException, failure), "Original exception propagates");
                NoRollback();
            });
            Run("required model does not silently skip loading", () =>
            {
                Task task = new SceneService(null).LoadScene(SceneType.DefaultLocation); Fault(task);
                Check(Addressables.Loads.Count == 0, "No work with missing required model");
                Check(UnityEngine.Debug.Errors.Count == 0, "No log-and-return fallback");
            });
            Console.WriteLine($"CANDIDATE: {_passed} checks passed"); return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }

    private static void Run(string name, Action test)
    {
        Addressables.Reset(); _context = new TestContext(); SynchronizationContext.SetSynchronizationContext(_context);
        test(); _passed++; Console.WriteLine("PASS " + name);
    }
    private static SceneService NewService() => new(new SceneModel(new SceneData()));
    private static void Finish(Task task) { _context.Finish(task); task.GetAwaiter().GetResult(); }
    private static void Fault(Task task) { _context.Finish(task); Check(task.IsFaulted, "Expected fault"); }
    private static void NoRollback()
    {
        Check(Addressables.Unloads.Count == 0 && Addressables.Releases.Count == 0, "No automatic failure cleanup");
        Check(UnityEngine.Debug.Errors.Count == 0, "Failure must propagate without catch-and-log");
    }
    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
}
