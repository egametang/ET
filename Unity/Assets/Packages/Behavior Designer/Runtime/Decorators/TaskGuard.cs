namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskDescription("The task guard task is similar to a semaphore in multithreaded programming. The task guard task is there to ensure a limited resource is not being overused. " +
                     "\n\nFor example, you may place a task guard above a task that plays an animation. Elsewhere within your behavior tree you may also have another task that plays a different " +
                     "animation but uses the same bones for that animation. Because of this you don't want that animation to play twice at the same time. Placing a task guard will let you " +
                     "specify how many times a particular task can be accessed at the same time.\n\nIn the previous animation task example you would specify an access count of 1. With this setup " +
                     "the animation task can be only controlled by one task at a time. If the first task is playing the animation and a second task wants to control the animation as well, it will " +
                     "either have to wait or skip over the task completely.")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=40")]
    [TaskIcon("{SkinColor}TaskGuardIcon.png")]
    public class TaskGuard : Decorator
    {
        [Tooltip("The number of times the child tasks can be accessed by parallel tasks at once")]
        public int maxTaskAccessCount;
        [Tooltip("The linked tasks that also guard a task. If the task guard is not linked against any other tasks it doesn't have much purpose. Marked as LinkedTask to " +
                 "ensure all tasks linked are linked to the same set of tasks")]
        [LinkedTask]
        public TaskGuard[] linkedTaskGuards = null;
        [Tooltip("If true the task will wait until the child task is available. If false then any unavailable child tasks will be skipped over")]
        public bool waitUntilTaskAvailable;

        // The number of tasks that are currently using a particular task.
        private int executingTasks = 0;
        // True if the current task is executing.
        private bool executing = false;

        public override bool CanExecute()
        {
            // The child task can execute if the number of executing tasks is less than the maximum number of tasks allowed.
            return executingTasks < maxTaskAccessCount && !executing;
        }

        public override void OnChildStarted()
        {
            // The child task has started to run. Increase the executing tasks counter and notify all of the other linked tasks.
            executingTasks++;
            executing = true;
            for (int i = 0; i < linkedTaskGuards.Length; ++i) {
                linkedTaskGuards[i].taskExecuting(true);
            }
        }

        public override TaskStatus OverrideStatus(TaskStatus status)
        {
            // return a running status if the children are currently waiting for a task to become available
            return (!executing && waitUntilTaskAvailable) ? TaskStatus.Running : status;
        }

        public void taskExecuting(bool increase)
        {
            // A linked task is currently executing a task that is being guarded. If the task just started executing then increase will be true and if it is ending then
            // true will be false.
            executingTasks += (increase ? 1 : -1);
        }

        public override void OnEnd()
        {
            // The child task has been executed or skipped over. Only decrement the executing tasks count if the child task was being executed. Following that
            // notify all of the linked tasks that we are done executing.
            if (executing) {
                executingTasks--;
                for (int i = 0; i < linkedTaskGuards.Length; ++i) {
                    linkedTaskGuards[i].taskExecuting(false);
                }
                executing = false;
            }
        }

        public override void OnReset()
        {
            // Reset the public properties back to their original values
            maxTaskAccessCount = 0;
            linkedTaskGuards = null;
            waitUntilTaskAvailable = true;
        }
    }
}