using System.Numerics;
using Dumb.Engine.Transform;

namespace Dumb.Engine.Tests.Transform;

public sealed class Affine3DTests
{
    private const float Epsilon = 1e-4f;

    private static void Near(float expected, float actual, float eps = Epsilon) =>
        Assert.True(
            MathF.Abs(expected - actual) <= eps,
            $"Expected {expected}, actual {actual}, diff {MathF.Abs(expected - actual)}");

    private static void Near(Vector3 expected, Vector3 actual, float eps = Epsilon)
    {
        Near(expected.X, actual.X, eps);
        Near(expected.Y, actual.Y, eps);
        Near(expected.Z, actual.Z, eps);
    }

    private static void Near(Affine3D expected, Affine3D actual, float eps = Epsilon)
    {
        Near(expected.M11, actual.M11, eps);
        Near(expected.M12, actual.M12, eps);
        Near(expected.M13, actual.M13, eps);
        Near(expected.M14, actual.M14, eps);

        Near(expected.M21, actual.M21, eps);
        Near(expected.M22, actual.M22, eps);
        Near(expected.M23, actual.M23, eps);
        Near(expected.M24, actual.M24, eps);

        Near(expected.M31, actual.M31, eps);
        Near(expected.M32, actual.M32, eps);
        Near(expected.M33, actual.M33, eps);
        Near(expected.M34, actual.M34, eps);
    }

    private static Vector3 TransformPointRowMajor(Vector3 p, Matrix4x4 m) =>
        new(
            p.X * m.M11 + p.Y * m.M21 + p.Z * m.M31 + m.M41,
            p.X * m.M12 + p.Y * m.M22 + p.Z * m.M32 + m.M42,
            p.X * m.M13 + p.Y * m.M23 + p.Z * m.M33 + m.M43
        );

    private static Matrix4x4 CreateSRTMatrix(Vector3 scale, Quaternion rotation, Vector3 translation) =>
        Matrix4x4.CreateScale(scale)
        * Matrix4x4.CreateFromQuaternion(rotation)
        * Matrix4x4.CreateTranslation(translation);

    private static Quaternion QuaternionFromAxisAngle(Vector3 axis, float radians) =>
        Quaternion.CreateFromAxisAngle(axis, radians);

    [Fact]
    public void Affine3D_BasicAffineRoundTrip_IsMathematicallyClosed()
    {
        var scale = new Vector3(2.0f, 3.0f, 4.0f);
        var rotation = QuaternionFromAxisAngle(new Vector3(1, 2, 3), 0.65f);
        var translation = new Vector3(10, -20, 30);
        var point = new Vector3(7, -5, 3);

        var affine = Affine3D.FromTRS(translation, rotation, scale);

        var numerics = CreateSRTMatrix(scale, rotation, translation);
        var expectedPoint = TransformPointRowMajor(point, numerics);

        Near(expectedPoint, affine.TransformPoint(point));
        Near(translation, affine.Translation);

        var roundTripNumerics = affine.ToMatrix4x4();
        Near(expectedPoint, TransformPointRowMajor(point, roundTripNumerics));

        var roundTripAffine = Affine3D.FromMatrix4x4(roundTripNumerics);
        Near(affine.TransformPoint(point), roundTripAffine.TransformPoint(point));
        Near(affine.Translation, roundTripAffine.Translation);
    }

    [Fact]
    public void Affine3D_CompositionOrder_MatchesSequentialApplication()
    {
        var parent = Affine3D.FromTRS(
            new Vector3(100, -50, 25),
            QuaternionFromAxisAngle(new Vector3(0, 1, 0), 0.8f),
            new Vector3(2, 2, 2));

        var child = Affine3D.FromTRS(
            new Vector3(3, 4, 5),
            QuaternionFromAxisAngle(new Vector3(1, 0, 0), -0.45f),
            new Vector3(1, 3, 2));

        var grandChild = Affine3D.FromTRS(
            new Vector3(-7, 8, -9),
            QuaternionFromAxisAngle(new Vector3(0, 0, 1), 1.1f),
            new Vector3(0.5f, 2.0f, 1.5f));

        var localPoint = new Vector3(1, 2, 3);

        var composed = grandChild * child * parent;

        var expected =
            parent.TransformPoint(
                child.TransformPoint(
                    grandChild.TransformPoint(localPoint)));

        var actual = composed.TransformPoint(localPoint);

        Near(expected, actual);
    }

    [Fact]
    public void Affine3D_Associativity_HoldsForAffineComposition()
    {
        var a = Affine3D.FromTRS(
            new Vector3(1, 2, 3),
            QuaternionFromAxisAngle(new Vector3(1, 1, 0), 0.3f),
            new Vector3(2, 1, 3));

        var b = Affine3D.FromTRS(
            new Vector3(-4, 5, -6),
            QuaternionFromAxisAngle(new Vector3(0, 1, 1), -0.7f),
            new Vector3(1.5f, 0.75f, 2.5f));

        var c = Affine3D.FromTRS(
            new Vector3(7, -8, 9),
            QuaternionFromAxisAngle(new Vector3(1, 0, 1), 1.2f),
            new Vector3(0.8f, 1.2f, 1.6f));

        var p = new Vector3(3, -2, 5);

        var left = (a * b) * c;
        var right = a * (b * c);

        Near(left.TransformPoint(p), right.TransformPoint(p));
    }

    [Fact]
    public void Affine3D_Inverse_FormsIdentityForPointsAndDirections()
    {
        var transform = Affine3D.FromTRS(
            new Vector3(10, -5, 3),
            QuaternionFromAxisAngle(new Vector3(1, 2, 3), 0.8f),
            new Vector3(2, 3, 4));

        var inverse = transform.Inverse();

        var p1 = new Vector3(7, -11, 13);
        var p2 = new Vector3(-3, 17, 0.5f);
        var direction = Vector3.Normalize(new Vector3(3, -4, 5));

        Near(p1, inverse.TransformPoint(transform.TransformPoint(p1)));
        Near(p2, inverse.TransformPoint(transform.TransformPoint(p2)));
        Near(direction, inverse.TransformDirection(transform.TransformDirection(direction)));

        var identityLike = transform * inverse;

        Near(p1, identityLike.TransformPoint(p1));
        Near(p2, identityLike.TransformPoint(p2));
        Near(direction, identityLike.TransformDirection(direction));
    }

    [Fact]
    public void Affine3D_SystemNumericsInterop_MatchesForSrtChain()
    {
        var scaleA = new Vector3(2, 3, 4);
        var rotA = QuaternionFromAxisAngle(new Vector3(0, 1, 0), 0.25f);
        var transA = new Vector3(10, 20, 30);

        var scaleB = new Vector3(1.5f, 0.5f, 2.5f);
        var rotB = QuaternionFromAxisAngle(new Vector3(1, 0, 1), -0.6f);
        var transB = new Vector3(-7, 8, -9);

        var affineA = Affine3D.FromSRT(scaleA, rotA, transA);
        var affineB = Affine3D.FromSRT(scaleB, rotB, transB);
        var affineCombined = affineA * affineB;

        var numericsA = CreateSRTMatrix(scaleA, rotA, transA);
        var numericsB = CreateSRTMatrix(scaleB, rotB, transB);
        var numericsCombined = numericsA * numericsB;

        var points = new[]
        {
            Vector3.Zero,
            new Vector3(1, 2, 3),
            new Vector3(-4, 5, -6),
            new Vector3(10, -20, 30),
        };

        foreach (var p in points)
        {
            var expected = TransformPointRowMajor(p, numericsCombined);
            var actual = affineCombined.TransformPoint(p);
            Near(expected, actual);
        }

        var roundTrip = Affine3D.FromMatrix4x4(affineCombined.ToMatrix4x4());

        foreach (var p in points)
        {
            Near(affineCombined.TransformPoint(p), roundTrip.TransformPoint(p));
        }
    }

    [Fact]
    public void Affine3D_MutuallyExclusiveCases_AreRejectedOrSeparated()
    {
        var singular = Affine3D.FromTRS(
            new Vector3(1, 2, 3),
            Quaternion.Identity,
            new Vector3(1, 0, 1));

        Assert.Throws<InvalidOperationException>(() => singular.Inverse());

        var affine = Affine3D.FromTRS(
            new Vector3(1, 2, 3),
            QuaternionFromAxisAngle(Vector3.UnitZ, 0.5f),
            new Vector3(2, 2, 2));

        var point = new Vector3(4, 5, 6);
        var direction = new Vector3(4, 5, 6);

        var pointResult = affine.TransformPoint(point);
        var directionResult = affine.TransformDirection(direction);

        Assert.NotEqual(pointResult, directionResult);

        Near(pointResult - affine.Translation, directionResult);
    }

    [Fact]
    public void Affine3D_ScaleRotationTranslation_AreNotCommutative()
    {
        var point = new Vector3(1, 2, 3);

        var scale = Affine3D.FromScale(new Vector3(2, 3, 4));
        var rotation = Affine3D.FromRotation(QuaternionFromAxisAngle(Vector3.UnitZ, MathF.PI / 2));
        var translation = Affine3D.FromTranslation(new Vector3(10, 20, 30));

        var srt = scale * rotation * translation;
        var trs = translation * rotation * scale;

        var a = srt.TransformPoint(point);
        var b = trs.TransformPoint(point);

        Assert.True(Vector3.Distance(a, b) > 1e-3f);
    }
}
