using TrueSync;
using System.Reflection;
using UnityEngine;

/**
* @brief Extensions added by TrueSync.
**/
public static class TrueSyncExtensions {

    public static TSVector ToTSVector(this Vector3 vector) {
        return new TSVector(vector.x, vector.y, vector.z);
    }

    public static TSVector2 ToTSVector2(this Vector3 vector) {
        return new TSVector2(vector.x, vector.y);
    }

    public static TSVector ToTSVector(this Vector2 vector) {
        return new TSVector(vector.x, vector.y, 0);
    }

    public static TSVector2 ToTSVector2(this Vector2 vector) {
        return new TSVector2(vector.x, vector.y);
    }

    public static Vector3 Abs(this Vector3 vector) {
		return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
	}

    public static TSQuaternion ToTSQuaternion(this Quaternion rot) {
        return new TSQuaternion(rot.x, rot.y, rot.z, rot.w);
    }

    public static Quaternion ToQuaternion(this TSQuaternion rot) {
        return new Quaternion((float)rot.x, (float)rot.y, (float)rot.z, (float)rot.w);
    }

    public static TSMatrix ToTSMatrix(this Quaternion rot) {
        return TSMatrix.CreateFromQuaternion(rot.ToTSQuaternion());
    }

    public static Vector3 ToVector(this TSVector jVector) {
        return new Vector3((float) jVector.x, (float) jVector.y, (float) jVector.z);
    }

    public static Vector3 ToVector(this TSVector2 jVector) {
        return new Vector3((float)jVector.x, (float)jVector.y, 0);
    }

    public static void Set(this TSVector jVector, TSVector otherVector) {
        jVector.Set(otherVector.x, otherVector.y, otherVector.z);
    }

    public static Quaternion ToQuaternion(this TSMatrix jMatrix) {
        return TSQuaternion.CreateFromMatrix(jMatrix).ToQuaternion();
    }

}