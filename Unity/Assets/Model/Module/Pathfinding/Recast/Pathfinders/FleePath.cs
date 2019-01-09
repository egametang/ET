using PF;

namespace PF {
	/** Returns a path heading away from a specified point to avoid.
	 * The search will terminate when G \> \a length (passed to the constructor) + FleePath.spread.\n
	 * \ingroup paths
	 * Can be used to make an AI to flee from an enemy (cannot guarantee that it will not be forced into corners though :D )\n
	 * \code
	 *
	 * // Call a FleePath call like this, assumes that a Seeker is attached to the GameObject
	 * Vector3 thePointToFleeFrom = Vector3.zero;
	 *
	 * // The path will be returned when the path is over a specified length (or more accurately when the traversal cost is greater than a specified value).
	 * // A score of 1000 is approximately equal to the cost of moving one world unit.
	 * int theGScoreToStopAt = 10000;
	 *
	 * // Create a path object
	 * FleePath path = FleePath.Construct (transform.position, thePointToFleeFrom, theGScoreToStopAt);
	 * // This is how strongly it will try to flee, if you set it to 0 it will behave like a RandomPath
	 * path.aimStrength = 1;
	 * // Determines the variation in path length that is allowed
	 * path.spread = 4000;
	 *
	 * // Get the Seeker component which must be attached to this GameObject
	 * Seeker seeker = GetComponent<Seeker>();
	 *
	 * // Start the path and return the result to MyCompleteFunction (which is a function you have to define, the name can of course be changed)
	 * seeker.StartPath(path, MyCompleteFunction);
	 *
	 * \endcode
	 * \astarpro */
	public class FleePath : RandomPath {
		/** Default constructor.
		 * Do not use this. Instead use the static Construct method which can handle path pooling.
		 */
		public FleePath () {}

		/** Constructs a new FleePath.
		 * The FleePath will be taken from a pool.
		 */
		public static FleePath Construct (Vector3 start, Vector3 avoid, int searchLength, OnPathDelegate callback = null) {
			var p = PathPool.GetPath<FleePath>();

			p.Setup(start, avoid, searchLength, callback);
			return p;
		}

		protected void Setup (Vector3 start, Vector3 avoid, int searchLength, OnPathDelegate callback) {
			Setup(start, searchLength, callback);
			aim = avoid-start;
			// TODO: Why is this multiplication by 10 here?
			// Might want to remove it
			aim *= 10;
			aim = start - aim;
		}
	}
}
