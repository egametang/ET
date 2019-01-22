namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskDescription("Similar to the parallel selector task, except the parallel complete task will return the child status as soon as the child returns success or failure." + 
                     "The child tasks are executed simultaneously.")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=136")]
    [TaskIcon("{SkinColor}ParallelCompleteIcon.png")]
    public class ParallelComplete : Composite
    {
        // The index of the child that is currently running or is about to run.
        private int currentChildIndex;
        // The task status of every child task.
        private TaskStatus[] executionStatus;

        public override void OnAwake()
        {
            // Create a new task status array that will hold the execution status of all of the children tasks.
            executionStatus = new TaskStatus[children.Count];
        }

        public override void OnChildStarted(int childIndex)
        {
            // One of the children has started to run. Increment the child index and set the current task status of that child to running.
            currentChildIndex++;
            executionStatus[childIndex] = TaskStatus.Running;
        }

        public override bool CanRunParallelChildren()
        {
            // This task can run parallel children.
            return true;
        }

        public override int CurrentChildIndex()
        {
            return currentChildIndex;
        }

        public override bool CanExecute()
        {
            // We can continue executing if we have more children that haven't been started yet.
            return currentChildIndex < children.Count;
        }

        public override void OnChildExecuted(int childIndex, TaskStatus childStatus)
        {
            // One of the children has finished running. Set the task status.
            executionStatus[childIndex] = childStatus;
        }

        public override void OnConditionalAbort(int childIndex)
        {
            // Start from the beginning on an abort
            currentChildIndex = 0;
            for (int i = 0; i < executionStatus.Length; ++i) {
                executionStatus[i] = TaskStatus.Inactive;
            }
        }

        public override TaskStatus OverrideStatus(TaskStatus status)
        {
            // Return the child task's status as soon as a child task returns success or failure.
            for (int i = 0; i < executionStatus.Length; ++i) {
                if (executionStatus[i] == TaskStatus.Success || executionStatus[i] == TaskStatus.Failure) {
                    return executionStatus[i];
                } else if (executionStatus[i] == TaskStatus.Inactive) {
                    return TaskStatus.Success;
                }
            }
            return TaskStatus.Running;
        }

        public override void OnEnd()
        {
            // Reset the execution status and the child index back to their starting values.
            for (int i = 0; i < executionStatus.Length; ++i) {
                executionStatus[i] = TaskStatus.Inactive;
            }
            currentChildIndex = 0;
        }
    }
}