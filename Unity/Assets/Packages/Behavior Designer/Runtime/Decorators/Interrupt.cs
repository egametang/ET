namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskDescription("The interrupt task will stop all child tasks from running if it is interrupted. The interruption can be triggered by the perform interruption task. " +
                     "The interrupt task will keep running its child until this interruption is called. If no interruption happens and the child task completed its " +
                     "execution the interrupt task will return the value assigned by the child task.")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=35")]
    [TaskIcon("{SkinColor}InterruptIcon.png")]
    public class Interrupt : Decorator
    {
        // When an interruption occurs return with this status.
        private TaskStatus interruptStatus = TaskStatus.Failure;
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

        public void DoInterrupt(TaskStatus status)
        {
            // An interruption has occurred. Update the interrupt status and notify the Behavior Manager. The Behavior Manager will stop all
            // child tasks from running.
            interruptStatus = status;

            BehaviorManager.instance.Interrupt(Owner, this);
        }

        public override TaskStatus OverrideStatus()
        {
            // Return the interruption status as our status.
            return interruptStatus;
        }

        public override void OnEnd()
        {
            // Reset the variables back to their starting values.
            interruptStatus = TaskStatus.Failure;
            executionStatus = TaskStatus.Inactive;
        }
    }
}