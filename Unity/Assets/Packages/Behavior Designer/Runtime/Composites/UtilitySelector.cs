using System.Collections.Generic;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskDescription("The utility selector task evaluates the child tasks using Utility Theory AI. The child task can override the GetUtility method and return the utility value " +
                     "at that particular time. The task with the highest utility value will be selected and the existing running task will be aborted. The utility selector " +
                     "task reevaluates its children every tick.")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=134")]
    [TaskIcon("{SkinColor}UtilitySelectorIcon.png")]
    public class UtilitySelector : Composite
    {
        // The index of the child that is currently running or is about to run.
        private int currentChildIndex = 0;
        // The highest utility value
        private float highestUtility;
        // The task status of the last child ran.
        private TaskStatus executionStatus = TaskStatus.Inactive;
        // Is the task being reevaluated?
        private bool reevaluating;
        // A list of children that can execute.
        private List<int> availableChildren = new List<int>();

        public override void OnStart()
        {
            highestUtility = float.MinValue;

            // Loop through each child task and determine its utility. The task with the highest utility will run first.
            availableChildren.Clear();
            for (int i = 0; i < children.Count; ++i) {
                float utility = children[i].GetUtility();
                if (utility > highestUtility) {
                    highestUtility = utility;
                    currentChildIndex = i;
                }
                availableChildren.Add(i);
            }
        }

        public override int CurrentChildIndex()
        {
            // The currentChildIndex is the task with the highest utility.
            return currentChildIndex;
        }

        public override void OnChildStarted(int childIndex)
        {
            // The child has started - set the execution status.
            executionStatus = TaskStatus.Running;
        }

        public override bool CanExecute()
        {
            // Continue to execute new tasks until a task returns success or there are no more children left. If reevaluating then return false
            // immediately because each task doesn't need to be reevaluted.
            if (executionStatus == TaskStatus.Success || executionStatus == TaskStatus.Running || reevaluating) {
                return false;
            }
            return availableChildren.Count > 0;
        }

        public override void OnChildExecuted(int childIndex, TaskStatus childStatus)
        {
            // The child status will be inactive immediately following an abort from OnReevaluationEnded. The status will be running if the 
            // child task is interrupted. Ignore the status for both of these. 
            if (childStatus != TaskStatus.Inactive && childStatus != TaskStatus.Running) {
                executionStatus = childStatus;
                // If the execution status is failure then a new task needs to be selected. Remove the current task from the available children
                // and select the next highest utility child. 
                if (executionStatus == TaskStatus.Failure) {
                    availableChildren.Remove(childIndex);

                    highestUtility = float.MinValue;
                    for (int i = 0; i < availableChildren.Count; ++i) {
                        float utility = children[availableChildren[i]].GetUtility();
                        if (utility > highestUtility) {
                            highestUtility = utility;
                            currentChildIndex = availableChildren[i];
                        }
                    }
                }
            }
        }

        public override void OnConditionalAbort(int childIndex)
        {
            // Set the current child index to the index that caused the abort
            currentChildIndex = childIndex;
            executionStatus = TaskStatus.Inactive;
        }

        public override void OnEnd()
        {
            // All of the children have run. Reset the variables back to their starting values.
            executionStatus = TaskStatus.Inactive;
            currentChildIndex = 0;
        }

        public override TaskStatus OverrideStatus(TaskStatus status)
        {
            return executionStatus;
        }

        // The utility selector task is a parallel task to allow the task utility to be reevaluated. The higest utility task will always run.
        public override bool CanRunParallelChildren()
        {
            return true;
        }

        // Can reevaluate to allow the task utilities to be rerun.
        public override bool CanReevaluate()
        {
            return true;
        }

        // The behavior tree wants to start reevaluating the tree.
        public override bool OnReevaluationStarted()
        {
            // Cannot reevaluate if the task hasn't even started yet
            if (executionStatus == TaskStatus.Inactive) {
                return false;
            }

            reevaluating = true;
            return true;
        }

        // Determine if a task with a higher utility exists.
        public override void OnReevaluationEnded(TaskStatus status)
        {
            reevaluating = false;

            // Loop through all of the available children and pick the task with the highest utility.
            int prevChildIndex = currentChildIndex;
            highestUtility = float.MinValue;
            for (int i = 0; i < availableChildren.Count; ++i) {
                float utility = children[availableChildren[i]].GetUtility();
                if (utility > highestUtility) {
                    highestUtility = utility;
                    currentChildIndex = availableChildren[i];
                }
            }

            // If the index is different then the current child task should be aborted and the higher utility task should be run.
            if (prevChildIndex != currentChildIndex) {
                BehaviorManager.instance.Interrupt(Owner, children[prevChildIndex], this);
                executionStatus = TaskStatus.Inactive;
            }
        }
    }
}