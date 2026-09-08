using UnityEngine.AddressableAssets;
namespace UnityEngine.ResourceManagement.AsyncOperations;
public readonly struct AsyncOperationHandle<T>(Addressables.Operation<T> operation)
{
    public Addressables.Operation<T> Operation => operation;
    private Addressables.Operation<T> Valid => IsValid() ? operation : throw new InvalidOperationException("Invalid handle access");
    public bool IsValid() => operation != null && operation.Valid;
    public bool IsDone => Valid.Done;
    public float PercentComplete => Valid.Done ? 1f : 0.5f;
    public AsyncOperationStatus Status => Valid.Status;
    public Exception OperationException => Valid.Error;
    public T Result => Valid.Result;
}
