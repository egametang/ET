namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskDescription("The return success task will always return success except when the child task is running.")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=39")]
    [TaskIcon("{SkinColor}ReturnSuccessIcon.png")]
    public class ReturnSuccess : Decorator
    {
        // The status of the child after it has finished running.
        private TaskStatus executionStatus = TaskStatus.Inactive;

        public override bool CanExecute()
        {
            // Continue executing until the child task returns success or failure.
            return executionStatus == TaskStatus.Inactive || executionStatus == TaskStatus.Running;
        }

        public override void OnChildExecuted(TaskStatus childStatus)
        {
            // Update the execution status after a child has finished running.
            executionStatus = childStatus;
        }

        public override TaskStatus Decorate(TaskStatus status)
        {
            // Return success even if the child task returned failure.
            if (status == TaskStatus.Failure) {
                return TaskStatus.Success;
            }
            return status;
        }

        public override void OnEnd()
        {
            // Reset the execution status back to its starting values.
            executionStatus = TaskStatus.Inactive;
        }
    }
}