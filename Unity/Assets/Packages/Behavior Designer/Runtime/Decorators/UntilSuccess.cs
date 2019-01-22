namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskDescription("The until success task will keep executing its child task until the child task returns success.")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=42")]
    [TaskIcon("{SkinColor}UntilSuccessIcon.png")]
    public class UntilSuccess : Decorator
    {
        // The status of the child after it has finished running.
        private TaskStatus executionStatus = TaskStatus.Inactive;

        public override bool CanExecute()
        {
            // Keep running until the child task returns success.
            return executionStatus == TaskStatus.Failure || executionStatus == TaskStatus.Inactive;
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