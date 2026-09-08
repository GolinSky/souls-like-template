using SoulsLike.Services.Scenes;
using SoulsLike.Services.Scenes.Data;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

public static class Program
{
    private static TestContext _context;
    private static int _passed;

    public static int Main(string[] args)
    {
        bool baseline = args.Contains("--baseline");
        try
        {
            Run("overlap", () =>
            {
                var service = NewService();
                Task first = service.LoadScene(SceneType.DefaultLocation);
                Task second = service.LoadScene(SceneType.DefaultLocation);
                Check(Addressables.Loads.Count == (baseline ? 2 : 1), "Loading request count");
                if (!baseline)
                {
                    Check(second.IsFaulted, "Overlap must fault immediately");
                    Finish(first); Finish(service.LoadScene(SceneType.DefaultLocation));
                }
            });
            Run("dependency failure", () =>
            {
                var service = NewService(); Addressables.FailLoads.Add("Zone2");
                Task task = service.LoadScene(SceneType.DefaultLocation); _context.Finish(task);
                Check(task.IsFaulted, "Dependency error must propagate");
                if (baseline)
                {
                    Check(Addressables.MaxPendingAdditive == 3, "Baseline concurrent dependencies");
                    Check(Addressables.Loaded.Contains("Zone1") && Addressables.Releases.Count == 0, "Baseline partial load retained");
                }
                else
                {
                    Check(Addressables.MaxPendingAdditive == 1, "Only one pending dependency");
                    Check(Addressables.Loaded.SetEquals(["Loading"]), "Only recovery scene retained");
                    Check(Addressables.Releases.Contains("load:Zone2"), "Failed handle released");
                    Check(Addressables.Unloads.SequenceEqual(["Zone1"]), "Successful dependency unloaded");
                    Check(!Addressables.Loads.Contains("Zone3"), "Stop starting dependencies after failure");
                    Addressables.FailLoads.Clear(); Finish(service.LoadScene(SceneType.DefaultLocation));
                }
            });
            if (baseline) { Console.WriteLine($"BASELINE: {_passed} executable reproductions passed"); return 0; }

            Run("success and repeat", () =>
            {
                var service = NewService(); int changed = 0; service.OnSceneChanged += _ => changed++;
                var progress = new List<float>(); service.OnProgressUpdated += progress.Add;
                for (int i = 0; i < 3; i++) Finish(service.LoadScene(SceneType.DefaultLocation));
                Check(changed == 3, "Exactly one completion per transition");
                Check(Addressables.MaxPendingAdditive == 1, "Sequential loads");
                Check(Addressables.Loads.Take(5).SequenceEqual(["Loading", "Zone1", "Zone2", "Zone3", "DefaultLocation"]), "Load ordering");
                Check(Addressables.Loaded.SetEquals(["Zone1", "Zone2", "Zone3", "DefaultLocation"]), "Full destination residency");
                Check(SceneManager.Active.path == "DefaultLocation", "Active target");
                Check(progress.All(x => x >= 0 && x <= 1) && progress.Contains(0.375f) && progress.Last() == 1, "Progress denominator includes all dependencies and target");
                Check(Addressables.Operations.Count(x => x.Valid) == 4, "One retained destination set");
            });
            Run("target failure reverse rollback", () =>
            {
                var service = NewService(); Addressables.FailLoads.Add("DefaultLocation");
                Fault(service.LoadScene(SceneType.DefaultLocation));
                Check(Addressables.Unloads.SequenceEqual(["Zone3", "Zone2", "Zone1"]), "Reverse cleanup");
                Check(Addressables.Releases.Contains("load:DefaultLocation"), "Failed target released");
                Check(Addressables.Loaded.SetEquals(["Loading"]), "Loading retained");
            });
            Run("loading failure and retry", () =>
            {
                var service = NewService(); Addressables.FailLoads.Add("Loading");
                Fault(service.LoadScene(SceneType.DefaultLocation));
                Check(Addressables.Releases.SequenceEqual(["load:Loading"]), "Failed loading owner released");
                Addressables.FailLoads.Clear(); Finish(service.LoadScene(SceneType.DefaultLocation));
            });
            Run("synchronous dependency start failure", () =>
            {
                var service = NewService(); Addressables.ThrowLoads.Add("Zone2");
                Fault(service.LoadScene(SceneType.DefaultLocation));
                Check(Addressables.Loaded.SetEquals(["Loading"]), "Started dependency rolled back");
            });
            Run("activation failure", () =>
            {
                var service = NewService(); SceneManager.FailActivation = true;
                Fault(service.LoadScene(SceneType.DefaultLocation));
                Check(Addressables.Unloads.SequenceEqual(["DefaultLocation", "Zone3", "Zone2", "Zone1"]), "Target also rolled back");
            });
            Run("progress callback failure settles in-flight load", () =>
            {
                var service = NewService(); var failure = new Exception("Progress callback");
                service.OnProgressUpdated += _ => { if (Addressables.Loads.Contains("Zone1")) throw failure; };
                Task task = service.LoadScene(SceneType.DefaultLocation); Fault(task);
                Check(ReferenceEquals(task.Exception.InnerException, failure), "Original callback failure preserved");
                Check(Addressables.Loaded.SetEquals(["Loading"]), "Pending dependency settled then unloaded");
            });
            Run("cleanup failures logged and original preserved", () =>
            {
                var service = NewService(); Addressables.FailLoads.Add("DefaultLocation");
                Addressables.FailUnloads.UnionWith(["Zone2", "Zone3"]);
                Task task = service.LoadScene(SceneType.DefaultLocation); Fault(task);
                Check(task.Exception.InnerException.Message.Contains("DefaultLocation"), "Original failure survives");
                Check(UnityEngine.Debug.Errors.Count == 2, "Every cleanup failure logged");
                Check(Addressables.Unloads.Contains("Zone1"), "Cleanup continues after failures");
            });
            Run("completion subscriber failure does not roll back", () =>
            {
                var service = NewService(); service.OnSceneChanged += _ => throw new Exception("Subscriber");
                Fault(service.LoadScene(SceneType.DefaultLocation));
                Check(Addressables.Loaded.Contains("DefaultLocation"), "Committed destination preserved");
                Check(Addressables.Unloads.SequenceEqual(["Loading"]), "No destination rollback");
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
    private static SceneService NewService() => new(new SceneModel());
    private static void Finish(Task task) { _context.Finish(task); task.GetAwaiter().GetResult(); }
    private static void Fault(Task task) { _context.Finish(task); Check(task.IsFaulted, "Expected fault"); }
    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
}
