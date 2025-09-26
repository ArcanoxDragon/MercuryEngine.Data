using System.Numerics;
using System.Runtime.CompilerServices;

namespace MercuryEngine.Data.Core.Utility;

public static class MathHelper
{
	/// <summary>
	/// Returns the number of bytes by which a position in a data buffer must be advanced in order
	/// to align the position with an even block of <paramref name="byteAlignment"/> bytes.
	/// </summary>
	public static uint GetNeededPaddingForAlignment(ulong currentPosition, uint byteAlignment)
	{
		var misalignment = currentPosition % byteAlignment;

		return misalignment == 0 ? 0 : (uint) ( byteAlignment - misalignment );
	}

	/// <inheritdoc cref="CreateXYZRotationMatrix(float,float,float)"/>
	public static Matrix4x4 CreateXYZRotationMatrix(Vector3 vector)
		=> CreateXYZRotationMatrix(vector.X, vector.Y, vector.Z);

	/// <summary>
	/// Creates a rotation matrix that rotates using the provided angles, first
	/// around the Z axis, then around the Y axis, then around the X axis. All
	/// angles are in radians.
	/// </summary>
	public static Matrix4x4 CreateXYZRotationMatrix(float x, float y, float z)
		=> Matrix4x4.CreateRotationX(x) *
		   Matrix4x4.CreateRotationY(y) *
		   Matrix4x4.CreateRotationZ(z);

	/// <summary>
	/// Decomposes the provided <paramref name="matrix"/> into three <see cref="Vector3"/>
	/// values representing translation, rotation, and scale respectively. The <paramref name="rotation"/>
	/// vector will contain Tait-Bryan Euler angles representing rotations along individual
	/// axes in the order X-Y-Z.
	/// </summary>
	public static unsafe void DecomposeWithEulerRotation(Matrix4x4 matrix, out Vector3 translation, out Vector3 rotation, out Vector3 scale)
	{
		// Decompose translation - just the first 3 columns of row 4
		translation = Unsafe.ReadUnaligned<Vector3>(&matrix.M41);

		// Decompose rotation
		if (Math.Abs(matrix.M13 - 1) < 1e-7) // M13 == 1
		{
			rotation.X = (float) Math.Atan2(-matrix.M21, -matrix.M31);
			rotation.Y = (float) ( -Math.PI / 2 );
			rotation.Z = 0;
		}
		else if (Math.Abs(matrix.M13 + 1) < 1e-7) // M13 == -1
		{
			rotation.X = (float) Math.Atan2(matrix.M21, matrix.M31);
			rotation.Y = (float) ( Math.PI / 2 );
			rotation.Z = 0;
		}
		else
		{
			rotation.X = (float) Math.Atan2(matrix.M23, matrix.M33);
			rotation.Y = (float) Math.Asin(-matrix.M13);
			rotation.Z = (float) Math.Atan2(matrix.M12, matrix.M11);
		}

		// Decompose scale (vector lengths of first, second, and third rows)
		var row1 = Unsafe.ReadUnaligned<Vector3>(&matrix.M11);
		var row2 = Unsafe.ReadUnaligned<Vector3>(&matrix.M11);
		var row3 = Unsafe.ReadUnaligned<Vector3>(&matrix.M11);

		scale = new Vector3(row1.Length(), row2.Length(), row3.Length());
	}
}