namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskDescription("The until failure task will keep executing its child task until the child task returns failure.")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=41")]
    [TaskIcon("{SkinColor}UntilFailureIcon.png")]
    public class UntilFailure : Decorator
    {
        // The status of the child after it has finished running.
        private TaskStatus executionStatus = TaskStatus.Inactive;

        public override bool CanExecute()
        {
            // Keep running until the child task returns failure.
            return executionStatus == TaskStatus.Success || executionStatus == TaskStatus.Inactive;
        }

        public override void OnChildExecuted(TaskStatus childStatus)
        {
            // Update the execution status after a child has finished running.
            executionStatus = childStatus;
        }

        public override void OnEnd()
        {
            // Reset the execution status back to its starting values.
            executionStatus = TaskStatus.Inactive;
        }
    }
}