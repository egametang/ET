using UnityEngine;
using UnityEditor;

namespace Pathfinding {
	[CustomEditor(typeof(SimpleSmoothModifier))]
	[CanEditMultipleObjects]
	public class SmoothModifierEditor : EditorBase {
		protected override void Inspector () {
			var smoothType = FindProperty("smoothType");

			PropertyField("smoothType");

			if (!smoothType.hasMultipleDifferentValues) {
				switch ((SimpleSmoothModifier.SmoothType)smoothType.enumValueIndex) {
				case SimpleSmoothModifier.SmoothType.Simple:
					if (PropertyField("uniformLength")) {
						PropertyField("maxSegmentLength");
						Clamp("maxSegmentLength", 0.005f);
					} else {
						IntSlider("subdivisions", 0, 6);
					}

					PropertyField("iterations");
					ClampInt("iterations", 0);

					PropertyField("strength");
					break;
				case SimpleSmoothModifier.SmoothType.OffsetSimple:
					PropertyField("iterations");
					ClampInt("iterations", 0);

					PropertyField("offset");
					Clamp("offset", 0);
					break;
				case SimpleSmoothModifier.SmoothType.Bezier:
					IntSlider("subdivisions", 0, 6);
					PropertyField("bezierTangentLength");
					break;
				case SimpleSmoothModifier.SmoothType.CurvedNonuniform:
					PropertyField("maxSegmentLength");
					Clamp("maxSegmentLength", 0.005f);
					PropertyField("factor");
					break;
				}
			}
		}
	}
}
