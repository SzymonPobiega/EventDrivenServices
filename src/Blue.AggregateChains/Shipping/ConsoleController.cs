using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

class ConsoleController : IDelayGenerator
{
    ConcurrentDictionary<string, TaskCompletionSource<bool>> waitingTasks = new ConcurrentDictionary<string, TaskCompletionSource<bool>>();

    public Task WaitFor(string sequence)
    {
        Console.WriteLine($"Type {sequence} to proceed.");
        var future = waitingTasks.GetOrAdd(sequence, key => new TaskCompletionSource<bool>());
        return future.Task;
    }

    public void OnTyped(string typedSequence)
    {
        if (waitingTasks.TryRemove(typedSequence, out var match))
        {
            match.SetResult(true);
        }
    }
}