using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace NantesGame.UI
{
    [MovedFrom("Nantes.UI")]
    public sealed class NantesSpaceMotion : MonoBehaviour
    {
        public NantesMenu menu;
        [FormerlySerializedAs("moon")] public Transform planet;
        public Transform saucer, saucerBody;
        public Camera spaceCamera;
        public Renderer stars, dust, clouds;
        public Renderer[] foregroundSmoke;
        public TrailRenderer wake;
        [FormerlySerializedAs("moonDegreesPerSecond")] public float planetDegreesPerSecond = .7f;
        public float flightLegSeconds = 19;
        public int flightSeed = 4725;
        public bool automatic = true;
        public float AnimationTime { get; private set; }
        public float PlanetSpinOffset { get; set; }
        public Vector3 Velocity { get; private set; }
        public Vector3 Acceleration { get; private set; }
        public float Throttle { get; private set; }
        public float Braking { get; private set; }
        public float TurnThrust { get; private set; }
        [Min(.1f)] public float vesselMass = 6;
        [Min(.1f)] public float maximumForce = 7.8f;
        const float PhysicsStep = 1f / 120;
        float simulatedTime;
        Vector3 position, acceleration;
        Quaternion attitude;
        bool initialized;
        Vector2 parallax;
        MaterialPropertyBlock starProperties;

        void Start() { Evaluate(0); }

        void LateUpdate()
        {
            bool reduced = menu != null && menu.ReducedMotion;
            if (wake != null)
            {
                wake.emitting = !reduced;
                if (reduced) wake.Clear();
            }
            if (automatic && !reduced) Evaluate(AnimationTime + Time.unscaledDeltaTime);
            Vector2 target = Vector2.zero;
            if (!reduced && Mouse.current != null)
            {
                Vector2 p = Mouse.current.position.ReadValue();
                target = new Vector2(Mathf.Clamp(p.x / Mathf.Max(1,Screen.width) - .5f, -.5f, .5f), Mathf.Clamp(p.y / Mathf.Max(1,Screen.height) - .5f, -.5f, .5f)) * .42f;
            }
            parallax = reduced ? Vector2.zero : Vector2.Lerp(parallax, target, 1 - Mathf.Exp(-Time.unscaledDeltaTime * 2));
            Vector2 drift = reduced ? Vector2.zero : new Vector2(Mathf.Sin(AnimationTime * .13f) * .22f, Mathf.Sin(AnimationTime * .09f) * .12f);
            spaceCamera.transform.localPosition = new Vector3(parallax.x + drift.x, parallax.y + drift.y, -25);
            spaceCamera.orthographicSize = Mathf.Max(5.4f, 9.6f / Mathf.Max(.1f, spaceCamera.aspect));
            spaceCamera.fieldOfView = 2 * Mathf.Atan(spaceCamera.orthographicSize / 25) * Mathf.Rad2Deg;
        }

        public void Evaluate(float seconds)
        {
            AnimationTime = seconds;
            planet.localRotation = Quaternion.Euler(-12, 125 + seconds * planetDegreesPerSecond + PlanetSpinOffset, -18);
            if (!initialized || seconds < simulatedTime)
            {
                initialized = true; simulatedTime = 0;
                position = FlightPoint(0);
                Velocity = (FlightPoint(.05f) - FlightPoint(-.05f)) * 10;
                acceleration = Vector3.zero;
                Throttle=0;Braking=0;TurnThrust=0;Acceleration=Vector3.zero;
                attitude = Quaternion.LookRotation(Velocity.normalized, Vector3.up);
            }
            while (simulatedTime + PhysicsStep <= seconds)
            {
                Simulate(PhysicsStep);
                simulatedTime += PhysicsStep;
            }
            saucer.localPosition = position + Velocity * (seconds - simulatedTime);
            saucer.localRotation = attitude;
            saucer.localScale = Vector3.one;
            saucerBody.localRotation = Quaternion.identity;
            if (stars != null)
            {
                if (starProperties == null) starProperties = new MaterialPropertyBlock();
                starProperties.SetFloat("_TwinkleTime", seconds);
                starProperties.SetFloat("_CloudTime", seconds);
                stars.SetPropertyBlock(starProperties);
                if (dust != null) dust.SetPropertyBlock(starProperties);
                if (clouds != null) clouds.SetPropertyBlock(starProperties);
                if (foregroundSmoke != null)
                    foreach (var layer in foregroundSmoke)
                        if (layer != null) layer.SetPropertyBlock(starProperties);
            }
        }

        void Simulate(float dt)
        {
            float lead = simulatedTime + .9f;
            Vector3 targetVelocity = (FlightPoint(lead + .05f) - FlightPoint(lead - .05f)) * 10;
            Vector3 force = ((FlightPoint(lead) - position) * .68f + (targetVelocity - Velocity) * 1.9f) * vesselMass;
            Vector3 wantedAcceleration = Vector3.ClampMagnitude(force, maximumForce) / vesselMass;
            acceleration = Vector3.MoveTowards(acceleration, wantedAcceleration, dt * 1.3f);
            Velocity += acceleration * dt;
            position += Velocity * dt;
            Acceleration = acceleration;
            if (Velocity.sqrMagnitude > .005f)
            {
                Vector3 direction = Velocity.normalized;
                Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
                float bank = Mathf.Clamp(-Vector3.Dot(acceleration,right) * 24, -19, 19);
                var target = Quaternion.LookRotation(direction, Vector3.up) * Quaternion.AngleAxis(bank, Vector3.forward);
                attitude = Quaternion.RotateTowards(attitude, target, 46 * dt);
            }
            Vector3 localForce = Quaternion.Inverse(attitude) * acceleration;
            float engineDemand = Mathf.Clamp01(localForce.z / (maximumForce / vesselMass));
            Throttle = Mathf.MoveTowards(Throttle, engineDemand, dt * 2.4f);
            Braking = Mathf.Clamp01(-localForce.z / (maximumForce / vesselMass));
            TurnThrust = Mathf.Clamp(localForce.x / (maximumForce / vesselMass), -1, 1);
        }

        public void ResumeVessel(Vector3 releasedPosition,Vector3 releasedVelocity,Quaternion releasedAttitude)
        {
            position=releasedPosition;Velocity=releasedVelocity;attitude=releasedAttitude;
            acceleration=Acceleration=Vector3.zero;simulatedTime=AnimationTime;initialized=true;
            saucer.localPosition=position;saucer.localRotation=attitude;Throttle=1;
        }

        public Vector3 FlightPoint(float seconds)
        {
            float leg = seconds / Mathf.Max(1, flightLegSeconds);
            int i = Mathf.FloorToInt(leg);
            float t = leg - i;
            t+=.085f*Mathf.Sin(t*Mathf.PI*2);
            Vector3 a = Waypoint(i - 1), b = Waypoint(i), c = Waypoint(i + 1), d = Waypoint(i + 2);
            // Catmull-Rom interpolation between waypoints.
            return .5f * (2 * b + (c - a) * t + (2 * a - 5 * b + 4 * c - d) * t * t + (-a + 3 * b - 3 * c + d) * t * t * t);
        }

        Vector3 Waypoint(int index)
        {
            int phase = (index % 6 + 6) % 6;
            float x = phase == 0 ? .85f : phase == 1 ? .36f : phase == 2 ? -.48f : phase == 3 ? -1.28f : phase == 4 ? -.08f : 1.24f;
            if (phase != 3 && phase != 5) x += (Noise(index, 1) - .5f) * .14f;
            float halfWidth = Mathf.Max(9.6f, 5.4f * (spaceCamera != null ? spaceCamera.aspect : 16f / 9));
            float z = -4.5f + Noise(index, 3) * 14;
            float depth = (z + 25) / 25;
            float height = phase == 0 ? 1.2f : phase == 1 ? .65f : phase == 2 ? 1.5f : phase == 3 ? 2.1f : phase == 4 ? 3.5f : 2.7f;
            return new Vector3(x * halfWidth * depth, (height + Noise(index,2) * .3f) * depth, z);
        }

        float Noise(int index, int lane)
        {
            unchecked
            {
                uint n = (uint)(flightSeed + index * 374761393 + lane * 668265263);
                n = (n ^ (n >> 13)) * 1274126177;
                return (n & 0x00ffffff) / 16777215f;
            }
        }
    }
}
