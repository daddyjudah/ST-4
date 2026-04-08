using Stateless;

namespace BugPro;

public sealed class Bug
{
    public enum State
    {
        New,
        Triaged,
        InProgress,
        WaitingForInfo,
        Deferred,
        Resolved,
        Closed,
        Reopened,
        Rejected,
        Duplicate,
        CannotReproduce
    }

    public enum Trigger
    {
        Triage,
        StartProgress,
        RequestInfo,
        ProvideInfo,
        Defer,
        Resume,
        Resolve,
        VerifyFix,
        Reopen,
        Close,
        MarkNotABug,
        MarkDuplicate,
        MarkCannotReproduce,
        ReturnToTriaged
    }

    private readonly StateMachine<State, Trigger> _stateMachine;
    private readonly StateMachine<State, Trigger>.TriggerWithParameters<bool> _verifyFixTrigger;

    public Bug()
    {
        History = new List<string>();
        _stateMachine = new StateMachine<State, Trigger>(State.New);
        _verifyFixTrigger = _stateMachine.SetTriggerParameters<bool>(Trigger.VerifyFix);

        Configure();
    }

    public IList<string> History { get; }

    public State CurrentState => _stateMachine.State;

    public bool CanFire(Trigger trigger) => _stateMachine.CanFire(trigger);

    public bool IsFinalState =>
        CurrentState is State.Closed or State.Rejected or State.Duplicate;

    public void Triage() => Fire(Trigger.Triage);

    public void StartProgress() => Fire(Trigger.StartProgress);

    public void RequestInfo() => Fire(Trigger.RequestInfo);

    public void ProvideInfo() => Fire(Trigger.ProvideInfo);

    public void Defer() => Fire(Trigger.Defer);

    public void Resume() => Fire(Trigger.Resume);

    public void Resolve() => Fire(Trigger.Resolve);

    public void VerifyFix(bool isFixed) => _stateMachine.Fire(_verifyFixTrigger, isFixed);

    public void Reopen() => Fire(Trigger.Reopen);

    public void Close() => Fire(Trigger.Close);

    public void MarkNotABug() => Fire(Trigger.MarkNotABug);

    public void MarkDuplicate() => Fire(Trigger.MarkDuplicate);

    public void MarkCannotReproduce() => Fire(Trigger.MarkCannotReproduce);

    public void ReturnToTriaged() => Fire(Trigger.ReturnToTriaged);

    public override string ToString() =>
        $"Bug state: {CurrentState}; final: {IsFinalState}; transitions: {History.Count}";

    private void Configure()
    {
        _stateMachine.OnTransitioned(transition =>
        {
            History.Add($"{transition.Source} --{transition.Trigger}--> {transition.Destination}");
        });

        _stateMachine.Configure(State.New)
            .Permit(Trigger.Triage, State.Triaged);

        _stateMachine.Configure(State.Triaged)
            .Permit(Trigger.StartProgress, State.InProgress)
            .Permit(Trigger.RequestInfo, State.WaitingForInfo)
            .Permit(Trigger.Defer, State.Deferred)
            .Permit(Trigger.MarkNotABug, State.Rejected)
            .Permit(Trigger.MarkDuplicate, State.Duplicate)
            .Permit(Trigger.MarkCannotReproduce, State.CannotReproduce);

        _stateMachine.Configure(State.InProgress)
            .Permit(Trigger.RequestInfo, State.WaitingForInfo)
            .Permit(Trigger.Defer, State.Deferred)
            .Permit(Trigger.Resolve, State.Resolved);

        _stateMachine.Configure(State.WaitingForInfo)
            .Permit(Trigger.ProvideInfo, State.Triaged)
            .Permit(Trigger.StartProgress, State.InProgress);

        _stateMachine.Configure(State.Deferred)
            .Permit(Trigger.Resume, State.Triaged);

        _stateMachine.Configure(State.Resolved)
            .PermitIf(_verifyFixTrigger, State.Closed, isFixed => isFixed)
            .PermitIf(_verifyFixTrigger, State.Reopened, isFixed => !isFixed)
            .Permit(Trigger.Reopen, State.Reopened);

        _stateMachine.Configure(State.CannotReproduce)
            .Permit(Trigger.Close, State.Closed)
            .Permit(Trigger.Reopen, State.Reopened);

        _stateMachine.Configure(State.Rejected)
            .Permit(Trigger.Reopen, State.Reopened);

        _stateMachine.Configure(State.Duplicate)
            .Permit(Trigger.Reopen, State.Reopened);

        _stateMachine.Configure(State.Closed)
            .Permit(Trigger.Reopen, State.Reopened);

        _stateMachine.Configure(State.Reopened)
            .Permit(Trigger.ReturnToTriaged, State.Triaged)
            .Permit(Trigger.StartProgress, State.InProgress);
    }

    private void Fire(Trigger trigger) => _stateMachine.Fire(trigger);
}

public static class Program
{
    public static void Main()
    {
        var bug = new Bug();

        Console.WriteLine("Bug workflow demo");
        Console.WriteLine(bug);

        bug.Triage();
        bug.StartProgress();
        bug.Resolve();
        bug.VerifyFix(false);
        bug.ReturnToTriaged();
        Console.WriteLine(bug);
        foreach (var item in bug.History)
        {
            Console.WriteLine(item);
        }
    }
}
