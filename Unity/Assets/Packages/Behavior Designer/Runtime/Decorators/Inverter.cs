namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskDescription("The inverter task will invert the return value of the child task after it has finished executing. " + 
                     "If the child returns success, the inverter task will return failure. If the child returns failure, the inverter task will return success.")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=36")]
    [TaskIcon("{SkinColor}InverterIcon.png")]
    public class Inverter : Decorator
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
            // Invert the task status.
            if (status == TaskStatus.Success) {
                return TaskStatus.Failure;
            } else if (status == TaskStatus.Failure) {
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