namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskDescription("Similar to the selector task, the parallel selector task will return success as soon as a child task returns success. " +
                     "The difference is that the parallel task will run all of its children tasks simultaneously versus running each task one at a time. " +
                     "If one tasks returns success the parallel selector task will end all of the child tasks and return success. " +
                     "If every child task returns failure then the parallel selector task will return failure.")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=28")]
    [TaskIcon("{SkinColor}ParallelSelectorIcon.png")]
    public class ParallelSelector : Composite
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
            // Assume all of the children have finished executing. Loop through the execution status of every child and check to see if any tasks are currently running
            // or succeeded. If a task is still running then all of the children are not done executing and the parallel selector task should continue to return a task status of running.
            // If a task succeeded then return success. The Behavior Manager will stop all of the children tasks. If no child task is running or has succeeded then the parallel selector
            // task failed and it will return failure.
            bool childrenComplete = true;
            for (int i = 0; i < executionStatus.Length; ++i) {
                if (executionStatus[i] == TaskStatus.Running) {
                    childrenComplete = false;
                } else if (executionStatus[i] == TaskStatus.Success) {
                    return TaskStatus.Success;
                }
            }
            return (childrenComplete ? TaskStatus.Failure : TaskStatus.Running);
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