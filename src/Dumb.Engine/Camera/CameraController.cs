using System.Numerics;
using Sia;

namespace Dumb.Engine.Cameras;

public enum CameraMode
{
    FreeLook,
    FirstPerson,
    ThirdPerson
}

public struct CameraController
{
    public CameraMode Mode;

    // FreeLook state
    public Vector3 OrbitTarget;
    public float OrbitDistance;

    // Shared orientation
    public float Yaw;
    public float Pitch;

    // ThirdPerson
    public Vector3 FollowOffset;

    // Settings
    public float MoveSpeed;
    public float MouseSensitivity;
    public float ZoomSpeed;
    public float MinDistance;
    public float MaxDistance;
    public float PitchMin;
    public float PitchMax;

    public static CameraController CreateFreeLook(Vector3 target, float distance = 10f,
        float yaw = MathF.PI / 4f, float pitch = MathF.PI / 6f)
        => new()
        {
            Mode = CameraMode.FreeLook,
            OrbitTarget = target,
            OrbitDistance = distance,
            Yaw = yaw,
            Pitch = pitch,
            MoveSpeed = 8f,
            MouseSensitivity = 0.003f,
            ZoomSpeed = 2f,
            MinDistance = 1f,
            MaxDistance = 100f,
            PitchMin = -MathF.PI / 2f + 0.01f,
            PitchMax = MathF.PI / 2f - 0.01f
        };

    public static CameraController CreateFirstPerson(float yaw = 0f, float pitch = 0f,
        float moveSpeed = 8f, float mouseSensitivity = 0.003f)
        => new()
        {
            Mode = CameraMode.FirstPerson,
            Yaw = yaw,
            Pitch = pitch,
            MoveSpeed = moveSpeed,
            MouseSensitivity = mouseSensitivity,
            PitchMin = -MathF.PI / 2f + 0.01f,
            PitchMax = MathF.PI / 2f - 0.01f
        };

    public static CameraController CreateThirdPerson(Vector3 followOffset,
        float yaw = 0f, float pitch = MathF.PI / 6f, float distance = 5f)
        => new()
        {
            Mode = CameraMode.ThirdPerson,
            FollowOffset = followOffset,
            OrbitDistance = distance,
            Yaw = yaw,
            Pitch = pitch,
            MouseSensitivity = 0.003f,
            ZoomSpeed = 2f,
            MinDistance = 1f,
            MaxDistance = 20f,
            PitchMin = -MathF.PI / 2f + 0.01f,
            PitchMax = MathF.PI / 2f - 0.01f
        };

    public void Update(
        Entity cameraEntity,
        float deltaTime,
        Vector2 lookDelta,
        float zoomDelta,
        Vector3 moveInput,
        bool inputEnabled,
        Vector3 thirdPersonTarget = default)
    {
        ref var camera = ref cameraEntity.Get<Camera>();

        if (inputEnabled)
        {
            Yaw += lookDelta.X * MouseSensitivity;
            Pitch -= lookDelta.Y * MouseSensitivity;
            Pitch = Math.Clamp(Pitch, PitchMin, PitchMax);
            Yaw %= MathF.Tau;
        }

        switch (Mode)
        {
            case CameraMode.FreeLook:
                UpdateFreeLook(ref camera, zoomDelta, inputEnabled);
                break;
            case CameraMode.FirstPerson:
                UpdateFirstPerson(ref camera, moveInput, deltaTime, inputEnabled);
                break;
            case CameraMode.ThirdPerson:
                UpdateThirdPerson(ref camera, thirdPersonTarget, zoomDelta);
                break;
        }
    }

    private void UpdateFreeLook(ref Camera camera, float zoomDelta, bool inputEnabled)
    {
        if (inputEnabled && Math.Abs(zoomDelta) > 0.001f)
        {
            OrbitDistance -= zoomDelta * ZoomSpeed * OrbitDistance * 0.1f;
            OrbitDistance = Math.Clamp(OrbitDistance, MinDistance, MaxDistance);
        }

        camera.SetYawPitch(Yaw, Pitch);
        camera.Position = OrbitTarget - camera.Forward * OrbitDistance;
    }

    private void UpdateFirstPerson(ref Camera camera, Vector3 moveInput, float deltaTime, bool inputEnabled)
    {
        camera.SetYawPitch(Yaw, Pitch);

        if (inputEnabled && moveInput.LengthSquared() > 0.0001f)
        {
            var forward = camera.Forward with { Y = 0 };
            forward = forward.LengthSquared() < 0.0001f
                ? Vector3.UnitZ
                : Vector3.Normalize(forward);

            var right = camera.Right with { Y = 0 };
            right = right.LengthSquared() < 0.0001f
                ? Vector3.UnitX
                : Vector3.Normalize(right);

            camera.Position += (forward * moveInput.Z + right * moveInput.X + Vector3.UnitY * moveInput.Y)
                * (MoveSpeed * deltaTime);
        }
    }

    private void UpdateThirdPerson(ref Camera camera, Vector3 target, float zoomDelta)
    {
        OrbitDistance -= zoomDelta * ZoomSpeed;
        OrbitDistance = Math.Clamp(OrbitDistance, MinDistance, MaxDistance);

        camera.SetYawPitch(Yaw, Pitch);
        var orbitCenter = target + FollowOffset;
        camera.Position = orbitCenter - camera.Forward * OrbitDistance;
    }
}
