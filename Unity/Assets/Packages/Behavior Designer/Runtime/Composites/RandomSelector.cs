using UnityEngine;
using System.Collections.Generic;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskDescription("Similar to the selector task, the random selector task will return success as soon as a child task returns success.  " +
                     "The difference is that the random selector class will run its children in a random order. The selector task is deterministic " +
                     "in that it will always run the tasks from left to right within the tree. The random selector task shuffles the child tasks up and then begins " +
                     "execution in a random order. Other than that the random selector class is the same as the selector class. It will continue running tasks " +
                     "until a task completes successfully. If no child tasks return success then it will return failure.")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=30")]
    [TaskIcon("{SkinColor}RandomSelectorIcon.png")]
    public class RandomSelector : Composite
    {
        [Tooltip("Seed the random number generator to make things easier to debug")]
        public int seed = 0;
        [Tooltip("Do we want to use the seed?")]
        public bool useSeed = false;

        // A list of indexes of every child task. This list is used by the Fischer-Yates shuffle algorithm.
        private List<int> childIndexList = new List<int>();
        // The random child index execution order.
        private Stack<int> childrenExecutionOrder = new Stack<int>();
        // The task status of the last child ran.
        private TaskStatus executionStatus = TaskStatus.Inactive;

#if SERVER
        private System.Random random;
#endif

        public override void OnAwake()
        {
            // If specified, use the seed provided.
            if (useSeed)
            {
#if !SERVER
#if !(UNITY_5_3)
                    Random.InitState(seed);
#else
                    Random.seed = seed;
#endif
#else
                random = new System.Random(seed);
#endif
            }
            else
            {
#if SERVER
                random = new System.Random();
#endif
            }

            // Add the index of each child to a list to make the Fischer-Yates shuffle possible.
            childIndexList.Clear();
            for (int i = 0; i < children.Count; ++i) {
                childIndexList.Add(i);
            }
        }

        public override void OnStart()
        {
            // Randomize the indecies
            ShuffleChilden();
        }

        public override int CurrentChildIndex()
        {
            // Peek will return the index at the top of the stack.
            return childrenExecutionOrder.Peek();
        }

        public override bool CanExecute()
        {
            // Continue exectuion if no task has return success and indexes still exist on the stack.
            return childrenExecutionOrder.Count > 0 && executionStatus != TaskStatus.Success;
        }

        public override void OnChildExecuted(TaskStatus childStatus)
        {
            // Pop the top index from the stack and set the execution status.
            if (childrenExecutionOrder.Count > 0) {
                childrenExecutionOrder.Pop();
            }
            executionStatus = childStatus;
        }

        public override void OnConditionalAbort(int childIndex)
        {
            // Start from the beginning on an abort
            childrenExecutionOrder.Clear();
            executionStatus = TaskStatus.Inactive;
            ShuffleChilden();
        }

        public override void OnEnd()
        {
            // All of the children have run. Reset the variables back to their starting values.
            executionStatus = TaskStatus.Inactive;
            childrenExecutionOrder.Clear();
        }

        public override void OnReset()
        {
            // Reset the public properties back to their original values
            seed = 0;
            useSeed = false;
        }

        private void ShuffleChilden()
        {
            // Use Fischer-Yates shuffle to randomize the child index order.
            for (int i = childIndexList.Count; i > 0; --i) {
#if !SERVER
                int j = Random.Range(0, i);
#else
                int j = random.Next(0, i);
#endif
                int index = childIndexList[j];
                childrenExecutionOrder.Push(index);
                childIndexList[j] = childIndexList[i - 1];
                childIndexList[i - 1] = index;
            }
        }
    }
}