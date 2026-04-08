using BugPro;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BugTests;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void NewBugStartsInNewState()
    {
        var bug = new Bug();

        Assert.AreEqual(Bug.State.New, bug.CurrentState);
    }

    [TestMethod]
    public void TriageMovesBugToTriaged()
    {
        var bug = new Bug();

        bug.Triage();

        Assert.AreEqual(Bug.State.Triaged, bug.CurrentState);
    }

    [TestMethod]
    public void StartProgressMovesTriagedBugToInProgress()
    {
        var bug = CreateTriagedBug();

        bug.StartProgress();

        Assert.AreEqual(Bug.State.InProgress, bug.CurrentState);
    }

    [TestMethod]
    public void RequestInfoFromTriagedMovesBugToWaitingForInfo()
    {
        var bug = CreateTriagedBug();

        bug.RequestInfo();

        Assert.AreEqual(Bug.State.WaitingForInfo, bug.CurrentState);
    }

    [TestMethod]
    public void ProvideInfoReturnsBugToTriaged()
    {
        var bug = CreateTriagedBug();
        bug.RequestInfo();

        bug.ProvideInfo();

        Assert.AreEqual(Bug.State.Triaged, bug.CurrentState);
    }

    [TestMethod]
    public void DeferFromTriagedMovesBugToDeferred()
    {
        var bug = CreateTriagedBug();

        bug.Defer();

        Assert.AreEqual(Bug.State.Deferred, bug.CurrentState);
    }

    [TestMethod]
    public void ResumeReturnsDeferredBugToTriaged()
    {
        var bug = CreateTriagedBug();
        bug.Defer();

        bug.Resume();

        Assert.AreEqual(Bug.State.Triaged, bug.CurrentState);
    }

    [TestMethod]
    public void MarkNotABugMovesBugToRejected()
    {
        var bug = CreateTriagedBug();

        bug.MarkNotABug();

        Assert.AreEqual(Bug.State.Rejected, bug.CurrentState);
    }

    [TestMethod]
    public void MarkDuplicateMovesBugToDuplicate()
    {
        var bug = CreateTriagedBug();

        bug.MarkDuplicate();

        Assert.AreEqual(Bug.State.Duplicate, bug.CurrentState);
    }

    [TestMethod]
    public void MarkCannotReproduceMovesBugToCannotReproduce()
    {
        var bug = CreateTriagedBug();

        bug.MarkCannotReproduce();

        Assert.AreEqual(Bug.State.CannotReproduce, bug.CurrentState);
    }

    [TestMethod]
    public void ResolveMovesBugToResolved()
    {
        var bug = CreateInProgressBug();

        bug.Resolve();

        Assert.AreEqual(Bug.State.Resolved, bug.CurrentState);
    }

    [TestMethod]
    public void VerifyFixWithTrueClosesResolvedBug()
    {
        var bug = CreateResolvedBug();

        bug.VerifyFix(true);

        Assert.AreEqual(Bug.State.Closed, bug.CurrentState);
    }

    [TestMethod]
    public void VerifyFixWithFalseReopensResolvedBug()
    {
        var bug = CreateResolvedBug();

        bug.VerifyFix(false);

        Assert.AreEqual(Bug.State.Reopened, bug.CurrentState);
    }

    [TestMethod]
    public void ReopenMovesClosedBugToReopened()
    {
        var bug = CreateClosedBug();

        bug.Reopen();

        Assert.AreEqual(Bug.State.Reopened, bug.CurrentState);
    }

    [TestMethod]
    public void ReturnToTriagedMovesReopenedBugToTriaged()
    {
        var bug = CreateClosedBug();
        bug.Reopen();

        bug.ReturnToTriaged();

        Assert.AreEqual(Bug.State.Triaged, bug.CurrentState);
    }

    [TestMethod]
    public void ReopenMovesDuplicateBugToReopened()
    {
        var bug = CreateTriagedBug();
        bug.MarkDuplicate();

        bug.Reopen();

        Assert.AreEqual(Bug.State.Reopened, bug.CurrentState);
    }

    [TestMethod]
    public void ReopenMovesRejectedBugToReopened()
    {
        var bug = CreateTriagedBug();
        bug.MarkNotABug();

        bug.Reopen();

        Assert.AreEqual(Bug.State.Reopened, bug.CurrentState);
    }

    [TestMethod]
    public void CloseMovesCannotReproduceBugToClosed()
    {
        var bug = CreateTriagedBug();
        bug.MarkCannotReproduce();

        bug.Close();

        Assert.AreEqual(Bug.State.Closed, bug.CurrentState);
    }

    [TestMethod]
    public void RequestInfoFromInProgressMovesBugToWaitingForInfo()
    {
        var bug = CreateInProgressBug();

        bug.RequestInfo();

        Assert.AreEqual(Bug.State.WaitingForInfo, bug.CurrentState);
    }

    [TestMethod]
    public void DeferFromInProgressMovesBugToDeferred()
    {
        var bug = CreateInProgressBug();

        bug.Defer();

        Assert.AreEqual(Bug.State.Deferred, bug.CurrentState);
    }

    [TestMethod]
    public void StartProgressFromWaitingForInfoMovesBugToInProgress()
    {
        var bug = CreateTriagedBug();
        bug.RequestInfo();

        bug.StartProgress();

        Assert.AreEqual(Bug.State.InProgress, bug.CurrentState);
    }

    [TestMethod]
    public void ReopenMovesCannotReproduceBugToReopened()
    {
        var bug = CreateTriagedBug();
        bug.MarkCannotReproduce();

        bug.Reopen();

        Assert.AreEqual(Bug.State.Reopened, bug.CurrentState);
    }

    [TestMethod]
    public void StartProgressCannotBeFiredFromNewState()
    {
        var bug = new Bug();

        Assert.ThrowsException<InvalidOperationException>(() => bug.StartProgress());
    }

    [TestMethod]
    public void ProvideInfoCannotBeFiredFromTriagedState()
    {
        var bug = CreateTriagedBug();

        Assert.ThrowsException<InvalidOperationException>(() => bug.ProvideInfo());
    }

    [TestMethod]
    public void VerifyFixCannotBeFiredFromInProgressState()
    {
        var bug = CreateInProgressBug();

        Assert.ThrowsException<InvalidOperationException>(() => bug.VerifyFix(true));
    }

    [TestMethod]
    public void CloseCannotBeFiredFromTriagedState()
    {
        var bug = CreateTriagedBug();

        Assert.ThrowsException<InvalidOperationException>(() => bug.Close());
    }

    [TestMethod]
    public void CanFireReportsAvailableTransitions()
    {
        var bug = CreateTriagedBug();

        Assert.IsTrue(bug.CanFire(Bug.Trigger.StartProgress));
        Assert.IsFalse(bug.CanFire(Bug.Trigger.Close));
    }

    [TestMethod]
    public void ClosedStateIsFinal()
    {
        var bug = CreateClosedBug();

        Assert.IsTrue(bug.IsFinalState);
    }

    [TestMethod]
    public void InProgressStateIsNotFinal()
    {
        var bug = CreateInProgressBug();

        Assert.IsFalse(bug.IsFinalState);
    }

    [TestMethod]
    public void HistoryStoresEachTransition()
    {
        var bug = new Bug();
        bug.Triage();
        bug.StartProgress();
        bug.Resolve();

        Assert.AreEqual(3, bug.History.Count);
        StringAssert.Contains(bug.History[0], "New --Triage--> Triaged");
    }

    [TestMethod]
    public void ToStringContainsCurrentState()
    {
        var bug = CreateResolvedBug();

        var description = bug.ToString();

        StringAssert.Contains(description, "Resolved");
    }

    private static Bug CreateTriagedBug()
    {
        var bug = new Bug();
        bug.Triage();
        return bug;
    }

    private static Bug CreateInProgressBug()
    {
        var bug = CreateTriagedBug();
        bug.StartProgress();
        return bug;
    }

    private static Bug CreateResolvedBug()
    {
        var bug = CreateInProgressBug();
        bug.Resolve();
        return bug;
    }

    private static Bug CreateClosedBug()
    {
        var bug = CreateResolvedBug();
        bug.VerifyFix(true);
        return bug;
    }
}
