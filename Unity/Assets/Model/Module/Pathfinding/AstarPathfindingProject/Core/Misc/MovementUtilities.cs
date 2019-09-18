using System.Collections;
using PF;
using Mathf = UnityEngine.Mathf;
using Vector2 = UnityEngine.Vector2;

namespace Pathfinding.Util {
	public static class MovementUtilities {
		/** Clamps the velocity to the max speed and optionally the forwards direction.
		 * \param velocity Desired velocity of the character. In world units per second.
		 * \param maxSpeed Max speed of the character. In world units per second.
		 * \param slowdownFactor Value between 0 and 1 which determines how much slower the character should move than normal.
		 *      Normally 1 but should go to 0 when the character approaches the end of the path.
		 * \param slowWhenNotFacingTarget Prevent the velocity from being too far away from the forward direction of the character
		 *      and slow the character down if the desired velocity is not in the same direction as the forward vector.
		 * \param forward Forward direction of the character. Used together with the \a slowWhenNotFacingTarget parameter.
		 *
		 * Note that all vectors are 2D vectors, not 3D vectors.
		 *
		 * \returns The clamped velocity in world units per second.
		 */
		public static Vector2 ClampVelocity (Vector2 velocity, float maxSpeed, float slowdownFactor, bool slowWhenNotFacingTarget, Vector2 forward) {
			// Max speed to use for this frame
			var currentMaxSpeed = maxSpeed * slowdownFactor;

			// Check if the agent should slow down in case it is not facing the direction it wants to move in
			if (slowWhenNotFacingTarget && (forward.x != 0 || forward.y != 0)) {
				float currentSpeed;
				var normalizedVelocity = VectorMath.Normalize(velocity.ToPFV2(), out currentSpeed);
				float dot = Vector2.Dot(normalizedVelocity.ToUnityV2(), forward);

				// Lower the speed when the character's forward direction is not pointing towards the desired velocity
				// 1 when velocity is in the same direction as forward
				// 0.2 when they point in the opposite directions
				float directionSpeedFactor = Mathf.Clamp(dot+0.707f, 0.2f, 1.0f);
				currentMaxSpeed *= directionSpeedFactor;
				currentSpeed = Mathf.Min(currentSpeed, currentMaxSpeed);

				// Angle between the forwards direction of the character and our desired velocity
				float angle = Mathf.Acos(Mathf.Clamp(dot, -1, 1));

				// Clamp the angle to 20 degrees
				// We cannot keep the velocity exactly in the forwards direction of the character
				// because we use the rotation to determine in which direction to rotate and if
				// the velocity would always be in the forwards direction of the character then
				// the character would never rotate.
				// Allow larger angles when near the end of the path to prevent oscillations.
				angle = Mathf.Min(angle, (20f + 180f*(1 - slowdownFactor*slowdownFactor))*Mathf.Deg2Rad);

				float sin = Mathf.Sin(angle);
				float cos = Mathf.Cos(angle);

				// Determine if we should rotate clockwise or counter-clockwise to move towards the current velocity
				sin *= Mathf.Sign(normalizedVelocity.x*forward.y - normalizedVelocity.y*forward.x);
				// Rotate the #forward vector by #angle radians
				// The rotation is done using an inlined rotation matrix.
				// See https://en.wikipedia.org/wiki/Rotation_matrix
				return new Vector2(forward.x*cos + forward.y*sin, forward.y*cos - forward.x*sin) * currentSpeed;
			} else {
				return Vector2.ClampMagnitude(velocity, currentMaxSpeed);
			}
		}

		/** Calculate an acceleration to move deltaPosition units and get there with approximately a velocity of targetVelocity */
		public static Vector2 CalculateAccelerationToReachPoint (Vector2 deltaPosition, Vector2 targetVelocity, Vector2 currentVelocity, float forwardsAcceleration, float rotationSpeed, float maxSpeed, Vector2 forwardsVector) {
			// Guard against div by zero
			if (forwardsAcceleration <= 0) return Vector2.zero;

			float currentSpeed = currentVelocity.magnitude;

			// Convert rotation speed to an acceleration
			// See https://en.wikipedia.org/wiki/Centripetal_force
			var sidewaysAcceleration = currentSpeed * rotationSpeed * Mathf.Deg2Rad;

			// To avoid weird behaviour when the rotation speed is very low we allow the agent to accelerate sideways without rotating much
			// if the rotation speed is very small. Also guards against division by zero.
			sidewaysAcceleration = Mathf.Max(sidewaysAcceleration, forwardsAcceleration);
			sidewaysAcceleration = forwardsAcceleration;

			// Transform coordinates to local space where +X is the forwards direction
			// This is essentially equivalent to Transform.InverseTransformDirection.
			deltaPosition = VectorMath.ComplexMultiplyConjugate(deltaPosition.ToPFV2(), forwardsVector.ToPFV2()).ToUnityV2();
			targetVelocity = VectorMath.ComplexMultiplyConjugate(targetVelocity.ToPFV2(), forwardsVector.ToPFV2()).ToUnityV2();
			currentVelocity = VectorMath.ComplexMultiplyConjugate(currentVelocity.ToPFV2(), forwardsVector.ToPFV2()).ToUnityV2();
			float ellipseSqrFactorX = 1 / (forwardsAcceleration*forwardsAcceleration);
			float ellipseSqrFactorY = 1 / (sidewaysAcceleration*sidewaysAcceleration);

			// If the target velocity is zero we can use a more fancy approach
			// and calculate a nicer path.
			// In particular, this is the case at the end of the path.
			if (targetVelocity == Vector2.zero) {
				// Run a binary search over the time to get to the target point.
				float mn = 0.01f;
				float mx = 10;
				while (mx - mn > 0.01f) {
					var time = (mx + mn) * 0.5f;

					// Given that we want to move deltaPosition units from out current position, that our current velocity is given
					// and that when we reach the target we want our velocity to be zero. Also assume that our acceleration will
					// vary linearly during the slowdown. Then we can calculate what our acceleration should be during this frame.

					//{ t = time
					//{ deltaPosition = vt + at^2/2 + qt^3/6
					//{ 0 = v + at + qt^2/2
					//{ solve for a
					// a = acceleration vector
					// q = derivative of the acceleration vector
					var a = (6*deltaPosition - 4*time*currentVelocity)/(time*time);
					var q = 6*(time*currentVelocity - 2*deltaPosition)/(time*time*time);

					// Make sure the acceleration is not greater than our maximum allowed acceleration.
					// If it is we increase the time we want to use to get to the target
					// and if it is not, we decrease the time to get there faster.
					// Since the acceleration is described by acceleration = a + q*t
					// we only need to check at t=0 and t=time.
					// Note that the acceleration limit is described by an ellipse, not a circle.
					var nextA = a + q*time;
					if (a.x*a.x*ellipseSqrFactorX + a.y*a.y*ellipseSqrFactorY > 1.0f || nextA.x*nextA.x*ellipseSqrFactorX + nextA.y*nextA.y*ellipseSqrFactorY > 1.0f) {
						mn = time;
					} else {
						mx = time;
					}
				}

				var finalAcceleration = (6*deltaPosition - 4*mx*currentVelocity)/(mx*mx);

				// Boosting
				{
					// The trajectory calculated above has a tendency to use very wide arcs
					// and that does unfortunately not look particularly good in some cases.
					// Here we amplify the component of the acceleration that is perpendicular
					// to our current velocity. This will make the agent turn towards the
					// target quicker.
					// How much amplification to use. Value is unitless.
					const float Boost = 1;
					finalAcceleration.y *= 1 + Boost;

					// Clamp the velocity to the maximum acceleration.
					// Note that the maximum acceleration constraint is shaped like an ellipse, not like a circle.
					float ellipseMagnitude = finalAcceleration.x*finalAcceleration.x*ellipseSqrFactorX + finalAcceleration.y*finalAcceleration.y*ellipseSqrFactorY;
					if (ellipseMagnitude > 1.0f) finalAcceleration /= Mathf.Sqrt(ellipseMagnitude);
				}

				return VectorMath.ComplexMultiply(finalAcceleration.ToPFV2(), forwardsVector.ToPFV2()).ToUnityV2();
			} else {
				// Here we try to move towards the next waypoint which has been modified slightly using our
				// desired velocity at that point so that the agent will more smoothly round the corner.

				// How much to strive for making sure we reach the target point with the target velocity. Unitless.
				const float TargetVelocityWeight = 0.5f;

				// Limit to how much to care about the target velocity. Value is in seconds.
				// This prevents the character from moving away from the path too much when the target point is far away
				const float TargetVelocityWeightLimit = 1.5f;
				float targetSpeed;
				var normalizedTargetVelocity = VectorMath.Normalize(targetVelocity.ToPFV2(), out targetSpeed);

				var distance = deltaPosition.magnitude;
				var targetPoint = deltaPosition.ToPFV2() - normalizedTargetVelocity * System.Math.Min(TargetVelocityWeight * distance * targetSpeed / (currentSpeed + targetSpeed), maxSpeed*TargetVelocityWeightLimit);

				// How quickly the agent will try to reach the velocity that we want it to have.
				// We need this to prevent oscillations and jitter which is what happens if
				// we let the constant go towards zero. Value is in seconds.
				const float TimeToReachDesiredVelocity = 0.1f;
				// TODO: Clamp to ellipse using more accurate acceleration (use rotation speed as well)
				var finalAcceleration = (targetPoint.normalized*maxSpeed - currentVelocity.ToPFV2()) * (1f/TimeToReachDesiredVelocity);

				// Clamp the velocity to the maximum acceleration.
				// Note that the maximum acceleration constraint is shaped like an ellipse, not like a circle.
				float ellipseMagnitude = finalAcceleration.x*finalAcceleration.x*ellipseSqrFactorX + finalAcceleration.y*finalAcceleration.y*ellipseSqrFactorY;
				if (ellipseMagnitude > 1.0f) finalAcceleration /= Mathf.Sqrt(ellipseMagnitude);

				return VectorMath.ComplexMultiply(finalAcceleration, forwardsVector.ToPFV2()).ToUnityV2();
			}
		}
	}
}
