using System.Threading;
using ETModel;
using PF;

namespace PF {
	/** Queue of paths to be processed by the system */
	class ThreadControlQueue 
	{

		Path head;
		Path tail;
		/** Create a new queue with the specified number of receivers.
		 * It is important that the number of receivers is fixed.
		 * Properties like AllReceiversBlocked rely on knowing the exact number of receivers using the Pop (or PopNoBlock) methods.
		 */
		public ThreadControlQueue (int numReceivers) {
		}

		/** True if the queue is empty */
		public bool IsEmpty {
			get {
				return head == null;
			}
		}


		/** Push a path to the front of the queue */
		public void PushFront (Path path) {
				if (tail == null) {// (tail == null) ==> (head == null)
					head = path;
					tail = path;
				} else {
					path.next = head;
					head = path;
				}
		}

		/** Push a path to the end of the queue */
		public void Push (Path path) {
			if (tail == null) {// (tail == null) ==> (head == null)
				head = path;
				tail = path;
			} else {
				tail.next = path;
				tail = path;
			}
		}

		/** Pops the next item off the queue.
		 * This call will block if there are no items in the queue or if the queue is currently blocked.
		 *
		 * \returns A Path object, guaranteed to be not null.
		 * \throws QueueTerminationException if #TerminateReceivers has been called.
		 * \throws System.InvalidOperationException if more receivers get blocked than the fixed count sent to the constructor
		 */
		public Path Pop () 
		{
			if (this.head == null)
			{
				return null;
			}
			Path p = head;

			var newHead = head.next;
			if (newHead == null) 
			{
				tail = null;
			}
			head.next = null;
			head = newHead;
			return p;
		}

		/** Pops the next item off the queue, this call will not block.
		 * To ensure stability, the caller must follow this pattern.
		 * 1. Call PopNoBlock(false), if a null value is returned, wait for a bit (e.g yield return null in a Unity coroutine)
		 * 2. try again with PopNoBlock(true), if still null, wait for a bit
		 * 3. Repeat from step 2.
		 *
		 * \throws QueueTerminationException if #TerminateReceivers has been called.
		 * \throws System.InvalidOperationException if more receivers get blocked than the fixed count sent to the constructor
		 */
		public Path PopNoBlock (bool blockedBefore) 
		{
			if (this.head == null)
			{
				return null;
			}
			Path p = head;

			var newHead = head.next;
			if (newHead == null) {
				tail = null;
			}
			head.next = null;
			head = newHead;
			return p;
		}
	}
}
