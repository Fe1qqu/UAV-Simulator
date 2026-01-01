public interface IControllable
{
    void ApplyThrottle(float value);
    void ApplyYaw(float value);
    void ApplyPitch(float value);
    void ApplyRoll(float value);
}
